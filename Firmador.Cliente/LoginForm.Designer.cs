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
            SuspendLayout();
            // 
            // lblUsuario
            // 
            lblUsuario.AutoSize = true;
            lblUsuario.Location = new Point(31, 19);
            lblUsuario.Margin = new Padding(4, 0, 4, 0);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(72, 25);
            lblUsuario.TabIndex = 0;
            lblUsuario.Text = "Usuario";
            // 
            // _usuario
            // 
            _usuario.Location = new Point(31, 50);
            _usuario.Margin = new Padding(4);
            _usuario.Name = "_usuario";
            _usuario.Size = new Size(369, 31);
            _usuario.TabIndex = 1;
            // 
            // lblContrasena
            // 
            lblContrasena.AutoSize = true;
            lblContrasena.Location = new Point(31, 94);
            lblContrasena.Margin = new Padding(4, 0, 4, 0);
            lblContrasena.Name = "lblContrasena";
            lblContrasena.Size = new Size(101, 25);
            lblContrasena.TabIndex = 2;
            lblContrasena.Text = "Contraseña";
            // 
            // _contrasena
            // 
            _contrasena.Location = new Point(31, 125);
            _contrasena.Margin = new Padding(4);
            _contrasena.Name = "_contrasena";
            _contrasena.Size = new Size(369, 31);
            _contrasena.TabIndex = 3;
            _contrasena.UseSystemPasswordChar = true;
            // 
            // btnIngresar
            // 
            btnIngresar.Location = new Point(162, 169);
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
            btnCancelar.Location = new Point(288, 169);
            btnCancelar.Margin = new Padding(4);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(112, 36);
            btnCancelar.TabIndex = 5;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            // 
            // LoginForm
            // 
            AcceptButton = btnIngresar;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            CancelButton = btnCancelar;
            ClientSize = new Size(475, 263);
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
            Text = "Iniciar sesión - Firmador";
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
    }
}
