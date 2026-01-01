using System;
using System.Linq;
using Mono.Cecil;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace Fries.Ilpp {
    public static class IlppUtils {
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
                catch { return true; }
            }

            return false;
        }
        
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
        private static bool tryGetMethodLocation(MethodDefinition m, out string file, out int line, out int col) {
            file = null; line = 0; col = 0;

            var di = m?.DebugInformation;
            if (di is not { HasSequencePoints: true }) return false;

            // 找到第一个“真正对应源码”的序列点
            foreach (var sp in di.SequencePoints) {
                if (sp == null) continue;
                if (sp.StartLine == hiddenLine) continue; // 编译器隐藏行
                if (sp.Document == null) continue;

                file = sp.Document.Url;   // 通常是绝对路径，也可能是相对路径
                line = sp.StartLine;
                col  = sp.StartColumn;
                return true;
            }
            return false;
        }
    }
}