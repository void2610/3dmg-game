using Player;
using VContainer.Unity;

/// <summary>
/// PlayerとCamera間の調停を行う純粋C#クラス
/// 毎フレーム、双方向のデータ受け渡しを仲介する
/// </summary>
public class PlayerCameraCoordinator : ITickable, ILateTickable
{
    private readonly PlayerController _player;
    private readonly TpsCamera _camera;
    private readonly GrapplingHook _grapplingHook;

    // コンストラクタでDI（VContainer推奨パターン）
    public PlayerCameraCoordinator(PlayerController player, TpsCamera camera, GrapplingHook grapplingHook)
    {
        _player = player;
        _camera = camera;
        _grapplingHook = grapplingHook;
    }

    /// <summary>
    /// Update相当：カメラの向きをPlayerとGrapplingHookに伝達
    /// </summary>
    public void Tick()
    {
        _player.SetMoveDirection(_camera.Forward, _camera.Right);

        // グラップリングフックにエイム方向を伝達
        if (_grapplingHook != null)
        {
            _grapplingHook.SetAimDirection(_camera.transform.forward);
        }
    }

    /// <summary>
    /// LateUpdate相当：Playerの位置をCameraに伝達
    /// </summary>
    public void LateTick()
    {
        _camera.FollowTarget(_player.transform);
    }
}
