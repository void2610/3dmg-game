using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    // グラップリングフック（ワイヤーアクション）- SpringJointを使用してシンプルに実装
    [RequireComponent(typeof(Rigidbody))]
    public class GrapplingHook : MonoBehaviour
    {
        [Header("ワイヤー射出起点")]
        [SerializeField] Transform leftWireOrigin;
        [SerializeField] Transform rightWireOrigin;

        [Header("ワイヤー設定")]
        [SerializeField] float maxRange = 50f;
        [SerializeField] float springForce = 500f;
        [SerializeField] float damper = 50f;
        [SerializeField] LayerMask targetLayers = ~0;

        [SerializeField] float reelSpeed = 5f;
        [SerializeField] float minDistance = 2f;

        [SerializeField] LineRenderer leftLineRenderer;
        [SerializeField] LineRenderer rightLineRenderer;
        [SerializeField] int lineSegments = 20;
        [SerializeField] float sagAmount = 0.5f;

        private SpringJoint _leftJoint;
        private SpringJoint _rightJoint;
        private Vector3 _aimDirection = Vector3.forward;
        private bool _reelPressed;

        public void SetAimDirection(Vector3 direction) => _aimDirection = direction.normalized;

        public void OnWireReel(InputValue value) => _reelPressed = value.Get<float>() > 0.5f;

        public void OnWireLeft(InputValue value)
        {
            var isPressed = value.Get<float>() > 0.5f;

            if (isPressed)
            {
                FireWire(ref _leftJoint, leftWireOrigin);
            }
            else
            {
                ReleaseWire(ref _leftJoint);
            }
        }

        public void OnWireRight(InputValue value)
        {
            var isPressed = value.Get<float>() > 0.5f;

            if (isPressed)
            {
                FireWire(ref _rightJoint, rightWireOrigin);
            }
            else
            {
                ReleaseWire(ref _rightJoint);
            }
        }

        private Vector3 GetOriginPosition(Transform origin) => origin.position;

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
                joint.anchor = transform.InverseTransformPoint(originPos);

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

        private void UpdateWireVisual(SpringJoint joint, Transform origin, LineRenderer line)
        {
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
            var linear = Vector3.Lerp(start, end, t);
            var sagOffset = sag * Mathf.Sin(t * Mathf.PI);
            return linear + Vector3.down * sagOffset;
        }

        private void ReelIn(SpringJoint joint)
            => joint.maxDistance = Mathf.Max(joint.maxDistance - reelSpeed * Time.deltaTime, minDistance);

        private void Update()
        {
            if (_reelPressed)
            {
                if (_leftJoint)
                {
                    ReelIn(_leftJoint);
                }
                if (_rightJoint)
                {
                    ReelIn(_rightJoint);
                }
            }
        }

        private void LateUpdate()
        {
            UpdateWireVisual(_leftJoint, leftWireOrigin, leftLineRenderer);
            UpdateWireVisual(_rightJoint, rightWireOrigin, rightLineRenderer);
        }
    }
}
