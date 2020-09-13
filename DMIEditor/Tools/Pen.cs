using System.Drawing;

namespace DMIEditor.Tools
{
    public class Pen : PixelDragTool
    {
        public override string Name => "Pen";

        protected override Color getColor() => MainWindow.Current.GetColor();
    }
}
