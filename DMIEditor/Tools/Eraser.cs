using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DMIEditor.Tools
{
    public class Eraser : DragTool
    {
        public override string Name => "Eraser";
        public Eraser(MainWindow main) : base(main) { }

        protected override void PixelAct(Point p)
        {
            setPixel(p, Color.Transparent);
        }
    }
}
