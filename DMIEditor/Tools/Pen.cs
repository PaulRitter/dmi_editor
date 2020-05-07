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

        public override bool pixelAct(Bitmap bitmap, int x, int y)
        {
            bitmap.SetPixel(x, y, main.GetColor());
            return true;
        }
    }
}
