using DMI_Parser;
using DMI_Parser.Extended;

namespace DMIEditor.Undo
{
    public class StateImageArraySizeChangeUndoItem : UndoItem
    {
        private DmiEXState _state;
        private DirCount _dirCount;
        private int _frames;
        private DmiEXImage[,] _images;
        
        public StateImageArraySizeChangeUndoItem(DmiEXState state)
        {
            _state = state;
            _dirCount = _state.Dirs;
            _frames = _state.Frames;
            _images = new DmiEXImage[(int)_dirCount,_frames];
            for (int dir = 0; dir < _images.GetLength(0); dir++)
            {
                for (int frame = 0; frame < _images.GetLength(1); frame++)
                {
                    _images[dir, frame] = (DmiEXImage)_state.GetImage(dir, frame).Clone();
                }
            }
        }

        public override void ReverseAction()
        {
            _state.Dirs = _dirCount;
            _state.Frames = _frames;
            _state.OverrideImageArray(_images);
        }
    }
}