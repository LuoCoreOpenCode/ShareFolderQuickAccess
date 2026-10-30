using System.Diagnostics;
using Microsoft.Win32;

namespace ShareFolderQuickAccess;

/// <summary>
/// 主窗体业务逻辑。控件声明与布局见 MainForm.Designer.cs（可在 VS 设计器中可视化编辑）。
/// 可靠性策略：名称访问失败 → 自动清会话重试 → IP 兜底直连；断线期间心跳提速；唤醒/解锁立即重连。
/// </summary>
public partial class MainForm : Form
{
    private enum NetState { Checking, Connected, NotConnected }

    private const int RetryIntervalMs = 15_000; // 断线期间的快速重试间隔

    private readonly AppSettings _settings = SettingsStore.Load();
    private readonly bool _openAndExit;
    private ContextMenuStrip? _trayMenu;

    private volatile bool _busy;
    private bool _exiting;
    private bool? _lastState;      // null = 未知
    private string? _goodPath;     // 最近一次探测成功的实际路径（名称或 IP 兜底）

    public MainForm(bool openAndExit = false)
    {
        _openAndExit = openAndExit;
        InitializeComponent();
        ApplySettingsToUi();
        InitTrayRuntime();
        ApplyHeartbeat(persist: false);
        SetStatus(NetState.Checking);

        // 唤醒 / 解锁瞬间主动重连，不等心跳
        SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

        if (_openAndExit)
            _ = OpenAndExitAsync();
        else
            _ = InitializeAsync();
    }

    // ───────────────────────── 设计器事件处理 ─────────────────────────

    private async void OpenButton_Click(object? sender, EventArgs e) => await OpenShareAsync();

    private async void DiagButton_Click(object? sender, EventArgs e) => await RunDiagnosticsAsync();

    private async void FixHostsButton_Click(object? sender, EventArgs e) => await FixHostsAsync();

    private void KeepAliveCheckBox_CheckedChanged(object? sender, EventArgs e) => ApplyHeartbeat(persist: true);

    private void IntervalUpDown_ValueChanged(object? sender, EventArgs e) => ApplyHeartbeat(persist: true);

    private async void Heartbeat_Tick(object? sender, EventArgs e) => await EnsureConnectedAsync(quiet: true);

    private void TrayIcon_DoubleClick(object? sender, EventArgs e) => ShowFromTray();

    // ───────────────────────── 系统事件 ─────────────────────────

