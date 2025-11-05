using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SETUNA.Main.Option;
using SETUNA.Main.Style;
using SETUNA.Main.StyleItems;

namespace SETUNA.Main
{
    public sealed partial class ScrapBase : BaseForm
    {
        private const int WS_EX_LAYERED = 524288;
        private const int GWL_EXSTYLE = -20;
        private Image _imgView;
        private bool _isClosing;
        private string _name;
        private DateTime _datetime;
        private int _scale;
        private bool _dragmode;
        private Point _dragpoint;
        private double _saveopacity;
        private bool _blTargetSet;
        private Pen _pen;
        private Point _ptTarget;
        private Timer StyleApplyTimer;
        private int StyleAppliIndex;
        private int _styleID;
        private List<CStyleItem> _styleItems;
        private bool IsStyleApply;
        private SetunaOption _optSetuna;
        private Point _styleClickPoint = Point.Empty;
        private double _targetOpacity;
        private bool _isMouseEnter;
        private double _mouseEnterOpacity;
        private double _mouseLeaveOpacity;
        private int _activeMargin;
        private int _inactiveMargin;
        private int _rolloverMargin;
        private InterpolationMode _interpolationmode;
        private Cache.CacheItem _cacheItem;
        private Action _applyFinished;
        private bool _solidFrame;
        private bool _visible = true;

        public ScrapManager scrapManager;
        public bool initialized;

        public delegate void ScrapEventHandler(object sender, ScrapEventArgs e);
        public delegate void ScrapSubMenuHandler(object sender, ScrapMenuArgs e);
        
        public event ScrapBase.ScrapEventHandler ScrapClose;
        public event ScrapBase.ScrapEventHandler ScrapActive;
        public event ScrapBase.ScrapEventHandler ScrapInactiveMouseEnter;
        public event ScrapBase.ScrapEventHandler ScrapInactiveMouseLeave;
        public event ScrapBase.ScrapSubMenuHandler ScrapRightMouseClick;
        public event ScrapBase.ScrapEventHandler ScrapLocationChanged;
        public event ScrapBase.ScrapEventHandler ScrapImageChanged;
        public event ScrapBase.ScrapEventHandler ScrapStyleApplied;
        public event ScrapBase.ScrapEventHandler ScrapStyleRemoved;

        public double MouseEnterOpacity
        {
            get => _mouseEnterOpacity;
            set
            {
                _mouseEnterOpacity = value;
                if (_isMouseEnter)
                {
                    TargetOpacity = _mouseEnterOpacity;
                }
            }
        }

        public double MouseLeaveOpacity
        {
            get => _mouseLeaveOpacity;
            set
            {
                _mouseLeaveOpacity = value;
                if (!_isMouseEnter)
                {
                    TargetOpacity = _mouseLeaveOpacity;
                }
            }
        }

        public int ActiveMargin
        {
            get => _activeMargin;
            set
            {
                _activeMargin = value;
                Padding = new Padding(_activeMargin);
            }
        }

        public int InactiveMargin
        {
            get => _inactiveMargin;
            set
            {
                _inactiveMargin = value;
                if (!_isMouseEnter)
                {
                    Padding = new Padding(_inactiveMargin);
                }
            }
        }

        public int RollOverMargin
        {
            get => _rolloverMargin;
            set
            {
                _rolloverMargin = value;
                if (_isMouseEnter)
                {
                    Padding = new Padding(_rolloverMargin);
                }
            }
        }

        public bool SolidFrame
        {
            get => _solidFrame;
            set => _solidFrame = value;
        }

        public ScrapBase()
        {
            InitializeComponent();
            _optSetuna = Mainform.Instance.optSetuna;
            KeyPreview = true;
            SolidFrame = true;
            _isClosing = false;
            _dragmode = false;
            _scale = 100;
            _blTargetSet = false;
            _ptTarget = default;
            Opacity = 1.0;
            TargetOpacity = 1.0;
            DateTime = System.DateTime.Now;
            Name = DateTime.ToCustomString();
            _interpolationmode = InterpolationMode.HighQualityBicubic;
            _pen = new Pen(Color.Blue);
            _pen.DashStyle = DashStyle.Dash;
            _pen.DashPattern = new float[] { 4f, 4f };
            CStyle cstyle = _optSetuna.FindStyle(_optSetuna.Scrap.createStyleID);
            cstyle?.Apply(this);
        }

