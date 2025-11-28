using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    // RigidbodyベースのTPSキャラクターコントローラー - カメラ相対移動でキャラクターを操作する
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("移動設定")]
        [SerializeField] private float moveSpeed = 6.0f;
        [SerializeField] private float rotationSpeed = 10.0f;

        [Header("接地判定")]
        [SerializeField] private float groundCheckRadius = 0.3f;
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask groundLayers = ~0;

        [Header("参照")]
        [SerializeField] private Animator animator;

        // カメラの向き（Coordinatorから毎フレーム設定される）
        private Vector3 _cameraForward;
        private Vector3 _cameraRight;

        // コンポーネント
        private Rigidbody _rb;

        // 入力値
        private Vector2 _moveInput;

        // Animatorパラメータ名
        private static readonly int SpeedParam = Animator.StringToHash("Speed");

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            // Rigidbody設定（回転を固定）
            _rb.constraints = RigidbodyConstraints.FreezeRotation;

            // Animatorが未設定の場合、子オブジェクトから取得
            animator ??= GetComponentInChildren<Animator>();
        }

        // カメラの向きを設定（Coordinatorから毎フレーム呼ばれる）
        public void SetMoveDirection(Vector3 cameraForward, Vector3 cameraRight)
        {
            _cameraForward = cameraForward;
            _cameraRight = cameraRight;
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void Update()
        {
            HandleRotation();
            UpdateAnimator();
        }

        private bool IsGrounded()
        {
            return Physics.SphereCast(
                transform.position + Vector3.up * (groundCheckRadius + 0.1f),
                groundCheckRadius,
                Vector3.down,
                out _,
                groundCheckDistance + 0.1f,
                groundLayers
            );
        }

        // 移動処理（カメラ相対）- FixedUpdateで実行
        private void HandleMovement()
        {
            // 接地中のみ移動入力を適用
            if (!IsGrounded())
                return;

            // 入力に基づいた移動ベクトル
            var moveDirection = _cameraForward * _moveInput.y + _cameraRight * _moveInput.x;

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                // 目標速度を設定（Y軸は維持）
                var targetVelocity = moveDirection.normalized * moveSpeed;
                _rb.linearVelocity = new Vector3(targetVelocity.x, _rb.linearVelocity.y, targetVelocity.z);
            }
            else
            {
                // 入力がない場合は水平速度を減衰
                _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
            }
        }

        // 回転処理 - Updateで実行（滑らかな回転のため）
        private void HandleRotation()
        {
            var moveDirection = _cameraForward * _moveInput.y + _cameraRight * _moveInput.x;

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                var targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        private void UpdateAnimator()
        {
            // 移動入力の大きさを0~1でAnimatorに渡す
            var speed = _moveInput.magnitude;
            animator.SetFloat(SpeedParam, speed);
        }

        // 移動入力のコールバック（Input System SendMessages）
        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }
    }
}
