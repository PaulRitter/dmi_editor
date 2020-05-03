using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DMIEditor.Tools
{
    public class Eraser : EditorTool
    {
        public override string Name => "Eraser";
        public Eraser(MainWindow main) : base(main) { }

        public override bool pixelAct(ref Bitmap bitmap, int x, int y)
        {
            bitmap.SetPixel(x, y, Color.Transparent);
            return true;
        }
    }
}
