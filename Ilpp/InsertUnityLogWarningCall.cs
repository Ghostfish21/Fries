using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Fries.Ilpp {
    public static class InsertUnityLogWarningCall {
        private static readonly ConditionalWeakTable<ModuleDefinition, MethodReference> logWarning = new();
        private static readonly object mutex = new();

        public static void call(ILProcessor il, ModuleDefinition module, string message) {
            var logWarning1 = importUnityDebugLogWarning(module);
            il.Append(Instruction.Create(OpCodes.Ldstr, message));
            il.Append(Instruction.Create(OpCodes.Call, logWarning1));
        }
        
        private static MethodReference importUnityDebugLogWarning(ModuleDefinition module) {
            lock (mutex) {
                if (logWarning.TryGetValue(module, out var ret)) return ret;
            
                // 优先从已引用的 UnityEngine.* 程序集中解析 UnityEngine.Debug
                foreach (var aref in module.AssemblyReferences) {
                    if (!aref.Name.StartsWith("UnityEngine", StringComparison.Ordinal)) continue;
                    try {
                        var asm = module.AssemblyResolver.Resolve(aref);
                        var debugType = asm?.MainModule?.GetType("UnityEngine.Debug");
                        if (debugType == null) continue;

                        // 选 LogWarning(object)（ldstr 的 string 也可作为 object 传）
                        var m = debugType.Methods.FirstOrDefault(x =>
                            x.Name == "LogWarning" &&
                            x.Parameters.Count == 1 &&
                            x.Parameters[0].ParameterType.FullName == "System.Object");

                        if (m == null) continue;
                        MethodReference logWarning1 = module.ImportReference(m);
                        if (logWarning1 != null) logWarning.Add(module, logWarning1);
                        return logWarning1;
                    }
                    catch { throw; }
                }

                throw new InvalidOperationException(
                    "Cannot resolve UnityEngine.Debug.LogWarning(object). Make sure UnityEngine.CoreModule is referenced.");
            }
        }

    }
}