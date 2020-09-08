using DMI_Parser.Extended;

namespace DMIEditor.Undo
{
    public class StateMovementChangeUndoItem : UndoItem
    {
        private DmiEXState _state;
        private bool _previousMovement;

        public StateMovementChangeUndoItem(DmiEXState state)
        {
            _state = state;
            _previousMovement = state.Movement;
        }

        public override void ReverseAction()
        {
            _state.Movement = _previousMovement;
        }
    }
}