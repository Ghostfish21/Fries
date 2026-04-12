using System;

namespace Fries.GobjPersistObjects {
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class GobjSrcgenTarget : Attribute {
        public GobjSrcgenTarget(bool areFieldsPersistByDefault = false) {}
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class NonPersist : Attribute { }
    [AttributeUsage(AttributeTargets.Field)]
    public class Persist : Attribute { }
}