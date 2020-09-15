using System;
using System.Windows.Controls;
using System.Windows.Input;
using DMI_Parser;
using DMI_Parser.Extended;
using DMIEditor.Undo;
using Xceed.Wpf.Toolkit;

namespace DMIEditor
{
    public partial class StateValueEditor : UserControl
    {
        public readonly DmiEXState State;
        public StateValueEditor(DmiEXState state)
        {
            State = state;
            InitializeComponent();
            
            //stateID
            id_editor.Text = state.Id;
            id_editor.KeyDown += ChangeID;
            id_editor.LostFocus += IDChanged;
            state.IdChanged += IDChanged;

            //dir count
            foreach (DirCount dir in Enum.GetValues(typeof(DirCount)))
            {
                dir_editor.Items.Add(dir);
            }
            dir_editor.SelectedItem = state.Dirs;
            dir_editor.SelectionChanged += ChangeDirs;
            state.DirCountChanged += DirsChanged;

            //frame count
            frame_editor.Value = state.Frames;
            frame_editor.KeyDown += ChangeFrames;
            frame_editor.LostFocus += FramesChanged;
            state.FrameCountChanged += FramesChanged;

            //loop
            loop_editor.Value = state.Loop;
            loop_editor.KeyDown += ChangeLoop;
            loop_editor.LostFocus += LoopChanged;
            state.LoopCountChanged += LoopChanged;
            loop_infinite_indicator.Text = state.Loop == 0 ? "(Infinite)" : "";

            //rewind
            rewind_editor.IsChecked = state.Rewind;
            rewind_editor.Click += ChangeRewind;
            state.RewindChanged += RewindChanged;

            
            //movement
            movement_editor.IsChecked = state.Movement;
            movement_editor.Click += ChangeMovement;
            state.MovementChanged += MovementChanged;
        }

        private void ChangeID(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
                
            try
            {
                MainWindow.Current.UndoManager.RegisterUndoItem(new StateIdChangeUndoItem(State));
                State.Id = id_editor.Text;
            }
            catch (ArgumentException)
            {
                ErrorPopupHelper.Create($"StateID \"{id_editor.Text}\" is not valid!");
            }
        }

        private void IDChanged(object sender, EventArgs e)
        {
            id_editor.Text = State.Id;
        }

        
        private void ChangeDirs(object sender, EventArgs e)
        {
            MainWindow.Current.UndoManager.RegisterUndoItem(new StateImageArraySizeChangeUndoItem(State));
            State.Dirs = (DirCount) dir_editor.SelectedItem;
        }

        private void DirsChanged(object sender, EventArgs e)
        {
            dir_editor.SelectionChanged -= ChangeDirs;
            dir_editor.SelectedItem = State.Dirs;
            dir_editor.SelectionChanged += ChangeDirs;
        }
        
        private void ChangeFrames(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (!frame_editor.Value.HasValue) return;
            
            MainWindow.Current.UndoManager.RegisterUndoItem(new StateImageArraySizeChangeUndoItem(State));
            State.Frames = frame_editor.Value.Value;
        }

        private void FramesChanged(object sender, EventArgs e)
        {
            frame_editor.Value = State.Frames;
        }
        
        private void ChangeLoop(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (!loop_editor.Value.HasValue) return;
            
            MainWindow.Current.UndoManager.RegisterUndoItem(new StateLoopChangeUndoItem(State));
            State.Loop = loop_editor.Value.Value;
        }

        private void LoopChanged(object sender, EventArgs e)
        {
            loop_infinite_indicator.Text = State.Loop == 0 ? "(Infinite)" : "";
            
            loop_editor.Value = State.Loop;
        }
        
        private void ChangeRewind(object sender, EventArgs e)
        {
            if (!rewind_editor.IsChecked.HasValue) return;
            
            MainWindow.Current.UndoManager.RegisterUndoItem(new StateRewindChangeUndoItem(State));
            State.Rewind = rewind_editor.IsChecked.Value;
        }

        private void RewindChanged(object sender, EventArgs e)
        {
            rewind_editor.IsChecked = State.Rewind;
        }
        
        private void ChangeMovement(object sender, EventArgs e)
        {
            if (!movement_editor.IsChecked.HasValue) return;
            
            MainWindow.Current.UndoManager.RegisterUndoItem(new StateMovementChangeUndoItem(State));
            State.Movement = movement_editor.IsChecked.Value;
        }

        private void MovementChanged(object sender, EventArgs e)
        {
            movement_editor.IsChecked = State.Movement;
        }
    }
}