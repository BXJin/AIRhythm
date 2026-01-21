using System.Linq;
using UnityEngine;
using TMPro;

public class ChartSelectUI : MonoBehaviour
{
    [SerializeField] private ChartLoader chartLoader;
    [SerializeField] private TMP_Dropdown dropdown;

    private void Start()
    {
        if (chartLoader == null || dropdown == null) return;

        // 목록 갱신
        chartLoader.RefreshChartList();

        dropdown.ClearOptions();
        dropdown.AddOptions(chartLoader.AvailableChartFiles.ToList());

        // 현재 로드된 파일이 있으면 그걸 선택
        if (!string.IsNullOrEmpty(chartLoader.CurrentFileName))
        {
            int idx = dropdown.options.FindIndex(o => o.text == chartLoader.CurrentFileName);
            if (idx >= 0) dropdown.value = idx;
        }
    }
}
