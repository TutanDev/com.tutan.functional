using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tutan.Functional
{
    /// <summary>
    /// Drawer for <see cref="SerializableOptional{T}"/>: a toggle bound to <c>_hasValue</c>
    /// next to the inner value field, which is enabled only while the toggle is on.
    /// Implements both the UI Toolkit path (<see cref="CreatePropertyGUI"/>) and an IMGUI
    /// fallback (<see cref="OnGUI"/>) for inspectors that are still drawn with IMGUI.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializableOptional<>), useForChildren: true)]
    public sealed class SerializableOptionalDrawer : PropertyDrawer
    {
        private const string HasValueField = "_hasValue";
        private const string ValueField = "_value";
        private const float ToggleWidth = 18f;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
            };

            var hasValueProp = property.FindPropertyRelative(HasValueField);
            var valueProp = property.FindPropertyRelative(ValueField);

            var toggle = new Toggle { value = hasValueProp.boolValue };
            toggle.BindProperty(hasValueProp);
            toggle.style.marginRight = 4;

            var valueField = new PropertyField(valueProp, property.displayName);
            valueField.style.flexGrow = 1;
            valueField.SetEnabled(hasValueProp.boolValue);

            // Track the serialized value rather than the toggle's change event so undo/redo,
            // multi-object edits, and script-driven changes also refresh the enabled state.
            root.TrackPropertyValue(hasValueProp, p => valueField.SetEnabled(p.boolValue));

            root.Add(toggle);
            root.Add(valueField);
            return root;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var hasValueProp = property.FindPropertyRelative(HasValueField);
            var valueProp = property.FindPropertyRelative(ValueField);

            EditorGUI.BeginProperty(position, label, property);

            var indented = EditorGUI.IndentedRect(position);
            var toggleRect = new Rect(indented.x, position.y, ToggleWidth, EditorGUIUtility.singleLineHeight);
            var valueRect = new Rect(indented.x + ToggleWidth, position.y, indented.width - ToggleWidth, position.height);

            // The rects are already indented; reset so the nested fields don't indent a second time.
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.PropertyField(toggleRect, hasValueProp, GUIContent.none);
            using (new EditorGUI.DisabledScope(!hasValueProp.boolValue))
                EditorGUI.PropertyField(valueRect, valueProp, label, includeChildren: true);

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property.FindPropertyRelative(ValueField), label, includeChildren: true);
    }
}
