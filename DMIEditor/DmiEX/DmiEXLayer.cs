﻿using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using DMI_Parser.Utils;

namespace DMIEditor
{
    public class DmiEXLayer : IComparable, ICloneable
    {
        public readonly Bitmap Bitmap;

        private int _index;
        private bool _visible = true;

        public event EventHandler IndexChanged;
        public event EventHandler VisibilityChanged;
        public event EventHandler ImageChanged;
        public event EventHandler Changed;

        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;
                VisibilityChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                IndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }


        public DmiEXLayer(Bitmap bitmap, int index)
        {
            Bitmap = bitmap;
            _index = index;
            IndexChanged += InvokeSomethingChanged;
            VisibilityChanged += InvokeSomethingChanged;
            ImageChanged += InvokeSomethingChanged;
        }

        private void InvokeSomethingChanged(object sender, EventArgs e) => Changed?.Invoke(this, EventArgs.Empty);

        public void setPixel(Point p, Color c)
        {
            if(c == getPixel(p)) return;
            Bitmap.SetPixel(p.X, p.Y, c);
            ImageChanged?.Invoke(this, EventArgs.Empty);
        }

        public Color getPixel(Point p) => Bitmap.GetPixel(p.X, p.Y);

        public BitmapImage toImage() => BitmapUtils.Bitmap2BitmapImage(Bitmap);

#nullable enable
        public int CompareTo(object? obj)
        {
            if (obj == null) return 1;

            if (obj is DmiEXLayer otherLayer)
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

        public object Clone() => new DmiEXLayer((Bitmap) Bitmap.Clone(), _index) {_visible = _visible};
    }
}