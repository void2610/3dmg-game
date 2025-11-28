using Player;
using UnityEngine;
using VContainer.Unity;

// PlayerとCamera間の調停を行う純粋C#クラス - 毎フレーム、双方向のデータ受け渡しを仲介する
public class PlayerCameraCoordinator : ITickable, ILateTickable
{
    private readonly PlayerController _player;
    private readonly TpsCamera _camera;
    private readonly GrapplingHook _grapplingHook;
    private readonly Rigidbody _playerRigidbody;

    public PlayerCameraCoordinator(PlayerController player, TpsCamera camera, GrapplingHook grapplingHook)
    {
        _player = player;
        _camera = camera;
        _grapplingHook = grapplingHook;
        _playerRigidbody = player.GetComponent<Rigidbody>();
    }

    // Update相当：カメラの向きをPlayerとGrapplingHookに伝達
    public void Tick()
    {
        _player.SetMoveDirection(_camera.Forward, _camera.Right);
        _grapplingHook.SetAimDirection(_camera.transform.forward);
        _camera.SetTargetVelocity(_playerRigidbody.linearVelocity);
    }

    // LateUpdate相当：Playerの位置をCameraに伝達
    public void LateTick()
    {
        _camera.FollowTarget(_player.transform);
    }
}
