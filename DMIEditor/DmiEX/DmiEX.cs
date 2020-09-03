using System;
using System.Collections.Generic;
using System.Drawing;
using DMI_Parser;
using DMI_Parser.Raw;

namespace DMIEditor.DmiEX
{
    public class DmiEX : Dmi
    {
        public DmiEX(float version, int width, int height) : base(version, width, height) {}

        public override ICloneable CreateEmptyImage()
        {
            return new DmiEXImage(Width, Height);
        }

        public static DmiEX FromDmi(string path) => FromDmi(Dmi.FromFile(path));
        
        public static DmiEX FromDmi(Dmi dmi)
        {
            DmiEX dmiEx = new DmiEX(dmi.Version, dmi.Width, dmi.Height);
            
            foreach (var state in dmi.States)
            {
                dmiEx.addState(DmiEXState.FromDmiState(dmiEx, state)); //todo change
            }

            return dmiEx;
        }

        public override void createNewState(string name)
        {
            RawDmiState raw = RawDmiState.Default(name);

            DmiEXImage[,] images = new DmiEXImage[1, 1];
            images[0,0] = (DmiEXImage) CreateEmptyImage();
            
            DmiEXState dmiState = new DmiEXState(this, images, raw);
            addState(dmiState);
        }
    }
}