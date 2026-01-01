using Mono.Cecil;

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
    }
}