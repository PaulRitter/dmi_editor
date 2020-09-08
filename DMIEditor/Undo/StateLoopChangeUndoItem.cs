using DMI_Parser.Extended;

namespace DMIEditor.Undo
{
    public class StateLoopChangeUndoItem : UndoItem
    {
        private DmiEXState _state;
        private int _previousLoop;

        public StateLoopChangeUndoItem(DmiEXState state)
        {
            _state = state;
            _previousLoop = state.Loop;
        }

        public override void ReverseAction()
        {
            _state.Loop = _previousLoop;
        }
    }
}