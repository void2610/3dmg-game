# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

進撃の巨人の「立体機動装置」をモチーフにしたUnity TPSアクションゲーム。

## 開発コマンド

```bash
# Unityコンパイルエラーチェック
./unity-tools/unity-compile.sh trigger . && sleep 3 && ./unity-tools/unity-compile.sh check .
```
**必ずコンパイルを実行する必要があります。checkだけの実行は如何なる場合でも認められません。**

## アーキテクチャ

### VContainer DI構成
`MainLifetimeScope`（Scripts/VContainer/）でDIを管理。

```
MainLifetimeScope
├── PlayerController (MonoBehaviour)
├── TpsCamera (MonoBehaviour)
├── GrapplingHook (MonoBehaviour)
└── PlayerCameraCoordinator (EntryPoint) ← Player↔Camera間の調停
```

### コンポーネント間連携
- **PlayerCameraCoordinator**: Player↔Camera間のデータ受け渡しを仲介する純粋C#クラス
  - `Tick()`: カメラの向きをPlayer/GrapplingHookに伝達、速度情報をカメラに伝達
  - `LateTick()`: Playerの位置をCameraに伝達

### 入力システム
- Unity Input System（SendMessages方式）
- 設定ファイル: `Assets/Settings/PlayerInputActions.inputactions`
- 各コンポーネントが`OnXxx(InputValue value)`メソッドでコールバックを受信

## 開発方針

### YAGNI原則
**ユーザーが指示した以外のものは一切実装しない**
- 将来の拡張性は考慮しない
- 予想実装の禁止
- 過度な抽象化禁止

### DI設計原則
- **ViewクラスのみMonoBehaviour継承**: Model、PresenterはピュアC#クラス
- **コンストラクタ注入**: Presenterはコンストラクタで依存関係を受け取る
- FindFirstObjectByTypeでViewが見つからない場合、手動でnullチェックを行わない

### 非同期処理
- **UniTask使用**: 全ての非同期処理にUniTaskを使用

## 開発プロセス

**🚨 コード実装時は以下の順序で作業を行う（省略厳禁）：**

1. **コード実装**
   - ユーザーの要求に従ってコードを実装

2. **Unityコンパイルチェック（必須）**
   ```bash
   ./unity-tools/unity-compile.sh trigger . && sleep 3 && ./unity-tools/unity-compile.sh check .
   ```
   - コンパイルエラーがある場合は修正してから次へ

3. **コーディング規約チェック（必須）**
   - **`unity-code-quality-checker`サブエージェントを必ず実行**
   - 違反事項がある場合は修正
   - **このステップを省略することは一切認められない**

4. **ユーザーへ報告**
   - 実装内容、コンパイル結果、規約チェック結果を報告
