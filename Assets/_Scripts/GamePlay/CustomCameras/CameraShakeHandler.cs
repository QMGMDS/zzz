using Cinemachine;
using Core.Event;
using UnityEngine;

namespace CustomCameras
{
    /// <summary>
    /// 摄像机抖动处理器，自行订阅 FloatEventChannelSO 接收震屏指令。
    /// 挂在 Cinemachine Virtual Camera 上，同级需有 CinemachineImpulseSource 组件。
    /// </summary>
    public class CameraShakeHandler : MonoBehaviour
    {
        [Tooltip("震屏事件通道，NormalAttackState 通过此通道广播抖动力度")]
        [SerializeField] private FloatEventChannelSO _channel;

        [Tooltip("Cinemachine Impulse Source 组件引用，需挂在同一 GameObject 上")]
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        private void OnEnable()
        {
            if (_channel != null)
            {
                _channel.Subscribe(HandleShake);
            }
        }

        private void OnDisable()
        {
            if (_channel != null)
            {
                _channel.Unsubscribe(HandleShake);
            }
        }

        private void HandleShake(float force)
        {
            if (force <= 0f) return;
            _impulseSource.GenerateImpulseWithForce(force);
        }
    }
}
