#if UNITY_EDITOR
using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace SPCharacter.Core.Editor
{
    /// <summary>
    /// 根运动曲线资产 Inspector - 将 AnimationClip 烘焙为本地累计位移曲线
    /// </summary>
    [CustomEditor(typeof(RootMotionProfileSO))]
    internal sealed class RootMotionProfileSOEditor : UnityEditor.Editor
    {
        private const string LocalXPropertyName = "_localX";
        private const string LocalZPropertyName = "_localZ";
        private const int DefaultSampleRate = 60;
        private const int MinSampleRate = 1;
        private const int MaxSampleRate = 120;

        private AnimationClip _sourceClip;
        private int _sampleRate = DefaultSampleRate;

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawBakeControls();
        }

        private void DrawBakeControls()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("离线烘焙", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(targets.Length != 1))
            {
                _sourceClip = (AnimationClip)EditorGUILayout.ObjectField(
                    "动画资源",
                    _sourceClip,
                    typeof(AnimationClip),
                    false);
                _sampleRate = EditorGUILayout.IntSlider("采样率", _sampleRate, MinSampleRate, MaxSampleRate);

                using (new EditorGUI.DisabledScope(_sourceClip == null))
                {
                    if (GUILayout.Button("从 AnimationClip 烘焙位移曲线"))
                        BakeSelectedClip();
                }
            }

            if (targets.Length != 1)
                EditorGUILayout.HelpBox("多选时不可烘焙 RootMotionProfile，请只选择一个资产", MessageType.Info);
        }

        private void BakeSelectedClip()
        {
            if (_sourceClip == null)
            {
                EditorUtility.DisplayDialog("烘焙失败", "请先选择 AnimationClip", "确定");
                return;
            }

            RootMotionProfileSO profile = (RootMotionProfileSO)target;
            if (!EditorUtility.DisplayDialog(
                    "覆盖位移曲线",
                    $"将用动画 {_sourceClip.name} 覆盖当前 LocalX 与 LocalZ 曲线，是否继续？",
                    "烘焙",
                    "取消"))
            {
                return;
            }

            try
            {
                RootMotionBakeResult bakeResult = RootMotionClipBaker.Bake(_sourceClip, _sampleRate);
                ApplyBakeResult(profile, bakeResult);
                ReportBakeResult(profile, bakeResult);
            }
            catch (Exception exception)
            {
                Debug.LogError($"{profile.name}: 烘焙根运动曲线失败：{exception.Message}", profile);
                EditorUtility.DisplayDialog("烘焙失败", exception.Message, "确定");
            }
        }

        private static void ApplyBakeResult(RootMotionProfileSO profile, RootMotionBakeResult bakeResult)
        {
            Undo.RecordObject(profile, "Bake Root Motion Profile");

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty(LocalXPropertyName).animationCurveValue = bakeResult.LocalX;
            serializedProfile.FindProperty(LocalZPropertyName).animationCurveValue = bakeResult.LocalZ;
            serializedProfile.ApplyModifiedProperties();

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
        }

        private static void ReportBakeResult(RootMotionProfileSO profile, RootMotionBakeResult bakeResult)
        {
            string message = $"{profile.name}: 已烘焙根运动曲线，来源 {bakeResult.SourceDescription}，采样点 {bakeResult.SampleCount}，总位移 X={bakeResult.TotalDisplacement.x:F3}m Z={bakeResult.TotalDisplacement.y:F3}m";
            if (bakeResult.HasDisplacement)
            {
                Debug.Log(message, profile);
                return;
            }

            Debug.LogWarning($"{message}，结果接近零，请确认动画资源包含根节点位移", profile);
        }

        /// <summary>
        /// 动画根运动烘焙器
        /// </summary>
        private static class RootMotionClipBaker
        {
            private const string RootPositionCurveSource = "根位移曲线";
            private const string SampledTransformSource = "临时 Transform 采样";
            private const float DisplacementEpsilon = 0.0001f;

            /// <summary>
            /// 烘焙动画根运动位移曲线
            /// </summary>
            /// <param name="clip">来源动画片段</param>
            /// <param name="sampleRate">采样率</param>
            /// <returns>根运动烘焙结果</returns>
            public static RootMotionBakeResult Bake(AnimationClip clip, int sampleRate)
            {
                if (clip == null)
                    throw new ArgumentNullException(nameof(clip));
                if (clip.length <= 0f)
                    throw new InvalidOperationException("AnimationClip 长度必须大于 0");

                int safeSampleRate = Mathf.Clamp(sampleRate, MinSampleRate, MaxSampleRate);
                AnimationCurve sourceX = FindRootPositionCurve(clip, "x");
                AnimationCurve sourceZ = FindRootPositionCurve(clip, "z");
                if (sourceX != null || sourceZ != null)
                    return BakeFromRootPositionCurves(clip, safeSampleRate, sourceX, sourceZ);

                return BakeFromSampledTransform(clip, safeSampleRate);
            }

            private static RootMotionBakeResult BakeFromRootPositionCurves(
                AnimationClip clip,
                int sampleRate,
                AnimationCurve sourceX,
                AnimationCurve sourceZ)
            {
                int sampleCount = CalculateSampleCount(clip, sampleRate);
                float originX = EvaluateOrZero(sourceX, 0f);
                float originZ = EvaluateOrZero(sourceZ, 0f);
                List<Vector2> samples = new List<Vector2>(sampleCount);

                for (int i = 0; i < sampleCount; i++)
                {
                    float normalizedTime = CalculateNormalizedTime(i, sampleCount);
                    float clipTime = normalizedTime * clip.length;
                    float localX = EvaluateOrZero(sourceX, clipTime) - originX;
                    float localZ = EvaluateOrZero(sourceZ, clipTime) - originZ;
                    samples.Add(new Vector2(localX, localZ));
                }

                return CreateBakeResult(samples, RootPositionCurveSource);
            }

            private static RootMotionBakeResult BakeFromSampledTransform(AnimationClip clip, int sampleRate)
            {
                int sampleCount = CalculateSampleCount(clip, sampleRate);
                List<Vector2> samples = new List<Vector2>(sampleCount);
                GameObject sampleRoot = new GameObject("RootMotionBakeSample")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

                try
                {
                    Vector3 origin = SampleRootPosition(clip, sampleRoot, 0f);
                    for (int i = 0; i < sampleCount; i++)
                    {
                        float normalizedTime = CalculateNormalizedTime(i, sampleCount);
                        float clipTime = normalizedTime * clip.length;
                        Vector3 displacement = SampleRootPosition(clip, sampleRoot, clipTime) - origin;
                        samples.Add(new Vector2(displacement.x, displacement.z));
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(sampleRoot);
                }

                return CreateBakeResult(samples, SampledTransformSource);
            }

            private static AnimationCurve FindRootPositionCurve(AnimationClip clip, string axis)
            {
                EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
                for (int i = 0; i < bindings.Length; i++)
                {
                    EditorCurveBinding binding = bindings[i];
                    if (!IsRootPositionBinding(binding, axis))
                        continue;

                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                    if (curve != null)
                        return curve;
                }

                return null;
            }

            private static bool IsRootPositionBinding(EditorCurveBinding binding, string axis)
            {
                if (!string.IsNullOrEmpty(binding.path))
                    return false;
                if (binding.type != typeof(Transform) && binding.type != typeof(Animator))
                    return false;

                string propertyName = binding.propertyName.Replace(" ", string.Empty).ToLowerInvariant();
                return propertyName == $"m_localposition.{axis}" ||
                       propertyName == $"localposition.{axis}" ||
                       propertyName == $"roott.{axis}" ||
                       propertyName == $"motiont.{axis}" ||
                       propertyName == $"m_roott.{axis}" ||
                       propertyName == $"m_motiont.{axis}";
            }

            private static Vector3 SampleRootPosition(AnimationClip clip, GameObject sampleRoot, float clipTime)
            {
                Transform sampleTransform = sampleRoot.transform;
                sampleTransform.localPosition = Vector3.zero;
                sampleTransform.localRotation = Quaternion.identity;
                sampleTransform.localScale = Vector3.one;
                clip.SampleAnimation(sampleRoot, clipTime);
                return sampleTransform.localPosition;
            }

            private static RootMotionBakeResult CreateBakeResult(IReadOnlyList<Vector2> samples, string sourceDescription)
            {
                AnimationCurve localX = CreateLinearCurve(samples, true);
                AnimationCurve localZ = CreateLinearCurve(samples, false);
                Vector2 totalDisplacement = samples[samples.Count - 1];
                bool hasDisplacement = totalDisplacement.sqrMagnitude > DisplacementEpsilon * DisplacementEpsilon;
                return new RootMotionBakeResult(localX, localZ, samples.Count, totalDisplacement, hasDisplacement, sourceDescription);
            }

            private static AnimationCurve CreateLinearCurve(IReadOnlyList<Vector2> samples, bool useX)
            {
                Keyframe[] keys = new Keyframe[samples.Count];
                for (int i = 0; i < samples.Count; i++)
                {
                    float normalizedTime = CalculateNormalizedTime(i, samples.Count);
                    float value = useX ? samples[i].x : samples[i].y;
                    float inTangent = CalculateInTangent(samples, useX, i);
                    float outTangent = CalculateOutTangent(samples, useX, i);
                    keys[i] = new Keyframe(normalizedTime, value, inTangent, outTangent);
                }

                AnimationCurve curve = new AnimationCurve(keys)
                {
                    preWrapMode = WrapMode.ClampForever,
                    postWrapMode = WrapMode.ClampForever
                };
                return curve;
            }

            private static float CalculateInTangent(IReadOnlyList<Vector2> samples, bool useX, int index)
            {
                if (samples.Count <= 1)
                    return 0f;
                if (index <= 0)
                    return CalculateSlope(samples, useX, 0, 1);

                return CalculateSlope(samples, useX, index - 1, index);
            }

            private static float CalculateOutTangent(IReadOnlyList<Vector2> samples, bool useX, int index)
            {
                if (samples.Count <= 1)
                    return 0f;
                if (index >= samples.Count - 1)
                    return CalculateSlope(samples, useX, index - 1, index);

                return CalculateSlope(samples, useX, index, index + 1);
            }

            private static float CalculateSlope(IReadOnlyList<Vector2> samples, bool useX, int fromIndex, int toIndex)
            {
                float fromTime = CalculateNormalizedTime(fromIndex, samples.Count);
                float toTime = CalculateNormalizedTime(toIndex, samples.Count);
                float fromValue = useX ? samples[fromIndex].x : samples[fromIndex].y;
                float toValue = useX ? samples[toIndex].x : samples[toIndex].y;
                float timeDelta = toTime - fromTime;
                if (Mathf.Approximately(timeDelta, 0f))
                    return 0f;

                return (toValue - fromValue) / timeDelta;
            }

            private static int CalculateSampleCount(AnimationClip clip, int sampleRate)
            {
                return Mathf.Max(2, Mathf.CeilToInt(clip.length * sampleRate) + 1);
            }

            private static float CalculateNormalizedTime(int index, int sampleCount)
            {
                if (sampleCount <= 1)
                    return 0f;

                return index / (float)(sampleCount - 1);
            }

            private static float EvaluateOrZero(AnimationCurve curve, float time)
            {
                return curve == null ? 0f : curve.Evaluate(time);
            }
        }

        /// <summary>
        /// 根运动烘焙结果
        /// </summary>
        private sealed class RootMotionBakeResult
        {
            public RootMotionBakeResult(
                AnimationCurve localX,
                AnimationCurve localZ,
                int sampleCount,
                Vector2 totalDisplacement,
                bool hasDisplacement,
                string sourceDescription)
            {
                LocalX = localX;
                LocalZ = localZ;
                SampleCount = sampleCount;
                TotalDisplacement = totalDisplacement;
                HasDisplacement = hasDisplacement;
                SourceDescription = sourceDescription;
            }

            /// <summary>本地 X 累计位移曲线</summary>
            public AnimationCurve LocalX { get; }

            /// <summary>本地 Z 累计位移曲线</summary>
            public AnimationCurve LocalZ { get; }

            /// <summary>采样点数量</summary>
            public int SampleCount { get; }

            /// <summary>总位移</summary>
            public Vector2 TotalDisplacement { get; }

            /// <summary>是否存在有效水平位移</summary>
            public bool HasDisplacement { get; }

            /// <summary>烘焙来源描述</summary>
            public string SourceDescription { get; }
        }
    }
}
#endif
