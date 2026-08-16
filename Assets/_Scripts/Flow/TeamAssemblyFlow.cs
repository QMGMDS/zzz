using System;
using System.Collections.Generic;

using UnityEngine;

using SPCamera.Contract;
using SPFramework.Service;
using SPResource.Contract;
using SPTeam.Contract;

namespace SPFlow
{
    /// <summary>
    /// 队伍装配流程 - 按装配计划实例化角色、移交名册并应用初始相机跟随 挂在队伍根物体上
    /// </summary>
    internal sealed class TeamAssemblyFlow : MonoBehaviour
    {
        private void Start()
        {
            if (!ModuleServiceHub.TryGet<ITeamService>(out ITeamService team))
                throw new InvalidOperationException($"{name}: 队伍服务未注册 无法装配队伍");

            if (!ModuleServiceHub.TryGet<IInstantiateResource>(out IInstantiateResource resource))
                throw new InvalidOperationException($"{name}: 资源实例化服务未注册 无法装配队伍");

            IReadOnlyList<TeamSlotPlan> plan = team.GetSlotPlan();
            List<TeamAssemblyEntry> entries = new List<TeamAssemblyEntry>(plan.Count);

            foreach (TeamSlotPlan slot in plan)
            {
                ResourceInstantiateResult result = resource.Instantiate(
                    new ResourceKey(slot.ResourceKey),
                    transform.position,
                    transform.rotation,
                    transform,
                    activate: false);

                if (!result.IsSuccess)
                {
                    ReleaseEntries(entries);
                    throw new InvalidOperationException($"{name}: 角色 {slot.CharacterId} 实例化失败 - {result.Error}");
                }

                entries.Add(new TeamAssemblyEntry(slot.CharacterId, result.Instance, result.Release));
            }

            team.InitializeRoster(entries);

            Transform initialCharacter = team.GetCharacterTransform(team.ActiveCharacterId);
            if (ModuleServiceHub.TryGet<ISetCameraFollowTarget>(out ISetCameraFollowTarget setter) && initialCharacter != null)
                setter.SetCameraFollowTarget(initialCharacter);
        }

        private static void ReleaseEntries(IReadOnlyList<TeamAssemblyEntry> entries)
        {
            foreach (TeamAssemblyEntry entry in entries)
                entry.Release();
        }
    }
}
