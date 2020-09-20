using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DMI_Parser;
using DMI_Parser.Extended;
using DMI_Parser.Utils;
using DMIEditor.Undo;
using Xceed.Wpf.Toolkit;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
using Point = System.Drawing.Point;
using Size = System.Windows.Size;


namespace DMIEditor
{
    public partial class ImageEditor : UserControl
    {
        public readonly StateEditor StateEditor;
        public readonly int DirIndex;
        public readonly int FrameIndex;
        public readonly DmiEXImage Image;
        
        private List<LayerButton> _layerButtons = new List<LayerButton>();
        
        private List<Point> _selectedPixels = new List<Point>();
        private event EventHandler SelectionChanged;
        
        private int _layerIndex;
        public int LayerIndex
        {
            get => _layerIndex;
            private set
            {
                if (value == _layerIndex) return;
                
                //check if image with that index exists
                Image.GetLayerByIndex(value);
                
                _layerIndex = value;
                OnLayerIndexChanged();
            }
        }

        private void OnLayerIndexChanged()
        {
            LayerIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        public DmiEXLayer SelectedLayer
        {
            get
            {
                //todo solve this differently, this is ew..
                try
                {
                    return Image.GetLayers().First((l) => l.Index == LayerIndex);
                }
                catch (InvalidOperationException) //layerindex not valid anymore
                {
                    LayerIndex = Image.GetLayers().First().Index;
                    return Image.GetLayers().First((l) => l.Index == LayerIndex);
                }
            }
        }

        private int HighestIndex => Image.GetLayers().Max((l) => l.Index);
        private int LowestIndex => Image.GetLayers().Min((l) => l.Index);

        public event EventHandler LayerIndexChanged;

        public ImageEditor(StateEditor stateEditor, int dirIndex, int frameIndex)
        {
            InitializeComponent();
            
            StateEditor = stateEditor;
            DirIndex = dirIndex;
            FrameIndex = frameIndex;
            
            Image = stateEditor.State.GetImage(DirIndex, FrameIndex);
            
            //so we dont get blurry images
            RenderOptions.SetBitmapScalingMode(hotspotImg, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(backgroundImg, BitmapScalingMode.NearestNeighbor);
            
            //binding events for drawing
            img.MouseLeftButtonDown += OnLeftMouseDownOnImage;
            img.MouseLeftButtonUp += OnLeftMouseUpOnImage;
            img.MouseMove += OnMouseMoveOnImage;
            img.MouseEnter += OnMouseEnterOnImage;
            img.MouseLeave += OnMouseExitedOnImage;

            Image.LayerListChanged += UpdateLayerUi;
            Image.ImageChanged += UpdateImageDisplay;

            stateEditor.FileEditor.DmiEx.SizeChanged += CreateBackgroundImage;
            stateEditor.State.HotspotListChanged += UpdateHotspotImage;
            MainWindow.Current.ToolSelectionChanged += UpdateHotspotImage;

            SelectionChanged += UpdateSelectionImage;
            
            CreateBackgroundImage();
            
            CreateLayerUi();
            
            UpdateImageDisplay();

            UpdateHotspotImage();
            
            UpdateSelectionImage();
        }

        private void CreateBackgroundImage(object sender = null, EventArgs e = null)
        {
            backgroundImg.Source = BitmapUtils.Bitmap2BitmapImage(BitmapHelper.CreateTransparentBackgroundMap(Image.Width * 2, Image.Height * 2));
        }

        private void UpdateHotspotImage(object sender = null, EventArgs e = null)
        {
            if(MainWindow.Current.ViewHotspots)
                CreateHotspotImage();
            else
                ClearHotspotImage();
        }

        private void CreateHotspotImage()
        {
            //im too stupid to retrieve a file/resource so im just gonna draw it programmatically
            Bitmap bm = new Bitmap(7,7);
            bm.SetPixel(+0,3, Color.Black);
            bm.SetPixel(0,2, Color.LightGray);
            bm.SetPixel(1,3, Color.Black);
            bm.SetPixel(1,2, Color.LightGray);
            bm.SetPixel(2,3, Color.Black);
            bm.SetPixel(2,2, Color.LightGray);
            bm.SetPixel(3,3, Color.Black);
            bm.SetPixel(4,3, Color.Black);
            bm.SetPixel(4,2, Color.LightGray);
            bm.SetPixel(5,3, Color.Black);
            bm.SetPixel(5,2, Color.LightGray);
            bm.SetPixel(6,3, Color.Black);
            bm.SetPixel(6,2, Color.LightGray);
            
            bm.SetPixel(3,0, Color.Black);
            bm.SetPixel(2,0, Color.LightGray);
            bm.SetPixel(3,1, Color.Black);
            bm.SetPixel(2,1, Color.LightGray);
            bm.SetPixel(3,2, Color.Black);
            bm.SetPixel(3,4, Color.Black);
            bm.SetPixel(2,4, Color.LightGray);
            bm.SetPixel(3,5, Color.Black);
            bm.SetPixel(2,5, Color.LightGray);
            bm.SetPixel(3,6, Color.Black);
            bm.SetPixel(2,6, Color.LightGray);

            Bitmap main = new Bitmap(Image.Width*7,Image.Height*7);
            Hotspot hotspot = StateEditor.State.GetHotspot(FrameIndex, DirIndex);
            if (hotspot == null)
            {
                ClearHotspotImage();
                return;
            }
            
            int off_x = hotspot.X * 7;
            int off_y = hotspot.Y * 7;
            using (Graphics g = Graphics.FromImage(main))
            {
                g.DrawImage(bm, off_x, off_y);
            }

            hotspotImg.Source = BitmapUtils.Bitmap2BitmapImage(main);
        }
        
        private void ClearHotspotImage()
        {
            hotspotImg.Source = null;
        }
        
        private void UpdateLayerUi(object sender, EventArgs e)
        {
            _layerButtons.Clear();
            LayerStackPanel.Children.Clear();
            CreateLayerUi();
        }
        
        private void CreateLayerUi()
        {
            var addHighBtn = new Button
            {
                Content = new TextBlock
                {
                    Text = "Add layer above"
                }
            };
            addHighBtn.Click += AddLayerOnTop;
            LayerStackPanel.Children.Add(addHighBtn);
            
            foreach (var btn in Image.GetLayers().Reverse().Select(layer => new LayerButton(this, layer)))
            {
                LayerStackPanel.Children.Add(btn);
                _layerButtons.Add(btn);
            }

            var addLowBtn = new Button
            {
                Content = new TextBlock
                {
                    Text = "Add layer below"
                }
            };
            addLowBtn.Click += AddLayerOnBottom;
            LayerStackPanel.Children.Add(addLowBtn);
        }

        public void ClearSelection()
        {
            _selectedPixels.Clear();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
        
        public void SetSelection(Point[] points)
        {
            _selectedPixels.Clear();
            SelectPixel(points);
        }
        
        public void SelectPixel(Point point) => SelectPixel(new[] {point});
        
        public void SelectPixel(Point[] points)
        {
            foreach (var point in points)
            {
                if (_selectedPixels.Any(p => p.Equals(point))) continue;
            
                _selectedPixels.Add(point);
            }
            
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void DeSelectPixel(Point point) => SelectPixel(new[] {point});
        
        public void DeSelectPixel(Point[] points)
        {
            foreach (var point in points)
            {
                _selectedPixels.RemoveAll(p => p.Equals(point));
            }
            
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
        
        private void UpdateSelectionImage(object sender = null, EventArgs e = null)
        {
            int resolution = 6;
            Bitmap bm = new Bitmap(Image.Width*resolution,Image.Height*resolution);
            using (Graphics g = Graphics.FromImage(bm))
            {
                foreach (var pixel in _selectedPixels)
                {
                    var neighbours =
                        _selectedPixels.FindAll(t => Math.Abs(pixel.X - t.X) <= 1 && Math.Abs(pixel.Y - t.Y) <= 1);
                    var has_upper_neighbour = neighbours.Any(t => t.X == pixel.X && t.Y == pixel.Y - 1);
                    var has_right_neighbour = neighbours.Any(t => t.X == pixel.X+1 && t.Y == pixel.Y);
                    var has_lower_neighbour = neighbours.Any(t => t.X == pixel.X && t.Y == pixel.Y + 1);
                    var has_left_neighbour = neighbours.Any(t => t.X == pixel.X-1 && t.Y == pixel.Y);

                    var selector_image = BitmapHelper.CreateSelectionBox(!has_upper_neighbour, !has_right_neighbour,
                        !has_lower_neighbour, !has_left_neighbour, resolution);
                    g.DrawImage(selector_image, pixel.X*resolution, pixel.Y*resolution);
                }
            }

            selectionImg.Source = BitmapUtils.Bitmap2BitmapImage(bm);
        }

        private void AddLayerOnTop(object sender, EventArgs e) => AddLayerAtIndex(HighestIndex + 1);
        private void AddLayerOnBottom(object sender, EventArgs e) => AddLayerAtIndex(LowestIndex - 1);

        private void AddLayerAtIndex(int index)
        {
            DmiEXLayer l = Image.AddLayer(index);
            MainWindow.Current.UndoManager.RegisterUndoItem(new LayerNewUndoItem(Image, l));
        }

        private void UpdateImageDisplay(object sender = null, EventArgs e = null)
        {
            img.Source = BitmapUtils.Bitmap2BitmapImage(Image.GetBitmap());
        }
        
        private void OnLeftMouseDownOnImage(object sender, MouseEventArgs e)
            => MainWindow.Current.SelectedTool?.onLeftMouseDown(Image, BitmapPoint(e));

        private void OnMouseMoveOnImage(object sender, MouseEventArgs e)
            => MainWindow.Current.SelectedTool?.onMouseMove(Image, BitmapPoint(e));

        private void OnLeftMouseUpOnImage(object sender, MouseEventArgs e)
            => MainWindow.Current.SelectedTool?.onLeftMouseUp(Image, BitmapPoint(e));

        private void OnMouseEnterOnImage(object sender, MouseEventArgs e)
            => MainWindow.Current.SelectedTool?.onMouseEnter(Image, BitmapPoint(e), e.LeftButton == MouseButtonState.Pressed);
        
        private void OnMouseExitedOnImage(object sender, MouseEventArgs e)
            => MainWindow.Current.SelectedTool?.onMouseExited(Image, BitmapPoint(e));
        
        // helpers to calculate from screen pixel pos -> bitmap pixel pos
        private Point BitmapPoint(MouseEventArgs e)
        {
            System.Windows.Point wP = e.GetPosition(img);
            int x = (int) Math.Floor(wP.X * (Image.Width / 96d));
            int y = (int)Math.Floor(wP.Y*(Image.Height/96d));

            x = x < Image.Width ? x : Image.Width - 1;
            y = y < Image.Height ? y : Image.Height - 1;
            
            return new Point(x, y);
        }
        
        private class LayerButton : LabeledImageButton
        {
            private readonly ImageEditor _imageEditor;
            private readonly DmiEXLayer _layer;
            private TextBlock _visibleText = new TextBlock();
            private IntegerUpDown _layerIndexEditor;
            public LayerButton(ImageEditor imageEditor, DmiEXLayer layer) : base(BitmapUtils.Bitmap2BitmapImage(layer.GetBitmap()), "")
            {
                _imageEditor = imageEditor;
                _layer = layer;

                StackPanel main_sp = new StackPanel{ Orientation = Orientation.Vertical};
                
                StackPanel upper_sp = (StackPanel) Content;
                Content = main_sp;
                main_sp.Children.Add(upper_sp);

                _visibleText.Text = _layer.Visible ? "Hide" : "Show";

                StackPanel buttonPanel = new StackPanel {Orientation = Orientation.Horizontal};

                Button visibleBtn = new Button
                {
                    Content = _visibleText
                };
                visibleBtn.Click += ToggleVisibility;
                upper_sp.Children.Add(visibleBtn);

                _layerIndexEditor = new IntegerUpDown()
                {
                    Increment = 1,
                    Value = _layer.Index,
                    AllowSpin = false,
                    ShowButtonSpinner = false
                };
                _layerIndexEditor.KeyDown += UpdateIndex;
                _layerIndexEditor.LostFocus += UpdateEditor;
                var p = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                p.Children.Add(new TextBlock(){Text = "Index: "});
                p.Children.Add(_layerIndexEditor);
                upper_sp.Children.Add(p);
                
                Button duplicateButton = new Button
                {
                    Content = "Duplicate"
                };
                duplicateButton.Click += DuplicateLayer;
                buttonPanel.Children.Add(duplicateButton);
                
                Button exportButton = new Button
                {
                    Content = "Export as State"
                };
                exportButton.Click += ExportLayer;
                buttonPanel.Children.Add(exportButton);
                
                Button deleteButton = new Button
                {
                    Content = "Delete"
                };
                deleteButton.Click += DeleteLayer;
                buttonPanel.Children.Add(deleteButton);
                
                main_sp.Children.Add(buttonPanel);

                Click += Clicked;

                _layer.ImageChanged += UpdateImage;
                _layer.IndexChanged += UpdateEditor;
                _layer.VisibilityChanged += UpdateVisibility;

                _imageEditor.LayerIndexChanged += UpdatePressState;
                
                UpdatePressState();
            }

            private void Clicked(object sender, RoutedEventArgs e)
            {
                if(e.Source != this) return;
                try{
                    _imageEditor.LayerIndex = _layer.Index;
                }
                catch (ArgumentException ex)
                {
                    ErrorPopupHelper.Create(ex);
                }
            }
            
            private void UpdateVisibility(object sender, EventArgs e)
            {
                _visibleText.Text = _layer.Visible ? "Hide" : "Show";
            }

            private void UpdatePressState(object sender = null, EventArgs e = null)
            {
                SetPressed(_layer.Index == _imageEditor.LayerIndex);
            }

            private void UpdateImage(object sender, EventArgs e)
            {
                SetImage(BitmapUtils.Bitmap2BitmapImage(_layer.GetBitmap()));
            }

            private void ToggleVisibility(object sender, EventArgs e)
            {
                _layer.Visible = !_layer.Visible;
            }

            private void DuplicateLayer(object sender, EventArgs e)
            {
                try{
                    DmiEXLayer l = (DmiEXLayer)_layer.Clone();
                    MainWindow.Current.UndoManager.RegisterUndoItem(new LayerNewUndoItem(_imageEditor.Image, l));
                    _imageEditor.Image.AddLayer(l);
                }
                catch (ArgumentException ex)
                {
                    ErrorPopupHelper.Create(ex);
                }
            }

            private void DeleteLayer(object sender, EventArgs e)
            {
                MainWindow.Current.UndoManager.RegisterUndoItem(new LayerDeletedUndoItem(_imageEditor.Image, _layer));
                try
                {
                    _imageEditor.Image.RemoveLayer(_layer);
                }
                catch (WarningException ex)
                {
                    ErrorPopupHelper.Create(ex);
                }
            }
            
            private void UpdateEditor(object sender, EventArgs e)
            {
                _layerIndexEditor.Value = _layer.Index;
            }

            private void UpdateIndex(object sender, KeyEventArgs e)
            {
                if (e.Key != Key.Enter) return;
                if (_layerIndexEditor.Value == null) return;
                
                MainWindow.Current.UndoManager.RegisterUndoItem(new LayerIndexChangeUndoItem(_layer));
                _imageEditor.Image.SetLayerIndex(_layer, _layerIndexEditor.Value.Value);
            }

            private void ExportLayer(object sender, EventArgs e)
            {
                new ExportLayerWindow(_layer).Show();
            }
        }
    }
}