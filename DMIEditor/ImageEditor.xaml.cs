using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DMI_Parser.Utils;
using DMIEditor.DmiEX;
using Xceed.Wpf.Toolkit;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;


namespace DMIEditor
{
    public partial class ImageEditor : UserControl
    {
        public readonly StateEditor StateEditor;
        public readonly int DirIndex;
        public readonly int FrameIndex;
        public readonly DmiEXImage Image;
        
        private List<LayerButton> _layerButtons = new List<LayerButton>();
        
        private int _layerIndex;
        public int LayerIndex
        {
            get => _layerIndex;
            private set
            {
                _layerIndex = value;
                LayerIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public DmiEXLayer SelectedLayer => Image.GetLayers().First((l) => l.Index == _layerIndex);

        private int HighestIndex => Image.GetLayers().Max((l) => l.Index);
        private int LowestIndex => Image.GetLayers().Min((l) => l.Index);

        public event EventHandler LayerIndexChanged;

        public ImageEditor(StateEditor stateEditor, int dirIndex, int frameIndex)
        {
            InitializeComponent();
            
            this.StateEditor = stateEditor;
            this.DirIndex = dirIndex;
            this.FrameIndex = frameIndex;
            this.Image = stateEditor.State.Images[DirIndex,FrameIndex];
            
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(backgroundImg, BitmapScalingMode.NearestNeighbor);
            
            //binding events for drawing
            img.MouseLeftButtonDown += OnLeftMouseDownOnImage;
            img.MouseLeftButtonUp += OnLeftMouseUpOnImage;
            img.MouseMove += OnMouseMoveOnImage;
            img.MouseEnter += OnMouseEnterOnImage;

            Image.LayerListChanged += UpdateLayerUi;
            Image.ImageChanged += UpdateImageDisplay;
            
            CreateBackgroundImage();
            
            CreateLayerUi();
            
            UpdateImageDisplay();
        }

        private void CreateBackgroundImage()
        {
            //creating background map (tiling)
            var backgroundMap = new Bitmap(Image.Width*2, Image.Height*2);
            var s = true;
            for (var i = 0; i < backgroundMap.Width; i++)
            {
                for (var j = 0; j < backgroundMap.Height; j++)
                {
                    var c = s ? Color.LightGray : Color.White;
                    s = !s;
                    backgroundMap.SetPixel(i, j, c);
                }
                s = !s; //offsetting every row
            }
            
            backgroundImg.Source = BitmapUtils.Bitmap2BitmapImage(backgroundMap);
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
            addHighBtn.Click += (sender, e) => Image.AddLayer(HighestIndex+1);
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
            addLowBtn.Click += (sender, e) => Image.AddLayer(LowestIndex-1);
            LayerStackPanel.Children.Add(addLowBtn);
        }
        
        private void SelectLayer(int index)
        {
            try
            {
                Image.GetLayerByIndex(index);
            }
            catch (ArgumentException)
            {
                return;
            }

            LayerIndex = index;
        }
        
        private void UpdateImageDisplay(object sender = null, EventArgs e = null)
        {
            img.Source = Image.GetImage();
        }
        
        private void OnLeftMouseDownOnImage(object sender, MouseEventArgs e)
            => MainWindow.Current.GetTool().onLeftMouseDown(Image, BitmapPoint(e));

        private void OnMouseMoveOnImage(object sender, MouseEventArgs e)
            => MainWindow.Current.GetTool().onMouseMove(Image, BitmapPoint(e));

        private void OnLeftMouseUpOnImage(object sender, MouseEventArgs e)
            => MainWindow.Current.GetTool().onLeftMouseUp(Image, BitmapPoint(e));

        private void OnMouseEnterOnImage(object sender, MouseEventArgs e)
            => MainWindow.Current.GetTool().onMouseEnter(Image, BitmapPoint(e), e.LeftButton == MouseButtonState.Pressed);
        
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
            public LayerButton(ImageEditor imageEditor, DmiEXLayer layer) : base(layer.GetImage(), "")
            {
                _imageEditor = imageEditor;
                _layer = layer;

                StackPanel sp = (StackPanel) Content;

                _visibleText.Text = _layer.Visible ? "Hide" : "Show";

                StackPanel buttonPanel = new StackPanel {Orientation = Orientation.Horizontal};

                Button visibleBtn = new Button
                {
                    Content = _visibleText
                };
                visibleBtn.Click += ToggleVisibility;
                buttonPanel.Children.Add(visibleBtn);

                Button duplicateButton = new Button
                {
                    Content = "Duplicate"
                };
                duplicateButton.Click += DuplicateLayer;
                buttonPanel.Children.Add(duplicateButton);
                
                _layerIndexEditor = new IntegerUpDown()
                {
                    Increment = 1,
                    Value = _layer.Index
                };
                _layerIndexEditor.ValueChanged += UpdateIndex;
                var p = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                p.Children.Add(new TextBlock(){Text = "Index: "});
                p.Children.Add(_layerIndexEditor);
                buttonPanel.Children.Add(p);
                
                Button deleteButton = new Button
                {
                    Content = "Delete"
                };
                deleteButton.Click += DeleteLayer;
                buttonPanel.Children.Add(deleteButton);
                
                sp.Children.Add(buttonPanel);

                Click += Clicked;

                _layer.ImageChanged += UpdateImage;
                _layer.IndexChanged += UpdateEditor;
                _layer.VisibilityChanged += UpdateVisibility;

                _imageEditor.LayerIndexChanged += UpdatePressState;
                
                UpdatePressState();
            }

            private void Clicked(object sender, EventArgs e)
            {
                _imageEditor.SelectLayer(_layer.Index);
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
                SetImage(_layer.GetImage());
            }

            private void ToggleVisibility(object sender, EventArgs e)
            {
                _layer.Visible = !_layer.Visible;
            }

            private void DuplicateLayer(object sender, EventArgs e)
            {
                _imageEditor.Image.AddLayer((DmiEXLayer)_layer.Clone());
            }

            private void DeleteLayer(object sender, EventArgs e)
            {
                try
                {
                    _imageEditor.Image.RemoveLayer(_layer.Index);
                }
                catch (WarningException ex)
                {
                    ErrorPopupHelper.Create(ex);
                }
            }
            
            private void UpdateEditor(object sender, EventArgs e)
            {
                _layerIndexEditor.ValueChanged -= UpdateIndex;
                _layerIndexEditor.Value = _layer.Index;
                _layerIndexEditor.ValueChanged += UpdateIndex;
            }

            private void UpdateIndex(object sender, EventArgs e)
            {
                if (_layerIndexEditor.Value == null) return;
                _imageEditor.Image.SetLayerIndex(_layer, _layerIndexEditor.Value.Value);
            }
        }
    }
}