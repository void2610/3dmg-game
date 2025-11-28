# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

進撃の巨人の「立体機動装置」をモチーフにしたUnity TPSアクションゲーム。

## 開発コマンド

### Unity開発ツール
```bash
# Unityコンパイルエラーチェック
./unity-tools/unity-compile.sh trigger . && sleep 3 && ./unity-tools/unity-compile.sh check .
```
**必ずコンパイルを実行する必要があります。 check だけの実行は如何なる場合でも認められません。**

## 開発方針

### YAGNI原則（You Aren't Gonna Need It）
**🚨 ユーザーが指示した以外のものは一切実装しない**

- **将来の拡張性は考慮しない**: 今必要でない機能は実装禁止
- **予想実装の禁止**: 「将来必要になるかも」は実装理由にならない
- **現在の要求のみ対応**: ユーザーの明確な指示のみを実装対象とする
- **過度な抽象化禁止**: 将来の変更に備えた複雑な設計は不要

```csharp
// ❌ YAGNI違反 - 将来の拡張を考慮した不要な抽象化
public interface IMovementProvider { }
public interface IAttackProvider { }
public class PlayerController : IMovementProvider, IAttackProvider { }

// ✅ YAGNI準拠 - 現在必要な機能のみ
public class PlayerController 
{
    public void Move() { /* 現在必要な移動処理のみ */ }
}
```

### コーディング規約チェック
- コード実装完了後は、**code-quality-checkerサブエージェント**を使用してコーディング規約への準拠を確認する
- サブエージェントが命名規則、フォーマット、コードスタイルをチェックする
  **🚨 コード実装完了後は必ず `unity-code-quality-checker` サブエージェントを実行すること**
- コンパイルチェックと同様に**省略は一切認められない**
- 命名規則、フォーマット、コードスタイルの違反を検出・修正する

### DI設計原則
- `MainLifetimeScope`: メインDI設定（Scripts/VContainer/）
- **ViewクラスのみMonoBehaviour継承**: Model、PresenterはピュアC#クラス
- **Presenterでの取得**: コンストラクタで`FindFirstObjectByType<T>()`を使用してViewを取得
- **コンストラクタ注入**: Presenterはコンストラクタで依存関係を受け取る

**重要**: FindFirstObjectByTypeでViewが見つからない場合、手動でnullチェックを行わない。Viewがないままアクセスすればnull参照例外で自動的にエラーが発生し、問題の原因が明確になる。

## Unity開発ベストプラクティス

### 非同期処理
- **UniTask使用**: 全ての非同期処理にUniTaskを使用
    - **async/await**: 現代的な非同期パターンを採用
    - **CancellationToken**: 適切なキャンセル処理を実装

### 開発プロセス

コード実装時は以下の順序で作業を行う：
**🚨 コード実装時は以下の順序で作業を行う（省略厳禁）：**

1. **コード実装**
    - ユーザーの要求に従ってコードを実装

2. **Unityコンパイルチェック**
2. **Unityコンパイルチェック（必須）**
    - 必ず以下のコマンドを実行してコンパイルエラーを確認
   ```bash
   ./unity-tools/unity-compile.sh trigger . && sleep 3 && ./unity-tools/unity-compile.sh check .
   ```
    - コンパイルエラーがある場合は修正してから次へ

3. **コーディング規約チェック**
    - code-quality-checkerサブエージェントを実行
3. **コーディング規約チェック（必須）**
    - **`unity-code-quality-checker` サブエージェントを必ず実行**
    - 違反事項がある場合は修正
    - **このステップを省略することは一切認められない**

4. **ユーザーへ報告**
    - 実装内容、コンパイル結果、規約チェック結果を報告


## 開発コマンド

### Unity開発ツール
```bash
# Unityコンパイルエラーチェック
./unity-tools/unity-compile.sh trigger . && sleep 3 && ./unity-tools/unity-compile.sh check .
```
**必ずコンパイルを実行する必要があります。 check だけの実行は如何なる場合でも認められません。**

## 開発方針

### YAGNI原則（You Aren't Gonna Need It）
**🚨 ユーザーが指示した以外のものは一切実装しない**

- **将来の拡張性は考慮しない**: 今必要でない機能は実装禁止
- **予想実装の禁止**: 「将来必要になるかも」は実装理由にならない
- **現在の要求のみ対応**: ユーザーの明確な指示のみを実装対象とする
- **過度な抽象化禁止**: 将来の変更に備えた複雑な設計は不要

```csharp
// ❌ YAGNI違反 - 将来の拡張を考慮した不要な抽象化
public interface IMovementProvider { }
public interface IAttackProvider { }
public class PlayerController : IMovementProvider, IAttackProvider { }

// ✅ YAGNI準拠 - 現在必要な機能のみ
public class PlayerController 
{
    public void Move() { /* 現在必要な移動処理のみ */ }
}
```

### コーディング規約チェック
- コード実装完了後は、**code-quality-checkerサブエージェント**を使用してコーディング規約への準拠を確認する
- サブエージェントが命名規則、フォーマット、コードスタイルをチェックする
  **🚨 コード実装完了後は必ず `unity-code-quality-checker` サブエージェントを実行すること**
- コンパイルチェックと同様に**省略は一切認められない**
- 命名規則、フォーマット、コードスタイルの違反を検出・修正する

### DI設計原則
- `MainLifetimeScope`: メインDI設定（Scripts/VContainer/）
- **ViewクラスのみMonoBehaviour継承**: Model、PresenterはピュアC#クラス
- **Presenterでの取得**: コンストラクタで`FindFirstObjectByType<T>()`を使用してViewを取得
- **コンストラクタ注入**: Presenterはコンストラクタで依存関係を受け取る

**重要**: FindFirstObjectByTypeでViewが見つからない場合、手動でnullチェックを行わない。Viewがないままアクセスすればnull参照例外で自動的にエラーが発生し、問題の原因が明確になる。

## Unity開発ベストプラクティス

### 非同期処理
- **UniTask使用**: 全ての非同期処理にUniTaskを使用
    - **async/await**: 現代的な非同期パターンを採用
    - **CancellationToken**: 適切なキャンセル処理を実装

### 開発プロセス

コード実装時は以下の順序で作業を行う：
**🚨 コード実装時は以下の順序で作業を行う（省略厳禁）：**

1. **コード実装**
    - ユーザーの要求に従ってコードを実装

2. **Unityコンパイルチェック**
2. **Unityコンパイルチェック（必須）**
    - 必ず以下のコマンドを実行してコンパイルエラーを確認
   ```bash
   ./unity-tools/unity-compile.sh trigger . && sleep 3 && ./unity-tools/unity-compile.sh check .
   ```
    - コンパイルエラーがある場合は修正してから次へ

3. **コーディング規約チェック**
    - code-quality-checkerサブエージェントを実行
3. **コーディング規約チェック（必須）**
    - **`unity-code-quality-checker` サブエージェントを必ず実行**
    - 違反事項がある場合は修正
    - **このステップを省略することは一切認められない**

4. **ユーザーへ報告**
    - 実装内容、コンパイル結果、規約チェック結果を報告