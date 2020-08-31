using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace DMIEditor.Tools
{
    public abstract class DragTool :  EditorTool
    {
        protected DragTool(MainWindow main) : base(main) {}

        public override bool onSelected() => true;

        private bool _mouseHeld;
        private CancellationTokenSource _cancelToken;

        public override void onMouseEnter(Bitmap target, Point p)
        {
            _cancelToken?.Cancel();
            _cancelToken = null;
        }

        public override void onMouseExit(Bitmap target, Point p)
        {
            _cancelToken?.Cancel();
            _cancelToken = new CancellationTokenSource();

            Task.Run(() =>
            {
                Task.Delay(1000, _cancelToken.Token).Wait();
                _mouseHeld = false;
            });
        }
        
        public override void onLeftMouseDown(Bitmap target, Point p)
        {
            _mouseHeld = true;
            PixelAct(target, p);
        }
        
        public override void onLeftMouseUp(Bitmap target, Point p)
        {
            _mouseHeld = false;
        }
        
        public override void onMouseMove(Bitmap target, Point p)
        {
            if(_mouseHeld) PixelAct(target, p);
        }

        //will try to modify the specified pixel with the selected tool
        protected abstract void PixelAct(Bitmap target, Point p);
    }
}