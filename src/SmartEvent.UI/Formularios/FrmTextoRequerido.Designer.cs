namespace SmartEvent.UI.Formularios;

partial class FrmTextoRequerido
{
    private System.ComponentModel.IContainer components = null;

    private Label lblInstruccion;
    private TextBox txtTexto;
    private Label lblContador;
    private Button btnAceptar;
    private Button btnCancelar;

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
        this.lblInstruccion = new Label();
        this.txtTexto = new TextBox();
        this.lblContador = new Label();
        this.btnAceptar = new Button();
        this.btnCancelar = new Button();
        this.SuspendLayout();
        //
        // lblInstruccion
        //
        this.lblInstruccion.Location = new System.Drawing.Point(14, 12);
        this.lblInstruccion.Name = "lblInstruccion";
        this.lblInstruccion.Size = new System.Drawing.Size(470, 40);
        this.lblInstruccion.TabIndex = 0;
        //
        // txtTexto
        //
        this.txtTexto.Location = new System.Drawing.Point(14, 55);
        this.txtTexto.MaxLength = 500;
        this.txtTexto.Multiline = true;
        this.txtTexto.Name = "txtTexto";
        this.txtTexto.ScrollBars = ScrollBars.Vertical;
        this.txtTexto.Size = new System.Drawing.Size(470, 110);
        this.txtTexto.TabIndex = 1;
        this.txtTexto.TextChanged += new System.EventHandler(this.txtTexto_TextChanged);
        //
        // lblContador
        //
        this.lblContador.AutoSize = true;
        this.lblContador.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblContador.Location = new System.Drawing.Point(14, 172);
        this.lblContador.Name = "lblContador";
        this.lblContador.Size = new System.Drawing.Size(0, 15);
        this.lblContador.TabIndex = 2;
        //
        // btnAceptar
        //
        this.btnAceptar.Enabled = false;
        this.btnAceptar.Location = new System.Drawing.Point(298, 196);
        this.btnAceptar.Name = "btnAceptar";
        this.btnAceptar.Size = new System.Drawing.Size(90, 30);
        this.btnAceptar.TabIndex = 3;
        this.btnAceptar.Text = "&Aceptar";
        this.btnAceptar.UseVisualStyleBackColor = true;
        this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
        //
        // btnCancelar
        //
        this.btnCancelar.DialogResult = DialogResult.Cancel;
        this.btnCancelar.Location = new System.Drawing.Point(394, 196);
        this.btnCancelar.Name = "btnCancelar";
        this.btnCancelar.Size = new System.Drawing.Size(90, 30);
        this.btnCancelar.TabIndex = 4;
        this.btnCancelar.Text = "&Cancelar";
        this.btnCancelar.UseVisualStyleBackColor = true;
        //
        // FrmTextoRequerido
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.CancelButton = this.btnCancelar;
        this.ClientSize = new System.Drawing.Size(498, 238);
        this.Controls.Add(this.btnCancelar);
        this.Controls.Add(this.btnAceptar);
        this.Controls.Add(this.lblContador);
        this.Controls.Add(this.txtTexto);
        this.Controls.Add(this.lblInstruccion);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FrmTextoRequerido";
        this.ShowInTaskbar = false;
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "SmartEvent";
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
