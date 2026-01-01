// namespace Fries.Ilpp {
//     using System;
//     using System.Linq;
//     using Mono.Cecil;
//     using Mono.Cecil.Cil;
//
//     public static class GetOrCreateMethod {
//         private static MethodDefinition call(TypeDefinition typeDef, string methodName) {
//             if (typeDef == null) return null;
//             if (typeDef.IsInterface) return null;
//             if (typeDef.IsEnum) return null;
//             if (typeDef.IsValueType) return null;
//             // if (IsGenericOrInsideGeneric(typeDef)) return null;
//
//             var existing = findExistingZeroParamVoidMethod(typeDef, methodName);
//             if (existing != null) return existing;
//
//             var created = createProtectedZeroParamVoidMethod(typeDef.Module, methodName);
//
//             var baseMethod = findBaseZeroParamVoidMethod(typeDef, methodName);
//             if (baseMethod != null) emitBasePrelude(created, typeDef, baseMethod);
//
//             ensureSingleRetAtEnd(created);
//             typeDef.Methods.Add(created);
//             return created;
//         }
//
//         private static MethodDefinition findExistingZeroParamVoidMethod(TypeDefinition typeDef, string methodName) {
//             return typeDef.Methods.FirstOrDefault(m =>
//                 m.Name == methodName && !m.HasParameters && !m.IsStatic && m.HasThis && !m.HasGenericParameters &&
//                 m.HasBody && m.ReturnType.FullName == typeDef.Module.TypeSystem.Void.FullName);
//         }


