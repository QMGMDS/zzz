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
            ITeamService team = ModuleServiceHub.Get<ITeamService>();
            IInstantiateResource resource = ModuleServiceHub.Get<IInstantiateResource>();

            IReadOnlyList<TeamSlotPlan> plan = team.GetSlotPlan();
            var entries = new List<TeamAssemblyEntry>(plan.Count);

            foreach (TeamSlotPlan slot in plan)
            {
                ResourceLoadResult result = resource.Instantiate(new ResourceLoadRequest(
                    new ResourceKey(slot.ResourceKey),
                    parent: transform,
                    worldPosition: transform.position,
                    worldRotation: transform.rotation,
                    shouldActivateAfterCreate: false));

                if (!result.IsSuccess)
                {
                    ReleaseEntries(entries);
                    throw new InvalidOperationException($"{name}: 角色 {slot.CharacterId} 实例化失败");
                }

                entries.Add(new TeamAssemblyEntry(slot.CharacterId, result.Instance, result.Handle.Release));
            }

            team.InitializeRoster(entries);

            if (ModuleServiceHub.TryGet<ISetCameraFollowTarget>(out ISetCameraFollowTarget setter))
                setter.SetCameraFollowTarget(team.GetCharacterTransform(team.ActiveCharacterId));
        }

        private static void ReleaseEntries(IReadOnlyList<TeamAssemblyEntry> entries)
        {
            foreach (TeamAssemblyEntry entry in entries)
                entry.Release();
        }
    }
}
