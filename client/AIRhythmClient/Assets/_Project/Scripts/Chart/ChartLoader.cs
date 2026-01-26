using System;
using System.IO;
using System.Linq;
using UnityEngine;
using ChartModels;

public class ChartLoader : MonoBehaviour
{
    [Header("Catalog")]
    [SerializeField] private ChartCatalog catalog;

    public ChartDto Loaded { get; private set; }

    /// <summary>
    /// Dropdown의 entry("builtin:xxx.json" / "download:xxx.json")로 로드.
    /// </summary>
    public bool LoadByEntry(string entry)
    {
        if (catalog == null)
        {
            Debug.LogError("[ChartLoader] Catalog is not assigned.");
            return false;
        }

        if (!catalog.TryGetPath(entry, out var fullPath))
        {
            Debug.LogError($"[ChartLoader] Entry not found in catalog: {entry}");
            return false;
        }

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"[ChartLoader] Chart file not found: {fullPath}");
            return false;
        }

        var json = File.ReadAllText(fullPath);
        Loaded = JsonUtility.FromJson<ChartDto>(json);

        if (Loaded == null)
        {
            Debug.LogError($"[ChartLoader] Failed to parse JSON: {fullPath}");
            return false;
        }

        Debug.Log($"[ChartLoader] Loaded OK: entry={entry}, song_id={Loaded.song_id}, notes={Loaded.notes?.Length ?? 0}");
        return true;
    }
    //[Header("Charts folder in StreamingAssets")]
    //[SerializeField] private string chartsFolderName = "Charts";

    //[Header("Default chart file name (optional)")]
    //[SerializeField] private string defaultChartFileName = "sample_metronome_120bpm_pattern.json";

    //[Header("Auto Load on Start")]
    //[SerializeField] private bool autoLoadOnStart = false; // 추가: 기본값 false

    //public ChartDto Loaded { get; private set; }
    //public string CurrentFileName { get; private set; }
    //public string[] AvailableChartFiles { get; private set; } = Array.Empty<string>();
    //public event Action<ChartDto> OnChartLoaded;

    //private void Start()
    //{
    //    RefreshChartList();

    //    // autoLoadOnStart가 true일 때만 자동 로드
    //    if (!autoLoadOnStart) return;

    //    // 1) default가 있으면 우선 로드, 없으면 첫 번째 로드
    //    if (!string.IsNullOrWhiteSpace(defaultChartFileName) && AvailableChartFiles.Contains(defaultChartFileName))
    //    {
    //        LoadByFileName(defaultChartFileName);
    //    }
    //    else if (AvailableChartFiles.Length > 0)
    //    {
    //        LoadByFileName(AvailableChartFiles[0]);
    //    }
    //    else
    //    {
    //        Debug.LogError($"No chart json files found in: {GetChartsFolderPath()}");
    //    }
    //}

    //public void RefreshChartList()
    //{
    //    var folder = GetChartsFolderPath();
    //    if (!Directory.Exists(folder))
    //    {
    //        Debug.LogError($"Charts folder not found: {folder}");
    //        AvailableChartFiles = Array.Empty<string>();
    //        return;
    //    }

    //    AvailableChartFiles = Directory
    //        .GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly)
    //        .Select(Path.GetFileName)
    //        .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
    //        .ToArray();

    //    Debug.Log($"Chart list refreshed. count={AvailableChartFiles.Length}");
    //}

    //public bool LoadByFileName(string fileName)
    //{
    //    var chartPath = Path.Combine(GetChartsFolderPath(), fileName);

    //    if (!File.Exists(chartPath))
    //    {
    //        Debug.LogError($"Chart file not found: {chartPath}");
    //        return false;
    //    }

    //    var json = File.ReadAllText(chartPath);

    //    Loaded = JsonUtility.FromJson<ChartDto>(json);

    //    if (Loaded == null)
    //    {
    //        Debug.LogError($"Failed to parse chart JSON: {chartPath}");
    //        return false;
    //    }

    //    CurrentFileName = fileName;

    //    var noteCount = Loaded.notes?.Length ?? 0;

    //    Debug.Log($"Chart loaded OK: file={fileName}, song_id={Loaded.song_id}, version={Loaded.chart_version}, diff={Loaded.difficulty}, notes={noteCount}");

    //    OnChartLoaded?.Invoke(Loaded);
    //    return true;
    //}

    //private string GetChartsFolderPath()
    //    => Path.Combine(Application.streamingAssetsPath, chartsFolderName);
}
