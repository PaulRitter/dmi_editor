using System.Drawing;
using System.Windows.Media.Imaging;

namespace DMIEditor
{
    public static class TransparentBackgroundHelper
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
    }
}