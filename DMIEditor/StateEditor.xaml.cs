using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DMI_Parser;
using DMIEditor.Tools;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using Xceed.Wpf.Toolkit;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using ResizeMode = System.Windows.ResizeMode;

namespace DMIEditor
{
    /// <summary>
    /// Interaktionslogik für StateEditor.xaml
    /// </summary>
    public partial class StateEditor : UserControl
    {
        private readonly FileEditor _fileEditor;
        private List<Layer> _layers;
        private int _layerIndex = 0;
        private List<LayerButton> _layerButtons = new List<LayerButton>();
        private List<ImageSelectionButton> _frameButtons = new List<ImageSelectionButton>();

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
            this._fileEditor = fileEditor;
            this.StateIndex = stateIndex;
            State = state;
            _layers = new List<Layer>();
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(img, EdgeMode.Aliased);

            //binding events for drawing
            img.MouseLeftButtonDown += OnLeftMouseDownOnImage;
            img.MouseLeftButtonUp += OnLeftMouseUpOnImage;
            img.MouseLeave += OnMouseExitOnImage;
            img.MouseMove += OnMouseMoveOnImage;
            img.MouseEnter += OnMouseEnterOnImage;

            //creating background map (tiling)
            var backgroundMap = new Bitmap(State.Width*2, State.Height*2);
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

            ImageFactory imgF = new ImageFactory().Load(backgroundMap);
            imgF.Resolution(State.Width * 2, State.Height * 2);

            MemoryStream stream = new MemoryStream(); 
            imgF.Save(stream);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.EndInit();

            backgroundImg.Source = bitmap;
            
            CreateImageButtons();
            
            //setting default image
            DirIndex = -1;
            FrameIndex = -1;
            SetImage(0,0);

            //create stateValue editUI
            CreateStateValueEditor();
            
            //subscribe to state events
            State.dirCountChanged += OnDirCountChanged;
            State.frameCountChanged += OnFrameCountChanged;
            
            //TODO delays (maybe add into frame selection)
        }

        private void CreateStateValueEditor()
        {
            //stateID
            var idBox = new TextBox {Text = State.Id};
            idBox.KeyDown += (sender, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    try
                    {
                        State.setID(idBox.Text);
                    }
                    catch (ArgumentException)
                    {
                        ErrorPopupHelper.Create($"StateID \"{idBox.Text}\" is not valid!");
                        idBox.Text = State.Id;
                    }                   
                }
            };
            stateValues.Children.Add(idBox);

            //dir count
            var dirCountBox = new ComboBox();
            foreach (DirCount dir in Enum.GetValues(typeof(DirCount)))
            {
                dirCountBox.Items.Add(dir);
            }
            dirCountBox.SelectedItem = State.Dirs;
            dirCountBox.SelectionChanged += (sender, args) =>
            {
                State.setDirs((DirCount) dirCountBox.SelectedItem);
            };
            stateValues.Children.Add(dirCountBox);
            

            //frame count
            var frameCountEditor = new IntegerUpDown()
            {
                Minimum = 1,
                Increment = 1,
                Maximum = 30, //arbitrary number, why would you ever need more than this?
                Value = State.Frames
            };
            frameCountEditor.ValueChanged += (sender, args) =>
            {
                var frames = frameCountEditor.Value;
                if(frames != null)
                    State.setFrames(frames.Value);
            };
            var p = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            p.Children.Add(new TextBlock(){Text = "Frame count: "});
            p.Children.Add(frameCountEditor);
            stateValues.Children.Add(p);

            //loop
            var loopCountEditor = new IntegerUpDown()
            {
                Minimum = 0,
                Increment = 1,
                Maximum = 30,
                Value = State.Loop
            };
            var infiniteIndicator = new TextBlock()
            {
                Text = State.Loop == 0 ? "(Infinite)" : ""
            };
            loopCountEditor.ValueChanged += (sender, args) =>
            {
                infiniteIndicator.Text = loopCountEditor.Value == 0 ? "(Infinite)" : "";

                State.setLoop(loopCountEditor.Value.Value);
            };
            p = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            p.Children.Add(new TextBlock(){Text = "Loop: "});
            p.Children.Add(loopCountEditor);
            p.Children.Add(infiniteIndicator);
            stateValues.Children.Add(p);

