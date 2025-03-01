using System;
using UnityEngine;

namespace Fries.Inspector {
    [AttributeUsage(AttributeTargets.Field)]
    public class FieldAnchorAttribute : PropertyAttribute { }
}