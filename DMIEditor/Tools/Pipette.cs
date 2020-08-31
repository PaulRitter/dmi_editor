using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DMIEditor.Tools
{
    public class Pipette : ClickTool
    {
        public override string Name => "Pipette";
        public Pipette(MainWindow main) : base(main) { }

        public override void PixelAct(Bitmap current, int x, int y)
        {
            Color c = current.GetPixel(x, y);
            if(c != Color.Transparent)
                main.SetColor(c);
        }
    }
}
