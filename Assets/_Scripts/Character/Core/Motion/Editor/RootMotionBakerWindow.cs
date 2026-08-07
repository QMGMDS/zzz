using UnityEditor;
using UnityEngine;

namespace SPCharacter.Core.Editor
{
    /// <summary>
    /// 根运动烘焙窗口 - 从 AnimationClip 的根节点位移曲线生成 RootMotionProfileSO。
    /// 支持 Generic 根节点 m_LocalPosition 与 Humanoid RootT 曲线，烘焙后可在 Inspector 二次修改。
    /// </summary>
    public sealed class RootMotionBakerWindow : EditorWindow
    {
        private AnimationClip _sourceClip;
        private RootMotionProfileSO _targetProfile;
        private int _sampleCount = 60;

        /// <summary>打开根运动烘焙窗口。</summary>
        [MenuItem("Tools/SPCharacter/Root Motion Baker")]
        public static void Open()
        {
            GetWindow<RootMotionBakerWindow>("Root Motion Baker");
        }

        private void OnGUI()
        {
            GUILayout.Label("根运动烘焙 - 将动画根节点位移烘焙为可编辑曲线", EditorStyles.boldLabel);

            _sourceClip = (AnimationClip)EditorGUILayout.ObjectField("源动画片段", _sourceClip, typeof(AnimationClip), false);
            _targetProfile = (RootMotionProfileSO)EditorGUILayout.ObjectField("目标 Profile", _targetProfile, typeof(RootMotionProfileSO), false);
            _sampleCount = EditorGUILayout.IntSlider("采样数", _sampleCount, 8, 240);

            bool canBake = _sourceClip != null && _targetProfile != null;
            EditorGUI.BeginDisabledGroup(!canBake);
            if (GUILayout.Button("烘焙到 Profile"))
                Bake();
            EditorGUI.EndDisabledGroup();

            if (_sourceClip == null)
                EditorGUILayout.HelpBox("请选择源动画片段。", MessageType.Info);
            if (_targetProfile == null)
                EditorGUILayout.HelpBox("请先创建 RootMotionProfileSO 资源，并拖入目标 Profile 槽位。", MessageType.Info);
        }

        private void Bake()
        {
            RootMotionProfileSO profile = _targetProfile;
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(_sourceClip);
            AnimationCurve xCurve = ExtractCurve(bindings, "m_LocalPosition.x", "RootT.x");
            AnimationCurve zCurve = ExtractCurve(bindings, "m_LocalPosition.z", "RootT.z");

            float duration = _sourceClip.length;
            AnimationCurve normalizedX = NormalizeCurve(xCurve, duration, _sampleCount);
            AnimationCurve normalizedZ = NormalizeCurve(zCurve, duration, _sampleCount);
            profile.LocalX = normalizedX;
            profile.LocalZ = normalizedZ;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"根运动烘焙完成：{profile.name}，简化后 X 关键帧 {normalizedX.keys.Length}，Z 关键帧 {normalizedZ.keys.Length}。");
        }

        private AnimationCurve ExtractCurve(EditorCurveBinding[] bindings, string genericProp, string humanoidProp)
        {
            foreach (EditorCurveBinding binding in bindings)
            {
                if (binding.propertyName != genericProp && binding.propertyName != humanoidProp)
                    continue;
                return AnimationUtility.GetEditorCurve(_sourceClip, binding) ?? new AnimationCurve();
            }
            return new AnimationCurve();
        }

        private static AnimationCurve NormalizeCurve(AnimationCurve source, float duration, int sampleCount)
        {
            if (source == null || duration <= 0f)
                return new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

            var normalized = new AnimationCurve();
            for (int i = 0; i < sampleCount; i++)
            {
                float tNorm = (float)i / (sampleCount - 1);
                float tReal = tNorm * duration;
                float value = source.Evaluate(tReal);
                normalized.AddKey(tNorm, value);
            }
            SimplifyCurve(normalized, 0.0005f);
            return normalized;
        }

        private static void SimplifyCurve(AnimationCurve curve, float threshold)
        {
            Keyframe[] keys = curve.keys;
            if (keys.Length <= 2)
                return;
            for (int i = keys.Length - 2; i > 0; i--)
            {
                float slope = Mathf.Abs(keys[i + 1].value - keys[i].value) + Mathf.Abs(keys[i].value - keys[i - 1].value);
                if (slope < threshold)
                    curve.RemoveKey(i);
            }
        }
    }
}