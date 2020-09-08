using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DMI_Parser;
using DMI_Parser.Extended;
using DMI_Parser.Utils;
using DMIEditor.Undo;
using Xceed.Wpf.Toolkit;

namespace DMIEditor
{
    /// <summary>
    /// Interaktionslogik für StateEditor.xaml
    /// </summary>
    public partial class StateEditor : UserControl
    {
        public readonly FileEditor FileEditor;
        
        public readonly int StateIndex; //todo ditch this
        public readonly DmiEXState State;
        
        public event EventHandler ImageEditorChanged;

        private ImageEditor _imageEditor;
        public ImageEditor ImageEditor
        {
            get => _imageEditor;
            private set
            {
                _imageEditor = value;
                imageEditorGrid.Child = _imageEditor;
                ImageEditorChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        public StateEditor(FileEditor fileEditor, int stateIndex, DmiEXState state)
        {
            InitializeComponent();

            FileEditor = fileEditor;
            StateIndex = stateIndex;
            State = state;
            
            //setting default image
            SetImageIndex(0,0);

            //create stateValue editUI
            CreateStateValueEditor();
            
            //subscribe to state events
            State.ImageArrayChanged += OnImageArrayChanged;

            CreateImageButtons();
            
            //TODO delays (maybe add into frame selection)
        }

        private void OnImageArrayChanged(object sender, EventArgs e)
        {
            CreateImageButtons();
        }

        private void CreateStateValueEditor()
        {
            //stateID
            var idBox = new TextBox {Text = State.Id};  //todo update on state change
            idBox.KeyDown += (sender, args) =>
            {
                if (args.Key != Key.Enter) return;
                
                try
                {
                    MainWindow.Current.UndoManager.RegisterUndoItem(new StateIdChangeUndoItem(State));
                    State.Id = idBox.Text;
                }
                catch (ArgumentException)
                {
                    ErrorPopupHelper.Create($"StateID \"{idBox.Text}\" is not valid!");
                    idBox.Text = State.Id;
                }
            };
            var p = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            p.Children.Add(new TextBlock(){Text = "ID: "});
            p.Children.Add(idBox);
            stateValues.Children.Add(p);

            //dir count
            var dirCountBox = new ComboBox(); //todo update on state change
            foreach (DirCount dir in Enum.GetValues(typeof(DirCount)))
            {
                dirCountBox.Items.Add(dir);
            }
            dirCountBox.SelectedItem = State.Dirs;
            dirCountBox.SelectionChanged += (sender, args) =>
            {
                MainWindow.Current.UndoManager.RegisterUndoItem(new StateImageArraySizeChangeUndoItem(State));
                State.Dirs = (DirCount) dirCountBox.SelectedItem;
            };
            p = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            p.Children.Add(new TextBlock(){Text = "Dir count: "});
            p.Children.Add(dirCountBox);
            stateValues.Children.Add(p);
            

            //frame count
            var frameCountEditor = new IntegerUpDown()  //todo update on state change
            {
                Minimum = 1,
                Increment = 1,
                Value = State.Frames
            };
            frameCountEditor.ValueChanged += (sender, args) =>
            {
                var frames = frameCountEditor.Value;
                if (frames != null)
                {
                    MainWindow.Current.UndoManager.RegisterUndoItem(new StateImageArraySizeChangeUndoItem(State));
                    State.Frames = frames.Value;
                }
            };
            p = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            p.Children.Add(new TextBlock(){Text = "Frame count: "});
            p.Children.Add(frameCountEditor);
            stateValues.Children.Add(p);

            //loop
            var loopCountEditor = new IntegerUpDown()  //todo update on state change
            {
                Minimum = 0,
                Increment = 1,
                Value = State.Loop
            };
            var infiniteIndicator = new TextBlock()
            {
                Text = State.Loop == 0 ? "(Infinite)" : ""
            };
            loopCountEditor.ValueChanged += (sender, args) =>
            {
                infiniteIndicator.Text = loopCountEditor.Value == 0 ? "(Infinite)" : "";
                MainWindow.Current.UndoManager.RegisterUndoItem(new StateLoopChangeUndoItem(State));
                State.Loop = loopCountEditor.Value.Value;
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
            var rewindBox = new CheckBox()  //todo update on state change
            {
                IsChecked = State.Rewind
            };
            rewindBox.Click += (sender, args) =>
            {
                State.Rewind = rewindBox.IsChecked.Value;
            };
            p = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            p.Children.Add(new TextBlock(){Text = "Rewind: "});
            p.Children.Add(rewindBox);
            
            //todo movement flag editor
            //todo update on state change
            
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
                ScrollViewer scrollViewer = new ScrollViewer {HorizontalScrollBarVisibility = ScrollBarVisibility.Auto};
                StackPanel framePanel = new StackPanel();
                scrollViewer.Content = framePanel;

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
                    ImageSelectionButton frameButton = new ImageSelectionButton(this, d, f, $"Frame {f+1}", State.GetImage(d, f));
                    framePanel.Children.Add(frameButton);
                }
                b.Child = scrollViewer;
                dirPanel.Children.Add(b);
            }
        }

        public void SetImageIndex(int dir, int frame)
        {
            if (ImageEditor?.DirIndex == dir && ImageEditor?.FrameIndex == frame) return;
            
            ImageEditor = new ImageEditor(this, dir, frame);
        }

        private class ImageSelectionButton : LabeledImageButton
        {
            private readonly StateEditor _stateEditor;
            private readonly DmiEXImage _image;
            public readonly int DirIndex;
            public readonly int FrameIndex;
            public ImageSelectionButton(StateEditor stateEditor, int dirIndex, int frameIndex, string labeltext, DmiEXImage image) : base (BitmapUtils.Bitmap2BitmapImage(image.GetBitmap()), labeltext)
            {
                DirIndex = dirIndex;
                FrameIndex = frameIndex;
                _image = image;
                _stateEditor = stateEditor;
                
                Click += Clicked;
                image.ImageChanged += UpdateImage;
                stateEditor.ImageEditorChanged += UpdatePressed;
                
                UpdatePressed();
            }

            private void Clicked(object sender, EventArgs e) => _stateEditor.SetImageIndex(DirIndex, FrameIndex);

            private void UpdatePressed(object sender = null, EventArgs e = null) => SetPressed(_stateEditor.ImageEditor.DirIndex == DirIndex && _stateEditor.ImageEditor.FrameIndex == FrameIndex);

            private void UpdateImage(object sender = null, EventArgs e = null)
                => SetImage(BitmapUtils.Bitmap2BitmapImage(_image.GetBitmap()));
        }
    }
}
