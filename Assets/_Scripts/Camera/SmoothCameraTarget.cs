using System;
using System.Collections;
using SPEvent;
using SPTeam;
using UnityEngine;

namespace SPCamera
{
    /// <summary>
    /// 平滑摄像机目标 - 挂载于 CameraLook 物体，角色切换时驱动其位置平滑过渡。
    /// CinemachineFreeLook 的 Follow / LookAt 始终指向此物体，无需运行时切换。
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class SmoothCameraTarget : MonoBehaviour
    {
        [Header("目标")]
        [Tooltip("队伍控制器引用，用于获取角色 Transform。")]
        [SerializeField] private TeamController _teamController;

        [Header("过渡参数")]
        [Tooltip("平滑时间，值越大过渡越慢。")]
        [SerializeField] private float _smoothTime = 0.3f;

        [Tooltip("最大移动速度，防止远距离切换时过渡过慢。")]
        [SerializeField] private float _maxSpeed = 40f;

        private Transform _targetCharacter;
        private Vector3 _velocity;
        private bool _isTransitioning;
        private Coroutine _transitionCoroutine;

        private void Awake()
        {
            if (_teamController == null)
                throw new InvalidOperationException($"{name}: 未设置 TeamController 引用。");
        }

        private void OnEnable()
        {
            GameEvent.CharacterSwitched += OnCharacterSwitched;
        }

        private void OnDisable()
        {
            GameEvent.CharacterSwitched -= OnCharacterSwitched;
        }

        private void Start()
        {
            Transform firstCharacter = _teamController.GetCharacterTransform(0);
            if (firstCharacter == null)
                throw new InvalidOperationException($"{name}: 索引 0 的角色 Transform 为空。");

            Vector3 startPos = firstCharacter.position;
            transform.position = new Vector3(startPos.x, transform.position.y, startPos.z);
            _targetCharacter = firstCharacter;
        }

        private void Update()
        {
            if (_isTransitioning) return;
            Vector3 pos = _targetCharacter.position;
            transform.position = new Vector3(pos.x, transform.position.y, pos.z);
        }

        /// <summary>
        /// 角色切换回调 - 进入过渡阶段，中断当前过渡并从当前位置启动新过渡。
        /// </summary>
        /// <param name="newIndex">新激活角色的索引</param>
        private void OnCharacterSwitched(int newIndex)
        {
            _targetCharacter = _teamController.GetCharacterTransform(newIndex);

            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);

            _isTransitioning = true;
            _transitionCoroutine = StartCoroutine(TransitionRoutine());
        }

        /// <summary>
        /// 逐帧平滑移动到目标角色位置，速度归零且距离足够近时结束。
        /// </summary>
        private IEnumerator TransitionRoutine()
        {
            while (true)
            {
                Vector3 current = transform.position;
                Vector3 target = _targetCharacter.position;
                target.y = current.y;

                transform.position = Vector3.SmoothDamp(current, target, ref _velocity, _smoothTime, _maxSpeed);
                _velocity.y = 0f;

                Vector2 flatCurrent = new Vector2(transform.position.x, transform.position.z);
                Vector2 flatTarget = new Vector2(target.x, target.z);
                if (Vector2.Distance(flatCurrent, flatTarget) < 0.01f)
                {
                    transform.position = new Vector3(target.x, transform.position.y, target.z);
                    _velocity = Vector3.zero;
                    _isTransitioning = false;
                    yield break;
                }

                yield return null;
            }
        }
    }
}
