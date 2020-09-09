using System;
using System.Windows.Controls;
using System.Windows.Input;
using DMI_Parser.Extended;
using DMIEditor.Undo;

namespace DMIEditor
{
    public partial class FileValueEditor : UserControl
    {
        private DmiEX _dmiEx;
        
        public FileValueEditor(DmiEX dmiEx)
        {
            _dmiEx = dmiEx;
            InitializeComponent();

            width_editor.Value = dmiEx.Width;
            width_editor.KeyDown += ChangeSize;
            dmiEx.WidthChanged += WidthChanged;

            height_editor.Value = dmiEx.Height;
            height_editor.KeyDown += ChangeSize;
            dmiEx.HeightChanged += HeightChanged;

            new_state_editor.KeyDown += NewState;
        }

        private void ChangeSize(object sender, KeyEventArgs e) //todo make this less shit to enable up/down buttons
        {
            if (e.Key != Key.Enter) return;

            int? width = width_editor.Value;
            int? height = height_editor.Value;
            if (width.HasValue)
            {
                _dmiEx.Width = width.Value;
            }

            if (height.HasValue)
            {
                _dmiEx.Height = height.Value;
            }
            //todo undoitem
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