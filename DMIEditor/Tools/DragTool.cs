using System;
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
        
        private void drawStart(DmiEXImage dmiExImage, Point p)
        {
            if(_mouseHeld) return;
            
            _mouseHeld = true;
            OnDrawStart(dmiExImage, p);
        }

        protected Point lastPoint = new Point(-1,-1);
        private void drawMove(DmiEXImage dmiExImage, Point p)
        {
            if (!_mouseHeld) return;
            if (p.Equals(lastPoint)) return;
            if (lastPoint.X == -1 && lastPoint.Y == -1) lastPoint = p;
            
            OnDrawMove(dmiExImage, p);
            lastPoint = p;
            recentExit = false;
        }
        
        private void drawStop(DmiEXImage dmiExImage, Point p)
        {
            if (!_mouseHeld) return;

            _mouseHeld = false;
            lastPoint = new Point(-1,-1);
            OnDrawStop(dmiExImage, p);
        }
        protected abstract void OnDrawStart(DmiEXImage dmiExImage, Point p);
        protected abstract void OnDrawMove(DmiEXImage dmiExImage, Point p);
        protected abstract void OnDrawStop(DmiEXImage dmiExImage, Point p);

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
    }
}