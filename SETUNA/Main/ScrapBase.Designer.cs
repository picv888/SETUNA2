namespace SETUNA.Main
{
	sealed partial class ScrapBase
	{
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.OpacityTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // timOpacity
            // 
            this.OpacityTimer.Interval = 15;
            this.OpacityTimer.Tick += OnOpacityTimerTick;
            // 
            // ScrapBase
            // 
            this.AllowDrop = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Gray;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1, 1);
            this.Name = "ScrapBase";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.Activated += OnScrapActivated;
            this.Deactivate += OnScrapDeactivate;
            this.SizeChanged += OnScrapSizeChanged;
            this.VisibleChanged += OnScrapVisibleChanged;
            this.DragDrop += OnScrapDragEnd;
            this.DragEnter += OnScrapDragBegin;
            this.KeyPress += OnScrapKeyPress;
            this.MouseClick += OnScrapMouseClick;
            this.MouseDoubleClick += OnScrapMouseDoubleClick;
            this.MouseDown += OnScrapMouseDown;
            this.MouseEnter += OnScrapMouseEnter;
            this.MouseLeave += OnScrapMouseLeave;
            this.MouseMove += OnScrapMouseMove;
            this.MouseUp += OnScrapMouseUp;
            this.ResumeLayout(false);
		}

		private global::System.Windows.Forms.Timer OpacityTimer;
		private global::System.ComponentModel.IContainer components;
	}
}
