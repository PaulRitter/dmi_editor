using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DMIEditor.Tools
{
    public class Pipette : EditorTool
    {
        public override string Name => "Pipette";
        public Pipette(MainWindow main) : base(main) { }

        public override bool pixelAct(Bitmap current, int x, int y)
        {
            Color c = current.GetPixel(x, y);
            if(c != Color.Transparent)
                main.SetColor(c);
            return false;
        }
    }
}
