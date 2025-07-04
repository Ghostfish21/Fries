using System;

namespace Fries.EventFunctions {
    [AttributeUsage(AttributeTargets.Method)]
    public class EventFunctionDeclarer : Attribute { }
}