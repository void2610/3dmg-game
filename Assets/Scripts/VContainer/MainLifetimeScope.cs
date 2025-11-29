using VContainer;
using VContainer.Unity;

// ゲーム全体のDIコンテナ設定
public class MainLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 現在DIで管理するコンポーネントなし
    }
}
