using System.Drawing;
using DMI_Parser.Extended;

namespace DMIEditor.Tools
{
    public abstract class ClickTool : EditorTool
    {
        protected ClickTool(MainWindow main) : base(main)
        {
        }

        public override void onLeftMouseDown(DmiEXImage dmiExImage, Point p) => PixelAct(p);

        public abstract void PixelAct(Point p);
    }
}