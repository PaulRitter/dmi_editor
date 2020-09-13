using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DMIEditor.Tools
{
    public class Pipette : ClickTool
    {
        public override string Name => "Pipette";
        public override void PixelAct(Point p)
        {
            Color c = getPixel(p);
            if(c != Color.Transparent) MainWindow.Current.SetColor(c);
        }
    }
}
