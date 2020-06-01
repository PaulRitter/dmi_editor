using System.Drawing;
using DMI_Parser;

namespace DMIEditor.Tools
{
    public class HotspotTool : PixelTool
    {
        public HotspotTool(MainWindow main) : base(main)
        {
        }

        public override string Name => "Hotspot";

        public override bool PixelAct(Bitmap current, int x, int y)
        {
            StateEditor editor = main.SelectedEditor.selectedStateEditor;
            int dir = editor.DirIndex;
            int frame = editor.FrameIndex;
            //todo create hotspot

            return false;
        }
    }
}