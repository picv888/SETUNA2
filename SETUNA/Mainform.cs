using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using com.clearunit;
using SETUNA.Main;
using SETUNA.Main.KeyItems;
using SETUNA.Main.Option;
using SETUNA.Main.Style;

namespace SETUNA
{
    public sealed partial class Mainform : BaseForm, IScrapKeyPressEventListener, IScrapMenuListener, ISingletonForm
    {
        private SplashForm frmSplash;
        private ClickCapture frmClickCapture;
        private static CaptureForm cap_form;
        private List<ScrapSource> _imgPool;
        private bool _iscapture;
        private bool _isoption;
        private bool _isstart;
        private List<Form> forms = new List<Form>();
        private bool allScrapActive = true;

        private delegate void ExternalStartupDelegate(string version, string[] args);

        public ScrapManager scrapBook;
        public SetunaOption optSetuna;
        public KeyItemBook keyBook;
        public Queue<ScrapBase> dustbox;

        public delegate void MouseWheelCallback(object sender, MouseEventArgs e);
        public event MouseWheelCallback MouseWheelCallbackEvent;

        public static Mainform Instance { private set; get; }

        public Mainform()
        {
            Instance = this;

            _isstart = false;
            _iscapture = false;
            _isoption = false;
            InitializeComponent();
            scrapBook = new ScrapManager(this);
            scrapBook.AddKeyPressListener(this);
            dustbox = new Queue<ScrapBase>();
            scrapBook.DustBox = dustbox;
            scrapBook.DustBoxCapacity = 5;
            _imgPool = new List<ScrapSource>();
            SetSubMenu();

            Text = $"SETUNA {Application.ProductVersion}";

            NetUtils.Init();
        }

        public bool IsStart
        {
            get => _isstart;
            set
            {
                _isstart = value;
                if (value && _imgPool.Count > 0)
                {
                    _imgPoolTimer.Start();
                }
            }
        }

        public bool IsCapture
        {
            get => _iscapture;
            set
            {
                _iscapture = value;
                if (!value && _imgPool.Count > 0)
                {
                    _imgPoolTimer.Start();
                }
            }
        }

        public bool IsOption
        {
            get => _isoption;
            set
            {
                _isoption = value;
                if (!value && _imgPool.Count > 0)
                {
                    _imgPoolTimer.Start();
                }
            }
        }

        private void SetSubMenu()
        {
            _setunaIconMenu.Items.Clear();
            _setunaIconMenu.Items.Add(new CScrapListStyle().GetToolStrip(scrapBook));
            _setunaIconMenu.Items.Add(new CDustBoxStyle().GetToolStrip(scrapBook));
            _setunaIconMenu.Items.Add(new CDustEraseStyle().GetToolStrip());
            _setunaIconMenu.Items.Add(new CDustScrapStyle().GetToolStrip());
            _setunaIconMenu.Items.Add(new ToolStripSeparator());
            _setunaIconMenu.Items.Add(new CCaptureStyle().GetToolStrip());
            _setunaIconMenu.Items.Add(new CPasteStyle().GetToolStrip());
            _setunaIconMenu.Items.Add(new ToolStripSeparator());
            _setunaIconMenu.Items.Add(new CShowVersionStyle().GetToolStrip());
            _setunaIconMenu.Items.Add(new COptionStyle().GetToolStrip());
            _setunaIconMenu.Items.Add(new ToolStripSeparator());
            _setunaIconMenu.Items.Add(new CShutDownStyle().GetToolStrip());
        }

