using System.Collections.Generic;
using System.Reflection;
using BehaviorDesigner.Editor;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEditor;
using UnityEngine;

namespace SPCharacterController.EditorTools
{
    /// <summary>
    /// AI 创建的纯代码生成新 .asset 资产，可直接删除
    /// 敌人行为树资产生成器 - 通过菜单代码构建 EnemyBehaviorTree.asset，
    /// 结构与 CCSource_AISO 约定的共享变量一一对应。
    /// </summary>
    public static class EnemyBehaviorTreeGenerator
    {
        private const string AssetPath = "Assets/Data/Character/InputSource/EnemyBehaviorTree.asset";

        private static readonly FieldInfo AbortTypeField =
            typeof(Composite).GetField("abortType", BindingFlags.NonPublic | BindingFlags.Instance);

        private static int _nextTaskId;

        [MenuItem("SPCharacterController/Generate Enemy Behavior Tree")]
        public static void Generate()
        {
            _nextTaskId = 0;
            var tree = ScriptableObject.CreateInstance<ExternalBehaviorTree>();

            BehaviorSource source = tree.GetBehaviorSource();
            if (source == null)
            {
                source = new BehaviorSource { Owner = tree };
                tree.BehaviorSource = source;
            }

            CreateVariables(source);
            BuildTree(source);

            // 序列化任务图为 JSON 数据后落盘资产
            JSONSerialization.Save(source);
            AssetDatabase.CreateAsset(tree, AssetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"敌人行为树资产已生成：{AssetPath}");
            Selection.activeObject = tree;
        }

        /// <summary>
        /// 创建行为树共享变量，运行时由 CCSource_AISO 注入覆盖参数值。
        /// </summary>
        private static void CreateVariables(BehaviorSource source)
        {
            source.SetVariable("SightRange", new SharedFloat { Name = "SightRange", IsShared = true, Value = 10f, Tooltip = "视野范围" });
            source.SetVariable("LostSightDelay", new SharedFloat { Name = "LostSightDelay", IsShared = true, Value = 2f, Tooltip = "丢失视野后等待巡逻的秒数" });
            source.SetVariable("AttackRange", new SharedFloat { Name = "AttackRange", IsShared = true, Value = 2f, Tooltip = "攻击范围" });
            source.SetVariable("PatrolRange", new SharedFloat { Name = "PatrolRange", IsShared = true, Value = 5f, Tooltip = "巡逻范围" });
            source.SetVariable("PatrolCooldown", new SharedFloat { Name = "PatrolCooldown", IsShared = true, Value = 3f, Tooltip = "巡逻冷却秒数" });
            source.SetVariable("TargetPosition", new SharedVector3 { Name = "TargetPosition", IsShared = true, Tooltip = "目标点" });
            source.SetVariable("NextPatrolTime", new SharedFloat { Name = "NextPatrolTime", IsShared = true, Tooltip = "下次允许巡逻的时刻" });
            source.SetVariable("OutMoveDirection", new SharedVector3 { Name = "OutMoveDirection", IsShared = true, Tooltip = "输出：移动方向" });
            source.SetVariable("OutWantToAttack", new SharedBool { Name = "OutWantToAttack", IsShared = true, Tooltip = "输出：攻击意图" });
        }

