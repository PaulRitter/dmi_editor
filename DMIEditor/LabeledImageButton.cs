using System;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DMIEditor
{
    public class LabeledImageButton : Button
    {
        private bool _pressed = false;

        public LabeledImageButton(Bitmap bm, string labeltext)
        {
            //create stackpanel
            StackPanel stackPanel = new StackPanel();

            //create image
            System.Windows.Controls.Image img = new System.Windows.Controls.Image
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
            stackPanel.Children.Add(img);

            //create label
            TextBlock label = new TextBlock
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center, Text = labeltext
            };
            //add to stackpanel
            stackPanel.Children.Add(label);

            //add stackpanel to btn
            Content = stackPanel;

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