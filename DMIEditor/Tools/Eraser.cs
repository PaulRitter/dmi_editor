using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DMIEditor.Tools
{
    public class Eraser : PixelDragTool
    {
        public override string Name => "Eraser";
        public Eraser(MainWindow main) : base(main) { }

        protected override Color getColor() => Color.Transparent;
    }
}
