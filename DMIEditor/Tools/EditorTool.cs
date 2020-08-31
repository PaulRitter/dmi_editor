using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DMIEditor.Tools
{
    public abstract class EditorTool
    {
        public abstract string Name { get; }
        //public readonly XYZ icon;
        internal MainWindow main;

        public EditorTool(MainWindow main)
        {
            this.main = main;
        }

        // returns true if selection should be locked in
        // false if previous tool should be kept
        // useful for button-press-only tools
        public abstract bool onSelected();

        public abstract void onMouseEnter(Bitmap target, Point p);
        public abstract void onMouseExit(Bitmap target, Point p);
        public abstract void onLeftMouseDown(Bitmap target, Point p);
        public abstract void onLeftMouseUp(Bitmap target, Point p);
        public abstract void onMouseMove(Bitmap target, Point p);

        protected void reRenderStateImage()
        {
            main.SelectedEditor.selectedStateEditor.ReRenderImage();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
