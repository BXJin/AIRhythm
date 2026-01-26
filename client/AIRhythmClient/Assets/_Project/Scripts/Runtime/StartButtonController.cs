using UnityEngine;
using TMPro;

public class StartButtonController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ChartLoader chartLoader;
    [SerializeField] private AudioLoader audioLoader;
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
        string entry = chartDropdown.options[chartDropdown.value].text;
        if (!chartLoader.LoadByEntry(entry)) return;

        var chart = chartLoader.Loaded;
        if (chart == null) return;

        // (1) 오디오 먼저 로드
        audioLoader.LoadForChart(
            chart,
            onOk: clip =>
            {
                // (2) 준비(오프셋/커서/스탯 등)
                conductor.SetClip(clip);
                if (cursor != null) cursor.ResetCursor();
                conductor.SetChartOffsetMs(chart.audio.audio_offset_ms);

                playSession?.ApplyChartAndPrepare();

                // (3) UI 상태 변경(너 UIStateManager 쓰면)
                uiStateManager.ShowPlaying();

                // (4) 시작
                conductor.StartSong();
            },
            onFail: err =>
            {
                Debug.LogError(err);
                // 실패 시 UI를 ChartSelect로 되돌리고 싶으면 여기서 state 변경
                uiStateManager?.SetState(UIState.ChartSelect);
            }
        );
    }
}
