using Core.Pool;
using GamePlay.Effects;
using UnityEngine;

namespace GamePlay.Combat
{
    /// <summary>
    /// 攻击特效调度服务，根据归一化时间和段配置在指定时刻生成挥砍特效。
    /// </summary>
    public static class EffectService
    {
        /// <summary>
        /// 每帧调用，根据归一化时间触发特效生成
        /// </summary>
        /// <param name="normalizedTime">当前攻击动画归一化时间</param>
        /// <param name="comboIndex">当前连击段索引</param>
        /// <param name="config">连击配置 SO</param>
        /// <param name="spawnPoint">特效生成挂点 Transform</param>
        /// <param name="currentSpawnIndex">当前已触发的特效索引，由外部传入并维护</param>
        public static void Update(
            float normalizedTime,
            int comboIndex,
            AttackComboConfigSO config,
            Transform spawnPoint,
            ref int currentSpawnIndex)
        {
            if (config == null || config.Segments == null || comboIndex >= config.Segments.Length) return;

            AttackSegmentConfig seg = config.Segments[comboIndex];
            EffectSpawnInfo[] spawns = seg.EffectSpawns;
            if (spawns == null || spawns.Length == 0) return;
            if (spawnPoint == null) return;

            while (currentSpawnIndex < spawns.Length
                   && normalizedTime >= spawns[currentSpawnIndex].NormalizedTime)
            {
                EffectSpawnInfo info = spawns[currentSpawnIndex];
                SlashEffect effect = PoolManager.Instance.Get<SlashEffect>("FX_Slash");
                if (effect != null)
                {
                    effect.transform.SetPositionAndRotation(
                        spawnPoint.TransformPoint(info.LocalPosition),
                        spawnPoint.rotation * Quaternion.Euler(info.LocalRotation)
                    );
                }
                currentSpawnIndex++;
            }
        }
    }
}
