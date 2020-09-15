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
            
            //subscribe to state events
            State.ImageArrayChanged += CreateImageButtons;

            CreateImageButtons();
            
            //TODO delays (maybe add into frame selection)
        }

        private void CreateImageButtons(object sender = null, EventArgs e = null)
        {
            frameSelectionGrid.Children.Clear();
            
            //set columns (dirs) and rows (frames)
            frameSelectionGrid.RowDefinitions.Clear();
            for (int i = 0; i < State.Frames + 1; i++) // + 1 for the header
            {
                frameSelectionGrid.RowDefinitions.Add(new RowDefinition{ Height = GridLength.Auto });
            }
            
            frameSelectionGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < (int)State.Dirs + 1; i++) // + 1 for delay-editor column
            {
                //set the column definition
                frameSelectionGrid.ColumnDefinitions.Add(new ColumnDefinition{ Width = GridLength.Auto });
                
                if(i == 0) continue;

                var actual_index = i - 1;
                //create the column header
                StackPanel p = new StackPanel{Orientation = Orientation.Horizontal};
                TextBlock header = new TextBlock
                {
                    Text = actual_index switch
                    {
                        0 => "SOUTH",
                        1 => "NORTH",
                        2 => "EAST",
                        3 => "WEST",
                        4 => "SOUTHEAST",
                        5 => "SOUTHWEST",
                        6 => "NORTHEAST",
                        7 => "NORTHWEST",
                        _ => "ERROR"
                    },
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                p.Children.Add(header);
                
                Button previewBtn = new Button{ Content = "Preview" };
                previewBtn.Click += (o, e) =>
                {
                    AnimationPreviewWindow w = new AnimationPreviewWindow(State, actual_index);
                    w.Show();
                };
                p.Children.Add(previewBtn);
                frameSelectionGrid.Children.Add(p);
                Grid.SetRow(p, 0);
                Grid.SetColumn(p, i);
            }
            
            //create delay editors
            for (int i = 0; i < State.Frames; i++)
            {
                DelayEditorPanel editorPanel = new DelayEditorPanel(State, i);
                
                frameSelectionGrid.Children.Add(editorPanel);
                Grid.SetRow(editorPanel, i+1);
                Grid.SetColumn(editorPanel, 0);
            }
            
            //populate grid
            for (int dir = 0; dir < (int)State.Dirs; dir++)
            {
                for (int frame = 0; frame < State.Frames; frame++)
                {
                    ImageSelectionButton frameButton = new ImageSelectionButton(this, dir, frame, "", State.GetImage(dir, frame));
                    frameSelectionGrid.Children.Add(frameButton);
                    Grid.SetRow(frameButton, frame+1);
                    Grid.SetColumn(frameButton, dir+1);
                }
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

        private class DelayEditorPanel : StackPanel
        {
            private DmiEXState _dmiExState;
            private DoubleUpDown _delayEditor;
            private int _frameIndex;
            public DelayEditorPanel(DmiEXState dmiExState, int frameIndex)
            {
                _dmiExState = dmiExState;
                _frameIndex = frameIndex;
                Orientation = Orientation.Vertical;

                Children.Add(new TextBlock
                {
                    Text = $"Frame {frameIndex + 1}",
                    HorizontalAlignment = HorizontalAlignment.Center
                });

                if (dmiExState.Delays == null) return;
                
                _delayEditor = new DoubleUpDown
                {
                    Minimum = 0.01d,
                    Increment = 0.1d,
                    Value = dmiExState.Delays[frameIndex],
                    Width = 50,
                    AllowSpin = false,
                    ShowButtonSpinner = false
                };
                _delayEditor.KeyDown += OnDelayEditorKeyDown;
                _delayEditor.LostFocus += (o, e) =>
                {
                    _delayEditor.Value = _dmiExState.Delays[_frameIndex];
                };
                dmiExState.DelayChanged += OnDelayChanged;
                Children.Add(_delayEditor);

                TextBlock secondsIndicator = new TextBlock
                {
                    Text = $"=> {(dmiExState.Delays[frameIndex]*0.1):0.##}s"
                };
                _delayEditor.ValueChanged += (o, e) =>
                {
                    if (!_delayEditor.Value.HasValue) return;
                    
                    secondsIndicator.Text = $"=> {(_delayEditor.Value.Value * 0.1):0.##}s";
                };
                Children.Add(secondsIndicator);
            }

            private void OnDelayEditorKeyDown(object sender, KeyEventArgs e)
            {
                if (e.Key != Key.Enter) return;

                if (!_delayEditor.Value.HasValue) return;
                
                MainWindow.Current.UndoManager.RegisterUndoItem(new StateDelayChangeUndoItem(_dmiExState, _frameIndex));
                _dmiExState.SetDelay(_frameIndex, _delayEditor.Value.Value);
            }

            private void OnDelayChanged(object sender, DelayChangedEventArgs e)
            {
                if(e.ChangedIndex != _frameIndex) return;

                _delayEditor.Value = _dmiExState.Delays[_frameIndex];
            }
        }
    }
}
