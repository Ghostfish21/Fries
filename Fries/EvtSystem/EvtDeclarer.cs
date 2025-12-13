using System;
using UnityEngine.Scripting;

namespace Fries.EvtSystem {
    [AttributeUsage(AttributeTargets.Struct)]
    public class EvtDeclarer : PreserveAttribute { }
    
    public class GlobalEvt {}
}