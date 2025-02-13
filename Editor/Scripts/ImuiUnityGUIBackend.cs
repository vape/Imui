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
            var scalingMode = serializedObject.FindProperty("scalingMode");
            var customScale = serializedObject.FindProperty("customScale");
            
            EditorGUILayout.PropertyField(raycastTarget);
            EditorGUILayout.PropertyField(scalingMode);

            if (scalingMode.intValue == (int)IO.UGUI.ImuiUnityGUIBackend.ScalingMode.Custom)
            {
                EditorGUILayout.PropertyField(customScale);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}