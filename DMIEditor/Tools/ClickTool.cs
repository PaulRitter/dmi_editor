using System.Drawing;
using DMI_Parser.Extended;

namespace DMIEditor.Tools
{
    public abstract class ClickTool : EditorTool
    {
        public override void onLeftMouseDown(DmiEXImage dmiExImage, Point p) => PixelAct(p);

        public abstract void PixelAct(Point p);
    }
}