        public void StartCapture()
        {
            if (IsCapture || Mainform.cap_form == null || IsOption)
            {
                return;
            }
            try
            {
                if (frmClickCapture != null)
                {
                    frmClickCapture.Stop();
                }
                IsCapture = true;
                Console.WriteLine(string.Concat(new object[]
                {
                    "9 - ",
                    DateTime.Now.ToString(),
                    " ",
                    DateTime.Now.Millisecond
                }));
                Mainform.cap_form.OnCaptureClose = new CaptureForm.CaptureClosedDelegate(EndCapture);
                Mainform.cap_form.ShowCapture(optSetuna.Setuna);
                Console.WriteLine(string.Concat(new object[]
                {
                    "16 - ",
                    DateTime.Now.ToString(),
                    " ",
                    DateTime.Now.Millisecond
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mainform StartCapture Exception:" + ex.Message);
                IsCapture = false;
                if (Mainform.cap_form != null)
                {
                    Mainform.cap_form.DialogResult = DialogResult.Cancel;
                }
                EndCapture(Mainform.cap_form);
            }
        }

        // Token: 0x060001F7 RID: 503 RVA: 0x0000A89C File Offset: 0x00008A9C
        private void EndCapture(CaptureForm cform)
        {
            try
            {
                Console.WriteLine("Mainform EndCapture Start---");
                if (cform.DialogResult == DialogResult.OK)
                {
                    using (var clipBitmap = cform.ClipBitmap)
                    {
                        if (clipBitmap != null)
                        {
                            scrapBook.AddScrap(clipBitmap, cform.ClipStart.X, cform.ClipStart.Y, cform.ClipSize.Width, cform.ClipSize.Height);
                        }
                    }
                }
                cform.Hide();
                Cursor.Clip = Rectangle.Empty;
                Console.WriteLine("Mainform EndCapture End---");
            }
            catch (Exception ex)
            {
                Console.WriteLine("MainForm EndCapture Exception:" + ex.Message);
            }
            finally
            {
                IsCapture = false;
                if (frmClickCapture != null)
                {
                    frmClickCapture.Restart();
                }
            }
        }

        public void Option()
        {
            if (IsCapture)
            {
                return;
            }
            IsOption = true;
            var opt = (SetunaOption)optSetuna.Clone();
            var list = new List<ScrapBase>();
            try
            {
                foreach (var scrapBase in scrapBook)
                {
                    if (scrapBase.Visible && scrapBase.TopMost)
                    {
                        list.Add(scrapBase);
                    }
                }
                foreach (var scrapBase2 in list)
                {
                    scrapBase2.TopMost = false;
                }
                base.TopMost = false;
                if (frmClickCapture != null)
                {
                    frmClickCapture.Stop();
                }
                var optionForm = new OptionForm(opt)
                {
                    StartPosition = FormStartPosition.CenterScreen
                };
                optionForm.ShowDialog();
                if (optionForm.DialogResult == DialogResult.OK)
                {
                    optSetuna = optionForm.Option;
                    OptionApply();
                }
                if (!optSetuna.RegistHotKey(base.Handle, HotKeyID.Capture))
                {
                    optSetuna.ScrapHotKeyEnable = false;
                    new HotkeyMsg
                    {
                        HotKey = optSetuna.ScrapHotKeys[(int)HotKeyID.Capture]
                    }.ShowDialog();
                }
                if (!optSetuna.RegistHotKey(base.Handle, HotKeyID.Function1))
                {
                    optSetuna.ScrapHotKeyEnable = false;
                    new HotkeyMsg
                    {
                        HotKey = optSetuna.ScrapHotKeys[(int)HotKeyID.Function1]
                    }.ShowDialog();
                }
                if (optionForm.DialogResult == DialogResult.OK)
                {
                    SaveOption();
                }
            }
            finally
            {
                base.TopMost = true;
                foreach (var scrapBase3 in list)
                {
                    scrapBase3.TopMost = true;
                }
                IsOption = false;
            }
        }

        private void OptionApply()
        {
            try
            {
                keyBook = optSetuna.GetKeyItemBook();
                if (optSetuna.Setuna.DustBoxEnable)
                {
                    scrapBook.DustBoxCapacity = (short)optSetuna.Setuna.DustBoxCapacity;
                }
                else
                {
                    scrapBook.DustBoxCapacity = 0;
                }
                if (!optSetuna.RegistHotKey(base.Handle, HotKeyID.Capture))
                {
                    optSetuna.ScrapHotKeyEnable = false;
                    new HotkeyMsg
                    {
                        HotKey = optSetuna.ScrapHotKeys[(int)HotKeyID.Capture]
                    }.ShowDialog();
                }
                if (!optSetuna.RegistHotKey(base.Handle, HotKeyID.Function1))
                {
                    optSetuna.ScrapHotKeyEnable = false;
                    new HotkeyMsg
                    {
                        HotKey = optSetuna.ScrapHotKeys[(int)HotKeyID.Function1]
                    }.ShowDialog();
                }
                if (optSetuna.Setuna.AppType == SetunaOption.SetunaOptionData.ApplicationType.ApplicationMode)
                {
                    base.ShowInTaskbar = true;
                    _setunaIcon.Visible = false;
                    base.MinimizeBox = true;
                    base.Visible = true;
                }
                else
                {
                    _setunaIcon.Visible = true;
                    base.ShowInTaskbar = false;
                    base.MinimizeBox = false;
                    base.WindowState = FormWindowState.Normal;
                    base.Visible = optSetuna.Setuna.ShowMainWindow;
                }
                _subMenu.Items.Clear();
                foreach (var num in optSetuna.Scrap.subMenuStyles)
                {
                    if (num >= 0)
                    {
                        using (var enumerator2 = optSetuna.Styles.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                var cstyle = enumerator2.Current;
                                if (cstyle.StyleID == num)
                                {
                                    _subMenu.Items.Add(cstyle.GetToolStrip());
                                }
                            }
                            continue;
                        }
                    }
                    var preStyle = CPreStyles.GetPreStyle(num);
                    if (preStyle != null)
                    {
                        _subMenu.Items.Add(preStyle.GetToolStrip());
                    }
                }
                if (optSetuna.Setuna.ClickCapture)
                {
                    if (frmClickCapture == null)
                    {
                        frmClickCapture = new ClickCapture(optSetuna.Setuna.ClickCaptureValue);
                        frmClickCapture.ClickCaptureEvent += frmClickCapture_ClickCaptureEvent;
                        frmClickCapture.Show();
                    }
                    else
                    {
                        frmClickCapture.ClickFlags = optSetuna.Setuna.ClickCaptureValue;
                        frmClickCapture.Restart();
                    }
                }
                else if (frmClickCapture != null)
                {
                    frmClickCapture.Close();
                    frmClickCapture.Dispose();
                    frmClickCapture = null;
                }

                _windowTimer.Enabled = optSetuna.Setuna.TopMostEnabled;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mainform OptionApply Exception:" + ex.Message);
            }
        }

        private void frmClickCapture_ClickCaptureEvent(object sender, EventArgs e)
        {
            StartCapture();
        }

        private void CloseSetuna()
        {
            base.Close();
        }

        public void ScrapKeyPress(object sender, ScrapKeyPressEventArgs e)
        {
            var keyItem = keyBook.FindKeyItem(e.key);
            if (keyItem != null)
            {
                var scrapBase = (ScrapBase)sender;
                keyItem.ParentStyle.Apply(scrapBase);
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x0312)
            {
                var id = (HotKeyID)m.WParam;
                switch (id)
                {
                    case HotKeyID.Capture:
                        StartCapture();
                        break;
                    case HotKeyID.Function1:
                        SetAllScrapsActive(!allScrapActive);
                        break;
                }
            }
        }

        private void SaveOption()
        {
            var configFile = SetunaOption.ConfigFile;
            var allType = SetunaOption.GetAllType();
            try
            {
                var xmlSerializer = new XmlSerializer(optSetuna.GetType(), allType);
                var fileStream = new FileStream(configFile, FileMode.Create);
                xmlSerializer.Serialize(fileStream, optSetuna);
                fileStream.Close();
            }
            catch
            {
                MessageBox.Show("无法保存配置文件。", "SETUNA2", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void LoadOption()
        {
            var configFile = SetunaOption.ConfigFile;
            try
            {
                if (!File.Exists(configFile))
                {
                    optSetuna = SetunaOption.GetDefaultOption();
                }
                else
                {
                    var allType = SetunaOption.GetAllType();
                    var xmlSerializer = new XmlSerializer(typeof(SetunaOption), allType);
                    var fileStream = new FileStream(configFile, FileMode.Open);
                    optSetuna = (SetunaOption)xmlSerializer.Deserialize(fileStream);
                    fileStream.Close();
                }
            }
            catch
            {
                optSetuna = SetunaOption.GetDefaultOption();
                MessageBox.Show("无法读取配置文件。\n使用默认设置。", "SETUNA2", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            finally
            {
                OptionApply();
            }
        }

        public void RestoreScrap(object sender, EventArgs e)
        {
            var toolStripMenuItem = (ToolStripMenuItem)sender;
            var list = new List<ScrapBase>();
            while (dustbox.Count > 0)
            {
                var scrapBase = dustbox.Dequeue();
                if (!scrapBase.Equals(toolStripMenuItem.Tag))
                {
                    list.Add(scrapBase);
                }
                else
                {
                    scrapBook.AddScrapThenDo(scrapBase);
                }
            }
            dustbox.Clear();
            foreach (var item in list)
            {
                dustbox.Enqueue(item);
            }
            new ScrapEventArgs();
        }

        private void miCapture_Click(object sender, EventArgs e)
        {
            StartCapture();
        }

        private void miOption_Click(object sender, EventArgs e)
        {
            Option();
        }

        private void miSetunaClose_Click(object sender, EventArgs e)
        {
            CloseSetuna();
        }

        public void OnActiveScrapInList(object sender, EventArgs e)
        {
            var toolStripMenuItem = (ToolStripMenuItem)sender;
            if (toolStripMenuItem.Tag != null)
            {
                var scrapBase = (ScrapBase)toolStripMenuItem.Tag;
                if (scrapBase.Visible)
                {
                    scrapBase.Activate();
                }
                else if (scrapBase.StyleForm is Main.StyleItems.CompactScrap compactScrap)
                {
                    compactScrap.Close();
                }
            }
        }

        private void OnButton1Click(object sender, EventArgs e)
        {
            StartCapture();
        }

        private void OnButton4Click(object sender, EventArgs e)
        {
            Option();
        }

        private void OnMainformClosing(object sender, FormClosingEventArgs e)
        {
            foreach (HotKeyID item in Enum.GetValues(typeof(HotKeyID)))
            {
                optSetuna.UnregistHotKey(Handle, item);
            }
        }

        // Token: 0x06000211 RID: 529 RVA: 0x0000B324 File Offset: 0x00009524
        private void OnMainformLoad(object sender, EventArgs e)
        {
            base.Visible = false;
            LoadOption();
            OptionApply();
            SaveOption();
            if (optSetuna.Setuna.ShowSplashWindow)
            {
                frmSplash = new SplashForm();
                base.AddOwnedForm(frmSplash);
                frmSplash.Show(this);
                frmSplash.SplashTimer.Start();
            }
            _imgPoolTimer.Start();
            Mainform.cap_form = new CaptureForm(optSetuna.Setuna);
            IsStart = true;

            SETUNA.Main.Layer.LayerManager.Instance.Init();
            SETUNA.Main.Cache.CacheManager.Instance.Init();
            _delayInitTimer.Start();
        }

        // Token: 0x06000212 RID: 530 RVA: 0x0000B3B6 File Offset: 0x000095B6
        private void OnSetunaIconMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                base.Activate();
                SETUNA.Main.Layer.LayerManager.Instance.RefreshLayer();
            }
        }

        private void OnSetunaIconMouseDoubleClick(object sender, EventArgs e)
        {
            Option();
        }

        // Token: 0x06000213 RID: 531 RVA: 0x0000B3BE File Offset: 0x000095BE
        public void ScrapMenuOpening(object sender, ScrapMenuArgs e)
        {
            _subMenu.Scrap = e.scrap;
            _subMenu.Show(e.scrap, e.scrap.PointToClient(Cursor.Position));

            _subMenu.MouseWheel -= SubMenu_MouseWheel;
            _subMenu.MouseWheel += SubMenu_MouseWheel;
        }

        private void SubMenu_MouseWheel(object sender, MouseEventArgs e)
        {
            MouseWheelCallbackEvent?.Invoke(sender, e);
        }

        // Token: 0x06000214 RID: 532 RVA: 0x0000B3F2 File Offset: 0x000095F2
        private void button2_Click(object sender, EventArgs e)
        {
            GC.Collect();
        }

        // Token: 0x06000215 RID: 533 RVA: 0x0000B3FC File Offset: 0x000095FC
        private void OnImgPoolTimerTick(object sender, EventArgs e)
        {
            if (_imgPool.Count == 0 || IsCapture || IsOption || !IsStart)
            {
                _imgPoolTimer.Stop();
                return;
            }

            for (var i = 0; i < _imgPool.Count; i++)
            {
                var scrap = _imgPool[i];
                if (scrap.IsDone)
                {
                    _imgPool.RemoveAt(i);

                    using (var scrapSource = scrap)
                    {
                        CreateScrapFromsource(scrapSource);
                    }

                    break;
                }
            }
        }

        // Token: 0x06000216 RID: 534 RVA: 0x0000B46C File Offset: 0x0000966C
        public void AddImageList(ScrapSource src)
        {
            _imgPool.Add(src);
            _imgPoolTimer.Start();
        }

        // Token: 0x06000217 RID: 535 RVA: 0x0000B488 File Offset: 0x00009688
        public void CreateScrapFromImage(Image image, Point location)
        {
            if (image == null)
            {
                return;
            }
            using (var bitmap = (Bitmap)image.Clone())
            {
                if (location == Point.Empty)
                {
                    location = Cursor.Position;
                }
                var x = location.X;
                var y = location.Y;
                scrapBook.AddScrap((Bitmap)bitmap.Clone(), x, y, bitmap.Width, bitmap.Height);
            }
        }

        private void CreateScrapFromsource(ScrapSource src)
        {
            CreateScrapFromImage(src.GetImage(), src.GetPosition());
        }

        void ISingletonForm.DetectExternalStartup(string version, string[] args)
        {
            base.Invoke(new Mainform.ExternalStartupDelegate(ExternalStartup), new object[]
            {
                version,
                args
            });
        }

        private void ExternalStartup(string version, string[] args)
        {
            if (Application.ProductVersion != version)
            {
                MessageBox.Show("SETUNA已经运行在不同的版本。", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (args.Length > 0)
            {
                CommandRun(args);
                return;
            }
            if (optSetuna.Setuna.DupType == SetunaOption.SetunaOptionData.OpeningType.Capture)
            {
                StartCapture();
            }
        }

        public void CommandRun(string[] args)
        {
            Console.WriteLine("-命令行参数--------------------");
            var num = 0;
            var rect = new Rectangle(0, 0, 0, 0);
            var fname = "";
            foreach (var text in args)
            {
                try
                {
                    var text2 = text;
                    var text3 = "";
                    if (text.Length > 3)
                    {
                        text3 = text2.Substring(0, 3);
                        if (text3.Substring(0, 1) == "/" && text3.Substring(2, 1) == ":")
                        {
                            text2 = text.Substring(text3.Length, text2.Length - text3.Length);
                        }
                        else
                        {
                            text3 = "";
                        }
                    }
                    if (text3.Length > 0)
                    {
                        if (text3 == "/R:")
                        {
                            var array = text2.Split(new char[]
                            {
                                ','
                            });
                            if (array.Length == 4)
                            {
                                rect = default(Rectangle);
                                rect.X = int.Parse(array[0]);
                                rect.Y = int.Parse(array[1]);
                                rect.Width = int.Parse(array[2]);
                                rect.Height = int.Parse(array[3]);
                                Console.WriteLine("[位置]" + rect.ToString());
                                goto IL_1C2;
                            }
                        }
                        if (text3 == "/P:")
                        {
                            fname = text2;
                        }
                        if (text3 == "/C:")
                        {
                            if (text2.ToUpper() == "OPTION")
                            {
                                num = 1;
                                goto IL_1C2;
                            }
                            if (text2.ToUpper() == "CAPTURE")
                            {
                                num = 2;
                                goto IL_1C2;
                            }
                            if (text2.ToUpper() == "SUBMENU")
                            {
                                num = 3;
                                goto IL_1C2;
                            }
                        }
                    }
                    AddImageList(new ScrapSourcePath(text2));
                    Console.WriteLine(text2);
                }
                catch
                {
                    Console.WriteLine("[Error]" + text);
                }
            IL_1C2:;
            }
            Console.WriteLine("---------------------------------------");
            if (rect.Width >= 10 && rect.Height >= 10)
            {
                CommandCutRect(rect, fname);
                return;
            }
            if (num != 0 && IsStart)
            {
                switch (num)
                {
                    case 1:
                        if (!IsOption)
                        {
                            Option();
                            return;
                        }
                        break;
                    case 2:
                        if (!IsCapture)
                        {
                            StartCapture();
                        }
                        break;
                    default:
                        return;
                }
            }
        }

        private void CommandCutRect(Rectangle rect, string fname)
        {
            using (var bitmap = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb))
            {
                var point = new Point(rect.X, rect.Y);
                CaptureForm.CopyFromScreen(bitmap, point);
                if (fname == "")
                {
                    AddImageList(new ScrapSourceImage(bitmap, point));
                }
            }
        }

        private void OnMainformShow(object sender, EventArgs e)
        {
        }

        private void OnSetunaIconMenuOpening(object sender, CancelEventArgs e)
        {
            e.Cancel = false;
        }

        private void OnWindowTimerTick(object sender, EventArgs e)
        {
            SETUNA.Main.WindowManager.Instance.Update();
        }

        private void SetAllScrapsActive(bool active)
        {
            if (allScrapActive == active)
            {
                return;
            }

            allScrapActive = active;

            if (active)
            {
                forms.ForEach(x => x.Visible = active);
                forms.Clear();

                Main.Layer.LayerManager.Instance.ResumeRefresh();
            }
            else
            {
                foreach (var item in scrapBook)
                {
                    if (item.Visible)
                    {
                        forms.Add(item);
                    }
                    else if ((item.StyleForm?.Visible ?? false) == true)
                    {
                        forms.Add(item.StyleForm);
                    }
                }

                forms.ForEach(x => x.Visible = active);

                Main.Layer.LayerManager.Instance.SuspendRefresh();
            }
        }

        private void OnDelayInitTimerTick(object sender, EventArgs e)
        {
            if (SETUNA.Main.Cache.CacheManager.Instance.IsInit)
            {
                _delayInitTimer.Enabled = false;
                SETUNA.Main.Layer.LayerManager.Instance.RefreshLayer();
                SETUNA.Main.Layer.LayerManager.Instance.DelayInit();
            }
        }
    }
}
