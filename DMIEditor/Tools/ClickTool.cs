using System.Drawing;

namespace DMIEditor.Tools
{
    public abstract class ClickTool : EditorTool
    {
        protected ClickTool(MainWindow main) : base(main)
        {
        }

        public override bool onSelected() => true;
        public override void onMouseEnter(Bitmap target, Point p){}
        public override void onMouseExit(Bitmap target, Point p){}
        public override void onLeftMouseUp(Bitmap target, Point p){}
        public override void onMouseMove(Bitmap target, Point p){}

        public override void onLeftMouseDown(Bitmap target, Point p)
            => PixelAct(target, p.X, p.Y);

        //returns true if bitmap was changed
        public virtual void PixelAct(Bitmap current, int x, int y) {}
    }
}