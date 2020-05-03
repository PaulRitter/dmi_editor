using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DMIEditor.Tools
{
    public class Pen : EditorTool
    {
        public override string Name => "Pen";
        public Pen(MainWindow main) : base(main) { }

        public override bool pixelAct(ref Bitmap bitmap, int x, int y)
        {
            bitmap.SetPixel(x, y, main.getColor());
            return true;
        }
    }
}