        ~ScrapBase()
        {
            ImageAllDispose();
        }

        private void ImageAllDispose()
        {
            ImageDispose(ref _imgView);
        }

        public double TargetOpacity
        {
            get => _targetOpacity;
            set
            {
                _targetOpacity = value > 1.0 ? 1.0 : value < 0.0 ? 0.0 : value;
                if (_targetOpacity != Opacity)
                {
                    OpacityTimer.Start();
                }
            }
        }

        private void OnOpacityTimerTick(object sender, EventArgs e)
        {
            // 计算当前与目标的差值
            double difference = TargetOpacity - Opacity;
            // 每次变化步长
            double OPACITY_STEP = 0.05;
            // 如果已经很接近目标值，直接设置并停止计时器
            if (Math.Abs(difference) <= OPACITY_STEP)
            {
                Opacity = TargetOpacity;
                OpacityTimer.Stop();
                return;
            }

            // 向目标值移动一步
            Opacity += Math.Sign(difference) * OPACITY_STEP;
        }

        public Point TargetLocation
        {
            get
            {
                if (_blTargetSet)
                {
                    return _ptTarget;
                }
                return base.Location;
            }
            set
            {
                _ptTarget = value;
                if (_ptTarget != base.Location)
                {
                    _blTargetSet = true;
                    OpacityTimer.Start();
                }
            }
        }

        public Image Image
        {
            get => _imgView;
            set
            {
                ImageAllDispose();
                _imgView = (Image)value.Clone();
                if (_imgView == null)
                {
                    Console.WriteLine("ScrapBase Image : unll");
                }
                Refresh();

                ScrapImageChanged?.Invoke(this, new ScrapEventArgs(this));
            }
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private void ImageDispose(ref Image img)
        {
            if (img != null)
            {
                img.Dispose();
                img = null;
            }
        }

        public Image GetViewImage()
        {
            var bitmap = new Bitmap(base.Width, base.Height, PixelFormat.Format24bppRgb);
            base.DrawToBitmap(bitmap, new Rectangle(0, 0, base.Width, base.Height));
            return bitmap;
        }

        public Image GetThumbnail()
        {
            var bitmap = new Bitmap(230, 150, PixelFormat.Format24bppRgb);
            var graphics = Graphics.FromImage(bitmap);
            graphics.FillRectangle(Brushes.DarkGray, 0, 0, bitmap.Width, bitmap.Height);
            if (_imgView.Width <= bitmap.Width - 1 || _imgView.Height <= bitmap.Height - 1)
            {
                graphics.DrawImageUnscaled(_imgView, 1, 1);
            }
            else
            {
                var size = new Size(_imgView.Width - 1, _imgView.Height - 1);
                double num;
                if (size.Width - bitmap.Width - 1 <= size.Height - bitmap.Height - 1)
                {
                    num = (bitmap.Width - 1) / (double)(size.Width - 1);
                }
                else
                {
                    num = (bitmap.Height - 1) / (double)(size.Height - 1);
                }
                size.Width = (int)(size.Width * num);
                size.Height = (int)(size.Height * num);
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(_imgView, 1, 1, size.Width, size.Height);
            }
            graphics.DrawRectangle(Pens.Black, 0, 0, bitmap.Width - 1, bitmap.Height - 1);
            return bitmap;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var margin = Padding.All;
            e.Graphics.InterpolationMode = _interpolationmode;

            if (Mainform.Instance.optSetuna.Setuna.BackgroundTransparentEnabled)
            {
                e.Graphics.Clear(Color.Green);
                TransparencyKey = Color.Green;
            }
            else
            {
                e.Graphics.Clear(Color.White);
            }
            e.Graphics.DrawImage(_imgView, margin, margin, (int)(_imgView.Width * (Scale / 100.0)), (int)(_imgView.Height * (Scale / 100.0)));
            e.Graphics.DrawRectangle(_pen, new Rectangle(0, 0, Width - 1, Height - 1));
        }

        public InterpolationMode InterpolationMode
        {
            get => _interpolationmode;
            set => _interpolationmode = value;
        }

        public new Padding Padding
        {
            get => base.Padding;
            set
            {
                base.Padding = value;

                var x = Left - value.All;
                var y = Top - value.All;
                var num = (int)(_imgView.Width * (_scale / 100f)) + value.All * 2;
                var num2 = (int)(_imgView.Height * (_scale / 100f)) + value.All * 2;
                SetBoundsCore(x, y, num, num2, BoundsSpecified.Location);
                base.ClientSize = new Size(num, num2);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            var styleForm = StyleForm;
            StyleForm = null;
            if (styleForm != null)
            {
                styleForm.Close();
            }

            if (e.CloseReason == CloseReason.ApplicationExitCall || e.CloseReason == CloseReason.TaskManagerClosing || e.CloseReason == CloseReason.WindowsShutDown)
            {
                Console.WriteLine("由系统结束");
            }
            else if (!_isClosing)
            {
                e.Cancel = true;
                OnScrapClose(new ScrapEventArgs(this));
            }
            base.OnFormClosing(e);
        }

        private void OnScrapKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Escape))
            {
                base.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            ImageAllDispose();
            _applyFinished = null;

            base.OnClosed(e);
        }

