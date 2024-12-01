using System;

namespace Fries.Inspector {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class IgnoreInInspectorAttribute : Attribute {
        
    }
}