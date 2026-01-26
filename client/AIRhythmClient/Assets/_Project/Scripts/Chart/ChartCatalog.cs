using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// ChartCatalog = 차트 파일 목록을 모아주는 "카탈로그".
/// - StreamingAssets/Charts  +  persistentDataPath/Charts 를 둘 다 스캔한다.
/// - Dropdown에는 "표시용 ID"를 넣고, 실제 경로는 여기서 다시 찾는다.
/// </summary>
public class ChartCatalog : MonoBehaviour
{
    [Header("Folders")]
    [SerializeField] private string chartsSubfolder = "Charts";  // Charts
    [SerializeField] private string chartExtension = "*.json";   // *.json

    // 표시용 목록(예: "builtin:sample.json", "download:abc.json")
    private readonly List<string> _entries = new();
    private readonly Dictionary<string, string> _entryToFullPath = new();

    public IReadOnlyList<string> Entries => _entries;

    public void Refresh()
    {
        _entries.Clear();
        _entryToFullPath.Clear();

        // 1) built-in (StreamingAssets)
        string builtinDir = Path.Combine(Application.streamingAssetsPath, chartsSubfolder);
        AddFolder(builtinDir, prefix: "builtin:");

        // 2) downloaded (persistentDataPath)
        string downloadDir = Path.Combine(Application.persistentDataPath, chartsSubfolder);
        AddFolder(downloadDir, prefix: "download:");

        Debug.Log($"[ChartCatalog] Refreshed. entries={_entries.Count}\n" +
                  $"  builtin={builtinDir}\n" +
                  $"  download={downloadDir}");
    }

    private void AddFolder(string folderPath, string prefix)
    {
        if (!Directory.Exists(folderPath))
        {
            // 다운로드 폴더는 처음엔 없을 수 있으니 자동 생성해도 OK
            if (prefix.StartsWith("download:", StringComparison.OrdinalIgnoreCase))
            {
                Directory.CreateDirectory(folderPath);
            }
            else
            {
                return;
            }
        }

        var files = Directory.GetFiles(folderPath, chartExtension, SearchOption.TopDirectoryOnly);

        foreach (var fullPath in files)
        {
            string fileName = Path.GetFileName(fullPath); // sample.json
            string entry = $"{prefix}{fileName}";

            // 중복 방지(같은 이름이 들어올 수 있음)
            if (_entryToFullPath.ContainsKey(entry)) continue;

            _entryToFullPath[entry] = fullPath;
            _entries.Add(entry);
        }
    }

    /// <summary>Dropdown에서 선택한 entry로 실제 파일 경로를 얻는다.</summary>
    public bool TryGetPath(string entry, out string fullPath)
        => _entryToFullPath.TryGetValue(entry, out fullPath);
}
