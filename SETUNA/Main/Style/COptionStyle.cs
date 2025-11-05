using System;

namespace SETUNA.Main.Style
{
    public class COptionStyle : CPreStyle
    {
        public COptionStyle()
        {
            _styleid = -10;
            _stylename = "选项";
        }

        public override void Apply(ScrapBase scrap)
        {
            Mainform.Instance.Option();
        }
    }
}
