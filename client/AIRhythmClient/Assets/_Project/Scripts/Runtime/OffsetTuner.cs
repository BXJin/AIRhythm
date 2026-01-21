using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class OffsetTuner : MonoBehaviour
{
    [SerializeField] private SongConductor conductor;
    [SerializeField] private TMP_Text offsetText;
    [SerializeField] private int stepMs = 10;

    private GameInput _input;
    private const string PrefKey = "rhythm_offset_ms";

    private void Awake()
    {
        _input = new GameInput();
    }

    private void OnEnable()
    {
        _input.GamePlay.Enable();
        _input.GamePlay.OffsetDown.performed += OnDown;
        _input.GamePlay.OffsetUp.performed += OnUp;
        _input.GamePlay.OffsetReset.performed += OnReset;
    }

    private void OnDisable()
    {
        _input.GamePlay.OffsetDown.performed -= OnDown;
        _input.GamePlay.OffsetUp.performed -= OnUp;
        _input.GamePlay.OffsetReset.performed -= OnReset;
        _input.GamePlay.Disable();
    }

    private void Start()
    {
        if (conductor == null) return;
        int saved = PlayerPrefs.GetInt(PrefKey, conductor.RuntimeOffsetMs);
        conductor.SetOffsetMs(saved);
        RefreshText();
    }

    private void OnDown(InputAction.CallbackContext ctx) => Add(-stepMs);
    private void OnUp(InputAction.CallbackContext ctx) => Add(+stepMs);
    private void OnReset(InputAction.CallbackContext ctx) => Set(0);

    private void Add(int delta) => Set(conductor.RuntimeOffsetMs + delta);

    private void Set(int ms)
    {
        conductor.SetOffsetMs(ms);
        PlayerPrefs.SetInt(PrefKey, ms);
        PlayerPrefs.Save();
        RefreshText();
    }

    private void RefreshText()
    {
        if (offsetText != null)
            offsetText.text = $"Offset: {conductor.RuntimeOffsetMs}";
    }
}
