using System.Drawing;
using System.Windows.Media.Imaging;

namespace DMIEditor
{
    public static class BitmapHelper
    {
        public static Bitmap CreateTransparentBackgroundMap(int width, int height)
        {
            var backgroundMap = new Bitmap(width, height);
            var s = true;
            for (var i = 0; i < backgroundMap.Width; i++)
            {
                for (var j = 0; j < backgroundMap.Height; j++)
                {
                    var c = s ? Color.LightGray : Color.White;
                    s = !s;
                    backgroundMap.SetPixel(i, j, c);
                }
                s = !s; //offsetting every row
            }

            return backgroundMap;
        }

        public static Bitmap CreateSelectionBox(bool up = false, bool right = false, bool down = false,
            bool left = false)
        {
            Bitmap bm = new Bitmap(8,8);
            if (up)
            {
                bool s = true;
                for (int i = 0; i < bm.Width; i++)
                {
                    var c = s ? Color.Black : Color.White;
                    bm.SetPixel(i, 0, c);
                    s = !s;
                }
            }
            
            if (right)
            {
                bool s = true;
                for (int i = 0; i < bm.Height; i++)
                {
                    var c = s ? Color.Black : Color.White;
                    bm.SetPixel(7, i, c);
                    s = !s;
                }
            }
            
            if (down)
            {
                bool s = true;
                for (int i = 0; i < bm.Width; i++)
                {
                    var c = s ? Color.Black : Color.White;
                    bm.SetPixel(i, 7, c);
                    s = !s;
                }
            }
            
            if (left)
            {
                bool s = true;
                for (int i = 0; i < bm.Height; i++)
                {
                    var c = s ? Color.Black : Color.White;
                    bm.SetPixel(0, i, c);
                    s = !s;
                }
            }

            return bm;
        }
    }
}