using System.Collections.Generic;
using System.Drawing;
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

        protected override void OnDrawMove(DmiEXImage dmiExImage, Point p)
        {
            if (getColor() == getPixel(p)) return;
            
            _changeBuffer.Add(new PixelChangeItem(p, getPixel(p)));
            setPixel(p, getColor());
        }

        protected override void OnDrawStop(DmiEXImage dmiExImage, Point p)
        {
            MainWindow.Current.UndoManager.RegisterUndoItem(new PixelChangeUndoItem(Layer, _changeBuffer));
            _changeBuffer = null;
        }
    }
}