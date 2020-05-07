using System;
using System.Drawing;

namespace DMIEditor
{
    public class Layer : IComparable
    {
        public readonly Bitmap Bitmap;

        private int _index;
        public int Index
        {
            get { return _index; }
            // set
            // {
            //     
            // }
        }

        public Layer(Bitmap bitmap, int index)
        {
            Bitmap = bitmap;
            _index = index;
        }


        public int CompareTo(object? obj)
        {
            if (obj == null) return 1;

            if (obj is Layer otherLayer)
            {
                if (otherLayer.Index < this.Index)
                {
                    return -1;
                }
                
                if (otherLayer.Index > this.Index)
                {
                    return 1;
                }

                return 0;
            }
            else
                throw new ArgumentException("Object is not a Layer");
        }
    }
}