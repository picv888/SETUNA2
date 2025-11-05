namespace SETUNA
{
    sealed partial class Mainform
    {
        private global::System.ComponentModel.IContainer _components;
        private global::System.Windows.Forms.Button _button1;
        private global::System.Windows.Forms.Button _button4;
        private global::System.Windows.Forms.NotifyIcon _setunaIcon;
        private global::SETUNA.Main.ContextStyleMenuStrip _setunaIconMenu;
        private global::SETUNA.Main.ContextStyleMenuStrip _subMenu;
        private global::System.Windows.Forms.ToolStripMenuItem _testToolStripMenuItem;
        private global::System.Windows.Forms.Timer _imgPoolTimer;
        private global::System.Windows.Forms.ToolTip _toolTip1;
        private global::System.Windows.Forms.Timer _windowTimer;
        private System.Windows.Forms.Timer _delayInitTimer;

        protected override void Dispose(bool disposing)
        {
            if (disposing && _components != null)
            {
                _components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Mainform));
            _button1 = new System.Windows.Forms.Button();
            _button4 = new System.Windows.Forms.Button();
            _imgPoolTimer = new System.Windows.Forms.Timer(_components);
            _windowTimer = new System.Windows.Forms.Timer(_components);
            _setunaIcon = new System.Windows.Forms.NotifyIcon(_components);
            _setunaIconMenu = new SETUNA.Main.ContextStyleMenuStrip(_components);
            _subMenu = new SETUNA.Main.ContextStyleMenuStrip(_components);
            _testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            _toolTip1 = new System.Windows.Forms.ToolTip(_components);
            _delayInitTimer = new System.Windows.Forms.Timer(_components);
            _subMenu.SuspendLayout();
            SuspendLayout();

            _button1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            _button1.Font = new System.Drawing.Font("微软雅黑", 14F);
            _button1.ForeColor = System.Drawing.Color.Gray;
            _button1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            _button1.Location = new System.Drawing.Point(0, 0);
            _button1.Name = "button1";
            _button1.Size = new System.Drawing.Size(253, 54);
            _button1.TabIndex = 0;
            _button1.Text = "截取";
            _button1.UseVisualStyleBackColor = true;
            _button1.Click += OnButton1Click;

            _button4.Dock = System.Windows.Forms.DockStyle.Right;
            _button4.Font = new System.Drawing.Font("微软雅黑", 9F);
            _button4.ForeColor = System.Drawing.Color.Gray;
            _button4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            _button4.Location = new System.Drawing.Point(252, 0);
            _button4.Name = "button4";
            _button4.Size = new System.Drawing.Size(44, 54);
            _button4.TabIndex = 4;
            _button4.Text = "选项";
            _button4.UseVisualStyleBackColor = true;
            _button4.Click += OnButton4Click;

            _imgPoolTimer.Tick += OnImgPoolTimerTick;
            _windowTimer.Enabled = true;
            _windowTimer.Interval = 500;
            _windowTimer.Tick += OnWindowTimerTick;

            _setunaIcon.ContextMenuStrip = _setunaIconMenu;
            _setunaIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("setunaIcon.Icon")));
            _setunaIcon.Text = "SETUNA2";
            _setunaIcon.DoubleClick += OnSetunaIconMouseDoubleClick;
            _setunaIcon.MouseClick += OnSetunaIconMouseClick;

            _setunaIconMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            _setunaIconMenu.Name = "setunaIconMenu";
            _setunaIconMenu.Scrap = null;
            _setunaIconMenu.Size = new System.Drawing.Size(61, 4);
            _setunaIconMenu.Opening += OnSetunaIconMenuOpening;

            _subMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            _subMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            _testToolStripMenuItem});
            _subMenu.Name = "subMenu";
            _subMenu.Scrap = null;
            _subMenu.Size = new System.Drawing.Size(148, 50);

            _testToolStripMenuItem.Name = "testToolStripMenuItem";
            _testToolStripMenuItem.Size = new System.Drawing.Size(147, 46);
            _testToolStripMenuItem.Text = "test";

            _toolTip1.IsBalloon = true;
            _toolTip1.ShowAlways = true;
            _toolTip1.StripAmpersands = true;
            _toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;

            _delayInitTimer.Interval = 1000;
            _delayInitTimer.Tick += OnDelayInitTimerTick;

            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            ClientSize = new System.Drawing.Size(296, 54);
            ContextMenuStrip = _setunaIconMenu;
            Controls.Add(_button4);
            Controls.Add(_button1);
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            MaximizeBox = false;
            MinimumSize = new System.Drawing.Size(100, 60);
            Name = "Mainform";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "SETUNA";
            TopMost = true;
            FormClosing += OnMainformClosing;
            Load += OnMainformLoad;
            _subMenu.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
