using DMI_Parser.Extended;

namespace DMIEditor.Undo
{
    public class StateNewUndoItem : UndoItem
    {
        private DmiEX _dmiEx;
        private DmiEXState _state;

        public StateNewUndoItem(DmiEX dmiEx, DmiEXState state)
        {
            _dmiEx = dmiEx;
            _state = state;
        }

        public override void ReverseAction()
        {
            _dmiEx.RemoveState(_state);
        }
    }
}