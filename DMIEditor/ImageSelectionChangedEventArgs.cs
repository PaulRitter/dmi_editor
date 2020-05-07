using System;
using System.Collections.Generic;
using System.Text;

namespace DMIEditor
{
    public class ImageSelectionChangedEventArgs : EventArgs
    {
        public readonly int State, Dir, Frame;
        public ImageSelectionChangedEventArgs(int state, int dir, int frame)
        {
            this.State = state;
            this.Dir = dir;
            this.Frame = frame;
        }
    }
}