    private void SystemEvents_PowerModeChanged(object? sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode != PowerModes.Resume) return;
        Log("→ 检测到系统唤醒，立即重连共享…");
        _ = EnsureConnectedAsync();
    }

    private void SystemEvents_SessionSwitch(object? sender, SessionSwitchEventArgs e)
    {
        if (e.Reason is not (SessionSwitchReason.SessionUnlock or SessionSwitchReason.ConsoleConnect)) return;
        Log("→ 检测到屏幕解锁/切回控制台，静默检查共享连接…");
        _ = EnsureConnectedAsync(quiet: true);
    }

    // ───────────────────────── 初始化 ─────────────────────────

    /// <summary>设计器里存的是默认值，运行时用保存的设置覆盖。</summary>
    private void ApplySettingsToUi()
    {
        pathBox.Text = _settings.SharePath;
        fallbackIpBox.Text = _settings.FallbackIp;
        keepAliveCheckBox.Checked = _settings.KeepAliveEnabled;
        intervalUpDown.Value = Math.Clamp(_settings.HeartbeatSeconds,
            (int)intervalUpDown.Minimum, (int)intervalUpDown.Maximum);
        trayCheckBox.Checked = _settings.MinimizeToTray;
    }

    /// <summary>托盘图标与右键菜单在代码中构建（菜单项含动态逻辑，不适合设计器序列化）。</summary>
    private void InitTrayRuntime()
    {
        trayIcon.Icon = SystemIcons.Application;
        trayIcon.Visible = false;

        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add("📂 打开共享", null, async (_, _) => { ShowFromTray(); await OpenShareAsync(); });
        _trayMenu.Items.Add("显示主界面", null, (_, _) => ShowFromTray());
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add("退出程序", null, (_, _) => { _exiting = true; Close(); });
        trayIcon.ContextMenuStrip = _trayMenu;
    }

    private async Task InitializeAsync()
    {
        Log("程序启动。点击「一键打开共享文件夹」即可访问，失效会话会被自动清理并重试。");
        if (keepAliveCheckBox.Checked)
            Log($"保活心跳已开启：每 {intervalUpDown.Value} 秒探测一次，空闲断连将自动恢复。");
        var fallback = BuildIpFallbackPath(pathBox.Text.Trim());
        if (fallback is not null)
            Log($"IP 兜底已启用：名称访问失败时自动改用 {fallback}");
        await EnsureConnectedAsync();
    }

    /// <summary>--open 启动器模式：连上→打开→退出；失败则留在窗口便于排查。</summary>
    private async Task OpenAndExitAsync()
    {
        bool ok = await EnsureConnectedAsync();
        if (ok && _goodPath is not null)
        {
            ShareAccess.OpenInExplorer(_goodPath);
            Log("→ 已打开共享文件夹，启动器模式退出。");
            await Task.Delay(400);
            _exiting = true;
            Close();
        }
        else
        {
            Log("⚠ 未能连接共享，窗口保持打开以便排查（可点「诊断」查看细节）。");
            Show();
            Activate();
        }
    }

    // ───────────────────────── 核心逻辑 ─────────────────────────

    /// <summary>
    /// 确保共享可访问：①按名称探测（失败清会话重试 3 次）→ ②仍失败则 IP 兜底直连 → ③都失败进入快速重试。
    /// </summary>
    private async Task<bool> EnsureConnectedAsync(bool quiet = false)
    {
        if (_busy) return false;
        _busy = true;
        try
        {
            var path = pathBox.Text.Trim();
            if (!ShareAccess.IsValidUnc(path))
            {
                Log("路径格式应为 \\\\服务器名\\共享名，请检查后重试。");
                SetStatus(NetState.NotConnected);
                return false;
            }

            // ① 正常路径：按服务器名称访问
            var (ok, attempt, lastCode) = await TryProbeWithRecoveryAsync(path, quiet);
            if (ok) return OnConnected(path, attempt > 1);
            if (ShareAccess.IsAuthError(lastCode) && await TryConnectWithCredentialsAsync(path, quiet))
                return OnConnected(path, recovered: true);

            // ② IP 兜底：名称解析/共享枚举失败时改用 IP 直连
            var ipPath = BuildIpFallbackPath(path);
            if (ipPath is not null)
            {
                Log($"→ 名称访问失败，尝试 IP 兜底：{ipPath}");
                var (ok2, attempt2, code2) = await TryProbeWithRecoveryAsync(ipPath, quiet);
                if (ok2)
                {
                    Log("✔ IP 兜底连接成功（名称解析暂时异常，建议尽快配置 hosts 根治）。");
                    return OnConnected(ipPath, recovered: true);
                }
                if (ShareAccess.IsAuthError(code2) && await TryConnectWithCredentialsAsync(ipPath, quiet))
                {
                    Log("✔ IP 兜底凭据连接成功。");
                    return OnConnected(ipPath, recovered: true);
                }
            }

            // ③ 全部失败：进入断线状态，心跳提速自动重试
            SetStatus(NetState.NotConnected);
            _goodPath = null;
            UpdateTimerRate();
            if (_lastState != false)
            {
                Log("⚠ 名称访问与 IP 兜底均失败。若同事也访问不了，请检查服务器是否在线（联系管理员）；否则本程序将持续自动重试。");
                _lastState = false;
            }
            return false;
        }
        finally
        {
            _busy = false;
        }
    }

    /// <summary>对指定路径探测最多 3 次；会话类错误（64/67/1219）先清理失效会话再重试。返回最后一次的错误码。</summary>
    private async Task<(bool Ok, int Attempt, int LastCode)> TryProbeWithRecoveryAsync(string path, bool quiet)
    {
        var server = ShareAccess.GetServerName(path);
        int lastCode = 0;
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            SetStatus(NetState.Checking);
            var (ok, code, err) = await Task.Run(() => ShareAccess.Probe(path));
            if (ok) return (true, attempt, 0);
            lastCode = code;

            // 只在状态变化或非静默模式下打日志，避免心跳刷屏
            if (!quiet || _lastState != false)
                Log($"✖ 第 {attempt} 次访问失败：{err}");

            if (attempt < 3)
            {
                if (code is 64 or 67 or 1219)
                {
                    Log("→ 清理失效网络会话，稍后重试…");
                    await Task.Run(() => ShareAccess.ClearStaleSessions(server, path));
                }
                else
                {
                    if (code == 53 && attempt == 1)
                        Log($"→ 服务器侧暂不可达。{server} 当前名称解析结果：{ShareAccess.TryResolveName(server)}");
                    Log("→ 稍后直接重试…");
                }
                await Task.Delay(800);
            }
        }
        return (false, 3, lastCode);
    }

    /// <summary>认证失败时弹出凭据对话框，用 WNet 带凭据连接（可选存入 Windows 凭据管理器）。心跳静默期只提示不打扰。</summary>
    private async Task<bool> TryConnectWithCredentialsAsync(string path, bool quiet)
    {
        if (quiet)
        {
            if (_lastState != false)
                Log("🔑 共享需要凭据（认证被拒）。心跳不弹窗打扰，点击「一键打开」即可输入凭据完成连接。");
            return false;
        }

        using var dlg = new CredentialDialog(path);
        if (dlg.ShowDialog(this) != DialogResult.OK)
        {
            Log("→ 已取消凭据输入。");
            return false;
        }
        if (dlg.UserName.Length == 0)
        {
            Log("✖ 用户名为空，未连接。");
            return false;
        }

        var (ok, msg) = await Task.Run(() => ShareAccess.ConnectWithCredentials(path, dlg.UserName, dlg.Password, dlg.SaveToVault));
        Log(ok ? $"🔑 {msg}（{path}）" : $"✖ 凭据连接失败：{msg}");
        return ok;
    }

    /// <summary>根据主路径与兜底 IP 构造 \\ip\share 路径；不满足条件返回 null。</summary>
    private string? BuildIpFallbackPath(string path)
    {
        var ip = fallbackIpBox.Text.Trim();
        if (ip.Length == 0 || !System.Net.IPAddress.TryParse(ip, out _)) return null;
        var server = ShareAccess.GetServerName(path);
        if (string.Equals(server, ip, StringComparison.OrdinalIgnoreCase)) return null; // 本来就是 IP
        var share = ShareAccess.GetShareName(path);
        return share.Length == 0 ? null : $@"\\{ip}\{share}";
    }

    private bool OnConnected(string usedPath, bool recovered)
    {
        bool stateChanged = _lastState != true;
        _goodPath = usedPath;
        _lastState = true;
        SetStatus(NetState.Connected);
        UpdateTimerRate();

        if (recovered) Log("✔ 自动重连成功，共享已恢复。");
        else if (stateChanged) Log("✔ 共享连接正常。");

        // 窗口隐藏在托盘时，恢复连接弹通知提醒
        if (stateChanged && recovered && !Visible)
            trayIcon.ShowBalloonTip(3000, "共享已恢复", $"{usedPath} 已重新可访问。", ToolTipIcon.Info);
        return true;
    }

    private async Task OpenShareAsync()
    {
        openButton.Enabled = false;
        try
        {
            if (await EnsureConnectedAsync() && _goodPath is not null)
            {
                ShareAccess.OpenInExplorer(_goodPath);
                Log(_goodPath == pathBox.Text.Trim()
                    ? "→ 已在资源管理器中打开共享文件夹。"
                    : $"→ 已通过 IP 兜底路径打开：{_goodPath}");
            }
        }
        finally
        {
            openButton.Enabled = true;
        }
    }

    private async Task RunDiagnosticsAsync()
    {
        var path = pathBox.Text.Trim();
        if (!ShareAccess.IsValidUnc(path))
        {
            Log("路径格式不正确，无法诊断。");
            return;
        }
        diagButton.Enabled = false;
        try
        {
            var server = ShareAccess.GetServerName(path);
            Log($"── 诊断 1/3：名称解析 {server} ──");
            Log(ShareAccess.TryResolveName(server));
            Log($"── 诊断 2/3：net view \\\\{server}（服务器共享列表）──");
            Log(await Task.Run(() => ShareAccess.RunNetCapture($@"view ""\\{server}""")));
            Log("── 诊断 3/3：net use（本机现有网络映射）──");
            Log(await Task.Run(() => ShareAccess.RunNetCapture("use")));
            Log("── 诊断完成 ──");
        }
        finally
        {
            diagButton.Enabled = true;
        }
    }

    /// <summary>
    /// 一键把「IP 兜底」中的 IP 与服务器名映射写入系统 hosts：
    /// 本实例只读检查 → UAC 提权子实例执行写入（自动备份 hosts.bak）→ 轮询确认 → 刷 DNS → 重连验证。
    /// </summary>
    private async Task FixHostsAsync()
    {
        var path = pathBox.Text.Trim();
        if (!ShareAccess.IsValidUnc(path))
        {
            Log("路径格式不正确，无法写 hosts。");
            return;
        }
        var server = ShareAccess.GetServerName(path);
        var ip = fallbackIpBox.Text.Trim();
        if (ip.Length == 0 || !System.Net.IPAddress.TryParse(ip, out _))
        {
            Log("请先在「IP 兜底」框填写服务器 IP，再执行 hosts 写入。");
            return;
        }
        if (string.Equals(server, ip, StringComparison.OrdinalIgnoreCase))
        {
            Log("共享路径本来就是 IP 地址，无需 hosts 映射。");
            return;
        }
        if (HostsEditor.EntryExists(server, ip))
        {
            Log($"✔ hosts 已包含映射：{ip}  {server}（无需重复写入）。");
            HostsEditor.FlushDns();
            await EnsureConnectedAsync();
            return;
        }

        var confirm = MessageBox.Show(this,
            $"将以管理员身份向 hosts 写入一行名称映射，根治名称解析：\n\n    {ip}\t{server}\n\n" +
            "（原 hosts 文件自动备份为 hosts.bak；需要在 UAC 弹窗点\"是\"）",
            "写入 hosts", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        try
        {
            var exe = Environment.ProcessPath ?? Application.ExecutablePath;
            Process.Start(new ProcessStartInfo(exe, $"--fix-hosts \"{server}\" \"{ip}\"")
            { UseShellExecute = true, Verb = "runas" });
        }
        catch
        {
            Log("✖ UAC 提权被取消或失败，未写入 hosts。可重试，或手动添加该行映射。");
            return;
        }

        Log("→ 等待 UAC 授权与 hosts 写入…");
        fixHostsButton.Enabled = false;
        try
        {
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(2000);
                if (HostsEditor.EntryExists(server, ip))
                {
                    HostsEditor.FlushDns();
                    Log($"✔ hosts 写入成功：{ip}  {server}，DNS 缓存已刷新。");
                    await EnsureConnectedAsync();
                    return;
                }
            }
            Log("⚠ 60 秒内未检测到写入结果（可能取消了 UAC）。可重试或手动添加。");
        }
        finally
        {
            fixHostsButton.Enabled = true;
        }
    }

    // ───────────────────────── 心跳定时器 ─────────────────────────

    private void ApplyHeartbeat(bool persist)
    {
        heartbeatTimer.Enabled = keepAliveCheckBox.Checked;
        UpdateTimerRate();
        if (persist)
        {
            _settings.KeepAliveEnabled = keepAliveCheckBox.Checked;
            _settings.HeartbeatSeconds = (int)intervalUpDown.Value;
            SettingsStore.Save(_settings);
        }
    }

    /// <summary>连接正常用设定间隔；断线时提速到 15 秒快速自愈。</summary>
    private void UpdateTimerRate()
    {
        if (!keepAliveCheckBox.Checked) return;
        heartbeatTimer.Interval = _lastState == true
            ? Math.Max(15, (int)intervalUpDown.Value) * 1000
            : RetryIntervalMs;
    }

    // ───────────────────────── 托盘 ─────────────────────────

    private void HideToTray()
    {
        Hide();
        trayIcon.Visible = true;
        trayIcon.ShowBalloonTip(2000, "已最小化到系统托盘", "保活心跳仍在运行；双击托盘图标可恢复窗口。", ToolTipIcon.Info);
    }

    private void ShowFromTray()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
        trayIcon.Visible = false;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!_exiting && trayCheckBox.Checked && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            HideToTray();
            return;
        }

        heartbeatTimer.Stop();
        SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
        SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;

        _settings.SharePath = pathBox.Text.Trim();
        _settings.FallbackIp = fallbackIpBox.Text.Trim();
        _settings.MinimizeToTray = trayCheckBox.Checked;
        SettingsStore.Save(_settings);

        trayIcon.Visible = false;
        base.OnFormClosing(e);
    }

    // ───────────────────────── 状态栏 / 日志 ─────────────────────────

    private void SetStatus(NetState state)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => SetStatus(state));
            return;
        }
        (statusLabel.Text, statusLabel.ForeColor) = state switch
        {
            NetState.Connected => (_goodPath is not null && _goodPath != pathBox.Text.Trim()
                ? "● 已连接（IP 兜底）" : "● 已连接", Color.ForestGreen),
            NetState.NotConnected => ("● 未连接", Color.Firebrick),
            _ => ("● 检测中…", Color.DarkOrange),
        };
    }

    private void Log(string message)
    {
        if (logBox.InvokeRequired)
        {
            logBox.BeginInvoke(() => Log(message));
            return;
        }
        // 防止长时间运行内存膨胀
        if (logBox.TextLength > 200_000) logBox.Clear();
        logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        logBox.SelectionStart = logBox.TextLength;
        logBox.ScrollToCaret();
    }
}
