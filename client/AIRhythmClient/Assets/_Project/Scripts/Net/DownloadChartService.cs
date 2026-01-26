using ChartModels;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadChartService : MonoBehaviour
{
    private string ChartsDir => Path.Combine(Application.persistentDataPath, "Charts");
    private string AudioDir => Path.Combine(Application.persistentDataPath, "Audio");

    private string _baseUrl;

    private void Awake()
    {
        var settings = ApiSettingsProvider.Load();
        _baseUrl = settings.baseUrl;
        Debug.Log($"[DownloadChartService] baseUrl={_baseUrl}");
    }

    private void EnsureDirs()
    {
        Directory.CreateDirectory(ChartsDir);
        Directory.CreateDirectory(AudioDir);
    }

    public void FetchIndex(Action<ChartIndexItem[]> onOk, Action<string> onFail)
    {
        StartCoroutine(FetchIndexCo(onOk, onFail));
    }

    private System.Collections.IEnumerator FetchIndexCo(Action<ChartIndexItem[]> onOk, Action<string> onFail)
    {
        EnsureDirs();

        string url = $"{_baseUrl}/api/charts";
        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onFail?.Invoke($"[FetchIndex] Failed: {req.error}\n{url}");
            yield break;
        }

        // 서버가 {"items":[...]} 형태로 주는 전제
        var json = req.downloadHandler.text;
        var res = JsonUtility.FromJson<ChartIndexResponse>(json);

        if (res == null || res.items == null)
        {
            onFail?.Invoke($"[FetchIndex] Invalid JSON: {json}");
            yield break;
        }

        onOk?.Invoke(res.items);
    }

    public void DownloadChartAndAudio(
        ChartIndexItem item,
        Action onOk,
        Action<string> onFail)
    {
        StartCoroutine(DownloadCo(item, onOk, onFail));
    }

    private System.Collections.IEnumerator DownloadCo(ChartIndexItem item, Action onOk, Action<string> onFail)
    {
        EnsureDirs();

        if (item == null || string.IsNullOrWhiteSpace(item.id))
        {
            onFail?.Invoke("[Download] item is null or id is empty.");
            yield break;
        }

        // 1) chart json
        string chartUrl = $"{_baseUrl}/api/charts/{item.id}/chart";
        using (var req = UnityWebRequest.Get(chartUrl))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                onFail?.Invoke($"[Download Chart] Failed: {req.error}\n{chartUrl}");
                yield break;
            }

            string chartText = req.downloadHandler.text;

            // 저장 파일명: 서버 제공 chartFile 사용(없으면 id.json)
            string chartName = string.IsNullOrWhiteSpace(item.chartFile) ? $"{item.id}.json" : item.chartFile;
            string chartPath = Path.Combine(ChartsDir, chartName);

            File.WriteAllText(chartPath, chartText);
            Debug.Log($"[Download] Chart saved: {chartPath}");
        }

        // 2) audio binary
        string audioUrl = $"{_baseUrl}/api/charts/{item.id}/audio";
        using (var req = UnityWebRequest.Get(audioUrl))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                onFail?.Invoke($"[Download Audio] Failed: {req.error}\n{audioUrl}");
                yield break;
            }

            byte[] bytes = req.downloadHandler.data;

            // 저장 파일명: 서버 제공 audioFile 사용(없으면 id.mp3)
            string audioName = string.IsNullOrWhiteSpace(item.audioFile) ? $"{item.id}.mp3" : item.audioFile;
            string audioPath = Path.Combine(AudioDir, audioName);

            File.WriteAllBytes(audioPath, bytes);
            Debug.Log($"[Download] Audio saved: {audioPath}");
        }

        onOk?.Invoke();
    }

    /// <summary>
    /// chartId만으로 차트(JSON)와 오디오를 내려받아 persistent에 저장.
    /// - 차트를 먼저 받아서 JSON에서 audio.file을 파싱한 다음, 오디오 파일명을 그에 맞춰 저장한다.
    /// </summary>
    public void DownloadByChartId(string chartId, Action onOk, Action<string> onFail)
    {
        StartCoroutine(DownloadByChartIdCo(chartId, onOk, onFail));
    }

    private System.Collections.IEnumerator DownloadByChartIdCo(string chartId, Action onOk, Action<string> onFail)
    {
        EnsureDirs();

        if (string.IsNullOrWhiteSpace(chartId))
        {
            onFail?.Invoke("[DownloadByChartId] chartId is empty.");
            yield break;
        }

        // 1) chart json
        string chartUrl = $"{_baseUrl}/api/charts/{chartId}/chart";
        string chartText;

        using (var req = UnityWebRequest.Get(chartUrl))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                onFail?.Invoke($"[Download Chart] Failed: {req.error}\n{chartUrl}");
                yield break;
            }
            chartText = req.downloadHandler.text;
        }

        // JSON 파싱해서 audio.file 얻기
        var dto = JsonUtility.FromJson<ChartDto>(chartText);
        if (dto == null || dto.audio == null || string.IsNullOrWhiteSpace(dto.audio.file))
        {
            onFail?.Invoke("[DownloadByChartId] invalid chart json (audio.file missing).");
            yield break;
        }

        // 저장: 차트 파일명은 chartId.json(단순)
        string chartPath = Path.Combine(ChartsDir, $"{chartId}.json");
        File.WriteAllText(chartPath, chartText);
        Debug.Log($"[Download] Chart saved: {chartPath}");

        // 2) audio binary
        string audioUrl = $"{_baseUrl}/api/charts/{chartId}/audio";
        byte[] audioBytes;

        using (var req = UnityWebRequest.Get(audioUrl))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                onFail?.Invoke($"[Download Audio] Failed: {req.error}\n{audioUrl}");
                yield break;
            }
            audioBytes = req.downloadHandler.data;
        }

        // audio.file 예: "Audio/abc123.mp3" → 파일명만 추출해서 저장
        string audioFileName = Path.GetFileName(dto.audio.file);
        string audioPath = Path.Combine(AudioDir, audioFileName);

        File.WriteAllBytes(audioPath, audioBytes);
        Debug.Log($"[Download] Audio saved: {audioPath}");

        onOk?.Invoke();
    }
}
