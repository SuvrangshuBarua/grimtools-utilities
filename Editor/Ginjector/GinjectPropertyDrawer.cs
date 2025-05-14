using UnityEditor;
using UnityEngine;
using GrimTools.Runtime;

namespace GrimTools.Editor
{
    [CustomPropertyDrawer(typeof(GinjectAttribute))]
    public class GinjectPropertyDrawer : PropertyDrawer
    {
        Texture2D icon;

        Texture2D LoadIcon()
        {
            if (icon == null)
            {
                icon = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.grimtools.utilities/Editor/Ginjector/Icon.png", typeof(Texture2D));
                if (icon == null)
                {
                    Debug.LogError("Failed to load icon at path: Packages/com.grimtools.utilities/Editor/Ginjector/Icon.png");
                }
            }
            return icon;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            icon = LoadIcon();
            var iconRect = new Rect(position.x, position.y, 16, 16);
            position.xMin += 24;

            if(icon != null)
            {
                var savedColor = GUI.color;
                GUI.color = property.objectReferenceValue == null ? savedColor : Color.green;
                GUI.DrawTexture(iconRect, icon);
                GUI.color = savedColor;
            }

            EditorGUI.PropertyField(position, property, label);
        }
    } 
}