        /// <summary>
        /// 构建行为树节点结构并连接父子关系。
        /// </summary>
        private static void BuildTree(BehaviorSource source)
        {
            var entry = CreateTask<EntryTask>("入口", new Vector2(0f, 0f));
            var root = CreateTask<Selector>("根选择", new Vector2(0f, 120f));

            // 战斗分支：索敌成功时追击或攻击；Both = 追击中每帧刷新玩家位置 + 巡逻时可被索敌打断
            var combatSequence = CreateTask<Sequence>("战斗", new Vector2(-240f, 260f));
            SetAbortType(combatSequence, AbortType.Both);
            var findPlayer = CreateTask<FindPlayerInSight>("视野索敌", new Vector2(-420f, 400f));
            findPlayer.SightRange = BindFloat("SightRange");
            findPlayer.LostSightDelay = BindFloat("LostSightDelay");
            findPlayer.NextPatrolTime = BindFloat("NextPatrolTime");
            findPlayer.TargetPosition = BindVector3("TargetPosition");

            var combatSelector = CreateTask<Selector>("战斗行为", new Vector2(-160f, 400f));

            // 攻击分支：LowerPriority = 追击中进入攻击范围立即出手
            var attackSequence = CreateTask<Sequence>("攻击", new Vector2(-320f, 540f));
            SetAbortType(attackSequence, AbortType.LowerPriority);
            var inAttackRange = CreateTask<IsInAttackRange>("攻击范围判断", new Vector2(-460f, 680f));
            inAttackRange.AttackRange = BindFloat("AttackRange");
            var requestAttack = CreateTask<RequestAttack>("请求攻击", new Vector2(-240f, 680f));
            requestAttack.OutWantToAttack = BindBool("OutWantToAttack");

            var chaseMove = CreateTask<MoveToTarget>("追击移动", new Vector2(-40f, 560f));
            chaseMove.TargetPosition = BindVector3("TargetPosition");
            chaseMove.OutMoveDirection = BindVector3("OutMoveDirection");

            // 巡逻分支：冷却完毕后在当前位置附近随机游走，到达后开始冷却计时
            var patrolSequence = CreateTask<Sequence>("巡逻", new Vector2(240f, 260f));
            var cooldownReady = CreateTask<IsPatrolCooldownReady>("巡逻冷却判断", new Vector2(80f, 420f));
            cooldownReady.NextPatrolTime = BindFloat("NextPatrolTime");
            var randomPoint = CreateTask<RandomPatrolPoint>("随机巡逻取点", new Vector2(280f, 420f));
            randomPoint.PatrolRange = BindFloat("PatrolRange");
            randomPoint.TargetPosition = BindVector3("TargetPosition");
            var patrolMove = CreateTask<MoveToTarget>("巡逻移动", new Vector2(240f, 560f));
            patrolMove.TargetPosition = BindVector3("TargetPosition");
            patrolMove.OutMoveDirection = BindVector3("OutMoveDirection");
            patrolMove.PatrolCooldown = BindFloat("PatrolCooldown");
            patrolMove.NextPatrolTime = BindFloat("NextPatrolTime");

            // EntryTask 仅作图编辑器根标记，不持有子节点，避免根任务被重复序列化
            root.AddChild(combatSequence, 0);
            root.AddChild(patrolSequence, 1);
            combatSequence.AddChild(findPlayer, 0);
            combatSequence.AddChild(combatSelector, 1);
            combatSelector.AddChild(attackSequence, 0);
            combatSelector.AddChild(chaseMove, 1);
            attackSequence.AddChild(inAttackRange, 0);
            attackSequence.AddChild(requestAttack, 1);
            patrolSequence.AddChild(cooldownReady, 0);
            patrolSequence.AddChild(randomPoint, 1);
            patrolSequence.AddChild(patrolMove, 2);

            source.EntryTask = entry;
            source.RootTask = root;
            source.DetachedTasks = new List<Task>();
        }

        /// <summary>
        /// 创建任务节点、分配唯一任务 ID 并初始化图编辑器显示数据。
        /// </summary>
        private static T CreateTask<T>(string friendlyName, Vector2 offset) where T : Task, new()
        {
            var task = new T { FriendlyName = friendlyName, ID = _nextTaskId++ };
            task.NodeData = new NodeData { Offset = offset, FriendlyName = friendlyName };
            return task;
        }

        /// <summary>
        /// 设置组合节点的条件中止类型。
        /// </summary>
        private static void SetAbortType(Composite composite, AbortType abortType)
        {
            AbortTypeField.SetValue(composite, abortType);
        }

        private static SharedFloat BindFloat(string variableName) =>
            new SharedFloat { Name = variableName, IsShared = true };

        private static SharedBool BindBool(string variableName) =>
            new SharedBool { Name = variableName, IsShared = true };

        private static SharedVector3 BindVector3(string variableName) =>
            new SharedVector3 { Name = variableName, IsShared = true };

        [MenuItem("SPCharacterController/Validate Enemy Behavior Tree")]
        public static void Validate()
        {
            var tree = AssetDatabase.LoadAssetAtPath<ExternalBehaviorTree>(AssetPath);
            if (tree == null)
            {
                Debug.LogError($"行为树资产不存在：{AssetPath}");
                return;
            }

            var go = new GameObject("~BTValidateTemp");
            try
            {
                var bt = go.AddComponent<BehaviorTree>();
                bt.DisableBehavior();
                bt.StartWhenEnabled = false;
                bt.ExternalBehavior = tree;
                bt.EnableBehavior();

                int taskCount = 0;
                CountTasks(bt.GetBehaviorSource().RootTask, ref taskCount);
                int variableCount = bt.GetAllVariables().Count;

                var sight = bt.GetVariable("SightRange") as SharedFloat;
                bt.SetVariableValue("SightRange", 10f);
                bool bindOk = sight != null && Mathf.Approximately(sight.Value, 10f);

                Debug.Log($"行为树加载验证 - 任务数: {taskCount}, 变量数: {variableCount}, 变量注入: {(bindOk ? "OK" : "失败")}");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// 递归统计已实例化的任务节点数量。
        /// </summary>
        private static void CountTasks(Task task, ref int count)
        {
            if (task == null) return;
            count++;
            if (task is ParentTask parent && parent.Children != null)
                foreach (var child in parent.Children)
                    CountTasks(child, ref count);
        }
    }
}
