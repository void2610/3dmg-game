using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    /// <summary>
    /// TPSフリーカメラ
    /// マウスで自由に視点を回転し、キャラクターを追従する
    /// </summary>
    public class TpsCamera : MonoBehaviour
    {
        [SerializeField] private float height = 1.5f;

        // ターゲット参照（DIでセットアップ）
        private Transform _target;

        [Header("カメラ設定")]
        [SerializeField] private float distance = 4.0f;
        [SerializeField] private float minDistance = 1.0f;
        [SerializeField] private float maxDistance = 10.0f;

        [Header("回転設定")]
        [SerializeField] private float sensitivity = 2.0f;
        [SerializeField] private float minVerticalAngle = -40f;
        [SerializeField] private float maxVerticalAngle = 70f;

        [Header("衝突設定")]
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private LayerMask collisionLayers = ~0;

        private Vector2 _lookInput;
        private float _yaw;
        private float _pitch;

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
        }

        private void LateUpdate()
        {
            HandleRotation();
            HandlePosition();
        }

        /// <summary>
        /// カメラの回転処理
        /// </summary>
        private void HandleRotation()
        {
            // マウス入力で回転角度を更新
            _yaw += _lookInput.x * sensitivity;
            _pitch -= _lookInput.y * sensitivity;

            // 垂直角度を制限
            _pitch = Mathf.Clamp(_pitch, minVerticalAngle, maxVerticalAngle);

            // 回転を適用
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        /// <summary>
        /// カメラの位置処理
        /// </summary>
        private void HandlePosition()
        {
            // 注視点（ターゲットの位置 + 高さオフセット）
            var lookAtPoint = _target.position + Vector3.up * height;
            // カメラの理想位置を計算
            var desiredPosition = lookAtPoint - transform.forward * distance;
            // 壁との衝突判定
            var actualDistance = CheckCollision(lookAtPoint, desiredPosition);

            // 最終的なカメラ位置
            var targetPosition = lookAtPoint - transform.forward * actualDistance;
            transform.position = targetPosition;
        }

        /// <summary>
        /// 壁との衝突判定を行い、適切なカメラ距離を返す
        /// </summary>
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
                return Mathf.Clamp(hit.distance, minDistance, distance);
            }

            // 衝突しなかった場合は設定距離を返す
            return distance;
        }

        #region Input System Callbacks

        /// <summary>
        /// 視点入力のコールバック（Input System SendMessages）
        /// </summary>
        public void OnLook(InputValue value)
        {
            _lookInput = value.Get<Vector2>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// カメラの前方向（Y成分を除去して正規化）
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                var forward = transform.forward;
                forward.y = 0f;
                return forward.normalized;
            }
        }

        /// <summary>
        /// カメラの右方向（Y成分を除去して正規化）
        /// </summary>
        public Vector3 Right
        {
            get
            {
                var right = transform.right;
                right.y = 0f;
                return right.normalized;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 追従ターゲットを設定（Coordinatorから毎フレーム呼ばれる）
        /// </summary>
        public void FollowTarget(Transform newTarget)
        {
            _target = newTarget;
        }

        /// <summary>
        /// カーソルのロック状態を切り替え
        /// </summary>
        public void SetCursorLock(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        #endregion
    }
}
