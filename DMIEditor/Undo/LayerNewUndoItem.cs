using System.Linq;
using DMI_Parser.Extended;

namespace DMIEditor.Undo
{
    public class LayerNewUndoItem : UndoItem
    {
        private DmiEXImage _image;
        private DmiEXLayer _layer;

        public LayerNewUndoItem(DmiEXImage image, DmiEXLayer layer)
        {
            _image = image;
            _layer = layer;
        }

        public override void ReverseAction()
        {
            // intentionally not reverting index clearing
            _image.RemoveLayer(_layer);
        }
    }
}