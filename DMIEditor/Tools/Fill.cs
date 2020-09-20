using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using DMI_Parser.Extended;
using DMIEditor.Undo;

namespace DMIEditor.Tools
{
    public class Fill : ClickTool
    {
        public override string Name => "Fill";
        
        public override void PixelAct(Point p)
        {
            Color oldColor = getPixel(p);

            if (oldColor == MainWindow.Current.GetColor())
                return;

            List<Point> points = Layer.GetBitmap().GetSimilarPoints(p);
            List<PixelChangeItem> changeItems = new List<PixelChangeItem>();
            foreach (var point in points)
            {
                changeItems.Add(new PixelChangeItem(point, getPixel(point)));
            }
            MainWindow.Current.UndoManager.RegisterUndoItem(new PixelChangeUndoItem(Layer, changeItems));
            Layer.SetPixels(points.ToArray(), MainWindow.Current.GetColor());
        }

        

        
    }
}
