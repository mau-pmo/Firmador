namespace Firmador.Cliente;

internal partial class LoginForm : Form
{
    public string Usuario => _usuario.Text.Trim();
    public string Contrasena => _contrasena.Text;

    public LoginForm()
    {
        InitializeComponent();
    }
}
