namespace Firmador.Cliente
{
    partial class MainForm
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
            btnBuscar = new Button();
            btnFirmarDocumentos = new Button();
            btnSalir = new Button();
            dgvDocumentos = new DataGridView();
            colSeleccionar = new DataGridViewCheckBoxColumn();
            colId = new DataGridViewTextBoxColumn();
            colTipoDocumento = new DataGridViewTextBoxColumn();
            colTitulo = new DataGridViewTextBoxColumn();
            colVerPdf = new DataGridViewButtonColumn();
            btnPaginaAnterior = new Button();
            btnPaginaSiguiente = new Button();
            lblPagina = new Label();
            lblCertificadoTitulo = new Label();
            lblCertificadoSeleccionado = new Label();
            btnSeleccionarCertificado = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvDocumentos).BeginInit();
            SuspendLayout();
            // 
            // btnBuscar
            // 
            btnBuscar.Location = new Point(874, 24);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(140, 44);
            btnBuscar.TabIndex = 0;
            btnBuscar.Text = "Buscar";
            btnBuscar.UseVisualStyleBackColor = true;
            btnBuscar.Click += btnBuscar_Click;
            // 
            // btnFirmarDocumentos
            // 
            btnFirmarDocumentos.Location = new Point(1030, 24);
            btnFirmarDocumentos.Name = "btnFirmarDocumentos";
            btnFirmarDocumentos.Size = new Size(140, 44);
            btnFirmarDocumentos.TabIndex = 1;
            btnFirmarDocumentos.Text = "Firmar";
            btnFirmarDocumentos.UseVisualStyleBackColor = true;
            btnFirmarDocumentos.Click += btnFirmarDocumentos_Click;
            // 
            // btnSalir
            // 
            btnSalir.Location = new Point(1186, 24);
            btnSalir.Name = "btnSalir";
            btnSalir.Size = new Size(132, 44);
            btnSalir.TabIndex = 2;
            btnSalir.Text = "Salir";
            btnSalir.UseVisualStyleBackColor = true;
            btnSalir.Click += btnSalir_Click;
            // 
            // dgvDocumentos
            // 
            dgvDocumentos.AllowUserToAddRows = false;
            dgvDocumentos.AllowUserToDeleteRows = false;
            dgvDocumentos.AllowUserToResizeRows = false;
            dgvDocumentos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvDocumentos.AutoGenerateColumns = false;
            dgvDocumentos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDocumentos.BackgroundColor = SystemColors.Window;
            dgvDocumentos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDocumentos.Columns.AddRange(new DataGridViewColumn[] { colSeleccionar, colId, colTipoDocumento, colTitulo, colVerPdf });
            dgvDocumentos.Location = new Point(24, 92);
            dgvDocumentos.MultiSelect = false;
            dgvDocumentos.Name = "dgvDocumentos";
            dgvDocumentos.RowHeadersVisible = false;
            dgvDocumentos.RowTemplate.Height = 33;
            dgvDocumentos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDocumentos.Size = new Size(1294, 482);
            dgvDocumentos.TabIndex = 3;
            dgvDocumentos.CellContentClick += dgvDocumentos_CellContentClick;
            dgvDocumentos.CurrentCellDirtyStateChanged += dgvDocumentos_CurrentCellDirtyStateChanged;
            // 
            // colSeleccionar
            // 
            colSeleccionar.DataPropertyName = "Seleccionado";
            colSeleccionar.FillWeight = 55F;
            colSeleccionar.HeaderText = "";
            colSeleccionar.MinimumWidth = 50;
            colSeleccionar.Name = "colSeleccionar";
            // 
            // colId
            // 
            colId.DataPropertyName = "Id";
            colId.HeaderText = "Id";
            colId.MinimumWidth = 8;
            colId.Name = "colId";
            colId.Visible = false;
            // 
            // colTipoDocumento
            // 
            colTipoDocumento.DataPropertyName = "TipoDocumento";
            colTipoDocumento.FillWeight = 140F;
            colTipoDocumento.HeaderText = "Tipo de documento";
            colTipoDocumento.MinimumWidth = 8;
            colTipoDocumento.Name = "colTipoDocumento";
            colTipoDocumento.ReadOnly = true;
            // 
            // colTitulo
            // 
            colTitulo.DataPropertyName = "Titulo";
            colTitulo.FillWeight = 220F;
            colTitulo.HeaderText = "Titulo";
            colTitulo.MinimumWidth = 8;
            colTitulo.Name = "colTitulo";
            colTitulo.ReadOnly = true;
            // 
            // colVerPdf
            // 
            colVerPdf.FillWeight = 85F;
            colVerPdf.HeaderText = "";
            colVerPdf.MinimumWidth = 8;
            colVerPdf.Name = "colVerPdf";
            colVerPdf.Text = "Ver PDF";
            colVerPdf.UseColumnTextForButtonValue = true;
            // 
            // btnPaginaAnterior
            // 
            btnPaginaAnterior.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPaginaAnterior.Location = new Point(994, 595);
            btnPaginaAnterior.Name = "btnPaginaAnterior";
            btnPaginaAnterior.Size = new Size(120, 38);
            btnPaginaAnterior.TabIndex = 4;
            btnPaginaAnterior.Text = "Anterior";
            btnPaginaAnterior.UseVisualStyleBackColor = true;
            btnPaginaAnterior.Click += btnPaginaAnterior_Click;
            // 
            // btnPaginaSiguiente
            // 
            btnPaginaSiguiente.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPaginaSiguiente.Location = new Point(1198, 595);
            btnPaginaSiguiente.Name = "btnPaginaSiguiente";
            btnPaginaSiguiente.Size = new Size(120, 38);
            btnPaginaSiguiente.TabIndex = 5;
            btnPaginaSiguiente.Text = "Siguiente";
            btnPaginaSiguiente.UseVisualStyleBackColor = true;
            btnPaginaSiguiente.Click += btnPaginaSiguiente_Click;
            // 
            // lblPagina
            // 
            lblPagina.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblPagina.Location = new Point(817, 601);
            lblPagina.Name = "lblPagina";
            lblPagina.Size = new Size(161, 25);
            lblPagina.TabIndex = 6;
            lblPagina.Text = "Sin busqueda";
            lblPagina.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblCertificadoTitulo
            // 
            lblCertificadoTitulo.AutoSize = true;
            lblCertificadoTitulo.Location = new Point(24, 24);
            lblCertificadoTitulo.Name = "lblCertificadoTitulo";
            lblCertificadoTitulo.Size = new Size(159, 25);
            lblCertificadoTitulo.TabIndex = 7;
            lblCertificadoTitulo.Text = "Certificado actual:";
            // 
            // lblCertificadoSeleccionado
            // 
            lblCertificadoSeleccionado.BorderStyle = BorderStyle.FixedSingle;
            lblCertificadoSeleccionado.Location = new Point(24, 52);
            lblCertificadoSeleccionado.Name = "lblCertificadoSeleccionado";
            lblCertificadoSeleccionado.Size = new Size(676, 28);
            lblCertificadoSeleccionado.TabIndex = 8;
            lblCertificadoSeleccionado.Text = "No seleccionado";
            // 
            // btnSeleccionarCertificado
            // 
            btnSeleccionarCertificado.Location = new Point(716, 45);
            btnSeleccionarCertificado.Name = "btnSeleccionarCertificado";
            btnSeleccionarCertificado.Size = new Size(140, 40);
            btnSeleccionarCertificado.TabIndex = 9;
            btnSeleccionarCertificado.Text = "Seleccionar";
            btnSeleccionarCertificado.UseVisualStyleBackColor = true;
            btnSeleccionarCertificado.Click += btnSeleccionarCertificado_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1342, 657);
            Controls.Add(btnSeleccionarCertificado);
            Controls.Add(lblCertificadoSeleccionado);
            Controls.Add(lblCertificadoTitulo);
            Controls.Add(lblPagina);
            Controls.Add(btnPaginaSiguiente);
            Controls.Add(btnPaginaAnterior);
            Controls.Add(dgvDocumentos);
            Controls.Add(btnSalir);
            Controls.Add(btnFirmarDocumentos);
            Controls.Add(btnBuscar);
            Name = "MainForm";
            Text = "Firmador";
            ((System.ComponentModel.ISupportInitialize)dgvDocumentos).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnBuscar;
        private Button btnFirmarDocumentos;
        private Button btnSalir;
        private DataGridView dgvDocumentos;
        private DataGridViewCheckBoxColumn colSeleccionar;
        private DataGridViewTextBoxColumn colId;
        private DataGridViewTextBoxColumn colTipoDocumento;
        private DataGridViewTextBoxColumn colTitulo;
        private DataGridViewButtonColumn colVerPdf;
        private Button btnPaginaAnterior;
        private Button btnPaginaSiguiente;
        private Label lblPagina;
        private Label lblCertificadoTitulo;
        private Label lblCertificadoSeleccionado;
        private Button btnSeleccionarCertificado;
    }
}
