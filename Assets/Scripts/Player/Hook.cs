using UnityEngine;

namespace Player
{
    // フック（ワイヤー）のコントローラー
    public class Hook : MonoBehaviour
    {
        public enum HookState
        {
            Hooking,
            Hooked,
            Disabled
        }

        public HookState State { get; private set; } = HookState.Disabled;

        private GameObject _player;
        private Vector3 _targetPosition;
        private SpringJoint _joint;
        private LineRenderer _lineRenderer;

        public Vector3 GetTargetPosition() => _targetPosition;

        public float GetWireLength() => Vector3.Distance(_player.transform.position, _targetPosition);

        public void SetHook(Vector3 target, GameObject player)
        {
            if (target == Vector3.zero)
            {
                return;
            }

            _player = player;
            _targetPosition = target;
            _joint = player.gameObject.AddComponent<SpringJoint>();
            _joint.autoConfigureConnectedAnchor = false;
            _joint.connectedAnchor = _targetPosition;
            _joint.maxDistance = GetWireLength();
            _joint.minDistance = 0;
            _joint.spring = 4.5f;
            _joint.damper = 7f;
            _joint.massScale = 4.5f;
            _lineRenderer.positionCount = 2;
            State = HookState.Hooked;
        }

        public void DisableHook()
        {
            State = HookState.Disabled;
            _targetPosition = Vector3.zero;
            _lineRenderer.positionCount = 0;
            Destroy(_joint);
        }

        public void ReelWire(float reelLength)
        {
            if (_joint && _joint.maxDistance - reelLength > 0)
            {
                _joint.maxDistance -= reelLength;
            }
        }

        public void SetWireLength(float length)
        {
            if (_joint)
            {
                _joint.maxDistance = length;
            }
        }

        private void Start()
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.startWidth = 0.1f;
            _lineRenderer.endWidth = 0.1f;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = Color.black;
            _lineRenderer.endColor = Color.black;
            DisableHook();
        }

        private void Update()
        {
            if (State == HookState.Hooked)
            {
                _lineRenderer.SetPosition(0, transform.position);
                _lineRenderer.SetPosition(1, _targetPosition);
            }
        }
    }
}
