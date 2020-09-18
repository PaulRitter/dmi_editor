using System;
using System.Windows.Controls;
using System.Windows.Input;
using DMI_Parser.Extended;
using DMIEditor.Undo;

namespace DMIEditor
{
    public partial class FileValueEditor : UserControl
    {
        private FileEditor _fileEditor;
        private DmiEX _dmiEx => _fileEditor?.DmiEx;
        
        public FileValueEditor(FileEditor fileEditor)
        {
            InitializeComponent();
            _fileEditor = fileEditor;

            width_editor.Value = _dmiEx.Width;
            width_editor.KeyDown += ChangeSize;
            width_editor.LostFocus += WidthChanged;
            _dmiEx.WidthChanged += WidthChanged;

            height_editor.Value = _dmiEx.Height;
            height_editor.KeyDown += ChangeSize;
            height_editor.LostFocus += HeightChanged;
            _dmiEx.HeightChanged += HeightChanged;

            new_state_editor.KeyDown += NewState;
        }

        private void ChangeSize(object sender, KeyEventArgs e) //todo make this less shit to enable up/down buttons
        {
            if (e.Key != Key.Enter) return;


            int? width = width_editor.Value;
            int? height = height_editor.Value;
            
            if(width.HasValue || height.HasValue)
                MainWindow.Current.UndoManager.RegisterUndoItem(new DmiEXSizeChangeUndoItem(_fileEditor));

            
            if (width.HasValue)
            {
                _dmiEx.Width = width.Value;
            }

            if (height.HasValue)
            {
                _dmiEx.Height = height.Value;
            }
        }
        
        private void WidthChanged(object sender, EventArgs e)
        {
            width_editor.Value = _dmiEx.Width;
        }
        
        private void HeightChanged(object sender, EventArgs e)
        {
            height_editor.Value = _dmiEx.Height;
        }
        
        private void NewState(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            try
            {
                var id = new_state_editor.Text;
                DmiEXState state = (DmiEXState)_dmiEx.AddNewState(id);
                    
                MainWindow.Current.UndoManager.RegisterUndoItem(new StateNewUndoItem(_dmiEx, state));
            }
            catch (ArgumentException ex)
            {
                ErrorPopupHelper.Create(ex);
            }
        }
    }
}