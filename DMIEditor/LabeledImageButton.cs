using System;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace DMIEditor
{
    public class LabeledImageButton : Button
    {
        private bool _pressed = false;
        protected Image img;
        protected TextBlock label;
        private StackPanel _stackPanel;
        public LabeledImageButton(Bitmap bm, string labeltext)
        {
            //create stackpanel
            _stackPanel = new StackPanel();

            //create image
            img = new Image
            {
                Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bm.GetHbitmap(),
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bm.Width, bm.Height)),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Stretch = Stretch.None
            };
            //add to stackpanel
            _stackPanel.Children.Add(img);

            //create label
            label = new TextBlock
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center, Text = labeltext
            };
            //add to stackpanel
            _stackPanel.Children.Add(label);

            //add stackpanel to btn
            Content = _stackPanel;

            //layout stuff
            Margin = new System.Windows.Thickness(3);
            SetPressed(false);
        }

        public void SetPressed(bool pressed)
        {
            this._pressed = pressed;
            Background = pressed ? System.Windows.Media.Brushes.LightBlue : System.Windows.Media.Brushes.LightGray;
        }

        public void SetHalfPressed()
        {
            this._pressed = false;
            Background = System.Windows.Media.Brushes.LightGreen;
        }

        public bool isPressed()
        {
            return _pressed;
        }
    }
}