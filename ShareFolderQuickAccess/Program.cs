namespace ShareFolderQuickAccess;

internal static class Program
{
    private static Mutex? _instanceMutex;

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        // --fix-hosts <name> <ip>：UAC 提权子实例专用——写入 hosts、刷新 DNS 后静默退出
        int fi = Array.FindIndex(args, a => string.Equals(a, "--fix-hosts", StringComparison.OrdinalIgnoreCase));
        if (fi >= 0)
        {
            try
            {
                if (fi + 2 < args.Length)
                {
                    HostsEditor.AppendEntry(args[fi + 1].Trim('"'), args[fi + 2].Trim('"'));
                    HostsEditor.FlushDns();
                }
            }
            catch { /* 写入结果由主实例轮询 hosts 判定 */ }
            return;
        }

        // --open：纯启动器模式，连上→打开资源管理器→自动退出（适合放桌面快捷方式）
        bool openAndExit = args.Any(a => string.Equals(a, "--open", StringComparison.OrdinalIgnoreCase));

        // 常驻模式做单实例保护，避免重复启动多个心跳
        if (!openAndExit)
        {
            _instanceMutex = new Mutex(initiallyOwned: true, @"Local\ShareFolderQuickAccess.SingleInstance", out bool isNew);
            if (!isNew)
            {
                MessageBox.Show("「共享文件夹一键直达」已在运行（可能在系统托盘中）。",
                    "共享文件夹一键直达", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        Application.Run(new MainForm(openAndExit));
        GC.KeepAlive(_instanceMutex);
    }
}
