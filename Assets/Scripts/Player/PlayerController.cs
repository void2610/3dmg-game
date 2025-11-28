using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

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

        [Header("ジャンプ/ブースト設定")]
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float boostForce = 15f;
        [SerializeField] private float airControlForce = 3f;

        [Header("参照")]
        [SerializeField] private Animator animator;
        [SerializeField] private VisualEffect boostVfx;

        [Header("VFX設定")]
        [SerializeField] private float boostVfxRate = 50f;

        private static readonly int _speedParam = Animator.StringToHash("Speed");
        private static readonly int _rateParam = Shader.PropertyToID("Rate");

        private Vector3 _cameraForward;
        private Vector3 _cameraRight;
        private Rigidbody _rb;
        private Vector2 _moveInput;
        private bool _boostPressed;

        public void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();

        public void OnBoost(InputValue value) => _boostPressed = value.Get<float>() > 0.5f;

        public void SetMoveDirection(Vector3 cameraForward, Vector3 cameraRight)
        {
            _cameraForward = cameraForward;
            _cameraRight = cameraRight;
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

        private Vector3 GetMoveDirection()
        {
            return _cameraForward * _moveInput.y + _cameraRight * _moveInput.x;
        }

        private void HandleMovement()
        {
            var moveDirection = GetMoveDirection();

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                var targetVelocity = moveDirection.normalized * moveSpeed;
                _rb.linearVelocity = new Vector3(targetVelocity.x, _rb.linearVelocity.y, targetVelocity.z);
            }
            else
            {
                _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
            }
        }

        private void HandleJump()
        {
            if (_boostPressed)
            {
                _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, jumpForce, _rb.linearVelocity.z);
            }
        }

        private void HandleAirControl()
        {
            var moveDirection = GetMoveDirection();

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                // 通常の空中制御
                _rb.AddForce(moveDirection.normalized * airControlForce, ForceMode.Acceleration);
            }
        }

        private void HandleAirBoost()
        {
            if (!_boostPressed) return;

            var moveDirection = GetMoveDirection();

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                // WASD入力あり → その方向に加速
                _rb.AddForce(moveDirection.normalized * boostForce, ForceMode.Acceleration);
            }
            else
            {
                // 入力なし → 上方向に加速
                _rb.AddForce(Vector3.up * boostForce, ForceMode.Acceleration);
            }
        }

        private void HandleRotation()
        {
            var moveDirection = GetMoveDirection();

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
            var speed = _moveInput.magnitude;
            animator.SetFloat(_speedParam, speed);
        }

        private void UpdateBoostVfx()
        {
            var rate = _boostPressed ? boostVfxRate : 0f;
            boostVfx.SetFloat(_rateParam, rate);
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            animator ??= GetComponentInChildren<Animator>();
        }

        private void FixedUpdate()
        {
            if (IsGrounded())
            {
                HandleMovement();
                HandleJump();
            }
            else
            {
                HandleAirControl();
                HandleAirBoost();
            }
        }

        private void Update()
        {
            HandleRotation();
            UpdateAnimator();
            UpdateBoostVfx();
        }
    }
}
