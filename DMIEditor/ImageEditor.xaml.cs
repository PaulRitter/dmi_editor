using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DMI_Parser.Utils;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;


namespace DMIEditor
{
    public partial class ImageEditor : UserControl
    {
        public new readonly StateEditor Parent;
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

        public DmiEXLayer SelectedLayer => Image.getLayers().First((l) => l.Index == _layerIndex);

        private int HighestIndex => Image.getLayers().Max((l) => l.Index);
        private int LowestIndex => Image.getLayers().Min((l) => l.Index);

        public event EventHandler LayerIndexChanged;

        public ImageEditor(StateEditor parent, int dirIndex, int frameIndex)
        {
            InitializeComponent();
            
            this.Parent = parent;
            this.DirIndex = dirIndex;
            this.FrameIndex = frameIndex;
            this.Image = parent.State.Images[DirIndex,FrameIndex];
            
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
                    var c = s ? Color.Gray : Color.White;
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
            addHighBtn.Click += (sender, e) => Image.addLayer(HighestIndex+1);
            LayerStackPanel.Children.Add(addHighBtn);
            
            foreach (var btn in Image.getLayers().Reverse().Select(layer => new LayerButton(this, layer)))
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
            addLowBtn.Click += (sender, e) => Image.addLayer(LowestIndex-1);
            LayerStackPanel.Children.Add(addLowBtn);
        }
        
        private void SelectLayer(int index)
        {
            try
            {
                Image.getLayerByIndex(index);
            }
            catch (ArgumentException e)
            {
                return;
            }

            LayerIndex = index;
        }
        
        private void UpdateImageDisplay(object sender = null, EventArgs e = null)
        {
            img.Source = Image.getImage();
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
            public readonly int LayerIndex;
            private readonly ImageEditor _imageEditor;
            private readonly DmiEXLayer _layer;
            private TextBlock _visibleText = new TextBlock();
            public LayerButton(ImageEditor imageEditor, DmiEXLayer layer) : base(layer.toImage(), $"Layer {layer.Index}")
            {
                LayerIndex = layer.Index;
                _imageEditor = imageEditor;
                _layer = layer;

                StackPanel sp = (StackPanel) Content;

                _visibleText.Text = _layer.Visible ? "Hide" : "Show";

                Button visibleBtn = new Button
                {
                    Content = _visibleText
                };
                sp.Children.Add(visibleBtn);

                visibleBtn.Click += ToggleVisibility;
                Click += Clicked;

                _layer.ImageChanged += UpdateImage;
                //todo _layer.indexChanged += updateIndex
                _layer.VisibilityChanged += UpdateVisibility;

                _imageEditor.LayerIndexChanged += UpdatePressState;
                
                UpdatePressState();
            }

            private void Clicked(object sender, EventArgs e)
            {
                _imageEditor.SelectLayer(LayerIndex);
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
                setImage(_layer.toImage());
            }

            private void ToggleVisibility(object sender, EventArgs e)
            {
                _layer.Visible = !_layer.Visible;
            }
        }
    }
}