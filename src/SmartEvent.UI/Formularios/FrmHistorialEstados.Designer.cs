namespace SmartEvent.UI.Formularios;

partial class FrmHistorialEstados
{
    private System.ComponentModel.IContainer components = null;

    private Label lblEncabezado;
    private DataGridView dgvHistorial;
    private Panel pnlPie;
    private Label lblResumen;
    private Button btnCerrar;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.lblEncabezado = new Label();
        this.dgvHistorial = new DataGridView();
        this.pnlPie = new Panel();
        this.lblResumen = new Label();
        this.btnCerrar = new Button();
        ((System.ComponentModel.ISupportInitialize)(this.dgvHistorial)).BeginInit();
        this.pnlPie.SuspendLayout();
        this.SuspendLayout();
        //
        // lblEncabezado
        //
        this.lblEncabezado.Dock = DockStyle.Top;
        this.lblEncabezado.Location = new System.Drawing.Point(0, 0);
        this.lblEncabezado.Name = "lblEncabezado";
        this.lblEncabezado.Padding = new Padding(12, 10, 12, 8);
        this.lblEncabezado.Size = new System.Drawing.Size(760, 52);
        this.lblEncabezado.TabIndex = 0;
        this.lblEncabezado.Text = "Cada fila es un cambio de estado registrado por el propio motor de base de datos.";
        //
        // dgvHistorial
        //
        this.dgvHistorial.AllowUserToAddRows = false;
        this.dgvHistorial.AllowUserToDeleteRows = false;
        this.dgvHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvHistorial.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvHistorial.Dock = DockStyle.Fill;
        this.dgvHistorial.Location = new System.Drawing.Point(0, 52);
        this.dgvHistorial.MultiSelect = false;
        this.dgvHistorial.Name = "dgvHistorial";
        this.dgvHistorial.ReadOnly = true;
        this.dgvHistorial.RowHeadersVisible = false;
        this.dgvHistorial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        this.dgvHistorial.Size = new System.Drawing.Size(760, 296);
        this.dgvHistorial.TabIndex = 1;
        this.dgvHistorial.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dgvHistorial_CellFormatting);
        //
        // pnlPie
        //
        this.pnlPie.Controls.Add(this.btnCerrar);
        this.pnlPie.Controls.Add(this.lblResumen);
        this.pnlPie.Dock = DockStyle.Bottom;
        this.pnlPie.Location = new System.Drawing.Point(0, 348);
        this.pnlPie.Name = "pnlPie";
        this.pnlPie.Size = new System.Drawing.Size(760, 52);
        this.pnlPie.TabIndex = 2;
        //
        // lblResumen
        //
        this.lblResumen.Location = new System.Drawing.Point(12, 16);
        this.lblResumen.Name = "lblResumen";
        this.lblResumen.Size = new System.Drawing.Size(620, 22);
        this.lblResumen.TabIndex = 0;
        //
        // btnCerrar
        //
        this.btnCerrar.DialogResult = DialogResult.OK;
        this.btnCerrar.Location = new System.Drawing.Point(654, 12);
        this.btnCerrar.Name = "btnCerrar";
        this.btnCerrar.Size = new System.Drawing.Size(90, 28);
        this.btnCerrar.TabIndex = 1;
        this.btnCerrar.Text = "&Cerrar";
        this.btnCerrar.UseVisualStyleBackColor = true;
        //
        // FrmHistorialEstados
        //
        this.AcceptButton = this.btnCerrar;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.CancelButton = this.btnCerrar;
        this.ClientSize = new System.Drawing.Size(760, 400);
        this.Controls.Add(this.dgvHistorial);
        this.Controls.Add(this.pnlPie);
        this.Controls.Add(this.lblEncabezado);
        this.MinimizeBox = false;
        this.Name = "FrmHistorialEstados";
        this.ShowInTaskbar = false;
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "Historial de estados";
        this.Load += new System.EventHandler(this.FrmHistorialEstados_Load);
        ((System.ComponentModel.ISupportInitialize)(this.dgvHistorial)).EndInit();
        this.pnlPie.ResumeLayout(false);
        this.ResumeLayout(false);
    }
}
