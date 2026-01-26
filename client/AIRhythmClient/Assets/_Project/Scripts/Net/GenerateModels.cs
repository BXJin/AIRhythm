using System;

[Serializable]
public class GenerateRequestDto
{
    public string prompt;
    public string genre;
    public int durationSec; // MVP: optional but useful
}

[Serializable]
public class GenerateStartResponseDto
{
    public string jobId;
}

[Serializable]
public class GenerateStatusResponseDto
{
    public string status;   // "queued" | "running" | "done" | "error"
    public float progress;  // 0~1
    public string chartId;  // done일 때 채워짐
    public string message;  // error일 때
}
