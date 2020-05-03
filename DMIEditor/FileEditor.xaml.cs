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
        private MainWindow main;
        public int currentState { get; private set; }
        public int currentDirection { get; private set; }
        public int currentFrame { get; private set; }
        private Bitmap currentBitmap;

        private List<SingleIndexButton> stateButtons = new List<SingleIndexButton>();
        private List<DoubleIndexButton> frameButtons = new List<DoubleIndexButton>();

        public FileEditor(DMI dmi, MainWindow main)
        {
            //to force updates to the ui we set this to values which 100% will be changed by calling the default 0,0,0
            currentState = -1;
            currentDirection = -1;
            currentFrame = -1;


            this.dmi = dmi;
            this.main = main;
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(img, EdgeMode.Aliased);

            //binding events for drawing
            img.MouseLeftButtonDown += leftMouseDown;
            img.MouseLeftButtonUp += leftMouseUp;
            img.MouseLeave += mouseExit;
            img.MouseMove += mouseMove;

            //creating backgroundmap (tiling)
            Bitmap backgroundMap = new Bitmap(dmi.width * 2, dmi.height * 2);
            bool s = true;
            for (int i = 0; i < backgroundMap.Width; i++)
            {
                for (int j = 0; j < backgroundMap.Height; j++)
                {
                    System.Drawing.Color c = s ? System.Drawing.Color.Gray : System.Drawing.Color.White;
                    s = !s;
                    backgroundMap.SetPixel(i, j, c);
                }
                s = !s; //offsetting every row
            }
            backgroundImg.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
               backgroundMap.GetHbitmap(),
               IntPtr.Zero,
               System.Windows.Int32Rect.Empty,
               BitmapSizeOptions.FromWidthAndHeight(backgroundMap.Width, backgroundMap.Height));

            //adding state buttons
            for (int i = 0; i < dmi.states.Count; i++)
            {
                Bitmap bm = dmi.states[i].getImage(0, 0);
                SingleIndexButton btn = new SingleIndexButton(this, i, bm, $"\"{dmi.states[i].id}\"" + (bm == null ? "Bitmap was null!!!" : ""));
                statePanel.Children.Add(btn);
                stateButtons.Add(btn);
            }

            //major jank but oh well
            statePanel.KeyDown += imageSelectionKeyHandler;
            KeyDown += imageSelectionKeyHandler;
            dirPanel.KeyDown += imageSelectionKeyHandler;

            //finally setting default state
            //TODO make states a tabview and have nothing selected by default
            setImageSelection(0,0,0);
        }

        // handles arrow keys for image selection
        private void imageSelectionKeyHandler(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    setImageSelection(currentState, currentDirection, currentFrame + 1);
                    break;
                case Key.Up:
                    setImageSelection(currentState, currentDirection, currentFrame - 1);
                    break;
                case Key.Right:
                    setImageSelection(currentState, currentDirection + 1, currentFrame);
                    break;
                case Key.Left:
                    setImageSelection(currentState, currentDirection - 1, currentFrame);
                    break;
                default:
                    break;
            }
        }

        //takes new image parameter and checks what parameters changed, as well as invoke the following methods if needed:
        //stateChanged, dirChanged, frameChanged, imageChanged
        public void setImageSelection(int newState, int newDirection, int newFrame)
        {
            bool imgChanged = false;

            if (newState >= 0 && newState < dmi.states.Count)
            {
                if(newState != currentState)
                {
                    imgChanged = true;
                    currentState = newState;
                    updateStateUI();
                }
            }
            else return; //throw outofrangeexception

            if (newDirection >= 0 && newDirection < dmi.states[newState].dirs)
            {
                if(newDirection != currentDirection)
                {
                    imgChanged = true;
                    this.currentDirection = newDirection;
                    updateDirUI();
                }
            }
            else return; //throw outofrangeexception

            if (newFrame >= 0 && newFrame < dmi.states[newState].frames)
            {
                if(newFrame != currentFrame)
                {
                    imgChanged = true;
                    this.currentFrame = newFrame;
                    updateFrameUI();
                }
            }
            else return; //throw outofrangeexception

            if(imgChanged)
                imageChanged();
        }

        //called when a new image should be displayed
        private void imageChanged()
        {
            currentBitmap = dmi.states[currentState].getImage(currentDirection, currentFrame);
            updateImageDisplay();
        }

        //called when the state changed, does NOT update the image, just the ui!
        private void updateStateUI()
        {
            foreach (SingleIndexButton button in stateButtons.Where<SingleIndexButton>(btn => btn.isPressed()))
            {
                button.setPressed(false);
            }

            foreach (SingleIndexButton button in stateButtons.Where<SingleIndexButton>(btn => (btn.index == currentState)))
            {
                button.setPressed(true);
            }

            //create dir and frame buttons
            dirPanel.Children.Clear();
            for (int d = 0; d < dmi.states[currentState].dirs; d++)
            {
                Border b = new Border();
                b.BorderThickness = new System.Windows.Thickness(0.5d);
                b.BorderBrush = System.Windows.Media.Brushes.Black;
                StackPanel framePanel = new StackPanel();
                TextBlock title = new TextBlock();
                title.Text = $"Dir {d + 1}";
                title.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                framePanel.Children.Add(title);
                for (int f = 0; f < dmi.states[currentState].frames; f++)
                {
                    DoubleIndexButton frameButton = new DoubleIndexButton(this, d, dmi.states[currentState].getImage(d, f), $"Frame {f+1}", f);
                    framePanel.Children.Add(frameButton);
                    frameButtons.Add(frameButton);
                }
                b.Child = framePanel;
                dirPanel.Children.Add(b);
            }
            //sets the proper layout for the buttons
            updateFrameButtons();
        }

        //called when the dir changed, does NOT update the image, just the ui!
        private void updateDirUI()
        {
            updateFrameButtons();
        }

        //called when the frame changed, does NOT update the image, just the ui!
        private void updateFrameUI()
        {
            updateFrameButtons();
        }

        private void updateFrameButtons()
        {
            foreach (DoubleIndexButton btn in frameButtons.Where<DoubleIndexButton>(btn => btn.isPressed()))
            {
                btn.setPressed(false);
            }

            foreach (DoubleIndexButton btn in frameButtons.Where<DoubleIndexButton>(btn => btn.index == currentDirection && btn.secondIndex == currentFrame))
            {
                btn.setPressed(true);
            }
        }

        // =====================
        // =====================
        // Tool handling
        private bool mouseHeld = false; //tracks wether or not left mouse button is held

        //will try to modify the specified pixel with the selected tool
        private void tryAct(int x, int y)
        {
            if (main.getTool().pixelAct(ref currentBitmap, x, y))
                updateImageDisplay();
        }

        // Event handling
        private void leftMouseDown(object sender, MouseEventArgs e)
        {
            mouseHeld = true;
            System.Windows.Point wP = e.GetPosition(img);
            tryAct(realPos(wP.X), realPos(wP.Y));
        }
        private void mouseMove(object sender, MouseEventArgs e)
        {
            if (mouseHeld)
            {
                System.Windows.Point wP = e.GetPosition(img);
                tryAct(realPos(wP.X), realPos(wP.Y));
            }
        }
        private void leftMouseUp(object sender, MouseEventArgs e)
        {
            mouseHeld = false;
        }
        private void mouseExit(object sender, MouseEventArgs e)
        {
            mouseHeld = false;
        }
        // =====================
        // =====================

        // helper to calculate from screen pixel pos -> bitmap pixel pos
        private int realPos(double p)
        {
            return (int)Math.Floor(p / 2d);
        }

        //updates the image display
        //very inefficient, needs to be reworked!!! somehow permanently uses up more and more ram
        public void updateImageDisplay()
        {
            Bitmap scaledMap = new Bitmap(currentBitmap.Width * 2, currentBitmap.Height * 2);
            for (int x = 0; x < scaledMap.Width; x++)
            {
                for (int y = 0; y < scaledMap.Height; y++)
                {
                    int real_x = realPos(x);
                    int real_y = realPos(y);
                    scaledMap.SetPixel(x, y, currentBitmap.GetPixel(real_x, real_y));
                }
            }

            img.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
               scaledMap.GetHbitmap(),
               IntPtr.Zero,
               System.Windows.Int32Rect.Empty,
               BitmapSizeOptions.FromWidthAndHeight(scaledMap.Width, scaledMap.Height));
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
                fileEditor.setImageSelection(index, 0, 0);
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
                fileEditor.setImageSelection(fileEditor.currentState, index, secondIndex);
            }
        }
    }
}
