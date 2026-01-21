using UnityEngine;

public enum JudgeResult
{
    None,
    Perfect,
    Great,
    Good,
    Miss
}

public static class JudgeService
{
    public static JudgeResult Evaluate(int deltaMs, JudgementConfig config)
    {
        int absDelta = Mathf.Abs(deltaMs);

        if (absDelta <= config.perfectMs)
        {
            return JudgeResult.Perfect;
        }
        else if (absDelta <= config.greatMs)
        {
            return JudgeResult.Great;
        }
        else if (absDelta <= config.goodMs)
        {
            return JudgeResult.Good;
        } 
        else
        {
            return JudgeResult.Miss;
        }
    }
}
