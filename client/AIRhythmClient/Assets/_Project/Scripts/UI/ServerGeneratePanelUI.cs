using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerGeneratePanelUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GenerateService generateService;
    [SerializeField] private DownloadChartService downloadService;
    [SerializeField] private ChartSelectUI localChartSelectUi; // 다운로드 후 로컬 Dropdown refresh

    [Header("UI")]
    [SerializeField] private TMP_InputField promptInput;
    [SerializeField] private TMP_Dropdown genreDropdown;
    [SerializeField] private TMP_InputField durationInput; // "15" 같은 숫자 입력(선택)

    [SerializeField] private Button generateButton;
    [SerializeField] private Button previewButton;
    [SerializeField] private Button downloadButton;

    [SerializeField] private TMP_Text statusText;

    [Header("Preview Audio")]
    [SerializeField] private AudioSource previewSource;

    private string _lastChartId;
    private bool _isBusy;

    private void Start()
    {
        SetIdleUi();
        SetStatus("Ready.");
    }

    public void OnClickGenerate()
    {
        if (_isBusy) return;

        string prompt = promptInput != null ? promptInput.text : "";
        string genre = GetSelectedGenre();
        int duration = ParseDurationSec();

        SetBusyUi(true);
        SetStatus("Generating...");

        generateService.StartGenerate(
            prompt, genre, duration,
            onJobStarted: jobId =>
            {
                SetStatus($"Job started: {jobId}");
            },
            onProgress: st =>
            {
                // running/queued 상태 표시
                if (st.status == "running" || st.status == "queued")
                    SetStatus($"{st.status} {Mathf.RoundToInt(st.progress * 100f)}%");
            },
            onDoneChartId: chartId =>
            {
                _lastChartId = chartId;
                SetStatus($"Done! chartId={chartId}");
                SetBusyUi(false);

                // 완료되면 미리듣기/다운로드 활성화
                if (previewButton != null) previewButton.interactable = true;
                if (downloadButton != null) downloadButton.interactable = true;
            },
            onFail: err =>
            {
                Debug.LogError(err);
                SetStatus("Error. Check Console.");
                SetBusyUi(false);
            }
        );
    }

    public void OnClickPreview()
    {
        if (string.IsNullOrWhiteSpace(_lastChartId)) return;

        SetStatus("Loading preview...");
        generateService.LoadPreviewClip(
            _lastChartId,
            onOk: clip =>
            {
                if (previewSource == null)
                {
                    SetStatus("PreviewSource missing.");
                    return;
                }

                previewSource.Stop();
                previewSource.clip = clip;
                previewSource.Play();

                SetStatus("Preview playing.");
            },
            onFail: err =>
            {
                Debug.LogError(err);
                SetStatus("Preview failed. Check Console.");
            }
        );
    }

    public void OnClickDownload()
    {
        if (_isBusy) return;
        if (string.IsNullOrWhiteSpace(_lastChartId)) return;

        SetBusyUi(true);
        SetStatus("Downloading...");

        downloadService.DownloadByChartId(
            _lastChartId,
            onOk: () =>
            {
                SetStatus("Download OK. Refreshing local list...");
                localChartSelectUi.RefreshDropdown();
                SetStatus("Ready to Play (Select from local dropdown).");
                SetBusyUi(false);
            },
            onFail: err =>
            {
                Debug.LogError(err);
                SetStatus("Download failed. Check Console.");
                SetBusyUi(false);
            }
        );
    }

    private string GetSelectedGenre()
    {
        if (genreDropdown == null || genreDropdown.options.Count == 0) return "default";
        return genreDropdown.options[genreDropdown.value].text;
    }

    private int ParseDurationSec()
    {
        if (durationInput == null) return 15;
        if (int.TryParse(durationInput.text, out int sec) && sec > 0) return sec;
        return 15;
    }

    private void SetBusyUi(bool busy)
    {
        _isBusy = busy;

        if (generateButton != null) generateButton.interactable = !busy;
        if (previewButton != null) previewButton.interactable = !busy && !string.IsNullOrWhiteSpace(_lastChartId);
        if (downloadButton != null) downloadButton.interactable = !busy && !string.IsNullOrWhiteSpace(_lastChartId);
    }

    private void SetIdleUi()
    {
        _lastChartId = null;
        if (previewButton != null) previewButton.interactable = false;
        if (downloadButton != null) downloadButton.interactable = false;
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log($"[ServerGeneratePanelUI] {msg}");
    }
}
