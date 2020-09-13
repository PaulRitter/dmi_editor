using System.Drawing;
using DMI_Parser;

namespace DMIEditor.Tools
{
    public class HotspotTool : ClickTool
    {
        public override string Name => "Hotspot";

        public override void OnSelected()
        {
            MainWindow.Current.ViewHotspots = true;
        }

        public override void OnDeselected()
        {
            MainWindow.Current.ViewHotspots = false;
        }

        public override void PixelAct(Point p)
        {
            //todo create hotspot
            Hotspot hotspot = State.AddHotspot(p.X, p.Y, ImageEditor.FrameIndex, ImageEditor.DirIndex);
            
            
        }
    }
}