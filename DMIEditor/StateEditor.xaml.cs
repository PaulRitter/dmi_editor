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
using DMI_Parser;
using DMI_Parser.Utils;
using Xceed.Wpf.Toolkit;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

namespace DMIEditor
{
    /// <summary>
    /// Interaktionslogik für StateEditor.xaml
    /// </summary>
    public partial class StateEditor : UserControl
    {
        private readonly FileEditor _fileEditor;

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

        private int HighestIndex => SelectedImage.getLayers().Max((l) => l.Index);
        private int LowestIndex => SelectedImage.getLayers().Min((l) => l.Index);

        public event EventHandler LayerIndexChanged;
        
        private List<LayerButton> _layerButtons = new List<LayerButton>();
        private List<ImageSelectionButton> _frameButtons = new List<ImageSelectionButton>();

        public readonly int StateIndex;
        public readonly DmiEXState State;
        
        //todo convert this to a point to make event attachable to set
        public int DirIndex { get; private set; }
        public int FrameIndex { get; private set; }

        public event EventHandler ImageIndexChanged;

        public DmiEXImage SelectedImage => State.Images[DirIndex, FrameIndex];
        public DmiEXLayer SelectedLayer => SelectedImage.getLayerByIndex(_layerIndex);
        
        public StateEditor(FileEditor fileEditor, int stateIndex, DmiEXState state)
        {
            _fileEditor = fileEditor;
            StateIndex = stateIndex;
            State = state;
            
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
            
            //binding events for drawing
            img.MouseLeftButtonDown += OnLeftMouseDownOnImage;
            img.MouseLeftButtonUp += OnLeftMouseUpOnImage;
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
            
            backgroundImg.Source = BitmapUtils.Bitmap2BitmapImage(backgroundMap);;

            ImageIndexChanged += UpdateImageDisplay;

            //setting default image
            SetImageIndex(0,0);
            UpdateImageDisplay();

            //create stateValue editUI
            CreateStateValueEditor();
            
            //subscribe to state events
            State.dirCountChanged += CreateImageButtons;
            State.frameCountChanged += CreateImageButtons;

            CreateImageButtons();

            CreateLayerUi();

            ImageIndexChanged += UpdateLayerUI;
            //TODO delays (maybe add into frame selection)
        }

        private void UpdateLayerUI(object sender, EventArgs e)
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
            addHighBtn.Click += (sender, e) => SelectedImage.addLayer(HighestIndex+1);
            LayerStackPanel.Children.Add(addHighBtn);
            
            foreach (var btn in State.Images[DirIndex, FrameIndex].getLayers().Reverse().Select(layer => new LayerButton(this, layer)))
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
            addLowBtn.Click += (sender, e) => SelectedImage.addLayer(LowestIndex-1);
            LayerStackPanel.Children.Add(addLowBtn);
        }


