using TMPro;
using UnityEngine;
using UnityEngine.UI; 

public class NoteScheduler : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SongConductor conductor;
    [SerializeField] private ChartLoader chartLoader;

    [Header("UI")]
    [SerializeField] private TMP_Text nextNoteText;

    [Header("Spawn Rule")]
    [Tooltip("노트가 t_ms에 눌러야 한다면, 그보다 몇 ms 전에 '등장(준비)' 상태로 볼지")]
    [SerializeField] private int spawnLeadTimeMs = 1000;

    private int _nextIndex = 0;

    private void Start()
    {
        if (nextNoteText != null)
            nextNoteText.text = "Ready";
    }

    private void Update()
    {
        if (conductor == null || chartLoader == null || nextNoteText == null) return;
        if (!conductor.IsStarted) return;

        var chart = chartLoader.Loaded;
        if (chart == null || chart.notes == null || chart.notes.Length == 0)
        {
            nextNoteText.text = "No chart notes";
            return;
        }

        // 모든 노트를 다 소비했으면 끝
        if (_nextIndex >= chart.notes.Length)
        {
            nextNoteText.text = "Done";
            return;
        }

        int now = conductor.NowSongTimeMs;
        int t = chart.notes[_nextIndex].t_ms;

        // delta: "다음 노트까지 남은 시간"
        int delta = t - now;

        // 1) 스폰 리드타임 안에 들어오면 "노트가 준비됨" 표시
        if (delta <= spawnLeadTimeMs)
        {
            nextNoteText.text = $"NOTE IN {delta}ms";
        }
        else
        {
            nextNoteText.text = $"Next in {delta}ms";
        }

        // 2) 노트 시각이 한참 지나면(예: 200ms) 자동으로 다음 노트로 넘김
        // (지금은 판정 단계가 아니므로, "놓쳤다"를 단순히 넘기는 처리)
        if (now > t + 200)
        {
            _nextIndex++;
        }
    }
}
