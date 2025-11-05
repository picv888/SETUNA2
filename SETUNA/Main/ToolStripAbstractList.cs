using System;
using System.Collections;
using System.Windows.Forms;
using Opulos.Core.UI;

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
            base.DropDownItems.Clear();
            base.DropDownItems.Insert(0, new ToolStripMenuItem("无"));
            
            ToolStripEx.BigButtons(DropDown);
        }

        protected void RefreshList()
        {
            if (_createdlist)
            {
                return;
            }
            base.DropDownItems.Clear();
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
                    base.DropDownItems.Insert(0, toolStripMenuItem);
                }
            }
            if (base.DropDownItems.Count == 0)
            {
                base.DropDownItems.Insert(0, new ToolStripMenuItem("无"));
            }
            _createdlist = true;
        }

        protected override void OnMouseHover(EventArgs e)
        {
            RefreshList();
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
