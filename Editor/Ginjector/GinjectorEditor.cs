#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using GrimTools.Runtime;

namespace GrimTools.Editor
{
    [CustomEditor(typeof(Ginjector))]
    public class GinjectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Ginjector ginjector = (Ginjector)target;

            if(GUILayout.Button("Validate Dependencies"))
            {
                ginjector.ValidateDependencies();   
            }

            if(GUILayout.Button("Clear All Injectable Fields"))
            {
                ginjector.ClearDependencies();
                EditorUtility.SetDirty(ginjector);
            }
        }
    }
}
#endif