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
            colVerPdf = new DataGridViewButtonColumn();
            btnPaginaAnterior = new Button();
            btnPaginaSiguiente = new Button();
            lblPagina = new Label();
            lblTotalDocumentos = new Label();
            lblCertificadoTitulo = new Label();
            lblCertificadoSeleccionado = new Label();
            btnSeleccionarCertificado = new Button();
            btnMarcarTodos = new Button();
            btnLimpiarSeleccion = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvDocumentos).BeginInit();
            SuspendLayout();
            // 
            // btnBuscar
            // 
            btnBuscar.Location = new Point(1099, 109);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(140, 44);
            btnBuscar.TabIndex = 2;
            btnBuscar.Text = "Buscar";
            btnBuscar.UseVisualStyleBackColor = true;
            btnBuscar.Click += btnBuscar_Click;
            // 
            // btnFirmarDocumentos
            // 
            btnFirmarDocumentos.Location = new Point(1255, 109);
            btnFirmarDocumentos.Name = "btnFirmarDocumentos";
            btnFirmarDocumentos.Size = new Size(140, 44);
            btnFirmarDocumentos.TabIndex = 3;
            btnFirmarDocumentos.Text = "Firmar";
            btnFirmarDocumentos.UseVisualStyleBackColor = true;
            btnFirmarDocumentos.Click += btnFirmarDocumentos_Click;
            // 
            // btnSalir
            // 
            btnSalir.Location = new Point(1275, 12);
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
            dgvDocumentos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDocumentos.BackgroundColor = SystemColors.Window;
            dgvDocumentos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDocumentos.Columns.AddRange(new DataGridViewColumn[] { colVerPdf });
            dgvDocumentos.Location = new Point(24, 168);
            dgvDocumentos.MultiSelect = false;
            dgvDocumentos.Name = "dgvDocumentos";
            dgvDocumentos.RowHeadersVisible = false;
            dgvDocumentos.RowHeadersWidth = 62;
            dgvDocumentos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDocumentos.Size = new Size(1371, 449);
            dgvDocumentos.TabIndex = 3;
            dgvDocumentos.CellContentClick += dgvDocumentos_CellContentClick;
            dgvDocumentos.CurrentCellDirtyStateChanged += dgvDocumentos_CurrentCellDirtyStateChanged;
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
            btnPaginaAnterior.Location = new Point(1071, 638);
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
            btnPaginaSiguiente.Location = new Point(1275, 638);
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
            lblPagina.Location = new Point(894, 644);
            lblPagina.Name = "lblPagina";
            lblPagina.Size = new Size(161, 25);
            lblPagina.TabIndex = 6;
            lblPagina.Text = "Sin busqueda";
            lblPagina.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTotalDocumentos
            // 
            lblTotalDocumentos.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblTotalDocumentos.Location = new Point(27, 645);
            lblTotalDocumentos.Name = "lblTotalDocumentos";
            lblTotalDocumentos.Size = new Size(311, 25);
            lblTotalDocumentos.TabIndex = 10;
            lblTotalDocumentos.Text = "Total de documentos: 0";
            lblTotalDocumentos.TextAlign = ContentAlignment.MiddleLeft;
            lblTotalDocumentos.Click += lblTotalDocumentos_Click;
            // 
            // lblCertificadoTitulo
            // 
            lblCertificadoTitulo.AutoSize = true;
            lblCertificadoTitulo.Location = new Point(24, 24);
            lblCertificadoTitulo.Name = "lblCertificadoTitulo";
            lblCertificadoTitulo.Size = new Size(152, 25);
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
            // btnMarcarTodos
            // 
            btnMarcarTodos.Font = new Font("Segoe UI", 8F);
            btnMarcarTodos.Location = new Point(47, 109);
            btnMarcarTodos.Margin = new Padding(3, 1, 3, 1);
            btnMarcarTodos.Name = "btnMarcarTodos";
            btnMarcarTodos.Size = new Size(97, 55);
            btnMarcarTodos.TabIndex = 0;
            btnMarcarTodos.Text = "Marcar todos";
            btnMarcarTodos.UseVisualStyleBackColor = true;
            btnMarcarTodos.Click += btnMarcarTodos_Click;
            // 
            // btnLimpiarSeleccion
            // 
            btnLimpiarSeleccion.Font = new Font("Segoe UI", 8F);
            btnLimpiarSeleccion.Location = new Point(170, 109);
            btnLimpiarSeleccion.Margin = new Padding(3, 1, 3, 1);
            btnLimpiarSeleccion.Name = "btnLimpiarSeleccion";
            btnLimpiarSeleccion.Size = new Size(113, 55);
            btnLimpiarSeleccion.TabIndex = 1;
            btnLimpiarSeleccion.Text = "Limpiar selección";
            btnLimpiarSeleccion.UseVisualStyleBackColor = true;
            btnLimpiarSeleccion.Click += btnLimpiarSeleccion_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1419, 700);
            Controls.Add(btnLimpiarSeleccion);
            Controls.Add(btnMarcarTodos);
            Controls.Add(lblTotalDocumentos);
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
        private Label lblTotalDocumentos;
        private Label lblCertificadoTitulo;
        private Label lblCertificadoSeleccionado;
        private Button btnSeleccionarCertificado;
        private Button btnMarcarTodos;
        private Button btnLimpiarSeleccion;
    }
}

