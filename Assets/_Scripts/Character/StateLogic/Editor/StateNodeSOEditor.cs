using UnityEditor;

namespace SPCharacterController.Editor
{
    /// <summary>
    /// 状态节点自定义检视面板 - 根据运动模式显示有效的根运动配置。
    /// </summary>
    [CustomEditor(typeof(StateNodeSO))]
    [CanEditMultipleObjects]
    public sealed class StateNodeSOEditor : UnityEditor.Editor
    {
        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(StateNodeSO.Animation)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(StateNodeSO.IsLooping)));

            SerializedProperty useInterruptWindow =
                serializedObject.FindProperty(nameof(StateNodeSO.UseInterruptWindow));
            EditorGUILayout.PropertyField(useInterruptWindow);
            if (useInterruptWindow.hasMultipleDifferentValues || useInterruptWindow.boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(StateNodeSO.InterruptWindow)));

            SerializedProperty motionMode = serializedObject.FindProperty(nameof(StateNodeSO.MotionMode));
            EditorGUILayout.PropertyField(motionMode);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(StateNodeSO.AllowRotation)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(StateNodeSO.SnapRotationOnExit)));

            if (motionMode.hasMultipleDifferentValues ||
                (CharacterMotionMode)motionMode.enumValueIndex == CharacterMotionMode.RootMotion)
            {
                DrawRootMotionProperties();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("特效", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(StateNodeSO.Effects)));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRootMotionProperties()
        {
            SerializedProperty useRootMotionRotation =
                serializedObject.FindProperty(nameof(StateNodeSO.UseRootMotionRotation));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("根运动", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(StateNodeSO.RootMotionScale)));
            EditorGUILayout.PropertyField(useRootMotionRotation);

            if (useRootMotionRotation.hasMultipleDifferentValues || useRootMotionRotation.boolValue)
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(nameof(StateNodeSO.RootMotionRotationScale)));
            }
        }
    }
}
