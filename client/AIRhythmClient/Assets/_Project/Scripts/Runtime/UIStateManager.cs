using UnityEngine;

public enum UIState
{
    None,
    ChartSelect,    // 곡 선택 화면
    Playing,        // 게임 중 (HUD 표시)
    Paused,         // 일시정지
    Result          // 결과 화면
}

public class UIStateManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject selectPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pauseOverlay;
    [SerializeField] private GameObject resultPanel;

    private UIState _currentState = UIState.None;

    private void Start()
    {
        // 초기 상태: 곡 선택 화면
        SetState(UIState.ChartSelect);
    }

    public void SetState(UIState newState)
    {
        if (_currentState == newState) return;

        _currentState = newState;

        // 모든 패널 일단 끄기
        if (selectPanel != null) selectPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (pauseOverlay != null) pauseOverlay.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);

        // 상태별로 필요한 패널만 켜기
        switch (newState)
        {
            case UIState.ChartSelect:
                if (selectPanel != null) selectPanel.SetActive(true);
                Time.timeScale = 1f;
                AudioListener.pause = false;
                break;

            case UIState.Playing:
                if (hudPanel != null) hudPanel.SetActive(true);
                Time.timeScale = 1f;
                AudioListener.pause = false;
                break;

            case UIState.Paused:
                if (hudPanel != null) hudPanel.SetActive(true);
                if (pauseOverlay != null) pauseOverlay.SetActive(true);
                Time.timeScale = 0f;
                AudioListener.pause = true;
                break;

            case UIState.Result:
                if (hudPanel != null) hudPanel.SetActive(true);
                if (resultPanel != null) resultPanel.SetActive(true);
                Time.timeScale = 0f;
                AudioListener.pause = true;
                break;
        }

        Debug.Log($"[UIStateManager] State changed to: {newState}");
    }

    public UIState CurrentState => _currentState;

    // 편의 메서드들
    public void ShowChartSelect() => SetState(UIState.ChartSelect);
    public void ShowPlaying() => SetState(UIState.Playing);
    public void ShowPaused() => SetState(UIState.Paused);
    public void ShowResult() => SetState(UIState.Result);
}
