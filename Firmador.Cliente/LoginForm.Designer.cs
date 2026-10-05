namespace Firmador.Cliente
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = new System.ComponentModel.Container();

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
            if (disposing)
            {
                foreach (var recurso in _recursosVisuales) recurso.Dispose();
                _recursosVisuales.Clear();
            }
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(96, 96);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = CrearFuente(10);
            BackColor = TemaVisual.Fondo;
            ForeColor = TemaVisual.Texto;
            ClientSize = new Size(460, 440);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Name = "LoginForm";
            Text = "Iniciar sesión - Firmador EDA";

            var principal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 1, RowCount = 3,
                Margin = Padding.Empty
            };
            principal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));
            principal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var cabecera = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty
            };
            cabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));
            cabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var imagen = TemaVisual.CrearIcono("\uE70F", 32, TemaVisual.Azul);
            _recursosVisuales.Add(imagen);
            cabecera.Controls.Add(new PictureBox
            {
                Image = imagen, SizeMode = PictureBoxSizeMode.Zoom, Size = new Size(32, 32),
                Anchor = AnchorStyles.Left, Margin = Padding.Empty, TabStop = false
            }, 0, 0);
            cabecera.Controls.Add(new Label
            {
                Text = "Firmador EDA", Font = CrearFuente(21, FontStyle.Bold), AutoSize = true,
                Anchor = AnchorStyles.Left, Margin = Padding.Empty
            }, 1, 0);
            principal.Controls.Add(cabecera, 0, 0);

            var tarjeta = new PanelBorde
            {
                Dock = DockStyle.Fill, Padding = new Padding(20), Margin = Padding.Empty, AutoScroll = true
            };
            var campos = new TableLayoutPanel
            {
                Dock = DockStyle.Top, Height = 272, ColumnCount = 1, RowCount = 9, Margin = Padding.Empty
            };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            foreach (var altura in new[] { 30, 38, 22, 36, 12, 22, 36 })
                campos.RowStyles.Add(new RowStyle(SizeType.Absolute, altura));
            campos.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            campos.Controls.Add(new Label
            {
                Text = "Iniciar sesión", Font = CrearFuente(14, FontStyle.Bold), AutoSize = true,
                Dock = DockStyle.Fill, Margin = Padding.Empty
            }, 0, 0);
            lblIngreseCredenciales = new Label
            {
                Text = "Ingrese sus credenciales de acceso al EDA", Font = CrearFuente(9),
                ForeColor = TemaVisual.TextoSecundario, Dock = DockStyle.Fill, Margin = Padding.Empty
            };
            campos.Controls.Add(lblIngreseCredenciales, 0, 1);
            lblUsuario = new Label { Text = "Usuario", Dock = DockStyle.Fill, Margin = Padding.Empty };
            lblContrasena = new Label { Text = "Contraseña", Dock = DockStyle.Fill, Margin = Padding.Empty };
            _usuario = new TextBox { Name = "_usuario", TabIndex = 0 };
            _contrasena = new TextBox { Name = "_contrasena", TabIndex = 0, UseSystemPasswordChar = true };
            campos.Controls.Add(lblUsuario, 0, 2);
            campos.Controls.Add(CrearEntrada(_usuario, 0), 0, 3);
            campos.Controls.Add(lblContrasena, 0, 5);
            campos.Controls.Add(CrearEntrada(_contrasena, 1), 0, 6);

            var acciones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false, Margin = Padding.Empty, TabIndex = 2
            };
            btnIngresar = new Button
            {
                Name = "btnIngresar", Text = "Ingresar", Size = new Size(108, 36),
                Margin = Padding.Empty, TabIndex = 1, Font = CrearFuente(10, FontStyle.Bold)
            };
            btnCancelar = new Button
            {
                Name = "btnCancelar", Text = "Cancelar", Size = new Size(108, 36),
                Margin = new Padding(0, 0, 8, 0), TabIndex = 0, DialogResult = DialogResult.Cancel
            };
            TemaVisual.EstilarBoton(btnIngresar, principal: true);
            TemaVisual.EstilarBoton(btnCancelar);
            acciones.Controls.Add(btnIngresar);
            acciones.Controls.Add(btnCancelar);
            campos.Controls.Add(acciones, 0, 8);
            tarjeta.Controls.Add(campos);
            principal.Controls.Add(tarjeta, 0, 2);
            Controls.Add(principal);
            AcceptButton = btnIngresar;
            CancelButton = btnCancelar;
            ResumeLayout(true);
        }

        private Panel CrearEntrada(TextBox entrada, int orden)
        {
            var borde = new PanelBorde
            {
                Dock = DockStyle.Fill, Margin = Padding.Empty, Padding = new Padding(10, 7, 10, 7),
                ResaltarFoco = true, TabIndex = orden
            };
            entrada.BorderStyle = BorderStyle.None;
            entrada.BackColor = TemaVisual.Superficie;
            entrada.ForeColor = TemaVisual.Texto;
            entrada.Dock = DockStyle.Fill;
            entrada.Enter += (_, _) => borde.Invalidate();
            entrada.Leave += (_, _) => borde.Invalidate();
            borde.Controls.Add(entrada);
            return borde;
        }

        private Label lblUsuario = null!;
        private TextBox _usuario = null!;
        private Label lblContrasena = null!;
        private TextBox _contrasena = null!;
        private Button btnIngresar = null!;
        private Button btnCancelar = null!;
        private Label lblIngreseCredenciales = null!;
    }
}
