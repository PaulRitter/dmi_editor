using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
using DMI_Parser;
using Color = System.Drawing.Color;

namespace DMIEditor
{
    /// <summary>
    /// Interaktionslogik für StateEditor.xaml
    /// </summary>
    public partial class StateEditor : UserControl
    {
        public readonly FileEditor FileEditor;
        private List<Layer> _layers;
        private int _layerIndex = 0;
        private List<LayerButton> _layerButtons = new List<LayerButton>();
        public readonly int StateIndex;
        public readonly DMIState State;
        public int DirIndex { get; private set; }
        public int FrameIndex { get; private set; }

        private Bitmap SelectedBitmap
        {
            get
            {
                return _layers.Find(l => l.Index == _layerIndex)?.Bitmap;
            }
        }

        public StateEditor(FileEditor fileEditor, int stateIndex, DMIState state)
        {
            this.FileEditor = fileEditor;
            this.StateIndex = stateIndex;
            State = state;
            _layers = new List<Layer>();
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(img, EdgeMode.Aliased);

            //binding events for drawing
            img.MouseLeftButtonDown += leftMouseDown;
            img.MouseLeftButtonUp += leftMouseUp;
            img.MouseLeave += mouseExit;
            img.MouseMove += mouseMove;

            //creating backgroundmap (tiling)
            Bitmap backgroundMap = new Bitmap(State.Width * 2, State.Height * 2);
            bool s = true;
            for (int i = 0; i < backgroundMap.Width; i++)
            {
                for (int j = 0; j < backgroundMap.Height; j++)
                {
                    Color c = s ? Color.Gray : Color.White;
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
            SetImage(0,0);
        }

        public event EventHandler<ImageSelectionChangedEventArgs> ImageSelectionChanged;

        private void OnImageSelectionChanged()
        {
            ImageSelectionChanged?.Invoke(this, new ImageSelectionChangedEventArgs(StateIndex, DirIndex, FrameIndex));
        }

        public void SetImage(int dir, int frame)
        {
            bool imgChanged = false;

            if (dir >= 0 && dir < (int)State.Dirs)
            {
                if (dir != DirIndex)
                {
                    imgChanged = true;
                    this.DirIndex = dir;
                    OnImageSelectionChanged();
                }
            }
            else return; //throw outofrangeexception

            if (frame >= 0 && frame < State.Frames)
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
                UpdateImageDisplay();
        }

        // =====================
        // =====================
        // Tool handling
        private bool _mouseHeld = false; //tracks wether or not left mouse button is held

        //will try to modify the specified pixel with the selected tool
        private void TryAct(int x, int y)
        {
            if (SelectedBitmap == null) return;
            
            if (FileEditor.Main.GetTool().pixelAct(SelectedBitmap, x, y))
                ReRenderImage();
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

        private void AddLayer(object sender, EventArgs e)
        {
            try
            {
                AddLayer(_layerIndex+1, new Bitmap(State.Width, State.Height));
            }
            catch (ArgumentException ex)
            {
                ErrorPopupHelper.Create(ex);
            }
        }
        
        public void AddLayer(int index, Bitmap bitmap)
        {
            if (index == 0) return; //index 0 is reserved for nothing selected
            
            foreach (Layer layer in _layers)
            {
                if(layer.Index == index) throw new ArgumentException("A Layer with that index already exists.");
            }
            Layer newLayer = new Layer(bitmap, index);
            newLayer.Changed += OnLayerChanged;
            _layers.Add(newLayer);
            _layers.Sort((l1, l2) => l1.CompareTo(l2)); //layers are always sorted from highest index to lowest
            UpdateLayerUi();
            SelectLayer(index);
        }

        public void OnLayerChanged(object sender, EventArgs e)
        {
            var layer = sender as Layer;
            if (layer != null)
            {
                if (!layer.Visible && layer.Index == _layerIndex)
                {
                    int prevIndex = _layerIndex;
                    foreach (var otherLayer in _layers)
                    {
                        if(otherLayer == layer) continue;
                        if (SelectLayer(otherLayer.Index)) break;
                    }
                    if (prevIndex == _layerIndex)
                    {
                        SelectLayer(0); //select nothing
                    }
                }
            }

            
            UpdateLayerUi();
            ReRenderImage();
        }

        public bool SelectLayer(int index)
        {
            if (index != 0){
                if(GetLayer(index) == null) return false;
                
                if(!GetLayer(index).Visible) return false;
            }
            
            _layerIndex = index;
            foreach (var btn in _layerButtons)
            {
                if (btn.LayerIndex == index)
                {
                    btn.SetPressed(true);
                }
                else
                {
                    btn.SetPressed(false);
                }
            }

            return true;
        }

        public Layer GetLayer(int index)
        {
            return _layers.Find(l => l.Index == index);
        }
        
        public void UpdateLayerUi()
        {
            LayerStackPanel.Children.Clear();
            _layerButtons.Clear();
            foreach (var layer in _layers)
            {
                var btn = new LayerButton(layer.Bitmap, layer.Index, this);
                
                LayerStackPanel.Children.Add(btn);
                _layerButtons.Add(btn);
            }

            Button addBtn = new Button
            {
                Content = new TextBlock
                {
                    Text = "+"
                }
            };
            addBtn.Click += AddLayer;
            LayerStackPanel.Children.Add(addBtn);
        }

        //updates the image display

        //very inefficient, needs to be reworked!!! somehow permanently uses up more and more ram

        public void UpdateImageDisplay()
        {
            _layers = new List<Layer>();
            UpdateLayerUi();
            
            AddLayer(1, State.getImage(DirIndex, FrameIndex));

            ReRenderImage();
        }
        
        public void ReRenderImage()
        {
            Bitmap actual = new Bitmap(State.Width * 2, State.Height * 2);
            for (int x = 0; x < State.Width; x++)
            {
                for (int y = 0; y < State.Height; y++)
                {
                    foreach (Layer layer in _layers)
                    {
                        if (!layer.Visible) continue;
                        
                        int actualX = x * 2;
                        int actualY = y * 2;
                        Color c = layer.Bitmap.GetPixel(x, y);
                        //TODO adding colors
                        if (c.ToArgb() != 0) // if this pixel has a value
                        {
                            actual.SetPixel(actualX, actualY, c);
                            actual.SetPixel(actualX+1,actualY,c);
                            actual.SetPixel(actualX, actualY+1, c);
                            actual.SetPixel(actualX+1,actualY+1,c);
                            break;
                        }
                    }
                }
            }
            
            img.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                actual.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(actual.Width, actual.Height));

            UpdateLayerUi();
        }

        private class LayerButton : LabeledImageButton
        {
            public readonly int LayerIndex;
            private readonly StateEditor _stateEditor;
            private TextBlock visibleText = new TextBlock();
            public LayerButton(Bitmap bm, int layerIndex, StateEditor stateEditor) : base(bm, $"Layer {layerIndex}")
            {
                this.LayerIndex = layerIndex;
                this._stateEditor = stateEditor;

                StackPanel sp = (StackPanel) Content;

                if (_stateEditor.GetLayer(LayerIndex).Visible)
                    visibleText.Text = "Hide";
                else
                    visibleText.Text = "Show";

                Button visibleBtn = new Button
                {
                    Content = visibleText
                };
                sp.Children.Add(visibleBtn);
                visibleBtn.Click += ToggleVisibility;

                Click += Clicked;
            }

            private void Clicked(object sender, EventArgs e)
            {
                _stateEditor.SelectLayer(LayerIndex);
            }

            private void ToggleVisibility(object sender, EventArgs e)
            {
                _stateEditor.GetLayer(LayerIndex).Visible = !_stateEditor.GetLayer(LayerIndex).Visible; //this change will automatically trigger the redo of the entire layer ui
            }
        }
    }
}
