using Player;
using VContainer;
using VContainer.Unity;

/// <summary>
/// ゲーム全体のDIコンテナ設定
/// MonoBehaviourの参照解決とCoordinatorの登録
/// </summary>
public class MainLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // シーン内のMonoBehaviourを登録
        builder.RegisterComponentInHierarchy<PlayerController>();
        builder.RegisterComponentInHierarchy<TpsCamera>();

        // Player↔Camera間の調停を行うCoordinator
        builder.RegisterEntryPoint<PlayerCameraCoordinator>();
    }
}
