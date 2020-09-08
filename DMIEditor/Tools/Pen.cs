using System.Drawing;

namespace DMIEditor.Tools
{
    public class Pen : PixelDragTool
    {
        public override string Name => "Pen";
        public Pen(MainWindow main) : base(main) { }

        protected override Color getColor() => main.GetColor();
    }
}
