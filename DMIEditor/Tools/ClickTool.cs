using System.Drawing;

namespace DMIEditor.Tools
{
    public abstract class ClickTool : EditorTool
    {
        protected ClickTool(MainWindow main) : base(main)
        {
        }

        public override bool onSelected() => true;
        public override void onMouseEnter(DmiEXImage dmiExImage, Point p, bool LeftMousePressed){}
        public override void onLeftMouseUp(DmiEXImage dmiExImage, Point p){}
        public override void onMouseMove(DmiEXImage dmiExImage, Point p){}

        public override void onLeftMouseDown(DmiEXImage dmiExImage, Point p) => PixelAct(p);

        //returns true if bitmap was changed
        public virtual void PixelAct(Point p) {}
    }
}