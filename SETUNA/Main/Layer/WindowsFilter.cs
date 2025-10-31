namespace SETUNA.Main.Layer
{
    class WindowsFilter : IWindowFilter
    {
        struct FilterInfo
        {
            public string TitleName { set; get; }
            public string ClassName { set; get; }


            public FilterInfo(string className)
            {
                TitleName = null;
                ClassName = className;
            }

            public FilterInfo(string titleName, string className)
            {
                TitleName = titleName;
                ClassName = className;
            }
        }

        static FilterInfo[] filterInfos = new FilterInfo[]
        {
            // 系统任务切换界面
            new FilterInfo("任务切换", "MultitaskingViewFrame"),
            // AutoHotkey的GUI界面
            new FilterInfo("AutoHotkeyGUI"),
        };

        bool IWindowFilter.IsFilter(WindowInfo windowInfo)
        {
            var titleName = windowInfo.TitleName;
            var className = windowInfo.ClassName;

            foreach (var item in filterInfos)
            {
                if (titleName != null && item.TitleName != null && titleName == item.TitleName) return true;
                if (className != null && item.ClassName != null && className == item.ClassName) return true;
            }

            return false;
        }
    }
}
