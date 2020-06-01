using System;
using System.Drawing;

namespace DMIEditor
{
    public class Layer : IComparable
    {
        public readonly Bitmap Bitmap;

        private int _index;
        private bool _visible = true;

        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler Changed;

        public Layer(Bitmap bitmap, int index)
        {
            Bitmap = bitmap;
            _index = index;
        }

        #nullable enable
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