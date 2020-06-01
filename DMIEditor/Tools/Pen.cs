using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DMIEditor.Tools
{
    public class Pen : PixelTool
    {
        public override string Name => "Pen";
        public Pen(MainWindow main) : base(main) { }

        public override bool PixelAct(Bitmap bitmap, int x, int y)
        {
            bitmap.SetPixel(x, y, main.GetColor());
            return true;
        }
    }
}
