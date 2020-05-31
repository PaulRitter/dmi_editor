using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DMI_Parser;
using Xceed.Wpf.Toolkit;
using Color = System.Drawing.Color;

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
            img.MouseLeftButtonDown += OnLeftMouseDown;
            img.MouseLeftButtonUp += OnLeftMouseUp;
            img.MouseLeave += OnMouseExit;
            img.MouseMove += OnMouseMove;

            //creating background map (tiling)
            var backgroundMap = new Bitmap(State.Width * 2, State.Height * 2);
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
            
            backgroundImg.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
               backgroundMap.GetHbitmap(),
               IntPtr.Zero,
               Int32Rect.Empty,
               BitmapSizeOptions.FromWidthAndHeight(backgroundMap.Width, backgroundMap.Height));
            
            CreateImageButtons();
            
            //setting default image
            DirIndex = -1;
            FrameIndex = -1;
            SetImage(0,0);

            //create stateValue editUI
            
            //stateID
            var idBox = new TextBox {Text = state.Id};
            idBox.KeyDown += (sender, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    try
                    {
                        state.setID(idBox.Text);
                    }
                    catch (ArgumentException)
                    {
                        ErrorPopupHelper.Create($"StateID \"{idBox.Text}\" is not valid!");
                        idBox.Text = state.Id;
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
            dirCountBox.SelectedItem = state.Dirs;
            dirCountBox.SelectionChanged += (sender, args) =>
            {
                state.setDirs((DirCount) dirCountBox.SelectedItem);
            };
            stateValues.Children.Add(dirCountBox);
            //subscribe to event
            state.dirCountChanged += OnDirCountChanged;

            //frame count
            var frameCountEditor = new IntegerUpDown()
            {
                Minimum = 1,
                Increment = 1,
                Maximum = 30, //arbitrary number, why would you ever need more than this?
                Value = state.Frames
            };
            frameCountEditor.ValueChanged += (sender, args) =>
            {
                var frames = frameCountEditor.Value;
                if(frames != null)
                    state.setFrames(frames.Value);
            };
            var p = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            p.Children.Add(new TextBlock(){Text = "Frame count: "});
            p.Children.Add(frameCountEditor);
            stateValues.Children.Add(p);
            //subscribe to event
            state.frameCountChanged += OnFrameCountChanged;

            //loop
            var loopCountEditor = new IntegerUpDown()
            {
                Minimum = 0,
                Increment = 1,
                Maximum = 30,
                Value = state.Loop
            };
            var infiniteIndicator = new TextBlock()
            {
                Text = state.Loop == 0 ? "(Infinite)" : ""
            };
            loopCountEditor.ValueChanged += (sender, args) =>
            {
                infiniteIndicator.Text = loopCountEditor.Value == 0 ? "(Infinite)" : "";

                state.setLoop(loopCountEditor.Value.Value);
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
                IsChecked = state.Rewind
            };
            rewindBox.Click += (sender, args) =>
            {
                state.setRewind(rewindBox.IsChecked.Value);
            };
            p = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            p.Children.Add(new TextBlock(){Text = "Rewind: "});
            p.Children.Add(rewindBox);
            stateValues.Children.Add(p);

            //TODO delays (maybe add into frame selection)
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

        // =====================
        // =====================
        // Tool handling
        private bool _mouseHeld; //tracks wether or not left mouse button is held

        //will try to modify the specified pixel with the selected tool
        private void TryAct(int x, int y)
        {
            if (SelectedBitmap == null) return;
            
            if (_fileEditor.Main.GetTool().pixelAct(SelectedBitmap, x, y))
                ReRenderImage();
        }

        // Event handling
        private void OnLeftMouseDown(object sender, MouseEventArgs e)
        {
            _mouseHeld = true;
            System.Windows.Point wP = e.GetPosition(img);
            TryAct(FileEditor.RealPos(wP.X), FileEditor.RealPos(wP.Y));
        }
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseHeld)
            {
                System.Windows.Point wP = e.GetPosition(img);
                TryAct(FileEditor.RealPos(wP.X), FileEditor.RealPos(wP.Y));
            }
        }
        private void OnLeftMouseUp(object sender, MouseEventArgs e)
        {
            _mouseHeld = false;
        }
        private void OnMouseExit(object sender, MouseEventArgs e)
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

        private void ReRenderImage()
        {
            var actual = new Bitmap(State.Width * 2, State.Height * 2);
            for (var x = 0; x < State.Width; x++)
            {
                for (var y = 0; y < State.Height; y++)
                {
                    foreach (var layer in _layers)
                    {
                        if (!layer.Visible) continue;
                        
                        var actualX = x * 2;
                        var actualY = y * 2;
                        var c = layer.Bitmap.GetPixel(x, y);
                        //TODO adding colors
                        
                        if (c.ToArgb() == 0) continue;
                        
                        actual.SetPixel(actualX, actualY, c);
                        actual.SetPixel(actualX+1,actualY,c);
                        actual.SetPixel(actualX, actualY+1, c);
                        actual.SetPixel(actualX+1,actualY+1,c);
                        break;
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
