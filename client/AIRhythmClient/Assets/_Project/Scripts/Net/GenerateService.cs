using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GenerateService : MonoBehaviour
{
    private string _baseUrl;

    [Header("Polling")]
    [SerializeField] private float pollIntervalSec = 0.8f;

    [Header("Preview")]
    [Tooltip("서버 오디오 기본 타입. mp3면 MPEG, wav면 WAV, ogg면 OGGVORBIS")]
    [SerializeField] private AudioType previewAudioType = AudioType.WAV;

    private void Awake()
    {
        var settings = ApiSettingsProvider.Load();
        _baseUrl = settings.baseUrl;
        Debug.Log($"[GenerateService] baseUrl={_baseUrl}");
    }

    public void StartGenerate(
        string prompt,
        string genre,
        int durationSec,
        Action<string> onJobStarted,
        Action<GenerateStatusResponseDto> onProgress,
        Action<string> onDoneChartId,
        Action<string> onFail)
    {
        StartCoroutine(StartGenerateCo(prompt, genre, durationSec, onJobStarted, onProgress, onDoneChartId, onFail));
    }

    private System.Collections.IEnumerator StartGenerateCo(
        string prompt,
        string genre,
        int durationSec,
        Action<string> onJobStarted,
        Action<GenerateStatusResponseDto> onProgress,
        Action<string> onDoneChartId,
        Action<string> onFail)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            onFail?.Invoke("[Generate] prompt is empty.");
            yield break;
        }

        var reqDto = new GenerateRequestDto
        {
            prompt = prompt,
            genre = genre,
            durationSec = durationSec <= 0 ? 15 : durationSec
        };

        string json = JsonUtility.ToJson(reqDto);
        byte[] body = Encoding.UTF8.GetBytes(json);

        string url = $"{_baseUrl}/api/generate";
        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onFail?.Invoke($"[Generate] POST failed: {req.error}\n{url}");
            yield break;
        }

        var startRes = JsonUtility.FromJson<GenerateStartResponseDto>(req.downloadHandler.text);
        if (startRes == null || string.IsNullOrWhiteSpace(startRes.jobId))
        {
            onFail?.Invoke($"[Generate] Invalid start response: {req.downloadHandler.text}");
            yield break;
        }

        onJobStarted?.Invoke(startRes.jobId);

        // poll status until done/error
        yield return PollStatusCo(startRes.jobId, onProgress, onDoneChartId, onFail);
    }

    private System.Collections.IEnumerator PollStatusCo(
        string jobId,
        Action<GenerateStatusResponseDto> onProgress,
        Action<string> onDoneChartId,
        Action<string> onFail)
    {
        string url = $"{_baseUrl}/api/generate/{jobId}";

        while (true)
        {
            using var req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onFail?.Invoke($"[Generate] Poll failed: {req.error}\n{url}");
                yield break;
            }

            var status = JsonUtility.FromJson<GenerateStatusResponseDto>(req.downloadHandler.text);
            if (status == null || string.IsNullOrWhiteSpace(status.status))
            {
                onFail?.Invoke($"[Generate] Invalid status response: {req.downloadHandler.text}");
                yield break;
            }

            onProgress?.Invoke(status);

            if (status.status == "done")
            {
                if (string.IsNullOrWhiteSpace(status.chartId))
                {
                    onFail?.Invoke("[Generate] done but chartId missing.");
                    yield break;
                }
                onDoneChartId?.Invoke(status.chartId);
                yield break;
            }

            if (status.status == "error")
            {
                onFail?.Invoke($"[Generate] error: {status.message}");
                yield break;
            }

            yield return new WaitForSeconds(pollIntervalSec);
        }
    }

    /// <summary>서버의 /api/charts/{chartId}/audio 를 임시로 받아서 미리듣기</summary>
    public void LoadPreviewClip(string chartId, Action<AudioClip> onOk, Action<string> onFail)
    {
        StartCoroutine(LoadPreviewClipCo(chartId, onOk, onFail));
    }

    private System.Collections.IEnumerator LoadPreviewClipCo(string chartId, Action<AudioClip> onOk, Action<string> onFail)
    {
        if (string.IsNullOrWhiteSpace(chartId))
        {
            onFail?.Invoke("[Preview] chartId is empty.");
            yield break;
        }

        string url = $"{_baseUrl}/api/charts/{chartId}/audio";
        using var req = UnityWebRequestMultimedia.GetAudioClip(url, previewAudioType);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onFail?.Invoke($"[Preview] Load audio failed: {req.error}\n{url}");
            yield break;
        }

        var clip = DownloadHandlerAudioClip.GetContent(req);
        if (clip == null)
        {
            onFail?.Invoke("[Preview] clip is null.");
            yield break;
        }

        onOk?.Invoke(clip);
    }
}
