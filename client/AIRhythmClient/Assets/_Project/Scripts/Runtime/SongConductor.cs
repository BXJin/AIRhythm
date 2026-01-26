using UnityEngine;

/// <summary>
/// SongConductor = 리듬게임의 "시계(Clock)" 담당.
/// - 오디오 엔진 시간(AudioSettings.dspTime)을 기준으로 현재 음악 시간을 계산.
/// - AudioSource.PlayScheduled로 "정해진 시각"에 재생을 시작해 싱크를 안정화.
/// </summary>
public class SongConductor : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] 
    private AudioSource musicSource;

    [Header("Sync (Chart/Device offset)")]
    [Tooltip("차트 기준 보정값(ms). +면 노트 기준을 늦추고, -면 빠르게 함.")]
    [SerializeField] 
    private int audioOffsetMs = 0;

    [SerializeField] private int runtimeOffsetMs = 0;
    public int RuntimeOffsetMs => runtimeOffsetMs;

    [Header("Start Scheduling")]
    [Tooltip("PlayScheduled를 예약할 때, 현재 시간에서 몇 초 뒤에 시작할지(여유 시간).")]
    [SerializeField] 
    private double startLeadSeconds = 0.10;

    private double _startDspTime;     // 음악 재생이 "시작되는" dspTime
    private bool _scheduled;
    private bool _started;

    private bool _paused;
    private double _pauseDspTime;

    /// <summary>현재 음악 시간(ms). (0부터 시작, offset 포함)</summary>
    public int NowSongTimeMs
    {
        get
        {
            if (!IsStarted) return 0;
            double elapsedSec = AudioSettings.dspTime - _startDspTime;
            if (elapsedSec < 0) elapsedSec = 0;

            int baseMs = (int)(elapsedSec * 1000.0);
            int ms = baseMs + audioOffsetMs + runtimeOffsetMs;
            return Mathf.Max(0, ms);
        }
    }

    /// <summary>음악이 실제로 시작되었는지</summary>
    public bool IsStarted => _started;

    private void Awake()
    {
        if (musicSource == null)
            Debug.LogError("[SongConductor] musicSource is not assigned.");
    }

    private void Update()
    {
        // PlayScheduled로 예약했더라도, '실제로 시작했는지'는 dspTime 기준으로 판단하는 게 가장 확실함.
        if (_scheduled && !_started && AudioSettings.dspTime >= _startDspTime)
            _started = true;
    }

    public void SetClip(AudioClip clip)
    {
        if (musicSource == null)
        {
            Debug.LogError("[SongConductor] musicSource is missing.");
            return;
        }
        musicSource.clip = clip;
    }

    public AudioClip CurrentClip => musicSource != null ? musicSource.clip : null;

    /// <summary>
    /// 음악을 dspTime 기준으로 예약 재생한다.
    /// Phase 1에서는 "클립 1개를 한 번 재생"하는 MVP 용도.
    /// </summary>
    public void StartSong()
    {
        if (musicSource == null || musicSource.clip == null)
        {
            Debug.LogError("[SongConductor] AudioSource or AudioClip is missing.");
            return;
        }

        // 항상 처음부터 시작
        musicSource.Stop();
        musicSource.time = 0f;

        _startDspTime = AudioSettings.dspTime + startLeadSeconds;
        musicSource.PlayScheduled(_startDspTime);

        _scheduled = true;
        _started = false;

        _paused = false;
        _pauseDspTime = 0;
    }

    /// <summary>재시작(개발 중 반복 테스트용)</summary>
    public void RestartSong() => StartSong();

    /// <summary>정지</summary>
    public void StopSong()
    {
        if (musicSource != null) musicSource.Stop();
        _scheduled = false;
        _started = false;
        _startDspTime = 0;

        _paused = false;
        _pauseDspTime = 0;
    }

    public void PauseSong()
    {
        if (_paused || !_started) return;
        // 현재 dspTime 저장
        _pauseDspTime = AudioSettings.dspTime;
        _paused = true;

        if(musicSource != null) 
            musicSource.Pause();
    }

    public void ResumeSong()
    {
        if (!_paused) return;
        // 멈춘 시간만큼 시작 시간을 뒤로 미룸
        double pausedDuration = AudioSettings.dspTime - _pauseDspTime;
        _startDspTime += pausedDuration;
        _paused = false;
        //_pauseDspTime = 0;
        if(musicSource != null) 
            musicSource.UnPause();
    }

    public void AddOffsetMs(int deltaMs) => runtimeOffsetMs += deltaMs;

    public int GetOffsetMs() => runtimeOffsetMs;

    public void SetOffsetMs(int ms) => runtimeOffsetMs = ms;

    public void SetChartOffsetMs(int ms) => audioOffsetMs = ms;
}
