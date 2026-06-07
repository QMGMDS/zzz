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

        // Move 防抖缓存
        private readonly float _flickerBuffer;
        private Vector2 _bufferedMove;
        private float _lastNonZeroMoveTime;


        // 独立可配的 BufferTimer 时长
        private readonly float _attackBufferTime;
        private readonly float _evadeBufferTime;

        // 瞬时栈上数据
        private RawInputData _rawData;
        private ulong _frameIndex;

        /// <summary>对外暴露的当前输入数据只读引用</summary>
        public InputData Current => _inputData;

        /// <summary>
        /// 创建输入采集员
        /// </summary>
        /// <param name="source">输入源（PlayerInputReader / AI 适配器等）</param>
        /// <param name="flickerBuffer">移动轴防抖窗口（秒）</param>
        /// <param name="attackBufferTime">攻击缓存时间（秒）</param>
        /// <param name="evadeBufferTime">闪避缓存时间（秒）</param>
        public InputCollector(IInputSource source, float flickerBuffer, float attackBufferTime, float evadeBufferTime)
        {
            _source = source;
            _flickerBuffer = flickerBuffer;
            _attackBufferTime = attackBufferTime;
            _evadeBufferTime = evadeBufferTime;

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
            else if (Time.time - _lastNonZeroMoveTime < _flickerBuffer)
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
            var lastProc = _inputData.LastFrameData.Processed;

            currentFrame.Processed.AttackBufferTimer = UpdateBuffer(lastProc.AttackBufferTimer, _rawData.AttackJustPressed, _attackBufferTime, dt);
            currentFrame.Processed.EvadeBufferTimer = UpdateBuffer(lastProc.EvadeBufferTimer, _rawData.EvadeJustPressed, _evadeBufferTime, dt);

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
            var f = _inputData.CurrentFrameData;
            f.Processed.AttackBufferTimer = 0f;
            _inputData.CurrentFrameData = f;
        }

        /// <summary>消费闪避输入——将 BufferTimer 归零，防止同帧内被重复消费</summary>
        public void ConsumeEvadePressed()
        {
            var f = _inputData.CurrentFrameData;
            f.Processed.EvadeBufferTimer = 0f;
            _inputData.CurrentFrameData = f;
        }

        #endregion
    }
}
