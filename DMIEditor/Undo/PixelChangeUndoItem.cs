using System.Collections.Generic;
using DMI_Parser.Extended;

namespace DMIEditor.Undo
{
    public class PixelChangeUndoItem : UndoItem
    {
        private DmiEXLayer _layer;
        private List<PixelChangeItem> _pixelChanges;

        public PixelChangeUndoItem(DmiEXLayer layer, List<PixelChangeItem> pixelChanges)
        {
            _layer = layer;
            _pixelChanges = pixelChanges;
        }

        public override void ReverseAction()
        {
            _layer.SetPixels(_pixelChanges.ToArray());
        }
    }
}