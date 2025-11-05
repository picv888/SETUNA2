using System.Collections;
using System.Windows.Forms;

namespace SETUNA.Main
{
    public class ToolStripDustboxList : ToolStripAbstractList
    {
        public ToolStripDustboxList(string text, ScrapManager scrapbook) : base(text, scrapbook)
        {
        }

        protected override bool IsExists()
        {
            return _scrapbook != null && _scrapbook.DustCount > 0;
        }

        protected override ArrayList GetItems()
        {
            return _scrapbook.DustBoxArray;
        }

        protected override void OnAddItem(ToolStripMenuItem addmi)
        {
            addmi.Click += Mainform.Instance.RestoreScrap;
        }
    }
}
