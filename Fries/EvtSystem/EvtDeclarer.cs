using System;

namespace Fries.EvtSystem {
    [AttributeUsage(AttributeTargets.Struct)]
    public class EvtDeclarer : Attribute { }
    
    public class GlobalEvt {}
}