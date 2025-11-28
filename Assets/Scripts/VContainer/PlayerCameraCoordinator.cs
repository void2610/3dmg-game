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

    // コンストラクタでDI（VContainer推奨パターン）
    public PlayerCameraCoordinator(PlayerController player, TpsCamera camera)
    {
        _player = player;
        _camera = camera;
    }

    /// <summary>
    /// Update相当：カメラの向きをPlayerに伝達
    /// </summary>
    public void Tick()
    {
        _player.SetMoveDirection(_camera.Forward, _camera.Right);
    }

    /// <summary>
    /// LateUpdate相当：Playerの位置をCameraに伝達
    /// </summary>
    public void LateTick()
    {
        _camera.FollowTarget(_player.transform);
    }
}
