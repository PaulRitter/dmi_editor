using System.Linq;
using DMI_Parser.Extended;

namespace DMIEditor.Undo
{
    public class LayerNewMemento : Memento
    {
        private DmiEXImage _image;
        private DmiEXLayer _layer;

        public LayerNewMemento(DmiEXImage image, DmiEXLayer layer)
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