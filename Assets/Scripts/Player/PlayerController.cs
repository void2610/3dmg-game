using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

namespace Player
{
    // Rigidbodyベースのプレイヤーコントローラー - 立体機動装置を操作する
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("フック設定")]
        [SerializeField] private Hook leftHook;
        [SerializeField] private Hook rightHook;
        [SerializeField] private LayerMask hookableLayer;

        [Header("移動設定")]
        [SerializeField] private float speed = 13f;
        [SerializeField] private float airSpeed = 9f;
        [SerializeField] private float jumpSpeed = 10f;

        [Header("VFX")]
        [SerializeField] private VisualEffect boostVfx;
        [SerializeField] private float boostVfxRate = 50f;

        private const float MaxDistance = 150f;
        private static readonly int _rateParam = Shader.PropertyToID("Rate");

        private Rigidbody _rb;
        private Vector3 _cameraForward;
        private Vector3 _cameraRight;
        private Vector3 _aimDirection;
        private bool _isGrounded;
        private Vector3 _gasTargetPosition;
        private bool _isUsingGas;
        private bool _oldIsUsingGas;

        // Input System入力値
        private Vector2 _moveInput;
        private bool _wireLeftPressed;
        private bool _wireRightPressed;
        private bool _wireReelPressed;
        private bool _boostPressed;

        // カメラから方向を受け取る
        public void SetCameraDirection(Vector3 forward, Vector3 right)
        {
            _cameraForward = forward;
            _cameraRight = right;
        }

        // カメラからエイム方向を受け取る
        public void SetAimDirection(Vector3 direction)
        {
            _aimDirection = direction;
        }

        private float GetCameraDirection()
        {
            return Mathf.Atan2(_cameraForward.x, _cameraForward.z) * Mathf.Rad2Deg;
        }

        private Vector3 GetHookPoint()
        {
            if (Physics.Raycast(transform.position, _aimDirection, out RaycastHit hit, MaxDistance, hookableLayer))
            {
                return hit.point;
            }
            return Vector3.zero;
        }

        private float GetTargetAngle()
        {
            if (_moveInput.sqrMagnitude > 0.01f)
            {
                return GetCameraDirection() + 90f - Mathf.Atan2(_moveInput.y, _moveInput.x) * Mathf.Rad2Deg;
            }
            return GetCameraDirection();
        }

        private bool CheckGround()
        {
            return Physics.RaycastAll(transform.position + Vector3.up, Vector3.down, 1.5f)
                .Any(hit => !hit.collider.isTrigger);
        }

        private Vector3 GetMoveDirection()
        {
            var moveDirection = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
            var targetDirection = _cameraForward * moveDirection.z + _cameraRight * moveDirection.x;
            return Vector3.Lerp(_rb.linearVelocity / speed, targetDirection, 0.2f);
        }

        private Vector3 GetGasDirection()
        {
            var direction = Vector3.zero;
            if (_wireReelPressed)
            {
                direction += Vector3.up;
            }
            if (_moveInput.y > 0.5f)
            {
                direction += transform.forward;
            }
            if (_moveInput.y < -0.5f)
            {
                direction -= transform.forward;
            }
            if (_moveInput.x < -0.5f)
            {
                direction -= transform.right;
            }
            if (_moveInput.x > 0.5f)
            {
                direction += transform.right;
            }
            return direction;
        }

        private void InAirMovement()
        {
            Vector3 d = GetGasDirection();
            if (d != Vector3.zero)
            {
                _rb.AddForce(d * airSpeed);
            }
        }

        private void Jump()
        {
            if (CheckGround())
            {
                _rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
            }
        }

        private void SetGasTargetPosition()
        {
            if (leftHook.State == Hook.HookState.Disabled && rightHook.State == Hook.HookState.Disabled)
            {
                return;
            }

            Vector3 d = Vector3.zero;
            if (leftHook.State == Hook.HookState.Hooked)
            {
                d += leftHook.GetTargetPosition() - transform.position;
                leftHook.SetWireLength(0f);
            }
            if (rightHook.State == Hook.HookState.Hooked)
            {
                d += rightHook.GetTargetPosition() - transform.position;
                rightHook.SetWireLength(0f);
            }

            _gasTargetPosition = d;
        }

        private void ResetGasTargetPosition()
        {
            _gasTargetPosition = Vector3.zero;
            if (leftHook.State == Hook.HookState.Hooked)
            {
                leftHook.SetWireLength(leftHook.GetWireLength());
            }
            if (rightHook.State == Hook.HookState.Hooked)
            {
                rightHook.SetWireLength(rightHook.GetWireLength());
            }
        }

        private void GasMovement()
        {
            if (leftHook.State == Hook.HookState.Disabled && rightHook.State == Hook.HookState.Disabled)
            {
                return;
            }

            _rb.AddForce(_gasTargetPosition * 10);
            UpdateBoostVfx(true);
        }

        private void UpdateBoostVfx(bool active)
        {
            boostVfx.SetFloat(_rateParam, active ? boostVfxRate : 0f);
        }

        // Input Systemコールバック
        public void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();

        public void OnWireLeft(InputValue value)
        {
            var pressed = value.Get<float>() > 0.5f;
            if (pressed && !_wireLeftPressed)
            {
                leftHook.SetHook(GetHookPoint(), gameObject);
            }
            else if (!pressed && _wireLeftPressed)
            {
                leftHook.DisableHook();
            }
            _wireLeftPressed = pressed;
        }

        public void OnWireRight(InputValue value)
        {
            var pressed = value.Get<float>() > 0.5f;
            if (pressed && !_wireRightPressed)
            {
                rightHook.SetHook(GetHookPoint(), gameObject);
            }
            else if (!pressed && _wireRightPressed)
            {
                rightHook.DisableHook();
            }
            _wireRightPressed = pressed;
        }

        public void OnWireReel(InputValue value) => _wireReelPressed = value.Get<float>() > 0.5f;

        public void OnBoost(InputValue value)
        {
            var pressed = value.Get<float>() > 0.5f;
            if (pressed && !_boostPressed)
            {
                _isUsingGas = true;
            }
            else if (!pressed && _boostPressed)
            {
                _isUsingGas = false;
            }
            _boostPressed = pressed;
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.linearVelocity = Vector3.zero;
        }

        private void Update()
        {
            _isGrounded = CheckGround();
        }

        private void FixedUpdate()
        {
            UpdateBoostVfx(false);

            if (_isGrounded)
            {
                if (_moveInput.y > 0.5f)
                {
                    _rb.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, GetTargetAngle(), 0f), Time.deltaTime * 10f);
                    var moveVelocity = GetMoveDirection() * speed;
                    _rb.linearVelocity = new Vector3(moveVelocity.x, _rb.linearVelocity.y, moveVelocity.z);
                }
                else
                {
                    _rb.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, GetCameraDirection(), 0f), Time.deltaTime * 5f);
                    var moveVelocity = GetMoveDirection() * (speed * 0.85f);
                    _rb.linearVelocity = new Vector3(moveVelocity.x, _rb.linearVelocity.y, moveVelocity.z);
                }

                if (_wireReelPressed)
                {
                    Jump();
                }
            }
            else
            {
                _rb.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, GetCameraDirection(), 0f), Time.deltaTime * 10f);
                InAirMovement();
            }

            if (_isUsingGas && !_oldIsUsingGas)
            {
                SetGasTargetPosition();
            }
            if (_isUsingGas)
            {
                GasMovement();
            }
            if (!_isUsingGas && _oldIsUsingGas)
            {
                ResetGasTargetPosition();
            }

            _oldIsUsingGas = _isUsingGas;
        }
    }
}