        public void OnScrapClose(ScrapEventArgs e)
        {
            if (ScrapClose != null)
            {
                ScrapClose(this, e);
            }
        }

        public void PrepareClose()
        {
            _isClosing = true;
            base.Close();
            GC.Collect();
        }

        public ScrapManager Manager
        {
            get => scrapManager;
            set
            {
                scrapManager = value;
                if (ScrapClose == null)
                {
                    ScrapClose = (ScrapBase.ScrapEventHandler)Delegate.Combine(ScrapClose, new ScrapBase.ScrapEventHandler(scrapManager.ScrapClose));
                    KeyDown += scrapManager.OnScrapKeyDown;
                }
            }
        }

        public new string Name
        {
            get => _name;
            set
            {
                _name = value;
                Text = _name;
            }
        }

        public DateTime DateTime
        {
            get => _datetime;
            set => _datetime = value;
        }

        public new int Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                if (_scale < -200)
                {
                    _scale = -200;
                }
                if (_scale > 200)
                {
                    _scale = 200;
                }
                base.Width = (int)(_imgView.Width * (_scale / 100f)) + Padding.All * 2;
                base.Height = (int)(_imgView.Height * (_scale / 100f)) + Padding.All * 2;
                Refresh();
            }
        }

        public int StyleID => _styleID;

        public Point StyleClickPoint => _styleClickPoint;

        public Cache.CacheItem CacheItem
        {
            set => _cacheItem = value;
            get => _cacheItem;
        }

        public Form StyleForm { set; get; }

        private void DragStart(Point pt)
        {
            _dragmode = true;
            _dragpoint = pt;
            _saveopacity = Opacity;
            SuspendLayout();
            TargetOpacity = 0.5 * _saveopacity;
            ResumeLayout();
        }

        private void DragEnd()
        {
            _dragmode = false;
            SuspendLayout();
            TargetOpacity = _saveopacity;
            ResumeLayout();
        }

        private void DragMove(Point pt)
        {
            if (_dragmode)
            {
                Left += pt.X - _dragpoint.X;
                Top += pt.Y - _dragpoint.Y;
            }
        }

        private void OnScrapMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DragStart(e.Location);
            }
        }

        private void OnScrapMouseMove(object sender, MouseEventArgs e)
        {
            DragMove(e.Location);
        }

        private void OnScrapMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DragEnd();
            }
        }

        private void OnScrapMouseEnter(object sender, EventArgs e)
        {
            _isMouseEnter = true;
            if (!Focused && ScrapInactiveMouseEnter != null)
            {
                ScrapInactiveMouseEnter(sender, new ScrapEventArgs(this));
            }
            double opacity = 1.0 - _optSetuna.Scrap.MouseEnterAlphaValue / 100.0;
            TargetOpacity = _optSetuna.Scrap.mouseEnterAlphaChange ? opacity : 1.0;
        }

        private void OnScrapMouseLeave(object sender, EventArgs e)
        {
            _isMouseEnter = false;
            if (!Focused && ScrapInactiveMouseLeave != null)
            {
                ScrapInactiveMouseLeave(sender, new ScrapEventArgs(this));
            }
            double opacity = 1.0 - _optSetuna.Scrap.MouseLeaveAlphaValue / 100.0;
            TargetOpacity = _optSetuna.Scrap.mouseLeaveAlphaChange ? opacity : 1.0;
        }

        private void OnScrapMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ScrapRightMouseClick?.Invoke(sender, new ScrapMenuArgs(this, null));
            }
        }

        public new bool Visible
        {
            get => base.Visible;
            set
            {
                if (!value && base.FormBorderStyle != FormBorderStyle.None)
                {
                    base.ShowInTaskbar = false;
                }
                else if (value && base.FormBorderStyle != FormBorderStyle.None)
                {
                    base.ShowInTaskbar = true;
                }
                base.Visible = value;

                _visible = value;
            }
        }

        public new void SetBounds(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBounds(x, y, width + Padding.All * 2, height + Padding.All * 2, specified);
        }

        public void ApplyStylesFromCache(CStyle style, Point clickpoint, Action applyFinished = null)
        {
            var styleItems = new List<CStyleItem>(style.Items);
            styleItems.RemoveAll(x => !StyleItemDictionary.CanRestore(x.GetType()));

            _applyFinished = applyFinished;
            ApplyStyles(style.StyleID, styleItems.ToArray(), clickpoint);
        }

        public void ApplyStyles(int styleID, CStyleItem[] styleItems, Point clickpoint)
        {
            if (IsStyleApply)
            {
                return;
            }
            _styleClickPoint = clickpoint;
            IsStyleApply = true;
            StyleAppliIndex = 0;
            _styleID = styleID;
            _styleItems = styleItems.ToList();
            if (StyleApplyTimer == null)
            {
                StyleApplyTimer = new Timer
                {
                    Enabled = false
                };
                StyleApplyTimer.Tick += OnStyleApplyTimerTick;
            }
            StyleApplyTimer.Interval = 1;
            StyleApplyTimer.Start();
        }

        public void OnStyleApplyTimerTick(object sender, EventArgs e)
        {
            StyleApplyTimer.Enabled = false;
            if (StyleAppliIndex < _styleItems.Count)
            {
                var num = 1;
                var scrapBase = this;
                try
                {
                    var cstyleItem = _styleItems[StyleAppliIndex];
                    if (initialized || (!initialized && cstyleItem.IsInitApply))
                    {
                        cstyleItem.Apply(ref scrapBase, out num, _styleClickPoint);
                    }
                    StyleAppliIndex++;
                    if (num <= 0)
                    {
                        num = 1;
                    }
                    StyleApplyTimer.Interval = num;
                    StyleApplyTimer.Enabled = true;
                    goto IL_AD;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ScrapBase ApplyStyleItem Exception:" + ex.ToString());
                    IsStyleApply = false;
                    goto IL_AD;
                }
            }
            else
            {
                _applyFinished?.Invoke();
                _applyFinished = null;

                if (ScrapStyleApplied != null)
                {
                    ScrapStyleApplied(this, new ScrapEventArgs(this));
                }
            }
            IsStyleApply = false;
        IL_AD:
            if (!IsStyleApply && !initialized)
            {
                initialized = true;
            }
        }

        public void RemoveStyle(Type styleItemType)
        {
            if (_styleItems.Count > 0)
            {
                _styleItems.RemoveAll(x => x.GetType() == styleItemType);
            }

            if (_styleItems.Count == 0)
            {
                _styleID = 0;
                _styleClickPoint = Point.Empty;

                if (ScrapStyleRemoved != null)
                {
                    ScrapStyleRemoved(this, new ScrapEventArgs(this));
                }
            }
        }

        private void OnScrapMouseDoubleClick(object sender, MouseEventArgs e)
        {
            Manager.WClickStyle(this, e.Location);
        }

        private void OnScrapDragEnd(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (var path in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    Manager.AddDragImageFileName(path);
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.Html))
            {
                var htmlContent = e.Data.GetData(DataFormats.Html) as string;
                var match = System.Text.RegularExpressions.Regex.Match(htmlContent, "<img.*?(width=\"(?<width>.*?)\".*?)?(height=\"(?<height>.*?)\".*?)?src=\"(?<src>.*?)\"");
                if (match.Success)
                {
                    int.TryParse(match.Groups["width"].Value, out var width);
                    int.TryParse(match.Groups["height"].Value, out var height);
                    var url = match.Groups["src"].Value;
                    Manager.AddDragImageUrl(url);
                }
            }
        }

        private void OnScrapDragBegin(object sender, DragEventArgs e)
        {
            if (Manager.IsImageDrag)
            {
                e.Effect = DragDropEffects.All;
            }
        }


        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);

            if (ScrapLocationChanged != null)
            {
                ScrapLocationChanged(e, new ScrapEventArgs(this));
            }
        }
    }
}
