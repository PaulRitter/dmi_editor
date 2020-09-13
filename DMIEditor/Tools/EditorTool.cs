﻿using System.Drawing;
using DMI_Parser.Extended;

namespace DMIEditor.Tools
{
    public abstract class EditorTool
    {
        public abstract string Name { get; }
        //public readonly XYZ icon;
        
        // returns true if selection should be locked in
        // false if previous tool should be kept
        // useful for button-press-only tools
        public virtual bool ShouldBeKept => true;
        public virtual void OnSelected(){}
        public virtual void OnDeselected(){}

        public virtual void onMouseEnter(DmiEXImage dmiExImage, Point p, bool LeftMousePressed){}

        public virtual void onMouseExited(DmiEXImage dmiExImage, Point p){}
        public virtual void onLeftMouseDown(DmiEXImage dmiExImage, Point p){}
        public virtual void onLeftMouseUp(DmiEXImage dmiExImage, Point p){}
        public virtual void onMouseMove(DmiEXImage dmiExImage, Point p){}

        protected void setPixel(Point p, Color c)
            => Layer.SetPixel(p, c);

        protected Color getPixel(Point p) => Layer.GetPixel(p);

        protected DmiEXLayer Layer => MainWindow.Current.SelectedEditor.SelectedStateEditor.ImageEditor.SelectedLayer;

        protected int ImageWidth => MainWindow.Current.SelectedEditor.SelectedStateEditor.ImageEditor.SelectedLayer.Width;
        protected int ImageHeight => MainWindow.Current.SelectedEditor.SelectedStateEditor.ImageEditor.SelectedLayer.Height;
        
        public override string ToString()
        {
            return Name;
        }
    }
}
