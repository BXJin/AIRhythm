using UnityEngine;

public class GrowingCircleNoteView : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SongConductor conductor;
    [SerializeField] private ChartLoader chartLoader;

    [Header("UI")]
    [SerializeField] private RectTransform bigCircle;     // 큰 원(고정)
    [SerializeField] private RectTransform smallCircle;   // 작은 원(커지는 원)

    [Header("Timing")]
    [Tooltip("노트 시간(t_ms)보다 몇 ms 전부터 작은 원을 보여줄지")]
    [SerializeField] private int spawnLeadTimeMs = 1000;

    [Header("Scale")]
    [Tooltip("등장 시점(리드타임 시작)의 작은 원 크기 배율")]
    [SerializeField] private float startScale = 0.2f;

    [Tooltip("정타(0ms)일 때 작은 원 크기 배율(큰 원과 같게 1.0)")]
    [SerializeField] private float endScale = 1.0f;

    [SerializeField] private NoteCursor cursor;

    private void Start()
    {
        if (bigCircle != null) bigCircle.gameObject.SetActive(true);
        if (smallCircle != null) smallCircle.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (conductor == null || chartLoader == null) return;
        if (!conductor.IsStarted) return;

        var chart = chartLoader.Loaded;
        if (chart == null || chart.notes == null || chart.notes.Length == 0) return;

        if (cursor.NextIndex >= chart.notes.Length)
        {
            if (smallCircle != null) smallCircle.gameObject.SetActive(false);
            return;
        }

        int now = conductor.NowSongTimeMs;
        int t = chart.notes[cursor.NextIndex].t_ms;
        int delta = t - now;

        // 아직 멀면 숨김
        if (delta > spawnLeadTimeMs)
        {
            if (smallCircle != null) smallCircle.gameObject.SetActive(false);
            return;
        }

        // 리드타임 안에 들어오면 보이기
        if (smallCircle != null && !smallCircle.gameObject.activeSelf)
            smallCircle.gameObject.SetActive(true);

        // progress01: 1 -> 0 (등장 -> 정타)
        float progress01 = Mathf.Clamp01(delta / (float)spawnLeadTimeMs);

        // 작은 원 scale: startScale -> endScale
        // 등장(1)일 때 startScale, 정타(0)일 때 endScale
        float scale = Mathf.Lerp(endScale, startScale, progress01);

        if (smallCircle != null)
            smallCircle.localScale = Vector3.one * scale;
    }
}
