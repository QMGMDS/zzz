using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 输入源抽象基类
    /// 所有输入源资产必须继承此类，以向黑板写入角色意图。
    /// 使得 SPCharacterController 可通过 Inspector 拖拽任意输入源实现。
    /// </summary>
    public abstract class CCSourceSO : ScriptableObject
    {
        /// <summary>
        /// 将输入源本帧数据翻译为角色意图，写入黑板。
        /// </summary>
        /// <param name="blackboard">角色运行时黑板</param>
        public abstract void WriteIntentions(CharacterRunTimeData blackboard);
    }
}
