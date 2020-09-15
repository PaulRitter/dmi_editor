using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DMI_Parser;
using DMI_Parser.Extended;
using DMI_Parser.Utils;
using DMIEditor.Undo;
using Xceed.Wpf.Toolkit;

namespace DMIEditor
{
    /// <summary>
    /// Interaktionslogik für FileEditor.xaml
    /// </summary>
    public partial class FileEditor : UserControl
    {
        public readonly DmiEX DmiEx;
        public string Path { get; set; }
        public readonly MainWindow Main;

        private List<StateButton> _stateButtons = new List<StateButton>();

        public FileEditor(DmiEX dmiEx, MainWindow main, string path)
        {
            DmiEx = dmiEx;
            Main = main;
            Path = path;
            InitializeComponent();

            dmiEx.StateListChanged += CreateStateButtons;
            
            //adding state buttons
            CreateStateButtons();
            
            // create dmi value editor
            dmiValueEditorGrid.Children.Add(new FileValueEditor(DmiEx));

            //image selection hotkeys
            stateTabControl.SelectionChanged += UpdateStateUi;
        }

        private void CreateStateButtons(object sender = null, EventArgs e = null)
        {
            statePanel.Children.Clear();
            for (int i = 0; i < DmiEx.States.Length; i++)
            {
                StateButton btn = new StateButton(this, i, (DmiEXState)DmiEx.States[i]);
                statePanel.Children.Add(btn);
                _stateButtons.Add(btn);
            }
        }

        private void SelectOrOpenState(int stateIndex)
        {
            foreach (StateEditorTabItem item in stateTabControl.Items)
            {
                if(item.StateEditor.StateIndex == stateIndex)
                {
                    item.IsSelected = true;
                    UpdateStateUi();
                    return;
                }
            }

            DmiEXState state = (DmiEXState) DmiEx.States[stateIndex];
            StateEditor stateEditor = new StateEditor(this, stateIndex, state);
            StateEditorTabItem tItem = new StateEditorTabItem(stateEditor);
            stateTabControl.Items.Add(tItem);

            StackPanel sp = new StackPanel();
            TextBlock txt = new TextBlock
            {
                Text = $"\"{state.Id}\"", VerticalAlignment = VerticalAlignment.Center
            };
            state.IdChanged += (o, e) =>
            {
                txt.Text = $"\"{state.Id}\"";
            };
            sp.Orientation = Orientation.Horizontal;
            sp.Children.Add(txt);

            TabCloseButton cBtn = new TabCloseButton(tItem);
            cBtn.Click += CloseState;
            sp.Children.Add(cBtn);

            tItem.Header = sp;
            tItem.IsSelected = true;
            UpdateStateUi();
        }

        private void CloseState(object sender, EventArgs e)
        {
            TabCloseButton btn = (TabCloseButton)sender;
            StateEditorTabItem tItem = (StateEditorTabItem)btn.tabItem;
            CloseState(tItem);
        }

        private void CloseState(StateEditorTabItem stateTab)
        {
            stateTabControl.Items.Remove(stateTab);
            UpdateStateUi();
        }

        public StateEditor GetStateEditor(DMIState state)
        {
            return (from StateEditorTabItem tabItem in stateTabControl.Items where tabItem.StateEditor.State == state select tabItem.StateEditor).FirstOrDefault();
        }

        // handles arrow keys for image selection
        /*private void ImageSelectionKeyHandler(object sender, KeyEventArgs e)
        {
            //getting currently selected Tab
            StateEditorTabItem currentTab = (StateEditorTabItem)stateTabControl.SelectedItem;
            if (currentTab == null) return; //we dont have anything selected

            StateEditor editor = currentTab.StateEditor;

            Point offset;
            switch (e.Key)
            {
                case Key.Down:
                    offset = new Point(0, 1);
                    break;
                case Key.Up:
                    offset = new Point(0, -1);
                    break;
                case Key.Right:
                    offset = new Point(1,0);
                    break;
                case Key.Left:
                    offset = new Point(-1,0);
                    break;
                default:
                    return;
            }

            editor.SetImage(editor.DirIndex + offset.X, editor.FrameIndex + offset.Y);
        }*/
        
        public StateEditor SelectedStateEditor => ((StateEditorTabItem) stateTabControl.SelectedItem).StateEditor;

        //called when the state changed, does NOT update the image, just the ui!
        private void UpdateStateUi(object sender = null, EventArgs e = null)
        {
            //getting currently selected Tab
            StateEditorTabItem currentTab = (StateEditorTabItem)stateTabControl.SelectedItem;

            //building a list of all currently opened states
            List<int> openedStates = (from StateEditorTabItem item in stateTabControl.Items select item.StateEditor.StateIndex).ToList();

            //setting proper layout for every button
            foreach (StateButton button in _stateButtons)
            {
                if (openedStates.Contains(button.StateIndex))
                {
                    if(currentTab != null && button.StateIndex == currentTab.StateEditor.StateIndex)
                    {
                        button.SetPressed(true);
                    }
                    else
                    {
                        button.SetHalfPressed();
                    }
                }
                else
                {
                    button.SetPressed(false);
                }
            }
            
            //setting the correct state value editor
            if (currentTab == null) return;
            stateValueEditorGrid.Children.Clear();
            stateValueEditorGrid.Children.Add(new StateValueEditor(currentTab.StateEditor.State));
        }

        private class StateButton : LabeledImageButton
        {
            private readonly FileEditor _fileEditor;
            public readonly int StateIndex;
            private readonly DmiEXState _state;
            public StateButton(FileEditor fileEditor, int stateIndex, DmiEXState state) : base(BitmapUtils.Bitmap2BitmapImage(state.GetImage(0,0).GetBitmap()),$"\"{state.Id}\"")
            {
                StateIndex = stateIndex;
                _state = state;
                _fileEditor = fileEditor;
                
                //register click event
                Click += Clicked;
                state.IdChanged += (sender, args) =>
                {
                    Label.Text = $"\"{state.Id}\"";
                };
                state.GetImage(0, 0).ImageChanged += ImageChanged;
            }

            protected virtual void Clicked(object sender, EventArgs e)
            {
                _fileEditor.SelectOrOpenState(StateIndex);
            }

            private void ImageChanged(object sender, EventArgs e)
            {
                SetImage(BitmapUtils.Bitmap2BitmapImage(_state.GetImage(0,0).GetBitmap()));
            }
        }

        private class StateEditorTabItem : TabItem
        {
            public readonly StateEditor StateEditor;
            public StateEditorTabItem(StateEditor stateEditor)
            {
                StateEditor = stateEditor;
                Content = stateEditor;
            }
        }
    }
}
