using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DMI_Parser.Extended;
using DMI_Parser.Utils;
using DMIEditor.Undo;
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
                catch (InvalidOperationException ex) //layerindex not valid anymore
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
            
            CreateBackgroundImage();
            
            CreateLayerUi();
            
            UpdateImageDisplay();
        }

        private void CreateBackgroundImage(object sender = null, EventArgs e = null)
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
                    Value = _layer.Index
                };
                _layerIndexEditor.ValueChanged += UpdateIndex;
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

            private void Clicked(object sender, EventArgs e)
            {
                
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
                _layerIndexEditor.ValueChanged -= UpdateIndex;
                _layerIndexEditor.Value = _layer.Index;
                _layerIndexEditor.ValueChanged += UpdateIndex;
            }

            private void UpdateIndex(object sender, EventArgs e)
            {
                if (_layerIndexEditor.Value == null) return;
                MainWindow.Current.UndoManager.RegisterUndoItem(new LayerIndexChangeUndoItem(_layer));
                _imageEditor.Image.SetLayerIndex(_layer, _layerIndexEditor.Value.Value);
            }

            private void ExportLayer(object sender, EventArgs e)
            {
                new ExportLayerPrompt(_layer, _imageEditor).Show();
            }

            private class ExportLayerPrompt : PromptWindow
            {
                private readonly FileEditor _editor;
                private readonly DmiEXLayer _layer;
                public ExportLayerPrompt(DmiEXLayer layer, ImageEditor editor) : base("Enter an ID for the new State:", "Export Layer to State")
                {
                    _layer = layer;
                    _editor = editor.StateEditor.FileEditor;
                }

                protected override void promptSent(string prompt)
                {
                    try
                    {
                        DmiEXState state = _layer.ToDmiExState(_editor.DmiEx, prompt);
                        MainWindow.Current.UndoManager.RegisterUndoItem(new StateNewUndoItem(_editor.DmiEx, state));
                        _editor.DmiEx.AddState(state);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    
                }
            }
        }
    }
}