namespace Firmador.Cliente;

internal sealed class LoginForm : Form
{
    private readonly TextBox _usuario = new() { Width = 260 };
    private readonly TextBox _contrasena = new() { Width = 260, UseSystemPasswordChar = true };

    public string Usuario => _usuario.Text.Trim();
    public string Contrasena => _contrasena.Text;

    public LoginForm()
    {
        Text = "Iniciar sesión - Firmador";
        ClientSize = new Size(340, 185);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        var aceptar = new Button { Text = "Ingresar", DialogResult = DialogResult.OK, Location = new Point(130, 135), Width = 90 };
        var cancelar = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Location = new Point(230, 135), Width = 90 };
        _usuario.Location = new Point(25, 40);
        _contrasena.Location = new Point(25, 100);
        Controls.AddRange([new Label { Text = "Usuario (email o CUIL)", Location = new Point(25, 15), AutoSize = true },
            _usuario, new Label { Text = "Contraseña", Location = new Point(25, 75), AutoSize = true }, _contrasena, aceptar, cancelar]);
        AcceptButton = aceptar;
        CancelButton = cancelar;
    }
}
