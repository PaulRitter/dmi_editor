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
        public readonly FileEditor FileEditor;
        private Bitmap _bitmap;
        public readonly int StateIndex;
        public int DirIndex { get; private set; }
        public int FrameIndex { get; private set; }
        public StateEditor(FileEditor fileEditor, int stateIndex)
        {
            this.FileEditor = fileEditor;
            this.StateIndex = stateIndex;
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(img, EdgeMode.Aliased);

            //binding events for drawing
            img.MouseLeftButtonDown += leftMouseDown;
            img.MouseLeftButtonUp += leftMouseUp;
            img.MouseLeave += mouseExit;
            img.MouseMove += mouseMove;

            //creating backgroundmap (tiling)
            Bitmap backgroundMap = new Bitmap(fileEditor.Dmi.Width * 2, fileEditor.Dmi.Height * 2);
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
            DirIndex = -1;
            FrameIndex = -1;
            SetImage(0, 0);
        }

        

        public event EventHandler<ImageSelectionChangedEventArgs> ImageSelectionChanged;

        private void OnImageSelectionChanged()
        {
            ImageSelectionChanged?.Invoke(this, new ImageSelectionChangedEventArgs(StateIndex, DirIndex, FrameIndex));
        }

        public void SetImage(int dir, int frame)
        {
            bool imgChanged = false;

            if (dir >= 0 && dir < (int)FileEditor.Dmi.States[StateIndex].dirs)
            {
                if (dir != DirIndex)
                {
                    imgChanged = true;
                    this.DirIndex = dir;
                    OnImageSelectionChanged();
                }
            }
            else return; //throw outofrangeexception

            if (frame >= 0 && frame < FileEditor.Dmi.States[StateIndex].frames)
            {
                if (frame != FrameIndex)
                {
                    imgChanged = true;
                    this.FrameIndex = frame;
                    OnImageSelectionChanged();
                }
            }
            else return; //throw outofrangeexception

            if (imgChanged)
                updateImageDisplay();
        }

        // =====================
        // =====================
        // Tool handling
        private bool _mouseHeld = false; //tracks wether or not left mouse button is held

        //will try to modify the specified pixel with the selected tool
        private void TryAct(int x, int y)
        {
            if (FileEditor.Main.GetTool().pixelAct(ref _bitmap, x, y))
                updateImageDisplay();
        }

        // Event handling
        private void leftMouseDown(object sender, MouseEventArgs e)
        {
            _mouseHeld = true;
            System.Windows.Point wP = e.GetPosition(img);
            TryAct(FileEditor.RealPos(wP.X), FileEditor.RealPos(wP.Y));
        }
        private void mouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseHeld)
            {
                System.Windows.Point wP = e.GetPosition(img);
                TryAct(FileEditor.RealPos(wP.X), FileEditor.RealPos(wP.Y));
            }
        }
        private void leftMouseUp(object sender, MouseEventArgs e)
        {
            _mouseHeld = false;
        }
        private void mouseExit(object sender, MouseEventArgs e)
        {
            _mouseHeld = false;
        }
        // =====================
        // =====================

        //updates the image display
        //very inefficient, needs to be reworked!!! somehow permanently uses up more and more ram
        public void updateImageDisplay()
        {
            _bitmap = FileEditor.Dmi.States[StateIndex].getImage(DirIndex, FrameIndex);
            Bitmap scaledMap = new Bitmap(_bitmap.Width * 2, _bitmap.Height * 2);
            for (int x = 0; x < scaledMap.Width; x++)
            {
                for (int y = 0; y < scaledMap.Height; y++)
                {
                    int realX = FileEditor.RealPos(x);
                    int realY = FileEditor.RealPos(y);
                    scaledMap.SetPixel(x, y, _bitmap.GetPixel(realX, realY));
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
