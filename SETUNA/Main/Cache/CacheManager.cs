using System;
using System.Collections.Generic;
using System.IO;

namespace SETUNA.Main.Cache
{
    public class CacheManager : IScrapAddedListener, IScrapRemovedListener, IScrapLocationChangedListener, IScrapImageChangedListener, IScrapStyleAppliedListener, IScrapStyleRemovedListener
    {
        public static readonly string Path = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SETUNA");

        public static readonly CacheManager Instance = new CacheManager();


        public bool IsInit { private set; get; }


        public void Init()
        {
            IsInit = false;
            var scrapBook = Mainform.Instance.scrapBook;
            scrapBook.AddScrapAddedListener(this);
            scrapBook.AddScrapRemovedListener(this);

            RestoreScraps(scrapBook);
        }

        public void DeInit()
        {
        }

        void RestoreScraps(ScrapManager mainBook)
        {
            var directoryInfo = new DirectoryInfo(Path);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            var directories = directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
            var list = new List<CacheItem>(directories.Length);
            foreach (var directory in directories)
            {
                var item = CacheItem.Read(directory.FullName);
                if ((item?.IsValid ?? false) == false)
                {
                    continue;
                }

                list.Add(item);
            }

            list.Sort((x, y) => x.SortingOrder.CompareTo(y.SortingOrder));
            AddScrap(0);



            void AddScrap(int index)
            {
                if (index >= list.Count)
                {
                    IsInit = true;
                    return;
                }

                var item = list[index];
                mainBook.AddScrapFromCache(item, () =>
                {
                    AddScrap(++index);
                });
            }
        }


        public void ScrapAdded(object sender, ScrapEventArgs e)
        {
            var scrap = e.scrap;

            // 已经绑定缓存则忽略
            if (scrap.CacheItem != null)
            {
                return;
            }

            var style = new Style
            {
                ID = scrap.StyleID,
                ClickPoint = scrap.StyleClickPoint
            };

            var cacheItem = CacheItem.Create(scrap.DateTime, scrap.Image, scrap.Location, style);
            scrap.CacheItem = cacheItem;
        }

        public void ScrapRemoved(object sender, ScrapEventArgs e)
        {
            var scrap = e.scrap;
            var cacheItem = scrap?.CacheItem;
            if (cacheItem == null)
            {
                return;
            }

            scrap.CacheItem = null;
            cacheItem.Delete();
        }

        public void ScrapLocationChanged(object sender, ScrapEventArgs e)
        {
            var scrap = e.scrap;
            var cacheItem = scrap?.CacheItem;
            if (cacheItem == null)
            {
                return;
            }

            cacheItem.Position = scrap.Location;
            cacheItem.SaveInfo();
        }

        public void OnScrapImageChanged(object sender, ScrapEventArgs e)
        {
            var scrap = e.scrap;
            var cacheItem = scrap?.CacheItem;
            var image = scrap?.Image;
            if (cacheItem == null || image == null)
            {
                return;
            }

            cacheItem.SaveImage(image);
        }

        public void OnScrapStyleApplied(object sender, ScrapEventArgs e)
        {
            var scrap = e.scrap;
            var styleID = scrap?.StyleID ?? 0;
            var cacheItem = scrap?.CacheItem;
            if (cacheItem == null || styleID == 0)
            {
                return;
            }

            cacheItem.Style = new Style(styleID, scrap.StyleClickPoint);
            cacheItem.SaveInfo();
        }

        public void OnScrapStyleRemoved(object sender, ScrapEventArgs e)
        {
            var scrap = e.scrap;
            var cacheItem = scrap?.CacheItem;
            if (cacheItem == null)
            {
                return;
            }

            cacheItem.Style = new Style(0, new System.Drawing.Point(0, 0));
            cacheItem.SaveInfo();
        }
    }
}