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

        protected override void OnDrawMove(DmiEXImage dmiExImage, Point p)
        {
            double d_x = Math.Abs(p.X - lastPoint.X);
            double d_y = Math.Abs(p.Y - lastPoint.Y);
            List<Point> pixels;
            if ((d_x > 1 || d_y > 1) && !recentExit)
            {
                pixels = GetPointsBetween(lastPoint, p);
            }
            else
            {
                //our pixels are adjacent, we dont need to interpolate
                pixels = new List<Point>(){p};
            }

            pixels.RemoveAll(point => point.X < 0 || point.Y < 0);
            pixels.RemoveAll(point => point.X >= Layer.Width || point.Y >= Layer.Height);
            pixels.RemoveAll(point => getColor() == getPixel(point));

            foreach (var pixel in pixels)
            {
                _changeBuffer.Add(new PixelChangeItem(pixel, getPixel(p)));
            }
            setPixels(pixels.ToArray(), getColor());
        }

        private List<Point> GetPointsBetween(Point p1, Point p2)
        {
            double step_size = 0.01; //todo adjust
            List<Point> points = new List<Point>();
            FloatingPoint pointer = new FloatingPoint(p1);

            int CompareX(Point start, Point end)
            {
                return end.X.CompareTo(start.X);
            }
            int CompareY(Point start, Point end)
            {
                return end.Y.CompareTo(start.Y);
            }
            
            int x_comp = CompareX(p1, p2);
            int y_comp = CompareY(p1, p2);
            FloatingPoint delta = new FloatingPoint(p1, p2).Multiply(step_size);
            while (x_comp != CompareX(p2, pointer.Floored()) || y_comp != CompareY(p2, pointer.Floored()))
            {
                pointer = pointer.Add(delta);
                Point current = pointer.Floored();
                if(!points.Contains(current)) points.Add(current);
            }
            return points;
        }

        private struct FloatingPoint //haha, get it?
        {
            public double X;
            public double Y;

            public FloatingPoint(Point p)
            {
                X = p.X;
                Y = p.Y;
            }
            
            public FloatingPoint(Point p1, Point p2)
            {
                X = p2.X - p1.X;
                Y = p2.Y - p1.Y;
            }

            public FloatingPoint Add(FloatingPoint p)
            {
                X += p.X;
                Y += p.Y;
                return this;
            }

            public FloatingPoint Multiply(double d)
            {
                X *= d;
                Y *= d;
                return this;
            }

            public Point Floored() => new Point((int)Math.Floor(X), (int)Math.Floor(Y));
        }

        protected override void OnDrawStop(DmiEXImage dmiExImage, Point p)
        {
            MainWindow.Current.UndoManager.RegisterUndoItem(new PixelChangeUndoItem(Layer, _changeBuffer));
            _changeBuffer = null;
        }
    }
}