//
//         private static MethodDefinition createProtectedZeroParamVoidMethod(ModuleDefinition module, string methodName) {
//             // 一律创建 protected
//             var attrs = MethodAttributes.Family | MethodAttributes.HideBySig;
//             var m = new MethodDefinition(methodName, attrs, module.TypeSystem.Void);
//             return m;
//         }
//
//         private static MethodDefinition findBaseZeroParamVoidMethod(TypeDefinition typeDef, string methodName) {
//             try {
//                 var bt = typeDef.BaseType;
//                 while (bt != null) {
//                     var resolved = bt.Resolve();
//                     if (resolved == null) break;
//
//                     var found = resolved.Methods.FirstOrDefault(m =>
//                         m.Name == methodName && !m.HasParameters && !m.IsStatic && m.HasThis &&
//                         !m.HasGenericParameters &&
//                         m.ReturnType.FullName == resolved.Module.TypeSystem.Void.FullName);
//
//                     if (found != null) return found;
//
//                     bt = resolved.BaseType;
//                 }
//             }
//             catch { // ignored
//             }
//
//             return null;
//         }
//
//         private static void emitBasePrelude(MethodDefinition created, TypeDefinition thisClass, MethodDefinition baseMethodDef) {
//             var il = created.Body.GetILProcessor();
//
//             if (!CanCallFrom(thisClass, baseMethodDef)) {
//                 InsertUnityLogWarningCall.call(il, thisClass.Module, $"Base method found, but unable to execute it from Class {thisClass.FullName}. Please check the accessibility level.");
//                 return;
//             }
//
//             emitCallBase(il, thisClass.Module, baseMethodDef);
//         }
//
//         private static void emitCallBase(ILProcessor il, ModuleDefinition module,
//             MethodDefinition baseMethodDef) {
//             // base.Xxx(): ldarg.0 + call baseMethod
//             il.Append(Instruction.Create(OpCodes.Ldarg_0));
//
//             var imported = importBaseMethodReference(module, baseMethodDef);
//             il.Append(Instruction.Create(OpCodes.Call, imported));
//         }
//
//         private static MethodReference importBaseMethodReference(ModuleDefinition module, 
//             MethodDefinition baseMethodDef) => module.ImportReference(baseMethodDef);
//         
//         private static void ensureSingleRetAtEnd(MethodDefinition method) {
//             var il = method.Body.GetILProcessor();
//             if (method.Body.Instructions.Count == 0 || method.Body.Instructions[^1].OpCode != OpCodes.Ret)
//                 il.Append(Instruction.Create(OpCodes.Ret));
//         }
//
//         // 1) 先把 TypeReference 规整成“可 Resolve 的元素类型”
//         private static TypeReference StripTypeSpecs(TypeReference t) {
//             while (t is TypeSpecification spec) t = spec.ElementType;
//             return t;
//         }
//
//         private static bool TryResolveTypeDef(TypeReference tr, ModuleDefinition contextModule, out TypeDefinition def) {
//             def = null;
//             if (tr == null || contextModule == null) return false;
//
//             tr = StripTypeSpecs(tr);
//
//             try {
//                 def = tr.Resolve();
//                 if (def != null) return true;
//             }
//             catch { /* keep trying */ }
//
//             // tr.Resolve() 失败时，尝试用 scope 的程序集去找 ExportedType 再 Resolve
//             if (tr.Scope is AssemblyNameReference anr) {
//                 try {
//                     var asm = contextModule.AssemblyResolver.Resolve(anr);
//                     var mod = asm?.MainModule;
//                     if (mod == null) return false;
//
//                     // ExportedTypes 里可能是转发表（TypeForwardedTo）
//                     var exported = mod.ExportedTypes.FirstOrDefault(et => et.FullName == tr.FullName);
//                     def = exported?.Resolve();
//                     return def != null;
//                 }
//                 catch {
//                     return false;
//                 }
//             }
//
//             return false;
//         }
//
//         private static (Guid mvid, int token) GetTypeId(TypeDefinition d)
//             => (d.Module.Mvid, d.MetadataToken.ToInt32());
//
//         private static bool SameTypeDef(TypeDefinition a, TypeDefinition b) {
//             if (a == null || b == null) return false;
//             return GetTypeId(a) == GetTypeId(b);
//         }
//
//         private static bool IsDerivedFrom(TypeDefinition derived, TypeReference baseType) {
//             if (derived == null || baseType == null) return false;
//
//             // 先 resolve 目标 base 的真实定义（如果这里都 resolve 不出来，就保守返回 false）
//             if (!TryResolveTypeDef(baseType, derived.Module, out var baseDef))
//                 return false;
//
//             var cur = derived;
//             for (int guard = 0; guard < 256; guard++) {
//                 TypeReference bt;
//                 try {
//                     bt = cur.BaseType;
//                 }
//                 catch {
//                     return false;
//                 }
//
//                 if (bt == null) return false;
//
//                 if (!TryResolveTypeDef(bt, derived.Module, out var btDef))
//                     return false;
//
//                 if (SameTypeDef(btDef, baseDef))
//                     return true;
//
//                 cur = btDef;
//             }
//
//             return false;
//         }
//
//
//         private static bool CanCallFrom(TypeDefinition thisClass, MethodDefinition target) {
//             if (thisClass == null || target == null) return false;
//
//             // 你当前的 emitCallBase 是 ldarg.0 + call，所以这里必须是实例方法
//             if (target.IsStatic || !target.HasThis) return false;
//             if (target.IsAbstract) return false;
//
//             // 泛型方法定义没法直接 call（需要实例化），保守起见直接拒绝
//             if (target.HasGenericParameters) return false;
//
//             // private / private scope 一律不可见
//             if (target.IsPrivate || target.IsCompilerControlled) return false;
//
//             bool isDerived = IsDerivedFrom(thisClass, target.DeclaringType);
//             bool sameOrFriendAssembly = IsSameAssembly(thisClass.Module.Assembly, target.Module.Assembly) ||
//                                         HasInternalsVisibleTo(target.Module.Assembly, thisClass.Module.Assembly);
//
//             if (target.IsPublic) return true;
//
//             // protected
//             if (target.IsFamily) return isDerived;
//
//             // internal
//             if (target.IsAssembly) return sameOrFriendAssembly;
//
//             // protected internal
//             if (target.IsFamilyOrAssembly) return isDerived || sameOrFriendAssembly;
//
//             // private protected
//             if (target.IsFamilyAndAssembly) return isDerived && sameOrFriendAssembly;
//
//             return false;
//         }
//
//         private static bool IsSameAssembly(AssemblyDefinition a, AssemblyDefinition b) {
//             if (a == null || b == null) return false;
//             // FullName 包含版本、文化、公钥token等信息，比 Name.\
//             // 更严格
//             return string.Equals(a.Name.FullName, b.Name.FullName, StringComparison.Ordinal);
//         }
//
//         private static bool HasInternalsVisibleTo(AssemblyDefinition provider, AssemblyDefinition requester) {
//             if (provider == null || requester == null) return false;
//
//             const string attrName = "System.Runtime.CompilerServices.InternalsVisibleToAttribute";
//             var requesterName = requester.Name?.Name;
//             if (string.IsNullOrEmpty(requesterName)) return false;
//
//             string requesterPublicKeyHex = TryGetPublicKeyHex(requester.Name);
//
//             foreach (var attr in provider.CustomAttributes) {
//                 if (attr.AttributeType?.FullName != attrName) continue;
//                 if (attr.ConstructorArguments.Count < 1) continue;
//
//                 var raw = attr.ConstructorArguments[0].Value as string;
//                 if (string.IsNullOrWhiteSpace(raw)) continue;
//
//                 // 格式一般是： "Friend.Assembly, PublicKey=0024..."
//                 var parts = raw.Split(',');
//                 var targetSimpleName = parts[0].Trim();
//
//                 if (!string.Equals(targetSimpleName, requesterName, StringComparison.OrdinalIgnoreCase))
//                     continue;
//
//                 // requester 是强命名时，要求 IVT 里也带匹配的 PublicKey（更保守，避免误判导致坏 IL）
//                 if (!string.IsNullOrEmpty(requesterPublicKeyHex)) {
//                     var pkPart = parts
//                         .Skip(1)
//                         .Select(p => p.Trim())
//                         .FirstOrDefault(p => p.StartsWith("PublicKey=", StringComparison.OrdinalIgnoreCase));
//
//                     if (pkPart == null) continue;
//
//                     var pk = pkPart.Substring("PublicKey=".Length).Trim();
//                     if (!string.Equals(pk, requesterPublicKeyHex, StringComparison.OrdinalIgnoreCase))
//                         continue;
//                 }
//
//                 return true;
//             }
//
//             return false;
//         }
//
//         private static string TryGetPublicKeyHex(AssemblyNameDefinition name) {
//             if (name == null) return null;
//             // Cecil: HasPublicKey 为 true 时 PublicKey 才是完整公钥
//             if (!name.HasPublicKey || name.PublicKey == null || name.PublicKey.Length == 0) return null;
//
//             var bytes = name.PublicKey;
//             char[] c = new char[bytes.Length * 2];
//             int idx = 0;
//             for (int i = 0; i < bytes.Length; i++) {
//                 byte b = bytes[i];
//                 c[idx++] = GetHex(b >> 4);
//                 c[idx++] = GetHex(b & 0xF);
//             }
//
//             return new string(c);
//         }
//
//         private static char GetHex(int v) => (char)(v < 10 ? ('0' + v) : ('a' + (v - 10)));
//     }
// }