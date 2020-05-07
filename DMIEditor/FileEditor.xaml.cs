using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
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
        public readonly Dmi Dmi;
        public readonly MainWindow Main;

        private List<StateButton> _stateButtons = new List<StateButton>();
        private List<ImageSelectionButton> _frameButtons = new List<ImageSelectionButton>();

        public FileEditor(Dmi dmi, MainWindow main)
        {
            this.Dmi = dmi;
            this.Main = main;
            InitializeComponent();

            //adding state buttons
            for (int i = 0; i < dmi.States.Count; i++)
            {
                Bitmap bm = dmi.States[i].getImage(0, 0);
                StateButton btn = new StateButton(this, i, bm, $"\"{dmi.States[i].Id}\"" + (bm == null ? "Bitmap was null!!!" : ""));
                statePanel.Children.Add(btn);
                _stateButtons.Add(btn);
            }

            //image selection hotkeys
            statePanel.KeyDown += ImageSelectionKeyHandler;
            KeyDown += ImageSelectionKeyHandler;
            dirPanel.KeyDown += ImageSelectionKeyHandler;
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

            DMIState state = Dmi.States[stateIndex];
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
            stateEditor.KeyDown += ImageSelectionKeyHandler;
            stateEditor.ImageSelectionChanged += UpdateFrameButtons;
            tItem.KeyDown += ImageSelectionKeyHandler;
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
        private void ImageSelectionKeyHandler(object sender, KeyEventArgs e)
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
        }

        private void UpdateStateUi(object sender, EventArgs e) => UpdateStateUi();

        //called when the state changed, does NOT update the image, just the ui!
        private void UpdateStateUi()
        {
            //getting currently selected Tab
            StateEditorTabItem currentTab = (StateEditorTabItem)stateTabControl.SelectedItem;

            //building a list of all currently opened states
            List<int> openedStates = new List<int>();
            foreach (StateEditorTabItem item in stateTabControl.Items)
            {
                openedStates.Add(item.StateEditor.StateIndex);
            }

            //setting proper layout for every button
            foreach (StateButton button in _stateButtons)
            {
                if (openedStates.Contains(button.stateIndex))
                {
                    if(currentTab != null && button.stateIndex == currentTab.StateEditor.StateIndex)
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

            //create dir and frame buttons
            dirPanel.Children.Clear();

            if (currentTab == null) return; //nothing else to do from here on out if we dont have a selected state

            for (int d = 0; d < (int)currentTab.StateEditor.State.Dirs; d++)
            {
                Border b = new Border
                {
                    BorderThickness = new System.Windows.Thickness(0.5d),
                    BorderBrush = System.Windows.Media.Brushes.Black
                };
                StackPanel framePanel = new StackPanel();
                TextBlock title = new TextBlock
                {
                    Text = $"Dir {d + 1}", HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                };
                framePanel.Children.Add(title);
                for (int f = 0; f < currentTab.StateEditor.State.Frames; f++)
                {
                    ImageSelectionButton frameButton = new ImageSelectionButton(this, d, f, currentTab.StateEditor.State.getImage(d, f), $"Frame {f+1}");
                    framePanel.Children.Add(frameButton);
                    _frameButtons.Add(frameButton);
                }
                b.Child = framePanel;
                dirPanel.Children.Add(b);
            }
            //sets the proper layout for the buttons
            UpdateFrameButtons(currentTab.StateEditor.DirIndex,currentTab.StateEditor.FrameIndex);
        }


        private void UpdateFrameButtons(object sender, ImageSelectionChangedEventArgs e)
        {
            UpdateFrameButtons(e.Dir, e.Frame);
        }

        private void UpdateFrameButtons(int dir, int frame)
        {
            foreach (ImageSelectionButton btn in _frameButtons.Where<ImageSelectionButton>(btn => btn.isPressed()))
            {
                btn.SetPressed(false);
            }

            foreach (ImageSelectionButton btn in _frameButtons.Where<ImageSelectionButton>(btn => btn.DirIndex == dir && btn.FrameIndex == frame))
            {
                btn.SetPressed(true);
            }
        }

        // helper to calculate from screen pixel pos -> bitmap pixel pos
        public static int RealPos(double p)
        {
            return (int)Math.Floor(p / 2d);
        }

        private class StateButton : LabeledImageButton
        {
            protected readonly FileEditor FileEditor;
            //stateindex
            public readonly int stateIndex;
            private bool _pressed = false;
            public StateButton(FileEditor fileEditor, int stateIndex, Bitmap bm, string labeltext) : base(bm,labeltext)
            {
                this.stateIndex = stateIndex;
                this.FileEditor = fileEditor;
                
                //register click event
                Click += Clicked;
            }

            protected virtual void Clicked(object sender, EventArgs e)
            {
                FileEditor.SelectOrOpenState(stateIndex);
            }
        }

        private class ImageSelectionButton : LabeledImageButton
        {
            public readonly FileEditor FileEditor;
            public readonly int DirIndex;
            public readonly int FrameIndex;
            public ImageSelectionButton(FileEditor fileEditor, int dirIndex, int frameIndex, Bitmap bm, string labeltext) : base (bm, labeltext)
            {
                this.DirIndex = dirIndex;
                this.FrameIndex = frameIndex;
                this.FileEditor = fileEditor;
                Click += Clicked;
            }
            protected void Clicked(object sender, EventArgs e)
            {
                StateEditorTabItem item = (StateEditorTabItem)FileEditor.stateTabControl.SelectedItem;
                item?.StateEditor.SetImage(DirIndex, FrameIndex);
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
