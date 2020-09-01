﻿using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using DMI_Parser;
using DMI_Parser.Raw;

namespace DMIEditor.DmiEX
{
    public class DmiEXState : DMIState
    {
        public static DmiEXState FromDmiState(DMIEditor.DmiEX.DmiEX parent, DMIState dmiState)
        {
            RawDmiState raw = dmiState.toRaw();
            
            DmiEXImage[,] images = new DmiEXImage[(int)raw.Dirs.Value,raw.Frames.Value];
            for (int dir = 0; dir < (int)raw.Dirs.Value; dir++)
            {
                for (int frame = 0; frame < raw.Frames.Value; frame++)
                {
                    images[dir, frame] = new DmiEXImage(dmiState.Images[dir, frame]);
                }
            }
            
            return new DmiEXState(parent, images, raw);
        }
        
        public new DmiEXImage[,] Images { get; private set; }
        
        public DmiEXState(Dmi parent, DmiEXImage[,] images, RawDmiState rawDmiState) : base(parent, null, rawDmiState)
        {
            Images = images;
        }

        public override BitmapImage getImage(int dir, int frame)
        {
            return Images[dir, frame].GetImage();
        }

        public override Bitmap getBitmap(int dir, int frame)
        {
            return Images[dir, frame].GetBitmap();
        }

        protected override void clearImageArray(int dirs, int frames)
        {
            Images = new DmiEXImage[dirs,frames];
        }

        protected override ICloneable[,] getOldImagesForArrayResize() => Images;

        public override int getImageCount() => Images.Length;

        protected override void addImage(int dir, int frame, object img)
        {
            Images[dir, frame] = (DmiEXImage) img;
        }
    }
}