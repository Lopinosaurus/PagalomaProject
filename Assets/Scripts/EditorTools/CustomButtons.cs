using MainGame.PlayerScripts;
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    [CustomEditor(typeof(PlayerLook))]
    [CanEditMultipleObjects] // only if you handle it properly
    public class YourClassNameEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Test screen shake", EditorStyles.toolbarButton))
            {
                ((PlayerLook)target).StartShake(5, 1);
            }
            DrawDefaultInspector();
        }
    }
}