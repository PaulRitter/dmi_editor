using DMI_Parser.Extended;

namespace DMIEditor.Undo
{
    public class StateIdChangeUndoItem : UndoItem
    {
        private DmiEXState _state;
        private string _previousId;

        public StateIdChangeUndoItem(DmiEXState state)
        {
            _state = state;
            _previousId = state.Id;
        }

        public override void ReverseAction()
        {
            _state.Id = _previousId;
        }
    }
}