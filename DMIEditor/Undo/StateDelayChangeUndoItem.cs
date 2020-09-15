using DMI_Parser.Extended;

namespace DMIEditor.Undo
{
    public class StateDelayChangeUndoItem : UndoItem
    {
        private DmiEXState _state;
        private int _frameIndex;
        private double _previousDelay;

        public StateDelayChangeUndoItem(DmiEXState state, int frameIndex)
        {
            _state = state;
            _frameIndex = frameIndex;
            _previousDelay = _state.Delays[frameIndex];
        }

        public override void ReverseAction()
        {
            _state.SetDelay(_frameIndex, _previousDelay);
        }
    }
}