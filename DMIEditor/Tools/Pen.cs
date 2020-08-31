using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DMIEditor.Tools
{
    public class Pen : DragTool
    {
        public override string Name => "Pen";
        public Pen(MainWindow main) : base(main) { }

        protected override void PixelAct(Bitmap bitmap, Point p)
        {
            bitmap.SetPixel(p.X, p.Y, main.GetColor());
            reRenderStateImage();
        }
    }
}
