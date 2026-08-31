namespace SmartEvent.UI.Formularios;

partial class FrmAnalisisIA
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlEncabezado;
    private Label lblNivelRiesgo;
    private Label lblResumen;
    private GroupBox grpAlertas;
    private TextBox txtAlertas;
    private GroupBox grpRecomendaciones;
    private TextBox txtRecomendaciones;
    private GroupBox grpCorreoSugerido;
    private TextBox txtCorreoSugerido;
    private Label lblAviso;
    private Label lblMetadatos;
    private Button btnCopiar;
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
        this.pnlEncabezado = new Panel();
        this.lblResumen = new Label();
        this.lblNivelRiesgo = new Label();
        this.grpAlertas = new GroupBox();
        this.txtAlertas = new TextBox();
        this.grpRecomendaciones = new GroupBox();
        this.txtRecomendaciones = new TextBox();
        this.grpCorreoSugerido = new GroupBox();
        this.txtCorreoSugerido = new TextBox();
        this.lblAviso = new Label();
        this.lblMetadatos = new Label();
        this.btnCopiar = new Button();
        this.btnCerrar = new Button();
        this.pnlEncabezado.SuspendLayout();
        this.grpAlertas.SuspendLayout();
        this.grpRecomendaciones.SuspendLayout();
        this.grpCorreoSugerido.SuspendLayout();
        this.SuspendLayout();
        //
        // pnlEncabezado
        //
        this.pnlEncabezado.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
        this.pnlEncabezado.Controls.Add(this.lblResumen);
        this.pnlEncabezado.Controls.Add(this.lblNivelRiesgo);
        this.pnlEncabezado.Location = new System.Drawing.Point(12, 12);
        this.pnlEncabezado.Name = "pnlEncabezado";
        this.pnlEncabezado.Size = new System.Drawing.Size(640, 86);
        this.pnlEncabezado.TabIndex = 0;
        //
        // lblNivelRiesgo
        //
        this.lblNivelRiesgo.AutoSize = true;
        this.lblNivelRiesgo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
        this.lblNivelRiesgo.Location = new System.Drawing.Point(12, 10);
        this.lblNivelRiesgo.Name = "lblNivelRiesgo";
        this.lblNivelRiesgo.Size = new System.Drawing.Size(120, 21);
        this.lblNivelRiesgo.TabIndex = 0;
        this.lblNivelRiesgo.Text = "Nivel de riesgo";
        //
        // lblResumen
        //
        this.lblResumen.Location = new System.Drawing.Point(14, 36);
        this.lblResumen.Name = "lblResumen";
        this.lblResumen.Size = new System.Drawing.Size(614, 44);
        this.lblResumen.TabIndex = 1;
        //
        // grpAlertas
        //
        this.grpAlertas.Controls.Add(this.txtAlertas);
        this.grpAlertas.Location = new System.Drawing.Point(12, 104);
        this.grpAlertas.Padding = new Padding(8, 6, 8, 8);
        this.grpAlertas.Name = "grpAlertas";
        this.grpAlertas.Size = new System.Drawing.Size(314, 132);
        this.grpAlertas.TabIndex = 1;
        this.grpAlertas.TabStop = false;
        this.grpAlertas.Text = "Alertas";
        //
        this.txtAlertas.BackColor = System.Drawing.SystemColors.Window;
        this.txtAlertas.BorderStyle = BorderStyle.None;
        this.txtAlertas.Dock = DockStyle.Fill;
        this.txtAlertas.Location = new System.Drawing.Point(8, 22);
        this.txtAlertas.Multiline = true;
        this.txtAlertas.Name = "txtAlertas";
        this.txtAlertas.ReadOnly = true;
        this.txtAlertas.ScrollBars = ScrollBars.Vertical;
        this.txtAlertas.Size = new System.Drawing.Size(298, 102);
        this.txtAlertas.TabIndex = 0;
        //
        // grpRecomendaciones
        //
        this.grpRecomendaciones.Controls.Add(this.txtRecomendaciones);
        this.grpRecomendaciones.Location = new System.Drawing.Point(338, 104);
        this.grpRecomendaciones.Padding = new Padding(8, 6, 8, 8);
        this.grpRecomendaciones.Name = "grpRecomendaciones";
        this.grpRecomendaciones.Size = new System.Drawing.Size(314, 132);
        this.grpRecomendaciones.TabIndex = 2;
        this.grpRecomendaciones.TabStop = false;
        this.grpRecomendaciones.Text = "Recomendaciones";
        //
        this.txtRecomendaciones.BackColor = System.Drawing.SystemColors.Window;
        this.txtRecomendaciones.BorderStyle = BorderStyle.None;
        this.txtRecomendaciones.Dock = DockStyle.Fill;
        this.txtRecomendaciones.Location = new System.Drawing.Point(8, 22);
        this.txtRecomendaciones.Multiline = true;
        this.txtRecomendaciones.Name = "txtRecomendaciones";
        this.txtRecomendaciones.ReadOnly = true;
        this.txtRecomendaciones.ScrollBars = ScrollBars.Vertical;
        this.txtRecomendaciones.Size = new System.Drawing.Size(298, 102);
        this.txtRecomendaciones.TabIndex = 0;
        //
        // grpCorreoSugerido
        //
        this.grpCorreoSugerido.Controls.Add(this.txtCorreoSugerido);
        this.grpCorreoSugerido.Location = new System.Drawing.Point(12, 242);
        this.grpCorreoSugerido.Name = "grpCorreoSugerido";
        this.grpCorreoSugerido.Size = new System.Drawing.Size(640, 140);
        this.grpCorreoSugerido.TabIndex = 3;
        this.grpCorreoSugerido.TabStop = false;
        this.grpCorreoSugerido.Text = "Borrador de correo sugerido";
        //
        this.txtCorreoSugerido.BackColor = System.Drawing.SystemColors.Window;
        this.txtCorreoSugerido.Dock = DockStyle.Fill;
        this.txtCorreoSugerido.Location = new System.Drawing.Point(3, 19);
        this.txtCorreoSugerido.Multiline = true;
        this.txtCorreoSugerido.Name = "txtCorreoSugerido";
        this.txtCorreoSugerido.ReadOnly = true;
        this.txtCorreoSugerido.ScrollBars = ScrollBars.Vertical;
        this.txtCorreoSugerido.Size = new System.Drawing.Size(634, 118);
        this.txtCorreoSugerido.TabIndex = 0;
        //
        // lblAviso
        //
        this.lblAviso.ForeColor = System.Drawing.Color.FromArgb(150, 90, 0);
        this.lblAviso.Location = new System.Drawing.Point(12, 388);
        this.lblAviso.Name = "lblAviso";
        this.lblAviso.Size = new System.Drawing.Size(640, 32);
        this.lblAviso.TabIndex = 4;
        this.lblAviso.Text = "Este analisis es orientativo y no se envia automaticamente. La decision de confirmar, " +
                             "cancelar o modificar la reserva es siempre suya.";
        //
        // lblMetadatos
        //
        this.lblMetadatos.AutoSize = true;
        this.lblMetadatos.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblMetadatos.Location = new System.Drawing.Point(12, 428);
        this.lblMetadatos.Name = "lblMetadatos";
        this.lblMetadatos.Size = new System.Drawing.Size(0, 15);
        this.lblMetadatos.TabIndex = 5;
        //
        // btnCopiar
        //
        this.btnCopiar.Location = new System.Drawing.Point(450, 424);
        this.btnCopiar.Name = "btnCopiar";
        this.btnCopiar.Size = new System.Drawing.Size(140, 28);
        this.btnCopiar.TabIndex = 6;
        this.btnCopiar.Text = "Copiar &borrador";
        this.btnCopiar.UseVisualStyleBackColor = true;
        this.btnCopiar.Click += new System.EventHandler(this.btnCopiar_Click);
        //
        // btnCerrar
        //
        this.btnCerrar.DialogResult = DialogResult.OK;
        this.btnCerrar.Location = new System.Drawing.Point(596, 424);
        this.btnCerrar.Name = "btnCerrar";
        this.btnCerrar.Size = new System.Drawing.Size(56, 28);
        this.btnCerrar.TabIndex = 7;
        this.btnCerrar.Text = "&Cerrar";
        this.btnCerrar.UseVisualStyleBackColor = true;
        //
        // FrmAnalisisIA
        //
        this.AcceptButton = this.btnCerrar;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.CancelButton = this.btnCerrar;
        this.ClientSize = new System.Drawing.Size(664, 464);
        this.Controls.Add(this.btnCerrar);
        this.Controls.Add(this.btnCopiar);
        this.Controls.Add(this.lblMetadatos);
        this.Controls.Add(this.lblAviso);
        this.Controls.Add(this.grpCorreoSugerido);
        this.Controls.Add(this.grpRecomendaciones);
        this.Controls.Add(this.grpAlertas);
        this.Controls.Add(this.pnlEncabezado);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FrmAnalisisIA";
        this.ShowInTaskbar = false;
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "Analisis de la reserva con IA";
        this.pnlEncabezado.ResumeLayout(false);
        this.pnlEncabezado.PerformLayout();
        this.grpAlertas.ResumeLayout(false);
        this.grpRecomendaciones.ResumeLayout(false);
        this.grpCorreoSugerido.ResumeLayout(false);
        this.grpCorreoSugerido.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
