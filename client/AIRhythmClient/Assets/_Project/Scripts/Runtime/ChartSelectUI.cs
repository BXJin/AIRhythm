using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ChartSelectUI : MonoBehaviour
{
    [SerializeField] private ChartCatalog catalog;
    [SerializeField] private ChartLoader chartLoader;
    [SerializeField] private TMP_Dropdown dropdown;

    private void Start()
    {
        RefreshDropdown();
        //if (chartLoader == null || dropdown == null) return;

        //// 목록 갱신
        //chartLoader.RefreshChartList();

        //dropdown.ClearOptions();
        //dropdown.AddOptions(chartLoader.AvailableChartFiles.ToList());

        //// 현재 로드된 파일이 있으면 그걸 선택
        //if (!string.IsNullOrEmpty(chartLoader.CurrentFileName))
        //{
        //    int idx = dropdown.options.FindIndex(o => o.text == chartLoader.CurrentFileName);
        //    if (idx >= 0) dropdown.value = idx;
        //}
    }

    // 버튼에 연결해도 됨: "Refresh" 버튼 OnClick -> ChartSelectUI.RefreshDropdown
    public void RefreshDropdown()
    {
        if (catalog == null || dropdown == null) return;

        catalog.Refresh();

        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(catalog.Entries));

        // 기본값(첫 항목) 선택
        if (dropdown.options.Count > 0)
            dropdown.value = 0;
    }
}
