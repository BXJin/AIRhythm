using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class HitJudgeController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SongConductor conductor;
    [SerializeField] private ChartLoader chartLoader;
    [SerializeField] private NoteCursor cursor;

    [Header("Config")]
    [SerializeField] private JudgementConfig judgementConfig;

    [Header("UI")]
    [SerializeField] private TMP_Text judgementText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text accText;

    [SerializeField] private ResultPanelController resultPanel;
    [SerializeField] private UIStateManager uiStateManager;

    private PlayStats _stats = new PlayStats();


    // Input System (Generated)
    private GameInput _input;

    private void Awake()
    {
        _input = new GameInput();
    }

    private void OnEnable()
    {
        _input.GamePlay.Enable();

        // Hit 액션이 "실제로 눌렸을 때" 콜백
        _input.GamePlay.Hit.performed += OnHitPerformed;
    }

    private void OnDisable()
    {
        _input.GamePlay.Hit.performed -= OnHitPerformed;
        _input.GamePlay.Disable();
    }

    private void Update()
    {
        if (conductor == null || chartLoader == null || judgementConfig == null) return;
        if (!conductor.IsStarted) return;

        var chart = chartLoader.Loaded;
        if (chart == null || chart.notes == null || chart.notes.Length == 0) return;
        if (cursor.NextIndex >= chart.notes.Length)
        {
            if (uiStateManager != null)
                uiStateManager.ShowResult();

            // ResultPanelController는 이제 데이터만 업데이트
            if (resultPanel != null)
                resultPanel.UpdateResult(_stats.Score, _stats.Accuracy01, _stats.MaxCombo);

            enabled = false; // 중복 호출 방지(간단)
            return;
        }

        int now = conductor.NowSongTimeMs;
        int t = chart.notes[cursor.NextIndex].t_ms;

        // 자동 Miss 처리(너무 지나가면 Miss로 소비)
        if (now > t + judgementConfig.missAfterMs)
        {
            Show(JudgeResult.Miss, now - t);
            _stats.Apply(JudgeResult.Miss);
            RefreshHud();
            cursor.Advance();
        }

        //if (judgementText != null)
        //{
        //    int now2 = conductor.NowSongTimeMs;
        //    int t2 =chartLoader.Loaded.notes[cursor.NextIndex].t_ms;
        //    judgementText.text = $"Next idx = {cursor.NextIndex}, delta={t2-now2}ms";
        //}
    }

    private void OnHitPerformed(InputAction.CallbackContext ctx)
    {
        if (conductor == null || chartLoader == null || judgementConfig == null) return;
        if (!conductor.IsStarted) return;

        var chart = chartLoader.Loaded;
        if (chart == null || chart.notes == null || chart.notes.Length == 0) return;
        if (cursor.NextIndex >= chart.notes.Length) return;

        _stats.MarkPlayerPressed();

        int now = conductor.NowSongTimeMs;
        int t = chart.notes[cursor.NextIndex].t_ms;

        int delta = t - now;
        var result = JudgeService.Evaluate(delta, judgementConfig);

        Show(result, delta);
        _stats.Apply(result);
        RefreshHud();

        // 1Key MVP: 누르면 무조건 다음 노트 소비
        cursor.Advance();
    }

    private void Show(JudgeResult result, int delta)
    {
        string msg = $"{result} (delta {delta}ms)";
        Debug.Log(msg);
        if (judgementText != null) judgementText.text = msg;
    }

    private void RefreshHud()
    {
        if (comboText != null) comboText.text = $"Combo: {_stats.Combo}";
        if (scoreText != null) scoreText.text = $"Score: {_stats.Score}";
        if (accText != null) accText.text = $"Acc: {(_stats.Accuracy01 * 100f):0.0}%";
    }

}
