using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    /// <summary>
    /// グラップリングフック（ワイヤーアクション）
    /// SpringJointを使用してシンプルに実装
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class GrapplingHook : MonoBehaviour
    {
        [Header("ワイヤー射出起点")]
        [SerializeField] private Transform leftWireOrigin;
        [SerializeField] private Transform rightWireOrigin;

        [Header("ワイヤー設定")]
        [SerializeField] private float maxRange = 50f;
        [SerializeField] private float springForce = 500f;
        [SerializeField] private float damper = 50f;
        [SerializeField] private LayerMask targetLayers = ~0;

        [Header("巻き取り設定")]
        [SerializeField] private float reelSpeed = 5f;
        [SerializeField] private float minDistance = 2f;

        // コンポーネント
        private Rigidbody _rb;

        // 左右のジョイント
        private SpringJoint _leftJoint;
        private SpringJoint _rightJoint;

        // エイム方向（カメラ前方）
        private Vector3 _aimDirection = Vector3.forward;

        // 入力状態
        private bool _leftPressed;
        private bool _rightPressed;

        /// <summary>
        /// いずれかのワイヤーがアクティブか
        /// </summary>
        public bool IsAnyWireActive => _leftJoint != null || _rightJoint != null;

        /// <summary>
        /// 左ワイヤーのアンカー位置
        /// </summary>
        public Vector3? LeftAnchorPoint => _leftJoint != null ? _leftJoint.connectedAnchor : null;

        /// <summary>
        /// 右ワイヤーのアンカー位置
        /// </summary>
        public Vector3? RightAnchorPoint => _rightJoint != null ? _rightJoint.connectedAnchor : null;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            // ワイヤー巻き取り
            if (_leftJoint != null && _leftPressed)
            {
                ReelIn(_leftJoint);
            }
            if (_rightJoint != null && _rightPressed)
            {
                ReelIn(_rightJoint);
            }
        }

        /// <summary>
        /// エイム方向を設定（Coordinatorから呼ばれる）
        /// </summary>
        public void SetAimDirection(Vector3 direction)
        {
            _aimDirection = direction.normalized;
        }

        /// <summary>
        /// ワイヤーを巻き取る
        /// </summary>
        private void ReelIn(SpringJoint joint)
        {
            if (joint == null) return;

            float newMaxDistance = joint.maxDistance - reelSpeed * Time.deltaTime;
            joint.maxDistance = Mathf.Max(newMaxDistance, minDistance);
        }

        #region Input System Callbacks

        /// <summary>
        /// 左ワイヤー入力
        /// </summary>
        public void OnWireLeft(InputValue value)
        {
            float inputValue = value.Get<float>();
            bool isPressed = inputValue > 0.5f;

            if (isPressed && !_leftPressed)
            {
                _leftPressed = true;
                FireWire(ref _leftJoint, leftWireOrigin);
            }
            else if (!isPressed && _leftPressed)
            {
                _leftPressed = false;
                ReleaseWire(ref _leftJoint);
            }
        }

        /// <summary>
        /// 右ワイヤー入力
        /// </summary>
        public void OnWireRight(InputValue value)
        {
            float inputValue = value.Get<float>();
            bool isPressed = inputValue > 0.5f;

            if (isPressed && !_rightPressed)
            {
                _rightPressed = true;
                FireWire(ref _rightJoint, rightWireOrigin);
            }
            else if (!isPressed && _rightPressed)
            {
                _rightPressed = false;
                ReleaseWire(ref _rightJoint);
            }
        }

        #endregion

        /// <summary>
        /// 射出起点の位置を取得
        /// </summary>
        private Vector3 GetOriginPosition(Transform origin)
        {
            return origin != null ? origin.position : transform.position;
        }

        /// <summary>
        /// ワイヤーを発射
        /// </summary>
        private void FireWire(ref SpringJoint joint, Transform origin)
        {
            // すでに接続中なら何もしない
            if (joint != null) return;

            Vector3 originPos = GetOriginPosition(origin);

            // レイキャストでターゲットを検索
            if (Physics.Raycast(originPos, _aimDirection, out var hit, maxRange, targetLayers))
            {
                // SpringJointを動的に生成
                joint = gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = hit.point;

                // ローカルアンカー（射出起点）を設定
                if (origin != null)
                {
                    joint.anchor = transform.InverseTransformPoint(originPos);
                }

                // ワイヤー長を現在の距離に設定
                float distance = Vector3.Distance(originPos, hit.point);
                joint.maxDistance = distance;
                joint.minDistance = 0f;

                // バネの設定
                joint.spring = springForce;
                joint.damper = damper;

                Debug.Log($"Wire attached at {hit.point}, distance: {distance}");
            }
        }

        /// <summary>
        /// ワイヤーを解除
        /// </summary>
        private void ReleaseWire(ref SpringJoint joint)
        {
            if (joint != null)
            {
                Destroy(joint);
                joint = null;
                Debug.Log("Wire released");
            }
        }

        /// <summary>
        /// すべてのワイヤーを解除
        /// </summary>
        public void ReleaseAll()
        {
            ReleaseWire(ref _leftJoint);
            ReleaseWire(ref _rightJoint);
            _leftPressed = false;
            _rightPressed = false;
        }
    }
}
