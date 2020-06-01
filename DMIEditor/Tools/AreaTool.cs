using System.Drawing;

namespace DMIEditor.Tools
{
    public abstract class AreaTool : EditorTool
    {
        protected AreaTool(MainWindow main) : base(main)
        {
        }
        
        //returns true if bitmap was changed
        //x and y mark upper left corner
        public virtual bool AreaAct(Bitmap current, int x, int y, int width, int length) { return false; }
    }
}