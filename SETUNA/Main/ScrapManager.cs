using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SETUNA.Main
{
    public class ScrapManager
    {
        public Queue<ScrapBase> DustBox
        {
            get => _dustbox;
            set => _dustbox = value;
        }

        public ArrayList DustBoxArray => new ArrayList(_dustbox.ToArray());

        public short DustBoxCapacity
        {
            get => _dustcap;
            set
            {
                _dustcap = value;
                while (_dustcap < _dustbox.Count)
                {
                    var scrapBase = _dustbox.Dequeue();
                    scrapBase.PrepareClose();
                }
            }
        }

        public Mainform BindForm => _mainform;

        public event ScrapManager.KeyPressHandler keyPressed;
        public event ScrapManager.ScrapAddedHandler ScrapAdded;
        public event ScrapManager.ScrapRemovedHandler scrapRemoved;

        public ScrapManager(Mainform mainform)
        {
            _scraps = new ArrayList();
            _dustbox = null;
            _dustcap = 0;
            _mainform = mainform;
        }

        ~ScrapManager()
        {
            try
            {
                foreach (var obj in _scraps)
                {
                    var scrapBase = (ScrapBase)obj;
                    scrapBase.PrepareClose();
                }
                _scraps.Clear();
                EraseDustBox();
            }
            finally
            {

            }
        }

        public void EraseDustBox()
        {
            if (_dustbox != null)
            {
                foreach (var scrapBase in _dustbox)
                {
                    scrapBase.PrepareClose();
                }
                _dustbox.Clear();
            }
            GC.Collect();
        }

        public IEnumerator<ScrapBase> GetEnumerator()
        {
            foreach (var obj in _scraps)
            {
                var scrap = (ScrapBase)obj;
                yield return scrap;
            }
            yield break;
        }

        public void AddScrap(Image img, int x, int y, int width, int height)
        {
            AddScrap(img, x, y, width, height, "");
        }

        public void AddScrap(Image img, int x, int y, int width, int height, string scrapname)
        {
            var scrapBase = new ScrapBase();
            if (scrapname != "")
            {
                scrapBase.Name = scrapname;
            }
            scrapBase.Image = img;
            scrapBase.SetBounds(x, y, img.Width, img.Height, BoundsSpecified.All);

            AddScrapThenDo(scrapBase);
        }

        public void AddScrapFromCache(Cache.CacheItem cacheItem, Action addFinished = null)
        {
            var image = cacheItem.ReadImage();
            var pos = cacheItem.Position;
            var style = cacheItem.Style;

            var scrapBase = new ScrapBase
            {
                DateTime = cacheItem.CreateTime,
                Name = cacheItem.CreateTime.ToCustomString(),
                Image = image
            };
            scrapBase.SetBounds(pos.X, pos.Y, image.Width, image.Height, BoundsSpecified.All);

            var cstyle = _mainform.optSetuna.FindStyle(style.ID);
            if (cstyle != null)
            {
                scrapBase.ApplyStylesFromCache(cstyle, style.ClickPoint, () =>
                {
                    AddScrapThenDo(scrapBase, scrapBase.Visible);
                    addFinished?.Invoke();
                });
            }

            // 设置所有参数再设置缓存对象
            scrapBase.CacheItem = cacheItem;

            if (cstyle == null)
            {
                AddScrapThenDo(scrapBase);
                addFinished?.Invoke();
            }
        }

        public void AddScrapThenDo(ScrapBase newScrap, bool show = true)
        {
            newScrap.ScrapRightMouseClick += _mainform.OnScrapRightMouseClick;
            newScrap.ScrapLocationChanged += Cache.CacheManager.Instance.ScrapLocationChanged;
            newScrap.ScrapImageChanged += Cache.CacheManager.Instance.OnScrapImageChanged;
            newScrap.ScrapStyleApplied += Cache.CacheManager.Instance.OnScrapStyleApplied;
            newScrap.ScrapStyleRemoved += Cache.CacheManager.Instance.OnScrapStyleRemoved;

            newScrap.Manager = this;
            _scraps.Add(newScrap);
            ScrapAdded?.Invoke(this, new ScrapEventArgs(newScrap));

            if (show)
            {
                newScrap.Refresh();
                newScrap.Show();
            }
        }

        public void ScrapClose(object sender, ScrapEventArgs e)
        {
            var scrapBase = e.scrap;
            _scraps.Remove(scrapBase);
            if (_dustbox != null && _dustcap > 0)
            {
                if (_dustbox.Count == _dustcap)
                {
                    var scrapBase2 = _dustbox.Dequeue();
                    scrapBase2.PrepareClose();
                }
                _dustbox.Enqueue(scrapBase);
                scrapBase.Hide();
            }
            else
            {
                scrapBase.PrepareClose();
            }
            if (scrapRemoved != null)
            {
                scrapRemoved(this, e);
            }
        }

        public void ShowAllScrap()
        {
            foreach (var obj in _scraps)
            {
                var scrapBase = (ScrapBase)obj;
                scrapBase.Show();
            }
        }

        public void HideAllScrap()
        {
            foreach (var obj in _scraps)
            {
                var scrapBase = (ScrapBase)obj;
                scrapBase.Hide();
            }
        }

        public void CloseAllScrap()
        {
            var list = _scraps.ToArray();
            foreach (var obj in list)
            {
                var scrapBase = (ScrapBase)obj;
                scrapBase.Close();
            }
        }

        public int ScrapCount
        {
            get
            {
                if (_scraps == null)
                {
                    return 0;
                }
                return _scraps.Count;
            }
        }

        public int DustCount
        {
            get
            {
                if (_dustbox == null)
                {
                    return 0;
                }
                return _dustbox.Count;
            }
        }

        public void OnScrapKeyDown(object sender, KeyEventArgs e)
        {
            if (keyPressed != null)
            {
                var scrapKeyPressEventArgs = new ScrapKeyPressEventArgs
                {
                    key = (e.KeyCode | e.Modifiers)
                };
                Console.WriteLine(scrapKeyPressEventArgs.key.ToString());
                keyPressed(sender, scrapKeyPressEventArgs);
            }
        }

        public void AddKeyPressListener(IScrapKeyPressEventListener listener)
        {
            keyPressed = (ScrapManager.KeyPressHandler)Delegate.Combine(keyPressed, new ScrapManager.KeyPressHandler(listener.ScrapKeyPress));
        }

        public void AddScrapAddedListener(IScrapAddedListener listener)
        {
            ScrapAdded = (ScrapManager.ScrapAddedHandler)Delegate.Combine(ScrapAdded, new ScrapManager.ScrapAddedHandler(listener.ScrapAdded));
        }

        public void AddScrapRemovedListener(IScrapRemovedListener listener)
        {
            scrapRemoved = (ScrapManager.ScrapRemovedHandler)Delegate.Combine(scrapRemoved, new ScrapManager.ScrapRemovedHandler(listener.ScrapRemoved));
        }

        public void WClickStyle(ScrapBase scrap, Point clickpoint)
        {
            var wclickStyleID = _mainform.optSetuna.Scrap.wClickStyleID;
            if (wclickStyleID != 0)
            {
                var cstyle = _mainform.optSetuna.FindStyle(wclickStyleID);
                if (cstyle != null)
                {
                    cstyle.Apply(ref scrap, clickpoint);
                }
            }
        }

        public void AddDragImageFileName(string path)
        {
            _mainform.AddImageList(new ScrapSourcePath(path));
        }

        public void AddDragImageUrl(string url, int width = 0, int height = 0)
        {
            _mainform.AddImageList(new ScrapSourceUrl(url, Cursor.Position, width, height));
        }

        public bool IsImageDrag => _mainform.optSetuna.Scrap.imageDrag;

        private Mainform _mainform;
        protected Queue<ScrapBase> _dustbox;
        protected short _dustcap;
        protected ArrayList _scraps;
        public delegate void KeyPressHandler(object sender, ScrapKeyPressEventArgs e);
        public delegate void ScrapAddedHandler(object sender, ScrapEventArgs e);
        public delegate void ScrapRemovedHandler(object sender, ScrapEventArgs e);
    }
}
