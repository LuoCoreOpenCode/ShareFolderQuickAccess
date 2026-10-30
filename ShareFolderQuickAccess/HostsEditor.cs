using System.Diagnostics;

namespace ShareFolderQuickAccess;

/// <summary>
/// hosts 文件检查与写入、DNS 缓存刷新。
/// 实际写入由 "--fix-hosts" 提权子实例完成（需要管理员权限），主实例只做只读检查与轮询确认。
/// </summary>
internal static class HostsEditor
{
    public static string HostsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc", "hosts");

    /// <summary>检查 hosts 是否已包含 ip → name 映射（忽略注释、大小写不敏感）。</summary>
    public static bool EntryExists(string name, string ip)
    {
        try
        {
            foreach (var raw in File.ReadLines(HostsPath))
            {
                var line = raw.Split('#')[0].Trim();
                if (line.Length == 0) continue;
                var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2
                    && string.Equals(parts[0], ip, StringComparison.OrdinalIgnoreCase)
                    && parts.Skip(1).Any(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }
        }
        catch { /* 读取失败按不存在处理 */ }
        return false;
    }

    /// <summary>追加映射行（先备份 hosts.bak；已存在则跳过）。返回是否实际写入。</summary>
    public static bool AppendEntry(string name, string ip)
    {
        if (EntryExists(name, ip)) return false;

        var bytes = File.ReadAllBytes(HostsPath);
        File.Copy(HostsPath, HostsPath + ".bak", overwrite: true);

        string prefix = bytes.Length == 0 || bytes[^1] == (byte)'\n' ? "" : "\r\n";
        // 只追加 ASCII 字节、不重写原文件，避免破坏原编码
        File.AppendAllText(HostsPath, $"{prefix}{ip}\t{name}\r\n", System.Text.Encoding.ASCII);
        return true;
    }

    /// <summary>刷新 DNS 解析缓存（ipconfig /flushdns）。</summary>
    public static void FlushDns()
    {
        try
        {
            using var p = Process.Start(new ProcessStartInfo("ipconfig.exe", "/flushdns")
            { UseShellExecute = false, CreateNoWindow = true });
            p?.WaitForExit(5000);
        }
        catch { /* 刷新失败不致命，系统缓存会自然过期 */ }
    }
}
