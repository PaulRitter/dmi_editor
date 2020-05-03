using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DMIEditor
{
    /// <summary>
    /// Interaktionslogik für StateEditor.xaml
    /// </summary>
    public partial class StateEditor : UserControl
    {
        public readonly FileEditor fileEditor;
        private Bitmap bitmap;
        public readonly int stateIndex;
        public int dirIndex { get; private set; }
        public int frameIndex { get; private set; }
        public StateEditor(FileEditor fileEditor, int stateIndex)
        {
            this.fileEditor = fileEditor;
            this.stateIndex = stateIndex;
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(img, EdgeMode.Aliased);

            //binding events for drawing
            img.MouseLeftButtonDown += leftMouseDown;
            img.MouseLeftButtonUp += leftMouseUp;
            img.MouseLeave += mouseExit;
            img.MouseMove += mouseMove;

            //creating backgroundmap (tiling)
            Bitmap backgroundMap = new Bitmap(fileEditor.dmi.width * 2, fileEditor.dmi.height * 2);
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
            
            //setting default image
            dirIndex = -1;
            frameIndex = -1;
            setImage(0, 0);
        }

        

        public event EventHandler<ImageSelectionChangedEventArgs> ImageSelectionChanged;

        private void onImageSelectionChanged()
        {
            ImageSelectionChanged?.Invoke(this, new ImageSelectionChangedEventArgs(stateIndex, dirIndex, frameIndex));
        }

        public void setImage(int dir, int frame)
        {
            bool imgChanged = false;

            if (dir >= 0 && dir < fileEditor.dmi.states[stateIndex].dirs)
            {
                if (dir != dirIndex)
                {
                    imgChanged = true;
                    this.dirIndex = dir;
                    onImageSelectionChanged();
                }
            }
            else return; //throw outofrangeexception

            if (frame >= 0 && frame < fileEditor.dmi.states[stateIndex].frames)
            {
                if (frame != frameIndex)
                {
                    imgChanged = true;
                    this.frameIndex = frame;
                    onImageSelectionChanged();
                }
            }
            else return; //throw outofrangeexception

            if (imgChanged)
                updateImageDisplay();
        }

        // =====================
        // =====================
        // Tool handling
        private bool mouseHeld = false; //tracks wether or not left mouse button is held

        //will try to modify the specified pixel with the selected tool
        private void tryAct(int x, int y)
        {
            if (fileEditor.main.getTool().pixelAct(ref bitmap, x, y))
                updateImageDisplay();
        }

        // Event handling
        private void leftMouseDown(object sender, MouseEventArgs e)
        {
            mouseHeld = true;
            System.Windows.Point wP = e.GetPosition(img);
            tryAct(FileEditor.realPos(wP.X), FileEditor.realPos(wP.Y));
        }
        private void mouseMove(object sender, MouseEventArgs e)
        {
            if (mouseHeld)
            {
                System.Windows.Point wP = e.GetPosition(img);
                tryAct(FileEditor.realPos(wP.X), FileEditor.realPos(wP.Y));
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

        //updates the image display
        //very inefficient, needs to be reworked!!! somehow permanently uses up more and more ram
        public void updateImageDisplay()
        {
            bitmap = fileEditor.dmi.states[stateIndex].getImage(dirIndex, frameIndex);
            Bitmap scaledMap = new Bitmap(bitmap.Width * 2, bitmap.Height * 2);
            for (int x = 0; x < scaledMap.Width; x++)
            {
                for (int y = 0; y < scaledMap.Height; y++)
                {
                    int real_x = FileEditor.realPos(x);
                    int real_y = FileEditor.realPos(y);
                    scaledMap.SetPixel(x, y, bitmap.GetPixel(real_x, real_y));
                }
            }

            img.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
               scaledMap.GetHbitmap(),
               IntPtr.Zero,
               System.Windows.Int32Rect.Empty,
               BitmapSizeOptions.FromWidthAndHeight(scaledMap.Width, scaledMap.Height));
        }
    }
}
