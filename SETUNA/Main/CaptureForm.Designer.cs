namespace SETUNA.Main
{
	// Token: 0x02000046 RID: 70
	sealed partial class CaptureForm
	{
		private void InitializeComponent()
		{
            this.SuspendLayout();
            // 
            // CaptureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(389, 332);
            this.ControlBox = false;
            this.Cursor = System.Windows.Forms.Cursors.Cross;
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "CaptureForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "CaptureForm";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CaptureForm_FormClosing);
            this.VisibleChanged += new System.EventHandler(this.CaptureForm_VisibleChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.CaptureForm_Paint);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CaptureForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CaptureForm_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CaptureForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CaptureForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CaptureForm_MouseUp);
            this.ResumeLayout(false);
		}
    }
}
