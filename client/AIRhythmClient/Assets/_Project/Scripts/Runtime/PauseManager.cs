using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private UIStateManager uiStateManager; // 변경
    [SerializeField] private SongConductor conductor;
    [SerializeField] private PlaySessionController playSession;

    private GameInput _input;

    private void Awake() => _input = new GameInput();

    private void OnEnable()
    {
        _input.GamePlay.Enable();
        _input.GamePlay.Pause.performed += OnPausePerformed;
        //_input.GamePlay.Restart.performed += OnRestart;
    }

    private void OnDisable()
    {
        _input.GamePlay.Pause.performed -= OnPausePerformed;
        //_input.GamePlay.Restart.performed -= OnRestart;
        _input.GamePlay.Disable();
    }
    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        if (uiStateManager == null || conductor == null) return;
        if (!conductor.IsStarted) return; // 곡 시작 전이면 pause 무시

        if (uiStateManager.CurrentState == UIState.Playing)
        {
            conductor.PauseSong();
            uiStateManager.SetState(UIState.Paused);
        }
        else if (uiStateManager.CurrentState == UIState.Paused)
        {
            conductor.ResumeSong();
            uiStateManager.SetState(UIState.Playing);
        }
    }

    // === PauseOverlay 버튼에서 호출 ===

    // Resume 버튼
    public void OnClickResume()
    {
        if (uiStateManager == null || conductor == null) return;

        conductor.ResumeSong();
        uiStateManager.SetState(UIState.Playing);
    }

    // Restart 버튼 (곡 처음부터)
    public void OnClickRestart()
    {
        if (uiStateManager == null || conductor == null) return;

        // UI는 Playing으로 복귀
        uiStateManager.SetState(UIState.Playing);

        // 커서/점수/판정 초기화는 playSession에 몰아두는 게 깔끔
        if (playSession != null)
            playSession.ApplyChartAndPrepare();

        conductor.RestartSong();
    }
}
