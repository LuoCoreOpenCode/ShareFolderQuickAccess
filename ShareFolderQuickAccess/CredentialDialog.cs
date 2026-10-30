namespace ShareFolderQuickAccess;

/// <summary>共享凭据输入对话框（设计器拆分：布局见 CredentialDialog.Designer.cs）。</summary>
public partial class CredentialDialog : Form
{
    public string UserName => txtUser.Text.Trim();
    public string Password => txtPass.Text;
    public bool SaveToVault => chkSave.Checked;

    public CredentialDialog(string target, string? prefillUser = null)
    {
        InitializeComponent();
        lblServer.Text = $"目标: {target}";
        if (!string.IsNullOrEmpty(prefillUser)) txtUser.Text = prefillUser;
    }
}
