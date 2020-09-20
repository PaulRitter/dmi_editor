using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using DMI_Parser.Extended;

namespace DMIEditor.Tools
{
    public abstract class DragTool :  EditorTool
    {
        private bool _mouseHeld;

        //todo drawstop when image/layer gets changed

        protected Point StartingPoint { get; private set;  } = new Point(-1,-1);
        private void drawStart(DmiEXImage dmiExImage, Point p)
        {
            if(_mouseHeld) return;

            StartingPoint = p;
            _mouseHeld = true;
            OnDrawStart(dmiExImage, p);
        }

        private Point lastPoint = new Point(-1,-1);
        private void drawMove(DmiEXImage dmiExImage, Point p)
        {
            if (!_mouseHeld) return;
            if (p.Equals(lastPoint)) return;
            if (lastPoint.X == -1 && lastPoint.Y == -1) lastPoint = p;
            
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
            
            foreach (var pixel in pixels)
            {
                OnDrawMove(dmiExImage, pixel);
            }
            lastPoint = p;
            recentExit = false;
            OnDrawMoveEnded(dmiExImage);
        }

        protected virtual void OnDrawMoveEnded(DmiEXImage image){}
        
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
        
        private void drawStop(DmiEXImage dmiExImage, Point p)
        {
            if (!_mouseHeld) return;

            _mouseHeld = false;
            lastPoint = new Point(-1,-1);
            OnDrawStop(dmiExImage, p);
        }

        protected virtual void OnDrawStart(DmiEXImage dmiExImage, Point p){}

        protected virtual void OnDrawMove(DmiEXImage dmiExImage, Point p){}
        protected virtual void OnDrawStop(DmiEXImage dmiExImage, Point p){}

        public override void onMouseEnter(DmiEXImage dmiExImage, Point p, bool leftMousePressed)
        {
            _cancellationToken?.Cancel();

            if (_mouseHeld == leftMousePressed) return;
            
            if(_mouseHeld)
                drawStop(dmiExImage, p);
            else
                drawStart(dmiExImage, p);
        }

        protected bool recentExit;
        private CancellationTokenSource _cancellationToken;
        public override void onMouseExited(DmiEXImage dmiExImage, Point p)
        {
            if (!_mouseHeld) return;

            recentExit = true;
            
            _cancellationToken?.Cancel();
            _cancellationToken = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
            {
                Task.Delay(2000, _cancellationToken.Token).Wait(_cancellationToken.Token);
                drawStop(dmiExImage, p);
            });
        }

        public override void onLeftMouseDown(DmiEXImage dmiExImage, Point p)
        {
            _cancellationToken?.Cancel();
            
            drawStart(dmiExImage, p);
            drawMove(dmiExImage, p);
        }
        
        public override void onLeftMouseUp(DmiEXImage dmiExImage, Point p)
        {
            _cancellationToken?.Cancel();

            drawStop(dmiExImage, p);
        }
        
        public override void onMouseMove(DmiEXImage dmiExImage, Point p)
        {
            _cancellationToken?.Cancel();

            drawMove(dmiExImage, p);
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

    }
}