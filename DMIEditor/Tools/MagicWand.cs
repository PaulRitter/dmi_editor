using System.Drawing;

namespace DMIEditor.Tools
{
    public class MagicWand : ClickTool
    {
        public override string Name => "MagicWand";
        public override void PixelAct(Point p)
        {
            var points = Layer.GetBitmap().GetSimilarPoints(p);
            ImageEditor.SetSelection(points.ToArray());
        }
    }
}