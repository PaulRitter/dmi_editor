using DMI_Parser.Extended;

namespace DMIEditor.Undo
{
    public class StateRewindChangeUndoItem : UndoItem
    {
        private DmiEXState _state;
        private bool _previousRewind;

        public StateRewindChangeUndoItem(DmiEXState state)
        {
            _state = state;
            _previousRewind = state.Rewind;
        }

        public override void ReverseAction()
        {
            _state.Rewind = _previousRewind;
        }
    }
}