using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using ChartModels;

/// <summary>
/// AudioLoader = 차트 JSON의 audio.file(예: "Audio/xxx.mp3")를 보고
/// persistentDataPath/Audio 또는 StreamingAssets/Audio에서 AudioClip을 로드한다.
/// - 파일 로드는 UnityWebRequestMultimedia.GetAudioClip(file://...) 방식이 가장 단순/호환이 좋다.
/// </summary>
public class AudioLoader : MonoBehaviour
{
    [Header("Folders")]
    [SerializeField] private string audioSubfolder = "Audio"; // Audio

    public void LoadForChart(ChartDto chart, Action<AudioClip> onOk, Action<string> onFail)
    {
        if (chart == null || chart.audio == null || string.IsNullOrWhiteSpace(chart.audio.file))
        {
            onFail?.Invoke("[AudioLoader] chart.audio.file is missing.");
            return;
        }

        // chart.audio.file 예: "Audio/120bpm.wav"
        // "Audio/" 제거하고 파일명만 뽑는다(서버 저장 규칙 단순화)
        string fileName = Path.GetFileName(chart.audio.file);

        // 1) persistent 우선
        string pPath = Path.Combine(Application.persistentDataPath, audioSubfolder, fileName);
        if (File.Exists(pPath))
        {
            StartCoroutine(LoadClipCoroutine(pPath, onOk, onFail));
            return;
        }

        // 2) StreamingAssets fallback (샘플/기본 제공용)
        string sPath = Path.Combine(Application.streamingAssetsPath, audioSubfolder, fileName);

        // StreamingAssets는 플랫폼에 따라 File.Exists가 안 먹는 경우가 있어,
        // 우선 요청을 시도해보고 실패하면 에러 처리한다.
        StartCoroutine(LoadClipCoroutine(sPath, onOk, onFail));
    }

    private System.Collections.IEnumerator LoadClipCoroutine(string fullPath, Action<AudioClip> onOk, Action<string> onFail)
    {
        // Windows/Mac: file:// + absolute path
        // Android StreamingAssets: jar:file://... 형태 -> UnityWebRequest가 처리 가능
        string url = ToFileUrl(fullPath);

        AudioType type = GuessAudioType(fullPath);

        using var req = UnityWebRequestMultimedia.GetAudioClip(url, type);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onFail?.Invoke($"[AudioLoader] Failed to load audio: {fullPath}\n{req.error}");
            yield break;
        }

        var clip = DownloadHandlerAudioClip.GetContent(req);
        if (clip == null)
        {
            onFail?.Invoke($"[AudioLoader] Loaded clip is null: {fullPath}");
            yield break;
        }

        onOk?.Invoke(clip);
    }

    private static string ToFileUrl(string path)
    {
        // 이미 http/https/file/jar 스킴이면 그대로
        if (path.Contains("://")) return path;

        // 절대경로 기준 file://
        // (Unity는 슬래시 기반을 좋아함)
        string p = path.Replace("\\", "/");
        return $"file:///{p}";
    }

    private static AudioType GuessAudioType(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".wav" => AudioType.WAV,
            ".mp3" => AudioType.MPEG,
            ".ogg" => AudioType.OGGVORBIS,
            _ => AudioType.UNKNOWN
        };
    }
}
