using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DMIEditor.Tools
{
    public class Eraser : PixelDragTool
    {
        public override string Name => "Eraser";

        protected override Color getColor() => Color.Transparent;
    }
}
