using DMI_Parser;
using DMI_Parser.Extended;

namespace DMIEditor.Undo
{
    public class LayerIndexChangeUndoItem : UndoItem
    {
        private DmiEXLayer _layer;
        private int _index;

        public LayerIndexChangeUndoItem(DmiEXLayer layer)
        {
            _layer = layer;
            _index = layer.Index;
        }

        public override void ReverseAction()
        {
            _layer.Index = _index;
        }
    }
}