﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DMI_Parser;

namespace DMIEditor
{
    /// <summary>
    /// Interaktionslogik für FileEditor.xaml
    /// </summary>
    public partial class FileEditor : UserControl
    {
        public readonly DmiEX DmiEx;
        public readonly MainWindow Main;

        private List<StateButton> _stateButtons = new List<StateButton>();

        public FileEditor(DmiEX dmiEx, MainWindow main)
        {
            this.DmiEx = dmiEx;
            this.Main = main;
            InitializeComponent();

            //adding state buttons
            for (int i = 0; i < dmiEx.States.Count; i++)
            {
                StateButton btn = new StateButton(this, i, (DmiEXState)dmiEx.States[i]);
                statePanel.Children.Add(btn);
                _stateButtons.Add(btn);
            }

            //image selection hotkeys
            stateTabControl.SelectionChanged += UpdateStateUi;
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
                Text = $"\"{state.Id}\"", VerticalAlignment = System.Windows.VerticalAlignment.Center
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
        }

        private class StateButton : LabeledImageButton
        {
            private readonly FileEditor _fileEditor;
            public readonly int StateIndex;
            private readonly DmiEXState _state;
            public StateButton(FileEditor fileEditor, int stateIndex, DmiEXState state) : base(state.getImage(0,0),$"\"{state.Id}\"")
            {
                StateIndex = stateIndex;
                _state = state;
                _fileEditor = fileEditor;
                
                //register click event
                Click += Clicked;
                state.idChanged += (sender, args) =>
                {
                    label.Text = $"\"{state.Id}\"";
                };
                state.Images[0, 0].ImageChanged += ImageChanged;
            }

            protected virtual void Clicked(object sender, EventArgs e)
            {
                _fileEditor.SelectOrOpenState(StateIndex);
            }

            private void ImageChanged(object sender, EventArgs e)
            {
                setImage(_state.getImage(0,0));
            }
        }

        private class StateEditorTabItem : TabItem
        {
            public readonly StateEditor StateEditor;
            public StateEditorTabItem(StateEditor stateEditor)
            {
                this.StateEditor = stateEditor;
                Content = stateEditor;
            }
        }
    }
}
