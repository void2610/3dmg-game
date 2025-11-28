using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    // グラップリングフック（ワイヤーアクション）- SpringJointを使用してシンプルに実装
    [RequireComponent(typeof(Rigidbody))]
    public class GrapplingHook : MonoBehaviour
    {
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private Transform leftWireOrigin;
        [SerializeField] private Transform rightWireOrigin;
        [SerializeField] private LineRenderer leftLineRenderer;
        [SerializeField] private LineRenderer rightLineRenderer;

        [Header("ワイヤーパラメータ")]
        [SerializeField] private float maxRange = 50f;
        [SerializeField] private float springForce = 500f;
        [SerializeField] private float damper = 50f;
        [SerializeField] private float manualReelSpeed = 5f;
        [SerializeField] private float autoReelForce = 3f;
        [SerializeField] private float minDistance = 2f;
        [SerializeField] private int lineSegments = 20;
        [SerializeField] private float sagAmount = 0.5f;

        private Rigidbody _rb;
        private SpringJoint _leftJoint;
        private SpringJoint _rightJoint;
        private Vector3 _aimDirection = Vector3.forward;
        private bool _reelPressed;

        public void SetAimDirection(Vector3 direction) => _aimDirection = direction.normalized;
        public void OnWireReel(InputValue value) => _reelPressed = value.Get<float>() > 0.5f;

        public void OnWireLeft(InputValue value)
        {
            var isPressed = value.Get<float>() > 0.5f;
            if (isPressed) FireWire(ref _leftJoint, leftWireOrigin);
            else ReleaseWire(ref _leftJoint);
        }

        public void OnWireRight(InputValue value)
        {
            var isPressed = value.Get<float>() > 0.5f;
            if (isPressed) FireWire(ref _rightJoint, rightWireOrigin);
            else ReleaseWire(ref _rightJoint);
        }

        // アンカー方向への速度成分を取得（正=近づく、負=離れる）
        private float GetVelocityTowardAnchor(SpringJoint joint)
        {
            var toAnchor = (joint.connectedAnchor - transform.position).normalized;
            return Vector3.Dot(_rb.linearVelocity, toAnchor);
        }

        // 自動巻き取り: 巻き取り力と速度の相互作用で連続的に変化
        private void AutoAdjustWireLength(SpringJoint joint)
        {
            // アンカー方向への速度成分（正=近づく、負=離れる）
            var velocityToward = GetVelocityTowardAnchor(joint);

            // 実効巻き取り速度 = 基本巻き取り力 + 速度成分
            // 近づいている(正) → 巻き取りが加速
            // 離れている(負) → 巻き取りが減速、さらに負ならワイヤー伸張
            var effectiveReelSpeed = autoReelForce + velocityToward;

            // maxDistanceを調整
            var deltaDistance = effectiveReelSpeed * Time.deltaTime;
            joint.maxDistance = Mathf.Max(joint.maxDistance - deltaDistance, minDistance);
        }

        private void FireWire(ref SpringJoint joint, Transform origin)
        {
            // すでに接続中なら何もしない
            if (joint) return;

            var originPos = origin.position;

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

            // カテナリー曲線で中間点を計算
            line.enabled = true;
            line.positionCount = lineSegments;
            for (var i = 0; i < lineSegments; i++)
            {
                var t = i / (float)(lineSegments - 1);
                var point = CalculateCatenary(origin.position, joint.connectedAnchor, t, sagAmount);
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
        {
            joint.maxDistance = Mathf.Max(joint.maxDistance - manualReelSpeed * Time.deltaTime, minDistance);
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            // 明示的巻き取り（優先）
            if (_reelPressed)
            {
                if (_leftJoint) ReelIn(_leftJoint);
                if (_rightJoint) ReelIn(_rightJoint);
            }
            else
            {
                // 自動巻き取り/伸張
                if (_leftJoint) AutoAdjustWireLength(_leftJoint);
                if (_rightJoint) AutoAdjustWireLength(_rightJoint);
            }
        }

        private void LateUpdate()
        {
            UpdateWireVisual(_leftJoint, leftWireOrigin, leftLineRenderer);
            UpdateWireVisual(_rightJoint, rightWireOrigin, rightLineRenderer);
        }
    }
}
