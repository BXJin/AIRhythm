using UnityEngine;

[CreateAssetMenu(fileName = "JudgementConfig", menuName = "AIRhythm/Judgement Config")]
public class JudgementConfig : ScriptableObject
{
    [Header("Windows (ms)")]
    public int perfectMs = 30;
    public int greatMs = 60;
    public int goodMs = 90;

    [Header("Miss threshold (ms)")]
    public int missAfterMs = 200; // 노트 시간이 지난 후 이 정도 지나면 자동 Miss로 넘길 기준
}
