using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    /// <summary>
    /// TPSキャラクターコントローラー
    /// カメラ相対移動でキャラクターを操作する
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("移動設定")]
        [SerializeField] private float moveSpeed = 4.0f;
        [SerializeField] private float rotationSpeed = 10.0f;
        [SerializeField] private float gravity = -9.81f;

        [Header("参照")]
        [SerializeField] private Animator animator;

        // カメラの向き（Coordinatorから毎フレーム設定される）
        private Vector3 _cameraForward;
        private Vector3 _cameraRight;

        // コンポーネント
        private CharacterController _characterController;

        // 入力値
        private Vector2 _moveInput;

        // 内部状態
        private Vector3 _velocity;

        // Animatorパラメータ名
        private static readonly int SpeedParam = Animator.StringToHash("Speed");

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            // Animatorが未設定の場合、子オブジェクトから取得
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        /// <summary>
        /// カメラの向きを設定（Coordinatorから毎フレーム呼ばれる）
        /// </summary>
        public void SetMoveDirection(Vector3 cameraForward, Vector3 cameraRight)
        {
            _cameraForward = cameraForward;
            _cameraRight = cameraRight;
        }

        private void Update()
        {
            HandleMovement();
            HandleGravity();
            UpdateAnimator();
        }

        /// <summary>
        /// 移動処理（カメラ相対）
        /// </summary>
        private void HandleMovement()
        {
            // 入力に基づいた移動ベクトル（Coordinatorから受け取ったカメラ向きを使用）
            var moveDirection = _cameraForward * _moveInput.y + _cameraRight * _moveInput.x;

            // 移動処理
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                // キャラクターを移動方向に向ける
                var targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );

                // 移動
                _characterController.Move(moveDirection.normalized * (moveSpeed * Time.deltaTime));
            }
        }

        /// <summary>
        /// 重力処理
        /// </summary>
        private void HandleGravity()
        {
            if (_characterController.isGrounded)
            {
                // 接地中は小さな下向き速度を維持（接地判定の安定化）
                _velocity.y = -2f;
            }
            else
            {
                // 重力を適用
                _velocity.y += gravity * Time.deltaTime;
            }

            _characterController.Move(_velocity * Time.deltaTime);
        }

        /// <summary>
        /// Animatorパラメータの更新
        /// </summary>
        private void UpdateAnimator()
        {
            if (animator == null) return;

            // 移動入力の大きさを0~1でAnimatorに渡す
            var speed = _moveInput.magnitude;
            animator.SetFloat(SpeedParam, speed);
        }

        #region Input System Callbacks

        /// <summary>
        /// 移動入力のコールバック（Input System SendMessages）
        /// </summary>
        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }

        #endregion
    }
}
