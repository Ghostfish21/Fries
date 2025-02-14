# if UNITY_EDITOR
using UnityEditor;
# endif

namespace Fries.Inspector.GameObjectBoxField {
    public class PickerData {
        # if UNITY_EDITOR
        public SerializedObject serializedObject;
        # endif
        public string propertyPath;
        public System.Type elementType;
    }
}