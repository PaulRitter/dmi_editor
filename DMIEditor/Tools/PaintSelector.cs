using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using DMI_Parser.Extended;

namespace DMIEditor.Tools
{
    public class PaintSelector : DragTool
    {
        public override string Name => "Selector";

        public override void OnDeselected()
        {
            ImageEditor.ClearSelection();
        }

        protected override void OnDrawMove(DmiEXImage dmiExImage, Point[] p)
        {
            if(Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                ImageEditor.DeSelectPixel(p);
            else
                ImageEditor.SelectPixel(p);
        }
    }
}