using UnityEngine;

namespace Player
{
    /// <summary>
    /// UnityChanアニメーションに埋め込まれたAnimationEventを受け取るダミーレシーバー
    /// エラー回避用
    /// </summary>
    public class UnityChanAnimationEventReceiver : MonoBehaviour
    {
        /// <summary>
        /// 表情変更イベント（未実装）
        /// </summary>
        public void OnCallChangeFace(string faceName)
        {
            // 表情変更が必要な場合はここに実装
        }
    }
}
