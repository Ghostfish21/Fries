# define SRCGEN_DEBUG

using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Fries.EvtsysSrcgen {

    [Generator]
    public class AttributeUsageCollector : ISourceGenerator {
        private static void resetLog(string assemblyName) {
# if SRCGEN_DEBUG
            try {
                string tempDir = Path.GetTempPath();
                string logFilePath = Path.Combine(tempDir, $"{assemblyName}-EvtsysSrcgen-Debug.txt");
                File.WriteAllText(logFilePath, "");
            }
            catch { }
# endif
        }
        public static void log(string message) {
# if SRCGEN_DEBUG
            try {
                string tempDir = Path.GetTempPath();
                string logFilePath = Path.Combine(tempDir, $"EvtsysSrcgen-Debug.txt");
                File.AppendAllText(logFilePath, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            } catch {}
# endif
        }
        public static void log(string assemblyName, string message) {
# if SRCGEN_DEBUG
            try {
                string tempDir = Path.GetTempPath();
                string logFilePath = Path.Combine(tempDir, $"{assemblyName}-EvtsysSrcgen-Debug.txt");
                File.AppendAllText(logFilePath, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            } catch {}
# endif
        }

        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new EvtAttrReceiver());
        }

        public void Execute(GeneratorExecutionContext context) { 
            string assemblyName = context.Compilation.AssemblyName;
            assemblyName = AssemblyNameUtils.toValidClassName(assemblyName);
            resetLog(assemblyName);
            
            var symbolDisplayFormat = SymbolDisplayFormat.FullyQualifiedFormat;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Reflection;");
            sb.AppendLine("using Fries.EvtSystem;"); 
            sb.AppendLine("namespace Fries.EvtSystem {");
            sb.AppendLine($"    public class {assemblyName}EvtInitializer : Fries.EvtSystem.EvtInitializer {{");
            sb.AppendLine("        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]");
            sb.AppendLine("        private static void create() { ");
            sb.AppendLine($"            var initializer = new {assemblyName}EvtInitializer();");
            sb.AppendLine("            Fries.EvtSystem.EvtInitializer.register(initializer);");
            sb.AppendLine("        }");
            sb.AppendLine($"        public {assemblyName}EvtInitializer() : base() {{}}");
            
            log(assemblyName, "Source Generator starting to work...");

            try {
                if (!(context.SyntaxReceiver is EvtAttrReceiver receiver)) {
                    log(assemblyName, "Source Generator quit working because this is not the correct SyntaxReceiver!");
                    return;
                }

                Compilation compilation = context.Compilation;
                
                object symbol4Method = compilation.GetTypeByMetadataName("Fries.EvtSystem.EvtListener");
                object symbol4Struct = compilation.GetTypeByMetadataName("Fries.EvtSystem.EvtDeclarer");

                if (symbol4Method == null) {
                    log(assemblyName, "Cannot find EvtListener type, did you delete it by accident? Terminating...");
                    return;
                }
                if (symbol4Struct == null) {
                    log(assemblyName, "Cannot find EvtDeclarer type, did you delete it by accident? Terminating...");
                    return;
                }
                INamedTypeSymbol targetAttrSymbol4Method = (INamedTypeSymbol)symbol4Method;
                INamedTypeSymbol targetAttrSymbol4Struct = (INamedTypeSymbol)symbol4Struct;
                
                log(assemblyName, $"Detected {receiver.candidateMethods.Count} methods with EvtListener attribute..."); 
                log(assemblyName, $"Detected {receiver.candidateStructs.Count} structs with EvtDeclarer attribute..."); 
                handleListener(receiver, context, assemblyName, sb, targetAttrSymbol4Method, symbolDisplayFormat);
                handleEvent(receiver, context, assemblyName, sb, targetAttrSymbol4Struct, symbolDisplayFormat);
                

                sb.AppendLine("    }");
                sb.AppendLine("}");

                context.AddSource($"{assemblyName}EvtInitializer.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));

                log(assemblyName, "Code generated: \n" + sb);
            }
            catch (Exception e) {
                log(assemblyName, "Encountered exception during code generation: " + e);
            }
        }

        private void handleListener(EvtAttrReceiver receiver, GeneratorExecutionContext context, string assemblyName, StringBuilder sb, INamedTypeSymbol targetAttrSymbol, SymbolDisplayFormat symbolDisplayFormat) {
            sb.AppendLine("        protected override void init(Action<string, Type, EvtListener, Delegate> registerEvtListenerByInfo, Action<MethodInfo> registerEvtListenerByReflection) {");
            sb.AppendLine("            base.init(registerEvtListenerByInfo, registerEvtListenerByReflection);");
            sb.AppendLine("            EvtListener listener;");
            
            Compilation compilation = context.Compilation;
            foreach (var method in receiver.candidateMethods) {
                SemanticModel model = compilation.GetSemanticModel(method.SyntaxTree);
                object methodSymbol = model.GetDeclaredSymbol(method);
                if (methodSymbol == null) {
                    log(assemblyName, $"Could not get method symbol of model {model}, skipping...");
                    continue;
                }

                if (!(methodSymbol is IMethodSymbol methodInfo)) {
                    log(assemblyName, $"Could not get valid method symbol of model {model}, skipping...");
                    continue;
                }

                var attributes = methodInfo.GetAttributes();

                AttributeData ad = null;
                foreach (var attribute in attributes) {
                    if (attribute.AttributeClass == null) continue;
                    if (!attribute.AttributeClass.Equals(targetAttrSymbol, SymbolEqualityComparer.Default))
                        continue;
                    ad = attribute;
                    break;
                }

                if (ad == null) {
                    log(assemblyName,
                        $"Turns out method {methodInfo.Name} does not have the target EvtListener attribute, skipping...");
                    continue;
                }

                ITypeSymbol evtTypeSymbol = ad.ConstructorArguments[0].Value as ITypeSymbol;
                if (evtTypeSymbol == null) {
                    log(assemblyName, $"Method {methodInfo.Name}'s Event Type Symbol is null! Skipping...");
                    continue;
                }

                string evtTypeFullName = evtTypeSymbol.ToDisplayString(symbolDisplayFormat);
                float priority = getArgValue<float>(ad, 1, "priority", 0f);
                bool canCancel = getArgValue<bool>(ad, 2, "canBeExternallyCancelled", false);

                string friendAssembliesCode = "null";
                var friendsArg = GetArgTypedConstant(ad, 3, "friendAssemblies");
                if (!friendsArg.IsNull) {
                    var values = friendsArg.Values.Select(v => $"\"{v.Value}\"");
                    friendAssembliesCode = $"new string[] {{ {string.Join(", ", values)} }}";
                }

                string priorityCode = priority + "f";
                string canCancelCode = canCancel.ToString().ToLower();

                string classFullName = methodInfo.ContainingType.ToDisplayString(symbolDisplayFormat);
                string methodName = methodInfo.Name;

                if (methodInfo.DeclaredAccessibility == Accessibility.Public) {
                    sb.AppendLine($"            listener = new EvtListener(typeof({evtTypeFullName}), {priorityCode}, {canCancelCode}, {friendAssembliesCode});");
                    if (methodInfo.Parameters.Length > 0) {
                        string paramTypeFullName = "";
                        foreach (var methodInfoParameter in methodInfo.Parameters)
                            paramTypeFullName +=
                                methodInfoParameter.Type.ToDisplayString(symbolDisplayFormat) + ", ";
                        paramTypeFullName = paramTypeFullName.Substring(0, paramTypeFullName.Length - 2);
                        sb.AppendLine(
                            $"            this.registerEvtListenerByInfo(\"{methodName}\", typeof({classFullName}), listener, new Action<{paramTypeFullName}>({classFullName}.{methodName}));");
                    }
                    else
                        sb.AppendLine(
                            $"            this.registerEvtListenerByInfo(\"{methodName}\", typeof({classFullName}), listener, new Action({classFullName}.{methodName}));");
                }
                else {
                    string bindingFlags = "BindingFlags.NonPublic | BindingFlags.Static";
                    sb.AppendLine(
                        $"            this.registerEvtListenerByReflection(typeof({classFullName}).GetMethod(\"{methodName}\", {bindingFlags}));");
                }

                sb.AppendLine();
            }
            
            sb.AppendLine("        }");
        }
        
        private void handleEvent(EvtAttrReceiver declarer, GeneratorExecutionContext context, string assemblyName, StringBuilder sb, INamedTypeSymbol targetAttrSymbol, SymbolDisplayFormat symbolDisplayFormat) {
            sb.AppendLine("        protected override void declare(Action<Type> registerEventByType) {");
            sb.AppendLine("            base.declare(registerEventByType);");
            
            Compilation compilation = context.Compilation;
            foreach (var structDeclaration in declarer.candidateStructs) {
                var model = compilation.GetSemanticModel(structDeclaration.SyntaxTree);

                if (model.GetDeclaredSymbol(structDeclaration) is INamedTypeSymbol structSymbol) {
                    string structTypeFullName = structSymbol.ToDisplayString(symbolDisplayFormat);
                    sb.AppendLine($"            this.registerEventByType(typeof({structTypeFullName}));");
                }

                sb.AppendLine();
            }
            
            sb.AppendLine("        }");
        }

        private T getArgValue<T>(AttributeData data, int position, string name, T defaultValue) {
            TypedConstant constant = GetArgTypedConstant(data, position, name);
            if (constant.IsNull) return defaultValue;
            return (T)constant.Value;
        }

        private TypedConstant GetArgTypedConstant(AttributeData data, int position, string name) {
            // 1. 优先检查命名参数 (e.g. priority: 5)
            foreach (var namedArg in data.NamedArguments) {
                if (namedArg.Key == name) return namedArg.Value;
            }
            // 2. 检查位置参数 (e.g. [Attr(type, 5)])
            if (data.ConstructorArguments.Length > position) 
                return data.ConstructorArguments[position];
            return default;
        }
    }
}