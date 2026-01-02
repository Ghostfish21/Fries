using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace Fries.Ilpp {
    public static class IlppUtils {
        # region Type Definition 方法

        public static MethodDefinition getMethod(TypeDefinition typeDef, string methodName) {
            var awake = typeDef.Methods.FirstOrDefault(m =>
                m.Name == methodName // 方法名匹配
                && !m.HasParameters // 无参
                && m.HasBody // 有方法体
                && !m.IsAbstract // 不是抽象
                && !m.IsStatic // 不是静态
                && m.HasThis // 会传递自身引用
                && !m.HasGenericParameters // 没有泛型
                && !m.IsConstructor // 不能是构造函数
                && !m.IsSetter // 不能是访问器
                && !m.IsGetter // 不能是访问器
                && m.ReturnType.MetadataType == MetadataType.Void);
            return awake;
        }

        public static bool containsAttr(TypeDefinition typeDef, string fullNameAndScope, string targetAssemblyName,
            out CustomAttribute customAttribute) {
            customAttribute = null;
            foreach (var typeDefCustomAttribute in typeDef.CustomAttributes) {
                if (typeDefCustomAttribute.AttributeType.FullName != fullNameAndScope) continue;
                if (targetAssemblyName != null &&
                    getScopeAssemblyName(typeDefCustomAttribute.AttributeType) != targetAssemblyName)
                    continue;

                customAttribute = typeDefCustomAttribute;
                return true;
            }

            return false;
        }

        public static bool containsAttr(TypeDefinition typeDef, string fullNameAndScope,
            string targetAssemblyName = null) {
            return containsAttr(typeDef, fullNameAndScope, targetAssemblyName);
        }

        private static string getScopeAssemblyName(TypeReference tr) {
            if (tr == null) return null;
            return tr.Scope switch {
                AssemblyNameReference anr => anr.FullName, // 引用的程序集名（最常见）
                ModuleDefinition md => md.Assembly?.Name?.FullName, // 同模块/同程序集里的类型
                _ => tr.Scope?.Name // 兜底
            };
        }

        public static bool isGenericOrInsideGeneric(TypeDefinition typeDef) {
            // 泛型类型本身 / 或者嵌套在泛型外部类型里，都跳过
            for (var cur = typeDef; cur != null; cur = cur.DeclaringType)
                if (cur.HasGenericParameters)
                    return true;

            for (TypeReference bt = typeDef.BaseType; bt != null;) {
                if (bt is GenericInstanceType) return true;

                try {
                    var def = bt.Resolve();
                    if (def == null) return true;
                    if (def.HasGenericParameters) return true;
                    bt = def.BaseType;
                }
                catch {
                    return true;
                }
            }

            return false;
        }

        public static bool typeDefEquals(TypeDefinition a, TypeDefinition b) {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;

            var ma = a.Module;
            var mb = b.Module;
            if (ma is null || mb is null) return false;
            if (a.MetadataToken.RID == 0 || b.MetadataToken.RID == 0) return false;
            return a.MetadataToken == b.MetadataToken && ma.Mvid == mb.Mvid;
        }

        private const int hiddenLine = 0xFEEFEE;

        private static string getTypeLocation(TypeDefinition typeDef) {
            const int HiddenLine = 0xFEEFEE; // 编译器隐藏行标记

            if (typeDef == null) return "NULL";

            foreach (var m in typeDef.Methods) {
                var di = m.DebugInformation;
                if (di == null || !di.HasSequencePoints) continue;

                foreach (var sp in di.SequencePoints) {
                    if (sp == null) continue;
                    if (sp.StartLine == HiddenLine) continue;
                    if (sp.Document == null) continue;

                    var url = sp.Document.Url;
                    if (string.IsNullOrEmpty(url)) continue;
                    return url;
                }
            }

            return "NULL";
        }

        # endregion

        # region Logger 方法

        public static DiagnosticMessage log(DiagnosticType type, string message, string filePath, int lineNumber,
            int columnNumber) {
            return new DiagnosticMessage {
                DiagnosticType = type,
                MessageData = message,
                File = filePath,
                Line = lineNumber,
                Column = columnNumber,
            };
        }

        public static DiagnosticMessage logWarning(string message, string filePath = null, int lineNumber = 0,
            int columnNumber = 0) {
            return log(DiagnosticType.Warning, message, filePath, lineNumber, columnNumber);
        }

        public static DiagnosticMessage logError(string message, string filePath, int lineNumber, int columnNumber) {
            return log(DiagnosticType.Error, message, filePath, lineNumber, columnNumber);
        }

        public static DiagnosticMessage logError(string message, TypeDefinition typeDef, string filePath = null,
            int lineNumber = -1, int columnNumber = -1) {
            filePath ??= getTypeLocation(typeDef);
            int index = filePath.IndexOf("/Assets/", StringComparison.Ordinal);
            if (index != -1) filePath = filePath.Substring(index);
            if (lineNumber == -1) lineNumber = 0;
            if (columnNumber == -1) columnNumber = 0;
            return log(DiagnosticType.Error, message, filePath, lineNumber, columnNumber);
        }

        # endregion

        # region Others 方法

        private static bool tryGetMethodLocation(MethodDefinition m, out string file, out int line, out int col) {
            file = null;
            line = 0;
            col = 0;

            var di = m?.DebugInformation;
            if (di is not { HasSequencePoints: true }) return false;

            // 找到第一个“真正对应源码”的序列点
            foreach (var sp in di.SequencePoints) {
                if (sp == null) continue;
                if (sp.StartLine == hiddenLine) continue; // 编译器隐藏行
                if (sp.Document == null) continue;

                file = sp.Document.Url; // 通常是绝对路径，也可能是相对路径
                line = sp.StartLine;
                col = sp.StartColumn;
                return true;
            }

            return false;
        }

        public static AssemblyDefinition getFriesDef(AssemblyDefinition assemblyDefinition,
            BaseAssemblyResolver resolver) {
            var selfName = assemblyDefinition.Name?.Name;
            if (selfName == "Fries" || selfName == "NewAssembly")
                return assemblyDefinition;
            
            AssemblyNameReference friesAssemblyRef = null;
            foreach (var assemblyDefinitionModule in assemblyDefinition.Modules) {
                friesAssemblyRef = assemblyDefinitionModule.AssemblyReferences.FirstOrDefault(r =>
                    r.Name.Contains("Fries") || r.Name.Contains("NewAssembly"));
                if (friesAssemblyRef != null) break;
            }
            if (friesAssemblyRef == null) return null;
            
            var rp = new ReaderParameters {
                ReadingMode = ReadingMode.Immediate,
                ReadWrite = false
            };
            return resolver.Resolve(friesAssemblyRef, rp);
        }

        public static bool getAttributeReadonlyBoolField(CustomAttribute attr, int declaredOrder) {
            if (attr.ConstructorArguments.Count > 0 && attr.ConstructorArguments[declaredOrder].Value is bool val)
                return val;
            return false;
        }

        # endregion
    }
}