﻿using System;
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

        public override void onMouseEnter(DmiEXImage dmiExImage, Point p, bool LeftMousePressed)
        {
            _mouseHeld = LeftMousePressed;
        }

        public override void onLeftMouseDown(DmiEXImage dmiExImage, Point p)
        {
            _mouseHeld = true;
            PixelAct(p);
        }
        
        public override void onLeftMouseUp(DmiEXImage dmiExImage, Point p)
        {
            _mouseHeld = false;
        }
        
        public override void onMouseMove(DmiEXImage dmiExImage, Point p)
        {
            if(_mouseHeld) PixelAct(p);
        }

        //will try to modify the specified pixel with the selected tool
        protected abstract void PixelAct(Point p);
    }
}