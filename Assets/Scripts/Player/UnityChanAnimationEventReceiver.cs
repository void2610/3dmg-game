using UnityEngine;

namespace Player
{
    // UnityChanアニメーションに埋め込まれたAnimationEventを受け取るダミーレシーバー（エラー回避用）
    public class UnityChanAnimationEventReceiver : MonoBehaviour
    {
        // 表情変更イベント（未実装）
        public void OnCallChangeFace(string faceName)
        {
        }
    }
}
