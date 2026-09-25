namespace Firmador.Cliente;

internal partial class LoginForm : Form
{
    public string Usuario => _usuario.Text.Trim();
    public string Contrasena => _contrasena.Text;

    public LoginForm()
    {
        InitializeComponent();
        btnIngresar.Click += Ingresar_Click;
    }

    private void Ingresar_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Usuario))
        {
            MessageBox.Show(this, "Ingrese un usuario.", "Inicio de sesión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _usuario.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(Contrasena))
        {
            MessageBox.Show(this, "Ingrese una contraseña.", "Inicio de sesión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _contrasena.Focus();
            return;
        }

        DialogResult = DialogResult.OK;
    }
}
