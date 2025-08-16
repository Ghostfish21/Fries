namespace Fries.InsertionEventSys {
    using UnityEditor;
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    # if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InsertionEventInformation))]
    public class InsertionEventInformationDrawer : PropertyDrawer {
        private const float LineHeight = 18f;
        private const float Spacing = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float height = LineHeight + Spacing; // For the main label

            // insertedClass and eventName
            height += (LineHeight + Spacing) * 2;

            // argsTypes
            SerializedProperty argsTypesProperty = property.FindPropertyRelative("argsTypes");
            if (argsTypesProperty != null && argsTypesProperty.isArray) {
                // Calculate height for argsTypes, considering potential wrapping
                // This is a more accurate calculation using GUIStyle.CalcHeight
                GUIContent argsLabel = new GUIContent("Argument Types:");
                string argsText = "";
                for (int i = 0; i < argsTypesProperty.arraySize; i++) {
                    SerializedProperty argTypeElement = argsTypesProperty.GetArrayElementAtIndex(i);
                    if (argTypeElement != null && argTypeElement.objectReferenceValue != null) {
                        argsText += argTypeElement.objectReferenceValue.name;
                    }
                    else {
                        argsText += "(null)";
                    }

                    if (i < argsTypesProperty.arraySize - 1) {
                        argsText += ", ";
                    }
                }

                GUIContent argsContent = new GUIContent(argsText);
                GUIStyle style = new GUIStyle(EditorStyles.label);
                style.wordWrap = true;
                height +=
                    style.CalcHeight(argsContent, EditorGUIUtility.currentViewWidth - EditorGUI.indentLevel * 15f) +
                    Spacing; // Indent level for padding
            }

            // Listeners
            SerializedProperty listenersProperty = property.FindPropertyRelative("listeners");
            if (listenersProperty != null && listenersProperty.isArray) {
                height += LineHeight + Spacing; // For the listeners count line
                if (listenersProperty.isExpanded) {
                    height += (LineHeight + Spacing) * listenersProperty.arraySize *
                              2; // For each listener (listenFrom and methodName)
                }
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            Rect currentPosition = new Rect(position.x, position.y, position.width, LineHeight);

            // Draw label for the entire property
            EditorGUI.LabelField(currentPosition, label);
            currentPosition.y += LineHeight + Spacing;

            // Save original GUI color
            Color originalColor = GUI.color;
            GUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f); // Faded gray

            EditorGUI.BeginDisabledGroup(true); // Make all fields read-only

            // insertedClass
            SerializedProperty insertedClassProperty = property.FindPropertyRelative("insertedClass");
            if (insertedClassProperty != null) {
                EditorGUI.PropertyField(currentPosition, insertedClassProperty,
                    new GUIContent("Inserted Class (Script)"));
                currentPosition.y += LineHeight + Spacing;
            }

            // eventName
            SerializedProperty eventNameProperty = property.FindPropertyRelative("eventName");
            if (eventNameProperty != null) {
                EditorGUI.PropertyField(currentPosition, eventNameProperty, new GUIContent("Event Name (String)"));
                currentPosition.y += LineHeight + Spacing;
            }

            // argsTypes
            SerializedProperty argsTypesProperty = property.FindPropertyRelative("argsTypes");
            if (argsTypesProperty != null && argsTypesProperty.isArray) {
                string argsText = "";
                for (int i = 0; i < argsTypesProperty.arraySize; i++) {
                    SerializedProperty argTypeElement = argsTypesProperty.GetArrayElementAtIndex(i);
                    if (argTypeElement != null && argTypeElement.objectReferenceValue != null) {
                        argsText += argTypeElement.objectReferenceValue.name;
                    }
                    else {
                        argsText += "(null)";
                    }

                    if (i < argsTypesProperty.arraySize - 1) {
                        argsText += ", ";
                    }
                }

                GUIContent argsContent = new GUIContent(argsText);
                GUIStyle style = new GUIStyle(EditorStyles.label);
                style.wordWrap = true;
                float argsHeight = style.CalcHeight(argsContent,
                    EditorGUIUtility.currentViewWidth - EditorGUI.indentLevel * 15f);
                EditorGUI.LabelField(new Rect(currentPosition.x, currentPosition.y, position.width, argsHeight),
                    new GUIContent("Argument Types:"), argsContent, style);
                currentPosition.y += argsHeight + Spacing;
            }

            // Listeners
            SerializedProperty listenersProperty = property.FindPropertyRelative("listeners");
            if (listenersProperty != null && listenersProperty.isArray) {
                // Draw foldout for listeners
                Rect foldoutRect = new Rect(currentPosition.x, currentPosition.y, position.width, LineHeight);
                listenersProperty.isExpanded = EditorGUI.Foldout(foldoutRect, listenersProperty.isExpanded,
                    new GUIContent($"Listeners: {listenersProperty.arraySize} (Count)"));
                currentPosition.y += LineHeight + Spacing;

                if (listenersProperty.isExpanded) {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < listenersProperty.arraySize; i++) {
                        SerializedProperty listenerElement = listenersProperty.GetArrayElementAtIndex(i);
                        if (listenerElement != null) {
                            // Draw listenFrom
                            SerializedProperty listenFromProperty = listenerElement.FindPropertyRelative("listenFrom");
                            if (listenFromProperty != null) {
                                EditorGUI.PropertyField(currentPosition, listenFromProperty,
                                    new GUIContent($"Listen From (Script)"));
                                currentPosition.y += LineHeight + Spacing;
                            }

                            // Draw methodName
                            SerializedProperty methodNameProperty = listenerElement.FindPropertyRelative("methodName");
                            if (methodNameProperty != null) {
                                EditorGUI.PropertyField(currentPosition, methodNameProperty,
                                    new GUIContent($"Method Name (String)"));
                                currentPosition.y += LineHeight + Spacing;
                            }
                        }
                    }

                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.EndDisabledGroup();
            GUI.color = originalColor; // Restore original GUI color

            EditorGUI.EndProperty();
        }
    }

# endif

}