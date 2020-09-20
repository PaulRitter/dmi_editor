using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DMI_Parser;
using DMI_Parser.Extended;
using DMI_Parser.Utils;

namespace DMIEditor
{
    public partial class AnimationPreviewWindow : Window
    {
        private readonly DmiEXState _state;
        private int _dirIndex;
        private int _frameIndex;
        public AnimationPreviewWindow(DmiEXState state, int dirIndex = 0)
        {
            InitializeComponent();
            
            _state = state;
            _dirIndex = dirIndex;
            RenderOptions.SetBitmapScalingMode(image_viewer, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(image_background, BitmapScalingMode.NearestNeighbor);

            foreach (Dir d in Enum.GetValues(typeof(Dir)))
            {
                if ((int) d == (int) _state.Dirs) break;
                dir_selector.Items.Add(d);
            }

            dir_selector.SelectedIndex = 0;
            dir_selector.SelectionChanged += (o, e) =>
            {
                if (dir_selector.SelectedItem == null) return;
                _dirIndex = (int) dir_selector.SelectedItem;
                UpdateImage();
            };
            
            CreateBackground();
            StartAnimation();
            
            //todo attach CreateBackground to width/height change
        }

        private void CreateBackground(object sender = null, EventArgs e = null)
        {
            image_background.Source = BitmapUtils.Bitmap2BitmapImage(
                TransparentBackgroundHelper.CreateTransparentBackgroundMap(_state.Width * 2, _state.Height * 2));
        }

        private void StartAnimation()
        {
            Task.Factory.StartNew(()=>
            {
                while (true)
                {
                    UpdateImage();
                    Task.Delay((int) (_state.Delays[_frameIndex] * 100)).Wait();
                    NextImage();    
                }
            });
        }
        
        private void UpdateImage()
        {
            if (_dirIndex >= (int) _state.Dirs) _dirIndex = 0;
            if (_frameIndex >= _state.Frames) _frameIndex = 0;

            Bitmap bm = _state.GetBitmap(_dirIndex, _frameIndex);
            BitmapImage img = BitmapUtils.Bitmap2BitmapImage(bm);
            img.Freeze();
            image_viewer.Dispatcher.BeginInvoke(new Action(() =>
                image_viewer.Source = img));
        }

        private void NextImage()
        {
            if (_frameIndex + 1 == _state.Frames) _frameIndex = 0;
            else _frameIndex++;
        }
    }
}