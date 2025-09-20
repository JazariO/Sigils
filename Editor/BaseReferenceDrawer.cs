using UnityEditor;
using UnityEngine;

namespace Proselyte.Sigils
{
    public abstract class BaseReferenceDrawer : PropertyDrawer
    {
        private readonly string[] popupOptions = { "Use Constant", "Use Variable" };
        private GUIStyle popupStyle;

        protected abstract string ConstantFieldName { get; }
        protected abstract string VariableFieldName { get; }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(popupStyle == null)
            {
                popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions")) { imagePosition = ImagePosition.ImageOnly };
            }

            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            SerializedProperty useConstant = property.FindPropertyRelative("UseConstant");
            SerializedProperty constantValue = property.FindPropertyRelative(ConstantFieldName);
            SerializedProperty variable = property.FindPropertyRelative(VariableFieldName);

            Rect buttonRect = new Rect(position.x, position.y, 20, EditorGUIUtility.singleLineHeight);
            Rect fieldRect = new Rect(position.x + 25, position.y, position.width - 25, EditorGUIUtility.singleLineHeight);

            int result = EditorGUI.Popup(buttonRect, useConstant.boolValue ? 0 : 1, popupOptions, popupStyle);
            useConstant.boolValue = result == 0;

            EditorGUI.PropertyField(fieldRect, useConstant.boolValue ? constantValue : variable, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
