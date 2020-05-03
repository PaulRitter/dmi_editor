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
        public readonly DMI dmi;
        public readonly MainWindow main;

        private List<SingleIndexButton> stateButtons = new List<SingleIndexButton>();
        private List<DoubleIndexButton> frameButtons = new List<DoubleIndexButton>();

        public FileEditor(DMI dmi, MainWindow main)
        {
            this.dmi = dmi;
            this.main = main;
            InitializeComponent();

            //adding state buttons
            for (int i = 0; i < dmi.states.Count; i++)
            {
                Bitmap bm = dmi.states[i].getImage(0, 0);
                SingleIndexButton btn = new SingleIndexButton(this, i, bm, $"\"{dmi.states[i].id}\"" + (bm == null ? "Bitmap was null!!!" : ""));
                statePanel.Children.Add(btn);
                stateButtons.Add(btn);
            }

            //image selection hotkeys
            statePanel.KeyDown += imageSelectionKeyHandler;
            KeyDown += imageSelectionKeyHandler;
            dirPanel.KeyDown += imageSelectionKeyHandler;
            stateTabControl.SelectionChanged += updateStateUI;
        }

        private void selectOrOpenState(int state)
        {
            foreach (StateEditorTabItem item in stateTabControl.Items)
            {
                if(item.stateEditor.stateIndex == state)
                {
                    item.IsSelected = true;
                    updateStateUI();
                    return;
                }
            }

            StateEditor stateEditor = new StateEditor(this, state);
            StateEditorTabItem tItem = new StateEditorTabItem(stateEditor);
            stateTabControl.Items.Add(tItem);

            StackPanel sp = new StackPanel();
            TextBlock txt = new TextBlock();
            txt.Text = $"\"{dmi.states[state].id}\"";
            txt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            sp.Orientation = Orientation.Horizontal;
            sp.Children.Add(txt);

            TabCloseButton cBtn = new TabCloseButton(tItem);
            cBtn.Click += closeState;
            sp.Children.Add(cBtn);

            tItem.Header = sp;
            tItem.IsSelected = true;
            stateEditor.KeyDown += imageSelectionKeyHandler;
            stateEditor.ImageSelectionChanged += updateFrameButtons;
            tItem.KeyDown += imageSelectionKeyHandler;
            updateStateUI();
        }

        private void closeState(object sender, EventArgs e)
        {
            TabCloseButton btn = (TabCloseButton)sender;
            StateEditorTabItem tItem = (StateEditorTabItem)btn.tabItem;
            closeState(tItem);
            
        }

        private void closeState(StateEditorTabItem stateTab)
        {
            stateTabControl.Items.Remove(stateTab);
            updateStateUI();
        }

        // handles arrow keys for image selection
        private void imageSelectionKeyHandler(object sender, KeyEventArgs e)
        {
            //getting currently selected Tab
            StateEditorTabItem currentTab = (StateEditorTabItem)stateTabControl.SelectedItem;
            if (currentTab == null) return; //we dont have anything selected

            StateEditor editor = currentTab.stateEditor;

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

            editor.setImage(editor.dirIndex + offset.X, editor.frameIndex + offset.Y);
        }

        private void updateStateUI(object sender, EventArgs e) => updateStateUI();

        //called when the state changed, does NOT update the image, just the ui!
        private void updateStateUI()
        {
            //getting currently selected Tab
            StateEditorTabItem currentTab = (StateEditorTabItem)stateTabControl.SelectedItem;
            if (currentTab == null) return; //we dont have anything selected

            //building a list of all currently opened states
            List<int> openedStates = new List<int>();
            foreach (StateEditorTabItem item in stateTabControl.Items)
            {
                openedStates.Add(item.stateEditor.stateIndex);
            }

            //setting proper layout for every button
            foreach (SingleIndexButton button in stateButtons)
            {
                if (openedStates.Contains(button.index))
                {
                    if(button.index == currentTab.stateEditor.stateIndex)
                    {
                        button.setPressed(true);
                    }
                    else
                    {
                        button.setHalfPressed();
                    }
                }
                else
                {
                    button.setPressed(false);
                }
            }

            //create dir and frame buttons
            dirPanel.Children.Clear();
            for (int d = 0; d < dmi.states[currentTab.stateEditor.stateIndex].dirs; d++)
            {
                Border b = new Border();
                b.BorderThickness = new System.Windows.Thickness(0.5d);
                b.BorderBrush = System.Windows.Media.Brushes.Black;
                StackPanel framePanel = new StackPanel();
                TextBlock title = new TextBlock();
                title.Text = $"Dir {d + 1}";
                title.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                framePanel.Children.Add(title);
                for (int f = 0; f < dmi.states[currentTab.stateEditor.stateIndex].frames; f++)
                {
                    DoubleIndexButton frameButton = new DoubleIndexButton(this, d, dmi.states[currentTab.stateEditor.stateIndex].getImage(d, f), $"Frame {f+1}", f);
                    framePanel.Children.Add(frameButton);
                    frameButtons.Add(frameButton);
                }
                b.Child = framePanel;
                dirPanel.Children.Add(b);
            }
            //sets the proper layout for the buttons
            updateFrameButtons(currentTab.stateEditor.dirIndex,currentTab.stateEditor.frameIndex);
        }


        private void updateFrameButtons(object sender, ImageSelectionChangedEventArgs e)
        {
            updateFrameButtons(e.dir, e.frame);
        }

        private void updateFrameButtons(int dir, int frame)
        {
            foreach (DoubleIndexButton btn in frameButtons.Where<DoubleIndexButton>(btn => btn.isPressed()))
            {
                btn.setPressed(false);
            }

            foreach (DoubleIndexButton btn in frameButtons.Where<DoubleIndexButton>(btn => btn.index == dir && btn.secondIndex == frame))
            {
                btn.setPressed(true);
            }
        }

        // helper to calculate from screen pixel pos -> bitmap pixel pos
        public static int realPos(double p)
        {
            return (int)Math.Floor(p / 2d);
        }

        private class SingleIndexButton : Button
        {
            protected FileEditor fileEditor;
            //stateindex
            public readonly int index;
            private bool pressed = false;
            public SingleIndexButton(FileEditor fileEditor, int index, Bitmap bm, string labeltext)
            {
                this.index = index;
                this.fileEditor = fileEditor;
                
                //create stackpanel
                StackPanel stackPanel = new StackPanel();

                //create image
                System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                if(bm != null) //until i fix the lineskip bug
                    img.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        bm.GetHbitmap(),
                        IntPtr.Zero,
                        System.Windows.Int32Rect.Empty,
                        BitmapSizeOptions.FromWidthAndHeight(bm.Width, bm.Height));
                img.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                img.Stretch = Stretch.None;
                //add to stackpanel
                stackPanel.Children.Add(img);

                //create label
                TextBlock label = new TextBlock();
                label.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                label.Text = labeltext;
                //add to stackpanel
                stackPanel.Children.Add(label);

                //add stackpanel to btn
                Content = stackPanel;

                //layout stuff
                Margin = new System.Windows.Thickness(3);
                setPressed(false);

                //register click event
                Click += clicked;
            }

            protected virtual void clicked(object sender, EventArgs e)
            {
                fileEditor.selectOrOpenState(index);
            }

            public void setPressed(bool pressed)
            {
                this.pressed = pressed;
                if (pressed)
                {
                    Background = System.Windows.Media.Brushes.LightBlue;
                }
                else
                {
                    Background = System.Windows.Media.Brushes.LightGray;
                }
            }

            public void setHalfPressed()
            {
                this.pressed = false;
                Background = System.Windows.Media.Brushes.LightGreen;
            }

            public bool isPressed() { return pressed; }
        }

        private class DoubleIndexButton : SingleIndexButton
        {
            //index is used for dirindex
            //secondindex is used for frameindex
            public readonly int secondIndex;
            public DoubleIndexButton(FileEditor fileEditor, int index, Bitmap bm, string labeltext, int secondIndex) : base (fileEditor, index, bm, labeltext)
            {
                this.secondIndex = secondIndex;
            }
            protected override void clicked(object sender, EventArgs e)
            {
                StateEditorTabItem item = (StateEditorTabItem)fileEditor.stateTabControl.SelectedItem;
                if (item == null) return; //uh oh
                item.stateEditor.setImage(index, secondIndex);
            }
        }

        private class StateEditorTabItem : TabItem
        {
            public readonly StateEditor stateEditor;
            public StateEditorTabItem(StateEditor stateEditor)
            {
                this.stateEditor = stateEditor;
                Content = stateEditor;
            }
        }
    }
}
