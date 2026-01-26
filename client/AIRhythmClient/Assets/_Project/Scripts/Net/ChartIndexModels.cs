using System;

[Serializable]
public class ChartIndexItem
{
    public string id;
    public string title;
    public string chartFile; // ¿¹: "abc123.json"
    public string audioFile; // ¿¹: "abc123.mp3"
}

[Serializable]
public class ChartIndexResponse
{
    public ChartIndexItem[] items;
}
