using UnityEngine;

public class NoteSchedulerDebug : MonoBehaviour
{
    [SerializeField] 
    private SongConductor conductor;

    [SerializeField] 
    private ChartLoader chartLoader;

    private int _nextIndex = 0;

    private void Update()
    {
        if (conductor == null || chartLoader == null) return;
        if (!conductor.IsStarted) return;
        if (chartLoader.Loaded == null || chartLoader.Loaded.notes == null) return;

        var notes = chartLoader.Loaded.notes;
        if (_nextIndex >= notes.Length) return;

        int now = conductor.NowSongTimeMs;
        int nextT = notes[_nextIndex].t_ms;
        int delta = nextT - now;

        // 디버그 출력(너무 많이 찍히면 보기 힘드니 200ms 단위로만 찍기)
        if (delta <= 200 && delta >= -200)
        {
            Debug.Log($"nextIndex={_nextIndex}, now={now}ms, next={nextT}ms, delta={delta}ms");
        }

        // 다음 노트 시간이 지나면 인덱스 전진(되돌아가지 않게)
        // (판정은 아직 안 함. 지금은 스케줄러 뼈대만.)
        if (now > nextT + 200)
        {
            _nextIndex++;
        }
    }
}
