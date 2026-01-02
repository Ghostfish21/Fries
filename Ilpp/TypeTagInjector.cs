// # define ILPP_DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using Fries.EvtSystem;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Unity.CompilationPipeline.Common.Diagnostics;
using MethodAttributes = Mono.Cecil.MethodAttributes; 

// 如果类本身不是 monobehaviour 就不处理
// 如果类本身带有 TypeTag Attr 则在 Awake 时，将这个类的名称作为 Tag 加入。
// 如果类的继承路径上有任何其他 TypeTag Attr，则也将这些添加为 Tag
// 在类的 OnDestroy 中插入移除该 GameObject 缓存的指令

namespace Fries.Ilpp.TypeTagIl {
    public class TypeTagInjector : ILPostProcessor {
        private static void log(string messageToPrint, List<DiagnosticMessage> messages) {
            # if ILPP_DEBUG
            messages.Add(IlppUtils.logWarning(messageToPrint));
            # endif
        }
        
        
        public override ILPostProcessor GetInstance() => this;

        public override bool WillProcess(ICompiledAssembly compiledAssembly) {
            var name = compiledAssembly.Name;

            if (name.StartsWith("Unity.") ||
                name.StartsWith("UnityEngine.") ||
                name.StartsWith("UnityEditor."))
                return false;

            if (name.Contains("NewAssembly") || name.Contains("Fries")) return true;
            
            return compiledAssembly.References.Any(r => r.EndsWith("NewAssembly.dll") || r.EndsWith("Fries.dll"));
        }
        
        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly) {
            List<DiagnosticMessage> messages = new List<DiagnosticMessage>();
            
            try {
                log("TypeTag Ilpp started...", messages);

                var pdbData = compiledAssembly.InMemoryAssembly.PdbData;
                var hasPdb = pdbData.Length > 0;
                
                if (!WillProcess(compiledAssembly)) {
                    var original = new InMemoryAssembly(compiledAssembly.InMemoryAssembly.PeData, compiledAssembly.InMemoryAssembly.PdbData);
                    return new ILPostProcessResult(original, messages);
                }

                using var stream = new MemoryStream(compiledAssembly.InMemoryAssembly.PeData);
                using var pdbStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData); 
                
                using var resolver = new DefaultAssemblyResolver();
                collectAssembly(resolver, compiledAssembly, messages);
                
                var readerParameters = new ReaderParameters {
                    AssemblyResolver = resolver,
                    ReadWrite = false,
                    ReadingMode = ReadingMode.Immediate,
                    ReadSymbols = hasPdb, 
                    SymbolStream = pdbStream, 
                    SymbolReaderProvider = hasPdb ? new PortablePdbReaderProvider() : null
                };
                using var assemblyDefinition = AssemblyDefinition.ReadAssembly(stream, readerParameters);

                bool isAssemblyModified = false;
                
                // 遍历处理所有程序集
                foreach (var module in assemblyDefinition.Modules) {
                    if (ProcessModule(assemblyDefinition, resolver, module, messages))
                        isAssemblyModified = true;
                }

                if (!isAssemblyModified) {
                    log("Ilpp exited due to no changes...", messages);
                    var original = new InMemoryAssembly(compiledAssembly.InMemoryAssembly.PeData, compiledAssembly.InMemoryAssembly.PdbData);
                    return new ILPostProcessResult(original, messages);
                }

                var pe = new MemoryStream();
                var pdb = new MemoryStream();
                var writerParameters = new WriterParameters {
                    SymbolWriterProvider = new PortablePdbWriterProvider(),
                    WriteSymbols = true,
                    SymbolStream = pdb
                };
                assemblyDefinition.Write(pe, writerParameters);

                return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), messages);
            } catch (Exception e) {
                messages.Add(IlppUtils.logError($"Caught exception during processing {e}", "NULL",0,0));
                var original = new InMemoryAssembly(compiledAssembly.InMemoryAssembly.PeData, compiledAssembly.InMemoryAssembly.PdbData);
                return new ILPostProcessResult(original, messages);            
            }
        }

        private TypeDefinition getTypeTagHelperDef(AssemblyDefinition assemblyDefinition, DefaultAssemblyResolver resolver, List<DiagnosticMessage> messages) {
            var typeTagHelperDef = assemblyDefinition.MainModule.GetType("Fries.CompCache.TypeTagHelper");
            if (typeTagHelperDef == null) {
                var friesAssemblyDef = IlppUtils.getFriesDef(assemblyDefinition, resolver);
                typeTagHelperDef = friesAssemblyDef.MainModule.GetType("Fries.CompCache.TypeTagHelper");
            }

            if (typeTagHelperDef != null) return typeTagHelperDef;
            log("Couldn't find Fries.CompCache.TypeTagHelper", messages);
            return null;
        }

        private void collectAssembly(BaseAssemblyResolver resolver, ICompiledAssembly compiledAssembly, List<DiagnosticMessage> messages) {
            var searchDirs = new HashSet<string>();
            foreach (var refPath in compiledAssembly.References) {
                try {
                    var dir = Path.GetDirectoryName(refPath);
                    if (!string.IsNullOrEmpty(dir)) searchDirs.Add(dir);
                }
                catch (Exception e) {
                    log("Failed to get assembly location: " + e, messages);
                }
            }
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                try {
                    if (asm.IsDynamic || string.IsNullOrEmpty(asm.Location)) continue;
                    var dir = Path.GetDirectoryName(asm.Location);
                    if (!string.IsNullOrEmpty(dir)) searchDirs.Add(dir);
                }
                catch (Exception e) {
                    log("Failed to get assembly location: " + e, messages);
                }
            }
            foreach (var dir in searchDirs) 
                resolver.AddSearchDirectory(dir);
        }

        private bool ProcessModule(AssemblyDefinition assemblyDefinition, DefaultAssemblyResolver resolver, ModuleDefinition module, List<DiagnosticMessage> messages) {
            bool isModuleModified = false;

            AssemblyDefinition fries = IlppUtils.getFriesDef(assemblyDefinition, resolver);
            if (fries == null) throw new NullReferenceException("Fries Assembly Definition not found!");
            log($"Ilpp processing module {module.Name}...", messages);
            
            // 获取目标方法
            TypeDefinition typeTagHelperDef = getTypeTagHelperDef(assemblyDefinition, resolver, messages);
            var addTagsMethodDef = typeTagHelperDef.Methods.First(m =>
                m.Name == "addTags" && m.IsStatic && m.Parameters.Count == 2);
            var addTagsMethodRef = module.ImportReference(addTagsMethodDef);
            var removeTagsMethodDef = typeTagHelperDef.Methods.First(m =>
                m.Name == "removeTags" && m.IsStatic && m.Parameters.Count == 2);
            var removeTagsMethodRef = module.ImportReference(removeTagsMethodDef);
            // 参数一，二类引用
            var monoBehaviourParamDef = addTagsMethodDef.Parameters[0].ParameterType.Resolve();
            var typeParamDef = addTagsMethodDef.Parameters[1].ParameterType.Resolve();
            // typeof 等价引用
            var getTypeFromHandleDef = typeParamDef.Methods.First(m =>
                m.Name == "GetTypeFromHandle" && m.IsStatic && m.Parameters.Count == 1);
            var getTypeFromHandleRef = module.ImportReference(getTypeFromHandleDef);

            // 遍历所有类
            foreach (var typeDef in module.Types) {
                if (ReferenceEquals(typeDef, module.Types[0])) continue;
                
                // 是接口的话就返回 - 接口没有构造函数
                if (typeDef.IsInterface) continue;
                if (typeDef.IsEnum) continue;
                if (typeDef.IsAbstract) continue;
                if (IlppUtils.isGenericOrInsideGeneric(typeDef)) continue;

                // 判断该类是不是 MonoBehaviour 的子类
                StringBuilder sb = new StringBuilder();
                int mark = 0;
                bool isMonoBehaviour = false;
                try {
                    sb.Append(typeDef.FullName);
                    mark = sb.Length;
                    sb.Append(" : ");
                    TypeDefinition bt = typeDef.BaseType.Resolve();
                    while (bt != null && bt.BaseType != null) {
                        sb.Append(bt.Name);
                        mark = sb.Length;
                        sb.Append(" : ");

                        if (IlppUtils.typeDefEquals(bt, monoBehaviourParamDef)) {
                            isMonoBehaviour = true;
                            break;
                        }

                        bt = bt.BaseType.Resolve();
                    }
                }
                catch (Exception e) {
                    sb.Length = mark;
                    messages.Add(IlppUtils.logWarning($"Unable to resolve for TypeTag, class {sb} is unable to resolve. Consider manually register for the TypeTag. (Exception captured: \n{e})"));
                }
                
                if (!isMonoBehaviour) continue;
                // 判断类上是否存在 TypeTag 标签
                if (!IlppUtils.containsAttr(typeDef, "Fries.CompCache.TypeTag", fries.FullName, out var attr))
                    continue;
                
                // 执行注入到 Awake 和 OnDestroy 开头
                var awake = IlppUtils.getMethod(typeDef, "Awake");
                var onDestroy = IlppUtils.getMethod(typeDef, "OnDestroy");
                if (awake == null || onDestroy == null) {
                    bool sw = IlppUtils.getAttributeReadonlyBoolField(attr, 0);
                    if (sw) continue;
                    messages.Add(IlppUtils.logError(
                        $"Class {typeDef.FullName} must provide both Awake and OnDestroy method in order to be correctly managed by TypeTag",
                        typeDef));
                    continue;
                }
                InjectCode(module, awake, typeDef, addTagsMethodRef, getTypeFromHandleRef, monoBehaviourParamDef);
                InjectCode(module, onDestroy, typeDef, removeTagsMethodRef, getTypeFromHandleRef, monoBehaviourParamDef);
                isModuleModified = true;
            }

            return isModuleModified;
        }

        private void InjectCode(ModuleDefinition module, MethodDefinition method, TypeDefinition instType,
            MethodReference addTagsMethod, MethodReference getTypeFromHandle, TypeDefinition monoBehaviourTypeDef) {
            method.Body.SimplifyMacros();

            var processor = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;

            Instruction injectionPoint = null;

            if (instructions.Count > 0)
                injectionPoint = instructions[0];
            else {
                // 理论上不会走到这里，因为即便是新建的 Awake 也有 ret，但防个万一
                var ret = processor.Create(OpCodes.Ret);
                processor.Append(ret);
                injectionPoint = ret;
            }

            var importedInstType = module.ImportReference(instType);
            var importedMonoBehaviour = module.ImportReference(monoBehaviourTypeDef);
            
            var ilList = new List<Instruction> {
                // 参数一 (MonoBehaviour) this
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Castclass, importedMonoBehaviour),
                // 参数二 typeof(携带目标Attr的类)
                processor.Create(OpCodes.Ldtoken, importedInstType),
                processor.Create(OpCodes.Call, getTypeFromHandle),
                // 调用 addTags 方法
                processor.Create(OpCodes.Call, addTagsMethod)
            };

            // 执行插入
            if (injectionPoint != null) {
                foreach (var instruction in ilList)
                    processor.InsertBefore(injectionPoint, instruction);
            }
            else {
                foreach (var instruction in ilList) processor.Append(instruction);
            }

            method.Body.OptimizeMacros();
        }
    }
}