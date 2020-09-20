using DMI_Parser.Extended;

namespace DMIEditor.Undo
{
    public class StateDeleteUndoItem : UndoItem
    {
        private DmiEX _dmiEx;
        private DmiEXState _state;
        private int _index;

        public StateDeleteUndoItem(DmiEX dmiEx, DmiEXState state)
        {
            _dmiEx = dmiEx;
            _index = dmiEx.GetStateIndex(state);
            _state = state;
        }

        public override void ReverseAction()
        {
            _dmiEx.AddState(_state, _index);
        }
    }
}