using Core.Input.Config;
using Core.Input.Data;
using UnityEngine;

namespace Core.Input
{
    /// <summary>
    /// 输入数据采集员
    /// 职责链：
    ///   1. 驱动 IInputSource 采样原始数据
    ///   2. 对原始数据进行后处理（Move 防抖 + BufferTimer 充能/衰减）
    ///   3. 提供 Consume 方法供外部显式核销输入
    ///   4. 对外暴露只读的当前/上一帧快照
    /// </summary>
    public class InputCollector
    {
        private readonly IInputSource _source;
        private readonly InputData _inputData;

        // 记录 Move 防抖运行时状态
        private Vector2 _bufferedMove;
        private float _lastNonZeroMoveTime;

        // 按键缓存运行时状态
        private float _attackBufferTimer;
        private float _evadeBufferTimer;

        // 瞬时栈上数据
        private RawInputData _rawData;
        private ulong _frameIndex;

        /// <summary>对外暴露的当前输入数据只读引用</summary>
        public InputData Current => _inputData;

        /// <summary>
        /// 创建输入采集员；后处理参数从 <see cref="InputPostProcessConfig"/> 读取。
        /// </summary>
        /// <param name="source">输入源（PlayerInputReader / AI 适配器等）</param>
        public InputCollector(IInputSource source)
        {
            _source = source;

            _inputData = new InputData
            {
                CurrentFrameData = new FrameInputData { FrameIndex = 0 },
                LastFrameData = new FrameInputData { FrameIndex = 0 }
            };

            _rawData = default;
            _bufferedMove = Vector2.zero;
            _lastNonZeroMoveTime = Time.time;
            _frameIndex = 0;
        }

        /// <summary>
        /// 每帧由外部驱动——推移帧历史 → 采样 → 后处理
        /// </summary>
        public void Update()
        {
            _inputData.LastFrameData = _inputData.CurrentFrameData;

            _source.FetchRawInput(ref _rawData);

            ProcessRawInput();

            _frameIndex++;
        }

        /// <summary>
        /// 输入数据的后处理方法
        /// </summary>
        private void ProcessRawInput()
        {
            var currentFrame = new FrameInputData
            {
                FrameIndex = _frameIndex,
                Raw = _rawData,
                Processed = default
            };

            // --- Move 轴防抖处理 ---
            if (_rawData.MoveAxis.sqrMagnitude > 0.01f)
            {
                _bufferedMove = _rawData.MoveAxis;
                _lastNonZeroMoveTime = Time.time;
                currentFrame.Processed.Move = _rawData.MoveAxis;
            }
            else if (Time.time - _lastNonZeroMoveTime < InputPostProcessConfig.InputFlickerBuffer)
            {
                // 处于防抖窗口内 使用缓存的最后一次有效值
                currentFrame.Processed.Move = _bufferedMove;
            }
            else
            {
                currentFrame.Processed.Move = Vector2.zero;
            }

            // --- BufferTimer 衰减 + 充能 ---
            float dt = Time.deltaTime;

            _attackBufferTimer = UpdateBuffer(_attackBufferTimer, _rawData.AttackJustPressed, InputPostProcessConfig.AttackBufferTime, dt);
            _evadeBufferTimer = UpdateBuffer(_evadeBufferTimer, _rawData.EvadeJustPressed, InputPostProcessConfig.EvadeBufferTime, dt);

            currentFrame.Processed.AttackPressed = _attackBufferTimer > 0f;
            currentFrame.Processed.EvadePressed = _evadeBufferTimer > 0f;

            _inputData.CurrentFrameData = currentFrame;
        }

        private static float UpdateBuffer(float lastTimer, bool justPressed, float bufferTime, float dt)
        {
            float newTimer = Mathf.Max(0f, lastTimer - dt);
            if (justPressed) newTimer = bufferTime;
            return newTimer;
        }

        #region Consume —— 显式消费核销

        /// <summary>消费攻击输入——将 BufferTimer 归零，防止同帧内被重复消费</summary>
        public void ConsumeAttackPressed()
        {
            _attackBufferTimer = 0f;
            var f = _inputData.CurrentFrameData;
            f.Processed.AttackPressed = false;
            _inputData.CurrentFrameData = f;
        }

        /// <summary>消费闪避输入——将 BufferTimer 归零，防止同帧内被重复消费</summary>
        public void ConsumeEvadePressed()
        {
            _evadeBufferTimer = 0f;
            var f = _inputData.CurrentFrameData;
            f.Processed.EvadePressed = false;
            _inputData.CurrentFrameData = f;
        }

        #endregion
    }
}
