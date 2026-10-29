using System.Text.Json;

namespace ShareFolderQuickAccess;

/// <summary>应用设置（保存在 %AppData%\ShareFolderQuickAccess\settings.json）。</summary>
public sealed class AppSettings
{
    public string SharePath { get; set; } = @"\\server\share";
    /// <summary>名称解析失败（错误 53）时的 IP 兜底地址；留空表示禁用兜底。</summary>
    public string FallbackIp { get; set; } = "192.168.1.10";
    public int HeartbeatSeconds { get; set; } = 60;
    public bool KeepAliveEnabled { get; set; } = true;
    public bool MinimizeToTray { get; set; } = true;
}

public static class SettingsStore
{
    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ShareFolderQuickAccess", "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var s = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath));
                if (s is not null) return s;
            }
        }
        catch { /* 配置损坏时用默认值 */ }
        return new AppSettings();
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { /* 保存失败不影响使用 */ }
    }
}
