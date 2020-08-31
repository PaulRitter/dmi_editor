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
        internal MainWindow main; //todo remove this since we got MainWindow.current

        public EditorTool(MainWindow main)
        {
            this.main = main;
        }

        // returns true if selection should be locked in
        // false if previous tool should be kept
        // useful for button-press-only tools
        public abstract bool onSelected();

        public abstract void onMouseEnter(DmiEXImage dmiExImage, Point p, bool LeftMousePressed);
        public abstract void onLeftMouseDown(DmiEXImage dmiExImage, Point p);
        public abstract void onLeftMouseUp(DmiEXImage dmiExImage, Point p);
        public abstract void onMouseMove(DmiEXImage dmiExImage, Point p);

        protected void setPixel(Point p, Color c)
            => main.SelectedEditor.SelectedStateEditor.ImageEditor.SelectedLayer.setPixel(p, c);

        protected Color getPixel(Point p) => main.SelectedEditor.SelectedStateEditor.ImageEditor.SelectedLayer.getPixel(p);

        protected int ImageWidth => main.SelectedEditor.SelectedStateEditor.ImageEditor.SelectedLayer.Bitmap.Width;
        protected int ImageHeight => main.SelectedEditor.SelectedStateEditor.ImageEditor.SelectedLayer.Bitmap.Height;
        
        public override string ToString()
        {
            return Name;
        }
    }
}
