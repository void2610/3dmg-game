using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    // グラップリングフック（ワイヤーアクション）- SpringJointを使用してシンプルに実装
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

        [SerializeField] private float reelSpeed = 5f;
        [SerializeField] private float minDistance = 2f;

        [SerializeField] private LineRenderer leftLineRenderer;
        [SerializeField] private LineRenderer rightLineRenderer;
        [SerializeField] private int lineSegments = 20;
        [SerializeField] private float sagAmount = 0.5f;

        // 左右のジョイント
        private SpringJoint _leftJoint;
        private SpringJoint _rightJoint;

        // エイム方向（カメラ前方）
        private Vector3 _aimDirection = Vector3.forward;

        // 入力状態
        private bool _leftPressed;
        private bool _rightPressed;

        // いずれかのワイヤーがアクティブか
        public bool IsAnyWireActive => _leftJoint || _rightJoint;

        // 左ワイヤーのアンカー位置
        public Vector3? LeftAnchorPoint => _leftJoint ? _leftJoint.connectedAnchor : null;

        // 右ワイヤーのアンカー位置
        public Vector3? RightAnchorPoint => _rightJoint ? _rightJoint.connectedAnchor : null;

        private void Update()
        {
            // ワイヤー巻き取り
            if (_leftJoint && _leftPressed)
            {
                ReelIn(_leftJoint);
            }
            if (_rightJoint && _rightPressed)
            {
                ReelIn(_rightJoint);
            }
        }

        private void LateUpdate()
        {
            // ワイヤー表示更新
            UpdateWireVisual(_leftJoint, leftWireOrigin, leftLineRenderer);
            UpdateWireVisual(_rightJoint, rightWireOrigin, rightLineRenderer);
        }

        // エイム方向を設定（Coordinatorから呼ばれる）
        public void SetAimDirection(Vector3 direction)
        {
            _aimDirection = direction.normalized;
        }

        private void ReelIn(SpringJoint joint)
        {
            var newMaxDistance = joint.maxDistance - reelSpeed * Time.deltaTime;
            joint.maxDistance = Mathf.Max(newMaxDistance, minDistance);
        }

        // 左ワイヤー入力
        public void OnWireLeft(InputValue value)
        {
            var inputValue = value.Get<float>();
            var isPressed = inputValue > 0.5f;

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

        // 右ワイヤー入力
        public void OnWireRight(InputValue value)
        {
            var inputValue = value.Get<float>();
            var isPressed = inputValue > 0.5f;

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

        private Vector3 GetOriginPosition(Transform origin)
        {
            return origin ? origin.position : transform.position;
        }

        private void FireWire(ref SpringJoint joint, Transform origin)
        {
            // すでに接続中なら何もしない
            if (joint) return;

            var originPos = GetOriginPosition(origin);

            // レイキャストでターゲットを検索
            if (Physics.Raycast(originPos, _aimDirection, out var hit, maxRange, targetLayers))
            {
                // SpringJointを動的に生成
                joint = gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = hit.point;

                // ローカルアンカー（射出起点）を設定
                if (origin)
                {
                    joint.anchor = transform.InverseTransformPoint(originPos);
                }

                // ワイヤー長を現在の距離に設定
                var distance = Vector3.Distance(originPos, hit.point);
                joint.maxDistance = distance;
                joint.minDistance = 0f;

                // バネの設定
                joint.spring = springForce;
                joint.damper = damper;
            }
        }

        private void ReleaseWire(ref SpringJoint joint)
        {
            if (joint)
            {
                Destroy(joint);
                joint = null;
            }
        }

        public void ReleaseAll()
        {
            ReleaseWire(ref _leftJoint);
            ReleaseWire(ref _rightJoint);
            _leftPressed = false;
            _rightPressed = false;
        }

        private void UpdateWireVisual(SpringJoint joint, Transform origin, LineRenderer line)
        {
            if (!line) return;

            if (!joint)
            {
                line.enabled = false;
                return;
            }

            line.enabled = true;
            var start = GetOriginPosition(origin);
            var end = joint.connectedAnchor;

            // カテナリー曲線で中間点を計算
            line.positionCount = lineSegments;
            for (var i = 0; i < lineSegments; i++)
            {
                var t = i / (float)(lineSegments - 1);
                var point = CalculateCatenary(start, end, t, sagAmount);
                line.SetPosition(i, point);
            }
        }

        private Vector3 CalculateCatenary(Vector3 start, Vector3 end, float t, float sag)
        {
            // 線形補間
            var linear = Vector3.Lerp(start, end, t);

            // たるみ（放物線近似: 中央が最もたるむ）
            var sagOffset = sag * Mathf.Sin(t * Mathf.PI);

            // 下向きにたるませる
            return linear + Vector3.down * sagOffset;
        }
    }
}
