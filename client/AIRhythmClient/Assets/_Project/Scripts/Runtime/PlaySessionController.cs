using UnityEngine;

public class PlaySessionController : MonoBehaviour
{
    [SerializeField] private ChartLoader chartLoader;
    [SerializeField] private SongConductor conductor;
    [SerializeField] private NoteCursor cursor;

    [Header("Optional")]
    [SerializeField] private ResultPanelController resultPanel;

    public void ApplyChartAndPrepare()
    {
        var chart = chartLoader.Loaded;
        if (chart == null) return;

        resultPanel?.Hide();
        cursor?.ResetCursor();
        conductor?.SetChartOffsetMs(chart.audio.audio_offset_ms);

        // 여기서는 StartSong 하지 말 것 (A안)
    }
}
