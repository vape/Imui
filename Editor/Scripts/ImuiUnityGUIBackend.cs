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
#if ENABLE_INPUT_SYSTEM
            var simulateTouchWithMouseInEditor = serializedObject.FindProperty("simulateTouchWithMouseInEditor");
#endif

            EditorGUILayout.PropertyField(raycastTarget);
            EditorGUILayout.PropertyField(scalingMode);

            if (scalingMode.intValue == (int)IO.UGUI.ImuiUnityGUIBackend.ScalingMode.Custom)
            {
                EditorGUILayout.PropertyField(customScale);
            }

#if ENABLE_INPUT_SYSTEM
            EditorGUILayout.PropertyField(simulateTouchWithMouseInEditor);
#endif

            serializedObject.ApplyModifiedProperties();
        }
    }
}
