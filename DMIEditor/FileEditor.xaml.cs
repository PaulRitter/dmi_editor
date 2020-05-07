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

        private List<SingleIndexButton> _stateButtons = new List<SingleIndexButton>();
        private List<DoubleIndexButton> _frameButtons = new List<DoubleIndexButton>();

        public FileEditor(Dmi dmi, MainWindow main)
        {
            this.Dmi = dmi;
            this.Main = main;
            InitializeComponent();

            //adding state buttons
            for (int i = 0; i < dmi.States.Count; i++)
            {
                Bitmap bm = dmi.States[i].getImage(0, 0);
                SingleIndexButton btn = new SingleIndexButton(this, i, bm, $"\"{dmi.States[i].id}\"" + (bm == null ? "Bitmap was null!!!" : ""));
                statePanel.Children.Add(btn);
                _stateButtons.Add(btn);
            }

            //image selection hotkeys
            statePanel.KeyDown += ImageSelectionKeyHandler;
            KeyDown += ImageSelectionKeyHandler;
            dirPanel.KeyDown += ImageSelectionKeyHandler;
            stateTabControl.SelectionChanged += UpdateStateUi;
        }

        private void SelectOrOpenState(int state)
        {
            foreach (StateEditorTabItem item in stateTabControl.Items)
            {
                if(item.StateEditor.StateIndex == state)
                {
                    item.IsSelected = true;
                    UpdateStateUi();
                    return;
                }
            }

            StateEditor stateEditor = new StateEditor(this, state);
            StateEditorTabItem tItem = new StateEditorTabItem(stateEditor);
            stateTabControl.Items.Add(tItem);

            StackPanel sp = new StackPanel();
            TextBlock txt = new TextBlock();
            txt.Text = $"\"{Dmi.States[state].id}\"";
            txt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
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
            foreach (SingleIndexButton button in _stateButtons)
            {
                if (openedStates.Contains(button.Index))
                {
                    if(currentTab != null && button.Index == currentTab.StateEditor.StateIndex)
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

            if (currentTab == null) return; //nothign else to do from here on out if we dont have a selected state

            for (int d = 0; d < (int)Dmi.States[currentTab.StateEditor.StateIndex].dirs; d++)
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
                for (int f = 0; f < Dmi.States[currentTab.StateEditor.StateIndex].frames; f++)
                {
                    DoubleIndexButton frameButton = new DoubleIndexButton(this, d, Dmi.States[currentTab.StateEditor.StateIndex].getImage(d, f), $"Frame {f+1}", f);
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
            foreach (DoubleIndexButton btn in _frameButtons.Where<DoubleIndexButton>(btn => btn.isPressed()))
            {
                btn.SetPressed(false);
            }

            foreach (DoubleIndexButton btn in _frameButtons.Where<DoubleIndexButton>(btn => btn.Index == dir && btn.SecondIndex == frame))
            {
                btn.SetPressed(true);
            }
        }

        // helper to calculate from screen pixel pos -> bitmap pixel pos
        public static int RealPos(double p)
        {
            return (int)Math.Floor(p / 2d);
        }

        private class SingleIndexButton : Button
        {
            protected readonly FileEditor FileEditor;
            //stateindex
            public readonly int Index;
            private bool _pressed = false;
            public SingleIndexButton(FileEditor fileEditor, int index, Bitmap bm, string labeltext)
            {
                this.Index = index;
                this.FileEditor = fileEditor;
                
                //create stackpanel
                StackPanel stackPanel = new StackPanel();

                //create image
                System.Windows.Controls.Image img = new System.Windows.Controls.Image
                {
                    Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        bm.GetHbitmap(),
                        IntPtr.Zero,
                        System.Windows.Int32Rect.Empty,
                        BitmapSizeOptions.FromWidthAndHeight(bm.Width, bm.Height)),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    Stretch = Stretch.None
                };
                //add to stackpanel
                stackPanel.Children.Add(img);

                //create label
                TextBlock label = new TextBlock
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center, Text = labeltext
                };
                //add to stackpanel
                stackPanel.Children.Add(label);

                //add stackpanel to btn
                Content = stackPanel;

                //layout stuff
                Margin = new System.Windows.Thickness(3);
                SetPressed(false);

                //register click event
                Click += Clicked;
            }

            protected virtual void Clicked(object sender, EventArgs e)
            {
                FileEditor.SelectOrOpenState(Index);
            }

            public void SetPressed(bool pressed)
            {
                this._pressed = pressed;
                Background = pressed ? System.Windows.Media.Brushes.LightBlue : System.Windows.Media.Brushes.LightGray;
            }

            public void SetHalfPressed()
            {
                this._pressed = false;
                Background = System.Windows.Media.Brushes.LightGreen;
            }

            public bool isPressed() { return _pressed; }
        }

        private class DoubleIndexButton : SingleIndexButton
        {
            //index is used for dirindex
            //secondindex is used for frameindex
            public readonly int SecondIndex;
            public DoubleIndexButton(FileEditor fileEditor, int index, Bitmap bm, string labeltext, int secondIndex) : base (fileEditor, index, bm, labeltext)
            {
                this.SecondIndex = secondIndex;
            }
            protected override void Clicked(object sender, EventArgs e)
            {
                StateEditorTabItem item = (StateEditorTabItem)FileEditor.stateTabControl.SelectedItem;
                item?.StateEditor.SetImage(Index, SecondIndex);
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
