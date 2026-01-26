using System;
using UnityEngine;

[Serializable]
public class ApiSettings
{
    public string baseUrl;
}

public static class ApiSettingsProvider
{
    // 빌드 심볼로 환경 선택
    // - DEV_BUILD: dev 설정 사용
    // - 기본: prod 설정 사용
    private const string DevPath = "Config/apisettings.dev";
    private const string ProdPath = "Config/apisettings.prod";

    public static ApiSettings Load()
    {
        string path =
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DevPath;
#else
        ProdPath;
#endif

        TextAsset asset = Resources.Load<TextAsset>(path);
        if (asset == null)
        {
            Debug.LogError($"[ApiSettingsProvider] Missing settings asset at Resources/{path}.json");
            return new ApiSettings { baseUrl = "http://localhost:5000" }; // 안전 기본값
        }

        var settings = JsonUtility.FromJson<ApiSettings>(asset.text);
        if (settings == null || string.IsNullOrWhiteSpace(settings.baseUrl))
        {
            Debug.LogError($"[ApiSettingsProvider] Invalid settings JSON: {path}");
            return new ApiSettings { baseUrl = "http://localhost:5000" };
        }

        return settings;
    }
}
