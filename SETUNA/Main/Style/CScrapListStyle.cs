using System.Windows.Forms;

namespace SETUNA.Main.Style
{
    // Token: 0x02000047 RID: 71
    public class CScrapListStyle : CPreStyle
    {
        public CScrapListStyle()
        {
            _styleid = -6;
            _stylename = "参考图名单";
        }

        public ToolStripItem GetToolStrip(ScrapManager scrapbook)
        {
            return new ToolStripScrapList(base.GetDisplayName(), scrapbook);
        }
    }
}