        private void CreateStateValueEditor()
        {
            //stateID
            var idBox = new TextBox {Text = State.Id};
            idBox.KeyDown += (sender, args) =>
            {
                if (args.Key != Key.Enter) return;
                
                try
                {
                    State.setID(idBox.Text);
                }
                catch (ArgumentException)
                {
                    ErrorPopupHelper.Create($"StateID \"{idBox.Text}\" is not valid!");
                    idBox.Text = State.Id;
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

        private void CreateImageButtons(object sender = null, EventArgs e = null)
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

                string dirText;
                switch (d)
                {
                    case 0:
                        dirText = "SOUTH";
                        break;
                    case 1:
                        dirText = "NORTH";
                        break;
                    case 2:
                        dirText = "EAST";
                        break;
                    case 3:
                        dirText = "WEST";
                        break;
                    case 4:
                        dirText = "SOUTHEAST";
                        break;
                    case 5:
                        dirText = "SOUTHWEST";
                        break;
                    case 6:
                        dirText = "NORTHEAST";
                        break;
                    case 7:
                        dirText = "NORTHWEST";
                        break;
                    default:
                        dirText = "ERROR";
                        break;
                }
                
                TextBlock title = new TextBlock
                {
                    Text = $"{dirText}", HorizontalAlignment = HorizontalAlignment.Center
                };
                framePanel.Children.Add(title);
                for (int f = 0; f < State.Frames; f++)
                {
                    ImageSelectionButton frameButton = new ImageSelectionButton(this, d, f, $"Frame {f+1}", State.Images[d, f]);
                    framePanel.Children.Add(frameButton);
                    _frameButtons.Add(frameButton);
                }
                b.Child = framePanel;
                dirPanel.Children.Add(b);
            }
        }

        private void UpdateImageDisplay(object sender = null, EventArgs e = null)
        {
            img.Source = SelectedImage.getImage();
        }

        private void SetImageIndex(int dir, int frame)
        {
            var imgChanged = false;
            SelectedImage.ImageChanged -= UpdateImageDisplay;
            SelectedImage.LayerListChanged -= UpdateLayerUI;

            if (dir >= 0 && dir < (int)State.Dirs)
            {
                if (dir != DirIndex)
                {
                    imgChanged = true;
                    DirIndex = dir;
                }
            }
            else throw new IndexOutOfRangeException("Dirindex is out of range");

            if (frame >= 0 && frame < State.Frames)
            {
                if (frame != FrameIndex)
                {
                    imgChanged = true;
                    FrameIndex = frame;
                }
            }
            else throw new IndexOutOfRangeException("Frameindex is out of range");

            SelectedImage.ImageChanged += UpdateImageDisplay;
            SelectedImage.LayerListChanged += UpdateLayerUI;

            if (imgChanged) ImageIndexChanged?.Invoke(this, EventArgs.Empty);
        }
        
        private void OnLeftMouseDownOnImage(object sender, MouseEventArgs e)
            => _fileEditor.Main.GetTool().onLeftMouseDown(SelectedImage, BitmapPoint(e));

        private void OnMouseMoveOnImage(object sender, MouseEventArgs e)
            => _fileEditor.Main.GetTool().onMouseMove(SelectedImage, BitmapPoint(e));

        private void OnLeftMouseUpOnImage(object sender, MouseEventArgs e)
            => _fileEditor.Main.GetTool().onLeftMouseUp(SelectedImage, BitmapPoint(e));

        private void OnMouseEnterOnImage(object sender, MouseEventArgs e)
            => _fileEditor.Main.GetTool().onMouseEnter(SelectedImage, BitmapPoint(e), e.LeftButton == MouseButtonState.Pressed);
        
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
        
        private void SelectLayer(int index)
        {
            try
            {
                State.Images[DirIndex, FrameIndex].getLayerByIndex(index);
            }
            catch (ArgumentException e)
            {
                return;
            }

            LayerIndex = index;
        }
        

        private class LayerButton : LabeledImageButton
        {
            public readonly int LayerIndex;
            private readonly StateEditor _stateEditor;
            private readonly DmiEXLayer _layer;
            private TextBlock _visibleText = new TextBlock();
            public LayerButton(StateEditor stateEditor, DmiEXLayer layer) : base(layer.toImage(), $"Layer {layer.Index}")
            {
                LayerIndex = layer.Index;
                _stateEditor = stateEditor;
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

                _stateEditor.LayerIndexChanged += UpdatePressState;
                
                UpdatePressState();
            }

            private void Clicked(object sender, EventArgs e)
            {
                _stateEditor.SelectLayer(LayerIndex);
            }

            private void UpdateVisibility(object sender, EventArgs e)
            {
                _visibleText.Text = _layer.Visible ? "Hide" : "Show";
            }

            private void UpdatePressState(object sender = null, EventArgs e = null)
            {
                SetPressed(_layer.Index == _stateEditor.LayerIndex);
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
        
        private class ImageSelectionButton : LabeledImageButton
        {
            private readonly StateEditor _stateEditor;
            private readonly DmiEXImage _image;
            public readonly int DirIndex;
            public readonly int FrameIndex;
            public ImageSelectionButton(StateEditor stateEditor, int dirIndex, int frameIndex, string labeltext, DmiEXImage image) : base (image.getImage(), labeltext)
            {
                DirIndex = dirIndex;
                FrameIndex = frameIndex;
                _image = image;
                _stateEditor = stateEditor;
                
                Click += Clicked;
                image.ImageChanged += UpdateImage;
                stateEditor.ImageIndexChanged += UpdatePressed;
                
                UpdatePressed();
            }

            private void Clicked(object sender, EventArgs e) => _stateEditor.SetImageIndex(DirIndex, FrameIndex);

            private void UpdatePressed(object sender = null, EventArgs e = null) => SetPressed(_stateEditor.DirIndex == DirIndex && _stateEditor.FrameIndex == FrameIndex);

            private void UpdateImage(object sender = null, EventArgs e = null)
                => setImage(_image.getImage());
        }
    }
}
