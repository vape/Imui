using Imui.IO.UGUI;
using UnityEditor;

namespace Imui.Editor.Scripts
{
    [CustomEditor(typeof(IO.UGUI.ImuiUnityGUIBackend))]
    public class ImuiUnityGUIBackend : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var raycastTarget = serializedObject.FindProperty("m_RaycastTarget");
            
            EditorGUILayout.PropertyField(raycastTarget);

            serializedObject.ApplyModifiedProperties();
        }
    }
}