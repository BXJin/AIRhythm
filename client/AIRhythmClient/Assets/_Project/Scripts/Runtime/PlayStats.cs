using UnityEngine;

public class PlayStats
{
    public int Combo { get; private set; }
    public int MaxCombo { get; private set; }

    public int Score { get; private set; }

    public int TotalHits { get; private set; }     // 누른 횟수(판정 발생 수)
    public int TotalNotesJudged { get; private set; } // 판정한 노트 수(=hits + auto miss)

    public int Perfect { get; private set; }
    public int Great { get; private set; }
    public int Good { get; private set; }
    public int Miss { get; private set; }

    public float Accuracy01
    {
        get
        {
            if (TotalNotesJudged <= 0) return 0f;

            // 가중치(임시): Perfect 1.0, Great 0.8, Good 0.5, Miss 0
            float sum = Perfect * 1.0f + Great * 0.8f + Good * 0.5f;
            return sum / TotalNotesJudged;
        }
    }

    public void Apply(JudgeResult result)
    {
        TotalNotesJudged++;

        switch (result)
        {
            case JudgeResult.Perfect:
                Perfect++;
                Combo++;
                Score += 1000;
                break;

            case JudgeResult.Great:
                Great++;
                Combo++;
                Score += 700;
                break;

            case JudgeResult.Good:
                Good++;
                Combo++;
                Score += 400;
                break;

            case JudgeResult.Miss:
                Miss++;
                Combo = 0;
                break;
        }

        if (Combo > MaxCombo) MaxCombo = Combo;
    }

    public void MarkPlayerPressed() => TotalHits++;
}
