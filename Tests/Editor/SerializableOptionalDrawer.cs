using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Tutan.Functional.Editor
{
    [CustomPropertyDrawer(typeof(SerializableOptional<>), useForChildren: true)]
    public sealed class SerializableOptionalDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
            };

            var hasValueProp = property.FindPropertyRelative("_hasValue");
            var valueProp = property.FindPropertyRelative("_value");

            var toggle = new Toggle { value = hasValueProp.boolValue };
            toggle.BindProperty(hasValueProp);
            toggle.style.marginRight = 4;

            var valueField = new PropertyField(valueProp, property.displayName);
            valueField.style.flexGrow = 1;
            valueField.SetEnabled(hasValueProp.boolValue);

            toggle.RegisterValueChangedCallback(evt => valueField.SetEnabled(evt.newValue));

            root.Add(toggle);
            root.Add(valueField);
            return root;
        }
    }
}