            //rewind
            var rewindBox = new CheckBox()
            {
                IsChecked = State.Rewind
            };
            rewindBox.Click += (sender, args) =>
            {
                State.setRewind(rewindBox.IsChecked.Value);
            };
            p = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            p.Children.Add(new TextBlock(){Text = "Rewind: "});
            p.Children.Add(rewindBox);
            stateValues.Children.Add(p);
        }
        
        private void OnDirCountChanged(object sender, EventArgs e)
        {
            CreateImageButtons();
        }

        private void OnFrameCountChanged(object sender, EventArgs e)
        {
            CreateImageButtons();
        }
        
        private void CreateImageButtons()
        {
            //create dir and frame buttons
            dirPanel.Children.Clear();
            
            for (int d = 0; d < (int)State.Dirs; d++)
            {
                Border b = new Border
                {
                    BorderThickness = new Thickness(0.5d),
                    BorderBrush = System.Windows.Media.Brushes.Black
                };
                StackPanel framePanel = new StackPanel();
                TextBlock title = new TextBlock
                {
                    Text = $"Dir {d + 1}", HorizontalAlignment = HorizontalAlignment.Center
                };
                framePanel.Children.Add(title);
                for (int f = 0; f < State.Frames; f++)
                {
                    ImageSelectionButton frameButton = new ImageSelectionButton(this, d, f, State.getImage(d, f), $"Frame {f+1}");
                    framePanel.Children.Add(frameButton);
                    _frameButtons.Add(frameButton);
                }
                b.Child = framePanel;
                dirPanel.Children.Add(b);
            }
            //sets the proper layout for the buttons
            UpdateFrameButtonsPressState();
        }
        
        private void UpdateFrameButtonsPressState()
        {
            foreach (ImageSelectionButton btn in _frameButtons.Where(btn => btn.isPressed()))
            {
                btn.SetPressed(false);
            }

            foreach (ImageSelectionButton btn in _frameButtons.Where(btn => btn.DirIndex == DirIndex && btn.FrameIndex == FrameIndex))
            {
                btn.SetPressed(true);
            }
        }

        private void SetImage(int dir, int frame)
        {
            var imgChanged = false;

            if (dir >= 0 && dir < (int)State.Dirs)
            {
                if (dir != DirIndex)
                {
                    imgChanged = true;
                    this.DirIndex = dir;
                }
            }
            else return; //throw outofrangeexception

            if (frame >= 0 && frame < State.Frames)
            {
                if (frame != FrameIndex)
                {
                    imgChanged = true;
                    this.FrameIndex = frame;
                }
            }
            else return; //throw outofrangeexception

            if (imgChanged)
            {
                UpdateImageDisplay();
                UpdateFrameButtonsPressState();
            }
        }
        
        private void OnLeftMouseDownOnImage(object sender, MouseEventArgs e)
            => _fileEditor.Main.GetTool().onLeftMouseDown(SelectedBitmap, BitmapPoint(e));

        private void OnMouseMoveOnImage(object sender, MouseEventArgs e)
            => _fileEditor.Main.GetTool().onMouseMove(SelectedBitmap, BitmapPoint(e));

        private void OnLeftMouseUpOnImage(object sender, MouseEventArgs e)
            => _fileEditor.Main.GetTool().onLeftMouseUp(SelectedBitmap, BitmapPoint(e));

        private void OnMouseEnterOnImage(object sender, MouseEventArgs e)
            => _fileEditor.Main.GetTool().onMouseEnter(SelectedBitmap, BitmapPoint(e));
        
        private void OnMouseExitOnImage(object sender, MouseEventArgs e)
            => _fileEditor.Main.GetTool().onMouseExit(SelectedBitmap, BitmapPoint(e));

        // helpers to calculate from screen pixel pos -> bitmap pixel pos
        public Point BitmapPoint(MouseEventArgs e)
        {
            System.Windows.Point wP = e.GetPosition(img);
            int x = (int) Math.Floor(wP.X * (State.Width / 96d));
            int y = (int)Math.Floor(wP.Y*(State.Height/96d));

            x = x < State.Width ? x : State.Width - 1;
            y = y < State.Height ? y : State.Height - 1;
            
            return new Point(x, y);
        }

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

        private void AddLayer(int index, Bitmap bitmap)
        {
            if (index == 0) return; //index 0 is reserved for nothing selected
            
            if (_layers.Any(layer => layer.Index == index))
            {
                throw new ArgumentException("A Layer with that index already exists.");
            }
            var newLayer = new Layer(bitmap, index);
            newLayer.Changed += OnLayerChanged;
            _layers.Add(newLayer);
            _layers.Sort((l1, l2) => l1.CompareTo(l2)); //layers are always sorted from highest index to lowest
            UpdateLayerUi();
            SelectLayer(index);
        }

        private void OnLayerChanged(object sender, EventArgs e)
        {
            if (!(sender is Layer layer)) return;
            
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
            UpdateLayerUi();
            ReRenderImage();
        }

        private bool SelectLayer(int index)
        {
            if (index != 0){
                if(GetLayer(index) == null) return false;
                
                if(!GetLayer(index).Visible) return false;
            }
            
            _layerIndex = index;
            foreach (var btn in _layerButtons)
            {
                btn.SetPressed(btn.LayerIndex == index);
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
            foreach (var btn in _layers.Select(layer => new LayerButton(layer.Bitmap, layer.Index, this)))
            {
                LayerStackPanel.Children.Add(btn);
                _layerButtons.Add(btn);
            }

            var addBtn = new Button
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
        //todo memoryleak
        private void UpdateImageDisplay()
        {
            _layers = new List<Layer>();
            UpdateLayerUi();
            
            AddLayer(1, State.getImage(DirIndex, FrameIndex));

            ReRenderImage();
        }

        public void ReRenderImage()
        {
            ImageFactory imgF = new ImageFactory();
            bool first = true;

            //just to make sure
            _layers.Sort((l1,l2)=>l1.Index.CompareTo(l2.Index));
            for (int i = 0; i < _layers.Count; i++)
            {
                Layer layer = _layers[i];
                if (!layer.Visible) continue;
                if (first)
                {
                    imgF.Load(layer.Bitmap);
                    first = false;
                    continue;
                }

                ImageLayer l = new ImageLayer();
                l.Image = layer.Bitmap;
                imgF.Overlay(l);
            }

            imgF.Resolution(State.Width, State.Height);
            imgF.Format(new PngFormat());
            imgF.BackgroundColor(Color.Transparent);

            MemoryStream stream = new MemoryStream(); 
            imgF.Save(stream);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.EndInit();

            img.Source = bitmap;

            UpdateLayerUi();
        }

        private class LayerButton : LabeledImageButton
        {
            public readonly int LayerIndex;
            private readonly StateEditor _stateEditor;
            private TextBlock _visibleText = new TextBlock();
            public LayerButton(Bitmap bm, int layerIndex, StateEditor stateEditor) : base(bm, $"Layer {layerIndex}")
            {
                this.LayerIndex = layerIndex;
                this._stateEditor = stateEditor;

                StackPanel sp = (StackPanel) Content;

                _visibleText.Text = _stateEditor.GetLayer(LayerIndex).Visible ? "Hide" : "Show";

                Button visibleBtn = new Button
                {
                    Content = _visibleText
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
        
        private class ImageSelectionButton : LabeledImageButton
        {
            private readonly StateEditor _stateEditor;
            public readonly int DirIndex;
            public readonly int FrameIndex;
            public ImageSelectionButton(StateEditor stateEditor, int dirIndex, int frameIndex, Bitmap bm, string labeltext) : base (bm, labeltext)
            {
                this.DirIndex = dirIndex;
                this.FrameIndex = frameIndex;
                this._stateEditor = stateEditor;
                Click += Clicked;
            }

            private void Clicked(object sender, EventArgs e)
            {
                _stateEditor.SetImage(DirIndex, FrameIndex);
            }
        }
    }
}
