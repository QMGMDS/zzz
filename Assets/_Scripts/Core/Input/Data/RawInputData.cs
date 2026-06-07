using UnityEngine;

namespace Core.Input.Data
{
    /// <summary>
    /// 原始输入数据——纯硬件事实汇报，不包含任何手感处理。
    /// </summary>
    public struct RawInputData
    {
        /// <summary>WASD 横向输入轴</summary>
        public Vector2 MoveAxis;

        /// <summary>闪避按键按下边沿触发</summary>
        public bool EvadeJustPressed;

        /// <summary>攻击按键按下边沿触发</summary>
        public bool AttackJustPressed;
    }
}
