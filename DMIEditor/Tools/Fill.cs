using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using DMI_Parser.Extended;
using DMIEditor.Undo;

namespace DMIEditor.Tools
{
    public class Fill : ClickTool
    {
        public override string Name => "Fill";
        public Fill(MainWindow main) : base(main) { }

        public override void PixelAct(Point p)
        {
            Color oldColor = getPixel(p);

            if (oldColor == main.GetColor())
                return;

            FillPropagator fillPropagator = new FillPropagator(this);
            fillPropagator.findPixels(oldColor, p);
            List<PixelChangeItem> changeItems = new List<PixelChangeItem>();
            foreach (var point in fillPropagator.Points)
            {
                changeItems.Add(new PixelChangeItem(point, getPixel(point)));
            }
            main.UndoManager.RegisterUndoItem(new PixelChangeUndoItem(Layer, changeItems));
            Layer.SetPixels(fillPropagator.Points.ToArray(), main.GetColor());
        }

        

        private class FillPropagator
        {
            public readonly List<Point> Points = new List<Point>();
            private List<Point> _alreadyProcessed = new List<Point>();
            private Size _bounds;
            private Bitmap _bitmap; //much better to just clone bitmap an make our requests here

            public FillPropagator(Fill fillTool)
            {
                _bounds = new Size(fillTool.ImageWidth, fillTool.ImageHeight);
                _bitmap = fillTool.Layer.GetBitmap();
            }

            //todo fix stackoverflow when pixels > 3637
            //maybe this can be fixed by making this a loop?
            public void findPixels(Color oldColor, Point pixel)
            {
                foreach (Point point in _alreadyProcessed)
                {
                    if (point.Equals(pixel))
                    {
                        return;
                    }
                }
                _alreadyProcessed.Add(pixel);
                if (pixel.X >= _bounds.Width || pixel.X < 0)
                    return;
                if (pixel.Y >= _bounds.Height || pixel.Y < 0)
                    return;

                if (!oldColor.Equals(_bitmap.GetPixel(pixel.X, pixel.Y))) return;
            
                Points.Add(pixel);

                findPixels(oldColor, new Point(pixel.X + 1, pixel.Y));
                findPixels(oldColor, new Point(pixel.X - 1, pixel.Y));
                findPixels(oldColor, new Point(pixel.X, pixel.Y + 1));
                findPixels(oldColor, new Point(pixel.X, pixel.Y - 1));
            }
        }
    }
}
