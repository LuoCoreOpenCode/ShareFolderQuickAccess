using System.Runtime.InteropServices;
using System.Text;

namespace ShareFolderQuickAccess;

/// <summary>WNet / Credential Manager 原生 API 封装。</summary>
internal static class NativeMethods
{
    public const int CONNECT_TEMPORARY = 0x00000001;      // 仅本次登录会话
    public const int CONNECT_UPDATE_PROFILE = 0x00000002; // 持久连接（重启后自动恢复）
    public const int CONNECT_INTERACTIVE = 0x00000008;    // 允许系统级交互提示兜底

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class NetResource
    {
        public int dwScope = 2;        // RESOURCE_GLOBALNET
        public int dwType = 1;         // RESOURCETYPE_DISK
        public int dwDisplayType = 0;  // RESOURCEDISPLAYTYPE_GENERIC
        public int dwUsage = 1;        // RESOURCEUSAGE_CONNECTABLE
        [MarshalAs(UnmanagedType.LPWStr)] public string lpLocalName = "";
        [MarshalAs(UnmanagedType.LPWStr)] public string lpRemoteName = "";
        [MarshalAs(UnmanagedType.LPWStr)] public string lpComment = "";
        [MarshalAs(UnmanagedType.LPWStr)] public string lpProvider = "";
    }

    [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int WNetAddConnection2(NetResource lpNetResource, string? lpPassword, string? lpUserName, int dwFlags);

    [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int WNetCancelConnection2(string lpName, int dwFlags, bool fForce);

    // ───────────────────────── Windows 凭据管理器 ─────────────────────────

    private const int CRED_TYPE_GENERIC = 1;
    private const int CRED_PERSIST_LOCAL_MACHINE = 2;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private class CREDENTIAL
    {
        public int Flags;
        public int Type = CRED_TYPE_GENERIC;
        [MarshalAs(UnmanagedType.LPWStr)] public string TargetName = "";
        [MarshalAs(UnmanagedType.LPWStr)] public string? Comment;
        public long LastWritten;
        public int CredentialBlobSize;
        public IntPtr CredentialBlob = IntPtr.Zero;
        public int Persist = CRED_PERSIST_LOCAL_MACHINE;
        public int AttributeCount;
        public IntPtr Attributes = IntPtr.Zero;
        [MarshalAs(UnmanagedType.LPWStr)] public string? TargetAlias;
        [MarshalAs(UnmanagedType.LPWStr)] public string UserName = "";
    }

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWrite(CREDENTIAL credential, int flags);

    /// <summary>
    /// 写入与 cmdkey /add 等价的通用凭据（目标格式 Domain:target=&lt;服务器&gt;），
    /// SMB 重定向器访问该服务器时会自动使用它。密码仅存于系统加密保管库。
    /// </summary>
    public static bool CredWriteServerCredential(string server, string userName, string password)
    {
        IntPtr blob = Marshal.StringToHGlobalUni(password);
        try
        {
            var cred = new CREDENTIAL
            {
                TargetName = $"Domain:target={server}",
                UserName = userName,
                CredentialBlob = blob,
                CredentialBlobSize = Encoding.Unicode.GetByteCount(password),
            };
            return CredWrite(cred, 0);
        }
        finally
        {
            Marshal.FreeHGlobal(blob);
        }
    }
}
