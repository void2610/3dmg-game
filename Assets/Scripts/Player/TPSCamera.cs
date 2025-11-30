using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    // TPSフリーカメラ - マウスで自由に視点を回転し、キャラクターを追従する
    [RequireComponent(typeof(Camera))]
    public class TpsCamera : MonoBehaviour
    {
        [SerializeField] private float height = 1.5f;

        [Header("カメラ設定")]
        [SerializeField] private float baseDistance = 4.0f;
        [SerializeField] private float minDistance = 1.0f;
        [SerializeField] private float maxDistance = 10.0f;

        [Header("回転設定")]
        [SerializeField] private float sensitivity = 2.0f;
        [SerializeField] private float minVerticalAngle = -40f;
        [SerializeField] private float maxVerticalAngle = 70f;

        [Header("衝突設定")]
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private LayerMask collisionLayers = ~0;

        [Header("ダイナミックカメラ - 距離")]
        [SerializeField] private float maxSpeedDistance = 8f;
        [SerializeField] private float speedForMaxDistance = 30f;
        [SerializeField] private float distanceSmoothTime = 0.3f;

        [Header("ダイナミックカメラ - FOV")]
        [SerializeField] private float baseFov = 60f;
        [SerializeField] private float maxSpeedFov = 80f;
        [SerializeField] private float fovSmoothTime = 0.2f;

        [Header("参照")]
        [SerializeField] private PlayerController player;

        private Rigidbody _playerRigidbody;
        private Vector2 _lookInput;
        private float _yaw;
        private float _pitch;
        private Vector3 _targetVelocity;
        private float _currentDistance;
        private float _currentFov;
        private Camera _camera;

        // SmoothDamp用の速度変数
        private float _distanceVelocity;
        private float _fovVelocity;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _playerRigidbody = player.GetComponent<Rigidbody>();
        }

        private void Start()
        {
            // 初期回転角度をカメラの現在の向きから取得
            var eulerAngles = transform.eulerAngles;
            _yaw = eulerAngles.y;
            _pitch = eulerAngles.x;

            // ピッチ角度を-180~180の範囲に正規化
            if (_pitch > 180f) _pitch -= 360f;

            // カーソルをロック
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // 初期値設定
            _currentDistance = baseDistance;
            _currentFov = baseFov;
            _camera.fieldOfView = baseFov;
        }

        private void Update()
        {
            // カメラの向きをPlayerに伝達
            player.SetCameraDirection(Forward, Right);
            player.SetAimDirection(transform.forward);

            // 速度情報を取得
            _targetVelocity = _playerRigidbody.linearVelocity;
        }

        private void LateUpdate()
        {
            UpdateDynamicDistance();
            UpdateDynamicFov();
            HandleRotation();
            HandlePosition();
        }

        private void UpdateDynamicDistance()
        {
            // 水平速度のみで計算
            var horizontalSpeed = new Vector3(_targetVelocity.x, 0f, _targetVelocity.z).magnitude;
            var speedRatio = Mathf.Clamp01(horizontalSpeed / speedForMaxDistance);
            var targetDistance = Mathf.Lerp(baseDistance, maxSpeedDistance, speedRatio);
            _currentDistance = Mathf.SmoothDamp(_currentDistance, targetDistance, ref _distanceVelocity, distanceSmoothTime);
        }

        private void UpdateDynamicFov()
        {
            // 水平速度のみで計算
            var horizontalSpeed = new Vector3(_targetVelocity.x, 0f, _targetVelocity.z).magnitude;
            var speedRatio = Mathf.Clamp01(horizontalSpeed / speedForMaxDistance);
            var targetFov = Mathf.Lerp(baseFov, maxSpeedFov, speedRatio);
            _currentFov = Mathf.SmoothDamp(_currentFov, targetFov, ref _fovVelocity, fovSmoothTime);
            _camera.fieldOfView = _currentFov;
        }

        private void HandleRotation()
        {
            // マウス入力で回転角度を更新
            _yaw += _lookInput.x * sensitivity;
            _pitch -= _lookInput.y * sensitivity;
            _pitch = Mathf.Clamp(_pitch, minVerticalAngle, maxVerticalAngle);

            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        private void HandlePosition()
        {
            // 注視点（Player位置 + 高さオフセット）
            var lookAtPoint = player.transform.position + Vector3.up * height;

            // カメラの理想位置を計算（動的距離を使用）
            var desiredPosition = lookAtPoint - transform.forward * _currentDistance;
            // 壁との衝突判定
            var actualDistance = CheckCollision(lookAtPoint, desiredPosition);

            // 最終的なカメラ位置
            transform.position = lookAtPoint - transform.forward * actualDistance;
        }

        private float CheckCollision(Vector3 lookAtPoint, Vector3 desiredPosition)
        {
            var direction = desiredPosition - lookAtPoint;
            var maxCheckDistance = direction.magnitude;

            // SphereCastで衝突判定
            if (Physics.SphereCast(
                    lookAtPoint,
                    collisionRadius,
                    direction.normalized,
                    out var hit,
                    maxCheckDistance,
                    collisionLayers,
                    QueryTriggerInteraction.Ignore))
            {
                // 衝突した場合は、その位置までの距離を返す
                return Mathf.Clamp(hit.distance, minDistance, _currentDistance);
            }

            // 衝突しなかった場合は動的距離を返す
            return _currentDistance;
        }

        // 視点入力のコールバック（Input System SendMessages）
        public void OnLook(InputValue value) => _lookInput = value.Get<Vector2>();

        // カメラの前方向（Y成分を除去して正規化）
        public Vector3 Forward
        {
            get
            {
                var forward = transform.forward;
                forward.y = 0f;
                return forward.normalized;
            }
        }

        // カメラの右方向（Y成分を除去して正規化）
        public Vector3 Right
        {
            get
            {
                var right = transform.right;
                right.y = 0f;
                return right.normalized;
            }
        }
    }
}
