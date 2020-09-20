using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DMI_Parser.Extended;
using DMIEditor.Undo;

namespace DMIEditor.Tools
{
    public abstract class PixelDragTool : DragTool
    {
        private List<PixelChangeItem> _changeBuffer;

        protected abstract Color getColor();
        
        protected override void OnDrawStart(DmiEXImage dmiExImage, Point p)
        {
            _changeBuffer = new List<PixelChangeItem>();
        }

        List<Point> pixels_to_change = new List<Point>();
        protected override void OnDrawMove(DmiEXImage dmiExImage, Point p)
        {
            if (getColor() == getPixel(p)) return;
            
            _changeBuffer.Add(new PixelChangeItem(p, getPixel(p)));
            pixels_to_change.Add(p);
        }

        protected override void OnDrawMoveEnded(DmiEXImage image)
        {
            setPixels(pixels_to_change.ToArray(), getColor());
            pixels_to_change.Clear();
        }

        protected override void OnDrawStop(DmiEXImage dmiExImage, Point p)
        {
            MainWindow.Current.UndoManager.RegisterUndoItem(new PixelChangeUndoItem(Layer, _changeBuffer));
            _changeBuffer = null;
        }
    }
}