using System;
using System.Collections.Generic;
using System.Text;

namespace DMIEditor
{
    public class ImageSelectionChangedEventArgs : EventArgs
    {
        public readonly int state, dir, frame;
        public ImageSelectionChangedEventArgs(int state, int dir, int frame)
        {
            this.state = state;
            this.dir = dir;
            this.frame = frame;
        }
    }
}
