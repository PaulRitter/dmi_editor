using System;
using System.Drawing;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageProcessor;
using Image = System.Windows.Controls.Image;

namespace DMIEditor
{
    public class LabeledImageButton : Button
    {
        private bool _pressed = false;
        protected Image Img;
        protected TextBlock Label;
        private StackPanel _stackPanel;

        public LabeledImageButton(BitmapImage bmI, string labeltext)
        {
            
            //create stackpanel
            _stackPanel = new StackPanel();

            //create image
            Img = new Image
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Stretch = Stretch.None
            };
            RenderOptions.SetBitmapScalingMode(Img, BitmapScalingMode.NearestNeighbor);

            SetImage(bmI);

                //add to stackpanel
            _stackPanel.Children.Add(Img);

            //create label
            Label = new TextBlock
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center, Text = labeltext
            };
            //add to stackpanel
            _stackPanel.Children.Add(Label);

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

        protected void SetImage(BitmapImage bitmap)
        {
            Img.Source = bitmap;
            //bitmap.Freeze();
            //Img.Dispatcher.BeginInvoke(new Action(() => Img.Source = bitmap));
        }
    }
}