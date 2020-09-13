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

            FillPropagator fillPropagator = new FillPropagator(this);
            List<Point> points = fillPropagator.findPixels(oldColor, p);
            List<PixelChangeItem> changeItems = new List<PixelChangeItem>();
            foreach (var point in points)
            {
                changeItems.Add(new PixelChangeItem(point, getPixel(point)));
            }
            MainWindow.Current.UndoManager.RegisterUndoItem(new PixelChangeUndoItem(Layer, changeItems));
            Layer.SetPixels(points.ToArray(), MainWindow.Current.GetColor());
        }

        

        private class FillPropagator
        {
            private Size _bounds;
            private Bitmap _bitmap; //much better to just clone bitmap an make our requests here

            public FillPropagator(Fill fillTool)
            {
                _bounds = new Size(fillTool.ImageWidth, fillTool.ImageHeight);
                _bitmap = fillTool.Layer.GetBitmap();
            }

            public List<Point> findPixels(Color oldColor, Point pixel)
            {
                List<Point> resultList = new List<Point>();

                List<Point> alreadyProcessed = new List<Point>();
                List<Point> workList = new List<Point> {pixel};
                while (workList.Count > 0)
                {
                    Point p = workList[^1];
                    workList.Remove(p);
                    
                    if (alreadyProcessed.Any(point => point.Equals(p)))
                        continue;
                    
                    alreadyProcessed.Add(p);
                    if (p.X >= _bounds.Width || p.X < 0)
                        continue;
                    if (p.Y >= _bounds.Height || p.Y < 0)
                        continue;

                    if (!oldColor.Equals(_bitmap.GetPixel(p.X, p.Y))) continue;
            
                    resultList.Add(p);
                    
                    workList.Add(new Point(p.X + 1, p.Y));
                    workList.Add(new Point(p.X - 1, p.Y));
                    workList.Add(new Point(p.X, p.Y + 1));
                    workList.Add(new Point(p.X, p.Y - 1));
                }

                return resultList;
            }
        }
    }
}
