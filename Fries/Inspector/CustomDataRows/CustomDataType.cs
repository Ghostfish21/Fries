using System;
using UnityEditor;

namespace Fries.Inspector.CustomDataRows {
    public interface CustomDataType {
        string getDisplayName();
        
        Type getType();

        object getDefaultValue();
    }
}