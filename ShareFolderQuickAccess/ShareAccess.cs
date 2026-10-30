using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ShareFolderQuickAccess;

/// <summary>
/// SMB 共享访问核心逻辑：探测、失效会话清理、net.exe 诊断、打开资源管理器。
/// </summary>
internal static partial class ShareAccess
{
    /// <summary>轻量探测：能枚举共享根目录即视为连通（空目录也算成功）。Code 为 Win32 错误码（0=成功）。</summary>
    public static (bool Ok, int Code, string Error) Probe(string path)
    {
        try
        {
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = false,
                IgnoreInaccessible = true,
                MatchCasing = MatchCasing.CaseInsensitive,
            };
            using var iter = Directory.EnumerateFileSystemEntries(path, "*", options).GetEnumerator();
            iter.MoveNext(); // 触发 Tree Connect；空共享返回 false 但不抛异常，同样视为连通
            return (true, 0, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.HResult & 0xFFFF, DescribeError(ex));
        }
    }

    /// <summary>清理本机对目标服务器/共享的失效会话（等价于 net use \\server /delete）。</summary>
    public static void ClearStaleSessions(string server, string path)
    {
        RunNet($@"use ""\\{server}"" /delete /y");
        RunNet($@"use ""{path}"" /delete /y");
    }

    /// <summary>在资源管理器中打开共享路径。</summary>
    public static void OpenInExplorer(string path)
    {
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
    }

    /// <summary>执行 net.exe 并捕获输出（诊断用）。</summary>
    public static string RunNetCapture(string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "net.exe",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            try
            {
                // net.exe 输出使用系统 OEM 代码页（中文系统为 GBK），显式指定避免乱码
                var oem = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
                psi.StandardOutputEncoding = oem;
                psi.StandardErrorEncoding = oem;
            }
            catch { /* 拿不到 OEM 编码就用默认 */ }

            using var p = Process.Start(psi);
            if (p is null) return "（无法启动 net.exe）";
            string stdout = p.StandardOutput.ReadToEnd();
            string stderr = p.StandardError.ReadToEnd();
            p.WaitForExit(8000);
            string text = (stdout + stderr).Trim();
            return string.IsNullOrEmpty(text) ? "（无输出）" : text;
        }
        catch (Exception ex)
        {
            return $"执行 net.exe 失败：{ex.Message}";
        }
    }

    private static void RunNet(string args) => RunNetCapture(args);

    /// <summary>校验 UNC 路径格式 \\server\share。</summary>
    [GeneratedRegex(@"^\\\\[^\\]+\\[^\\]+")]
    private static partial Regex UncPattern();

    public static bool IsValidUnc(string path) => UncPattern().IsMatch(path);

    public static string GetServerName(string path)
    {
        // \\server\share\... → server
        var trimmed = path.TrimStart('\\');
        int idx = trimmed.IndexOf('\\');
        return idx > 0 ? trimmed[..idx] : trimmed;
    }

    /// <summary>提取 UNC 路径中的共享名段（\\server\share\sub → share）。</summary>
    public static string GetShareName(string path)
    {
        var parts = path.TrimStart('\\').Split('\\', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? parts[1] : string.Empty;
    }

    /// <summary>用系统完整解析链（hosts→DNS→NetBIOS/LLMNR）解析服务器名，用于 53 错误时区分"解析失败"还是"服务器真不可达"。</summary>
    public static string TryResolveName(string server)
    {
        try
        {
            var addrs = System.Net.Dns.GetHostAddresses(server);
            return addrs.Length == 0 ? "解析返回空结果" : string.Join(", ", addrs.Select(a => a.ToString()));
        }
        catch (Exception ex)
        {
            return $"名称解析失败（{ex.GetType().Name}: {ex.Message}）";
        }
    }

    /// <summary>认证类错误判定：需要用户提供/更新凭据才能继续。</summary>
    public static bool IsAuthError(int code) => code is 5 or 86 or 1219 or 1311 or 1312 or 1326;

    /// <summary>
    /// 用显式凭据建立 SMB 连接（WNetAddConnection2）；1219 凭据冲突时自动清旧会话重试一次。
    /// save=true 时同时把凭据写入 Windows 凭据管理器（系统加密保管，不落配置文件）。
    /// </summary>
    public static (bool Ok, string Message) ConnectWithCredentials(string uncPath, string user, string password, bool save)
    {
        var nr = new NativeMethods.NetResource { lpRemoteName = uncPath };
        int flags = (save ? NativeMethods.CONNECT_UPDATE_PROFILE : NativeMethods.CONNECT_TEMPORARY)
                  | NativeMethods.CONNECT_INTERACTIVE;

        int r = NativeMethods.WNetAddConnection2(nr, password, user, flags);
        if (r == 1219) // 同一服务器存在不同账号的会话 → 清掉再试
        {
            ClearStaleSessions(GetServerName(uncPath), uncPath);
            r = NativeMethods.WNetAddConnection2(nr, password, user, flags);
        }
        if (r != 0) return (false, $"WNetAddConnection2 失败：{DescribeWin32Code(r)}");

        if (save)
        {
            bool stored = NativeMethods.CredWriteServerCredential(GetServerName(uncPath), user, password);
            return stored
                ? (true, "已连接，凭据已存入 Windows 凭据管理器")
                : (true, "已连接，但凭据保存失败（下次访问可能仍需输入）");
        }
        return (true, "已连接（凭据仅本次会话有效）");
    }

    /// <summary>把 Win32 错误码翻译成中文说明。</summary>
    public static string DescribeWin32Code(int code) => code switch
    {
        5 => "拒绝访问（错误 5）：当前账号无权限或凭据已失效",
        53 => "找不到网络路径（错误 53）：服务器不可达",
        64 => "指定的网络名不再可用（错误 64）：会话已失效",
        67 => "找不到网络名（错误 67 / 0x80070043）：会话失效或共享暂时不可用",
        86 => "需要进一步的身份验证（错误 86）",
        1219 => "凭据冲突（错误 1219）：同一服务器存在不同账号的连接",
        1326 => "用户名或密码错误（错误 1326）",
        1311 or 1312 => "找不到登录会话（错误 " + code + "，凭据问题）",
        2250 => "该网络连接不存在（错误 2250）",
        _ => $"Win32 错误 {code}",
    };

    /// <summary>把异常（含 HResult 错误码）翻译成中文说明。</summary>
    private static string DescribeError(Exception ex)
    {
        int code = ex.HResult & 0xFFFF;
        return code != 0 ? $"{DescribeWin32Code(code)}（原始异常：{ex.GetType().Name}）" : ex.Message;
    }
}
