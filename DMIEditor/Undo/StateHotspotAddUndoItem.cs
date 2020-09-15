using DMI_Parser;
using DMI_Parser.Extended;

namespace DMIEditor.Undo
{
    public class StateHotspotAddUndoItem : UndoItem
    {
        private DmiEXState _state;
        private Hotspot _hotspot;

        public StateHotspotAddUndoItem(DmiEXState state, Hotspot hotspot)
        {
            _state = state;
            _hotspot = hotspot;
        }

        public override void ReverseAction()
        {
            _state.RemoveHotspot(_hotspot);
        }
    }
}