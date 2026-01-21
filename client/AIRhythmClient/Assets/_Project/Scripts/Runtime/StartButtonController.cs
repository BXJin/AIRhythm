using UnityEngine;
using TMPro;

public class StartButtonController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ChartLoader chartLoader;
    [SerializeField] private SongConductor conductor;
    [SerializeField] private NoteCursor cursor;
    [SerializeField] private PlaySessionController playSession;
    
    [Header("UI")]
    [SerializeField] private TMP_Dropdown chartDropdown;
    [SerializeField] private UIStateManager uiStateManager; //추가

    public void OnClickStart()
    {
        // 0) Dropdown 유효성
        if (chartDropdown == null || chartDropdown.options.Count == 0)
        {
            Debug.LogError("[StartButtonController] Dropdown is empty.");
            return;
        }

        // 1) 선택된 파일명 가져오기
        string fileName = chartDropdown.options[chartDropdown.value].text;

        // 2) 차트 로드
        if (chartLoader == null || !chartLoader.LoadByFileName(fileName))
        {
            Debug.LogError($"[StartButtonController] Failed to load chart: {fileName}");
            return;
        }

        // 3) 준비 단계
        if (playSession != null)
        {
            playSession.ApplyChartAndPrepare();
        }
        else
        {
            var chart = chartLoader.Loaded;
            if (chart == null)
            {
                Debug.LogError("[StartButtonController] Loaded chart is null.");
                return;
            }

            if (cursor != null) cursor.ResetCursor();
            if (conductor != null) conductor.SetChartOffsetMs(chart.audio.audio_offset_ms);
        }

        // 4) UI 전환 
        if (uiStateManager != null) 
            uiStateManager.ShowPlaying();

        // 5) 시작
        if (conductor != null) 
            conductor.StartSong();
    }
}
