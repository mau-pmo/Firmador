namespace Firmador.Cliente
{
    partial class LoginForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblUsuario = new Label();
            _usuario = new TextBox();
            lblContrasena = new Label();
            _contrasena = new TextBox();
            btnIngresar = new Button();
            btnCancelar = new Button();
            lblIngreseCredenciales = new Label();
            SuspendLayout();
            // 
            // lblUsuario
            // 
            lblUsuario.AutoSize = true;
            lblUsuario.Location = new Point(65, 103);
            lblUsuario.Margin = new Padding(4, 0, 4, 0);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(76, 25);
            lblUsuario.TabIndex = 0;
            lblUsuario.Text = "Usuario:";
            // 
            // _usuario
            // 
            _usuario.Location = new Point(65, 134);
            _usuario.Margin = new Padding(4);
            _usuario.Name = "_usuario";
            _usuario.Size = new Size(465, 31);
            _usuario.TabIndex = 1;
            // 
            // lblContrasena
            // 
            lblContrasena.AutoSize = true;
            lblContrasena.Location = new Point(65, 190);
            lblContrasena.Margin = new Padding(4, 0, 4, 0);
            lblContrasena.Name = "lblContrasena";
            lblContrasena.Size = new Size(105, 25);
            lblContrasena.TabIndex = 2;
            lblContrasena.Text = "Contraseña:";
            // 
            // _contrasena
            // 
            _contrasena.Location = new Point(65, 221);
            _contrasena.Margin = new Padding(4);
            _contrasena.Name = "_contrasena";
            _contrasena.Size = new Size(465, 31);
            _contrasena.TabIndex = 3;
            _contrasena.UseSystemPasswordChar = true;
            // 
            // btnIngresar
            // 
            btnIngresar.Location = new Point(294, 301);
            btnIngresar.Margin = new Padding(4);
            btnIngresar.Name = "btnIngresar";
            btnIngresar.Size = new Size(112, 36);
            btnIngresar.TabIndex = 4;
            btnIngresar.Text = "Ingresar";
            btnIngresar.UseVisualStyleBackColor = true;
            // 
            // btnCancelar
            // 
            btnCancelar.DialogResult = DialogResult.Cancel;
            btnCancelar.Location = new Point(420, 301);
            btnCancelar.Margin = new Padding(4);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(112, 36);
            btnCancelar.TabIndex = 5;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            // 
            // lblIngreseCredenciales
            // 
            lblIngreseCredenciales.AutoSize = true;
            lblIngreseCredenciales.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblIngreseCredenciales.Location = new Point(65, 40);
            lblIngreseCredenciales.Name = "lblIngreseCredenciales";
            lblIngreseCredenciales.Size = new Size(413, 25);
            lblIngreseCredenciales.TabIndex = 6;
            lblIngreseCredenciales.Text = "Por favor ingrese sus credenciales de acceso al EDA";
            // 
            // LoginForm
            // 
            AcceptButton = btnIngresar;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            CancelButton = btnCancelar;
            ClientSize = new Size(690, 390);
            Controls.Add(lblIngreseCredenciales);
            Controls.Add(btnCancelar);
            Controls.Add(btnIngresar);
            Controls.Add(_contrasena);
            Controls.Add(lblContrasena);
            Controls.Add(_usuario);
            Controls.Add(lblUsuario);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Iniciar sesión - Firmador Cliente EDA";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblUsuario;
        private TextBox _usuario;
        private Label lblContrasena;
        private TextBox _contrasena;
        private Button btnIngresar;
        private Button btnCancelar;
        private Label lblIngreseCredenciales;
    }
}
