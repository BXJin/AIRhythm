using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ServerChartBrowserUI : MonoBehaviour
{
    [SerializeField] private DownloadChartService downloadService;

    [Header("UI")]
    [SerializeField] private TMP_Dropdown serverDropdown;

    [Header("Local Refresh")]
    [SerializeField] private ChartSelectUI localChartSelectUi; // 다운로드 후 로컬 dropdown refresh용

    private ChartIndexItem[] _items;

    public void OnClickFetchList()
    {
        downloadService.FetchIndex(
            onOk: items =>
            {
                _items = items;
                serverDropdown.ClearOptions();

                var labels = new List<string>();
                foreach (var it in items)
                    labels.Add($"{it.title} ({it.id})");

                serverDropdown.AddOptions(labels);
                if (serverDropdown.options.Count > 0) serverDropdown.value = 0;

                Debug.Log($"[ServerChartBrowserUI] fetched items={items.Length}");
            },
            onFail: err => Debug.LogError(err)
        );
    }

    public void OnClickDownloadSelected()
    {
        if (_items == null || _items.Length == 0) return;
        int idx = serverDropdown.value;
        var item = _items[idx];

        downloadService.DownloadChartAndAudio(
            item,
            onOk: () =>
            {
                Debug.Log("[ServerChartBrowserUI] Download OK. Refresh local dropdown.");
                localChartSelectUi.RefreshDropdown(); // persistent 스캔 반영
            },
            onFail: err => Debug.LogError(err)
        );
    }
}
