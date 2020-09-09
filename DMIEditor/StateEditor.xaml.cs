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
            State.ImageArrayChanged += OnImageArrayChanged;

            CreateImageButtons();
            
            //TODO delays (maybe add into frame selection)
        }

        private void OnImageArrayChanged(object sender, EventArgs e)
        {
            CreateImageButtons();
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
