using System.Drawing;

namespace DMIEditor.Tools
{
    public abstract class PixelTool : EditorTool
    {
        protected PixelTool(MainWindow main) : base(main)
        {
        }
        
        //returns true if bitmap was changed
        public virtual bool PixelAct(Bitmap current, int x, int y) { return false; }
    }
}