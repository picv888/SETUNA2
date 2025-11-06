using System;
using System.Collections;
using System.Windows.Forms;
using Opulos.Core.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace SETUNA.Main
{
    public class ToolStripAbstractList : ToolStripMenuItem
    {
        public ToolStripAbstractList()
        {
        }

        public ToolStripAbstractList(string text, ScrapManager scrapbook) : base(text)
        {
            _scrapbook = scrapbook;
            RefreshList();
            ToolStripEx.BigButtons(DropDown);
        }

        protected override void OnDropDownShow(EventArgs e)
        {
            RefreshList();
            base.OnDropDownShow(e);
        }

        protected void RefreshList()
        {
            DropDownItems.Clear();
            if (_scrapbook != null)
            {
                foreach (var obj in GetItems())
                {
                    var scrapBase = (ScrapBase)obj;
                    var text = string.Concat(new object[]
                    {
                        scrapBase.Name,
                        "\n",
                        scrapBase.Width,
                        " x ",
                        scrapBase.Height
                    });
                    var toolStripMenuItem = new ToolStripMenuItem(text, scrapBase.Image, new EventHandler(Mainform.Instance.OnActiveScrapInList))
                    {
                        Tag = scrapBase,
                        ImageScaling = ToolStripItemImageScaling.None
                    };
                    OnAddItem(toolStripMenuItem);
                    DropDownItems.Insert(0, toolStripMenuItem);
                }
            }
            if (DropDownItems.Count == 0)
            {
                DropDownItems.Add(new ToolStripMenuItem("无"));
            }
            // 添加一个不可见的菜单项，用于解决.NET framework的一个BUG：当子菜单只有一个菜单项时，子菜单会显示在屏幕左上角
            var temp = new ToolStripMenuItem(" ");
            temp.Visible = false;
            DropDownItems.Add(temp);
        }

        protected override void OnMouseHover(EventArgs e)
        {
            //RefreshList();
            base.OnMouseHover(e);
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            _createdlist = false;
            base.OnDropDownClosed(e);
        }

        protected virtual void OnAddItem(ToolStripMenuItem addmi)
        {
        }

        protected virtual bool IsExists()
        {
            return false;
        }

        protected virtual ArrayList GetItems()
        {
            return null;
        }

        protected ScrapManager _scrapbook;

        private bool _createdlist;

    }
}
