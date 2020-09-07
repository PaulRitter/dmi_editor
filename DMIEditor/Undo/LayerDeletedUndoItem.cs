using DMI_Parser.Extended;

namespace DMIEditor.Undo
{
    public class LayerDeletedUndoItem : UndoItem
    {
        private DmiEXImage _image;
        private DmiEXLayer _layer;

        public LayerDeletedUndoItem(DmiEXImage image, DmiEXLayer layer)
        {
            _image = image;
            _layer = layer;
        }

        public override void ReverseAction()
        {
            _image.AddLayer(_layer);
        }
    }
}