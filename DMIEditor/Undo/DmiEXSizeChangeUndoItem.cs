using System.IO;
using DMI_Parser.Extended;

namespace DMIEditor.Undo
{
    public class DmiEXSizeChangeUndoItem : UndoItem
    {
        private FileEditor _fileEditor;
        private DmiEX _dmiEx;

        public DmiEXSizeChangeUndoItem(FileEditor fileEditor)
        {
            _fileEditor = fileEditor;
            _dmiEx = (DmiEX)fileEditor.DmiEx.Clone();
        }

        public override void ReverseAction()
        {
            _fileEditor.attachDmiEX(_dmiEx);
        }
    }
}