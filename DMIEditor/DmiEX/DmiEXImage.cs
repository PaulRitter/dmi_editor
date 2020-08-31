using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using DMI_Parser.Utils;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;

namespace DMIEditor
{
    public class DmiEXImage : ICloneable
    {
        private List<DmiEXLayer> _layers = new List<DmiEXLayer>();
        public int Width { get; private set; }
        public int Height { get; private set; }
        public event EventHandler ImageChanged;
        public event EventHandler LayerListChanged;


        public DmiEXImage(int width, int height)
        {
            Width = width;
            Height = height;
            addLayer(new DmiEXLayer(new Bitmap(width,height), 0));
        }

        public DmiEXImage(Bitmap bm)
        {
            Width = bm.Width;
            Height = bm.Height;
            addLayer(new DmiEXLayer(bm, 0));
        }

        public void setLayerIndex(DmiEXLayer layer, int index)
        {
            layer.Index = index;
            int duplicateIndex = index;
            DmiEXLayer[] layers = _layers.ToArray();
            for (int i = 0; i < layers.Length; i++)
            {
                DmiEXLayer l = layers[i];
                if (l.Index != duplicateIndex || l == layer) continue;
                l.Index++;
                duplicateIndex++;
            }
        }

        public void addLayer(DmiEXLayer l)
        {
            //moving all layers in the way one step up
            int increaseAtIndex = l.Index;
            DmiEXLayer[] layers = _layers.ToArray();
            for (int i = 0; i < layers.Length; i++)
            {
                DmiEXLayer layer = layers[i];
                if (layer.Index != increaseAtIndex) continue;
                layer.Index++;
                increaseAtIndex++;
            }
            
            _layers.Add(l);
            sortLayers();
            l.IndexChanged += sortLayers;
            
            l.Changed += (sender, e) => ImageChanged?.Invoke(this, EventArgs.Empty); //any change on the layer means a change on the image
            LayerListChanged?.Invoke(this, EventArgs.Empty);
        }
        
        public void addLayer(int index) => addLayer(new DmiEXLayer(new Bitmap(Width, Height), index));

        public void removeLayer(int index)
        {
            DmiEXLayer l = getLayerByIndex(index);
            _layers.Remove(l);
        }
        
        private void sortLayers(object sender = null, EventArgs e = null)
            => _layers.Sort((l1,l2)=>l1.Index.CompareTo(l2.Index));

        public DmiEXLayer[] getLayers() => _layers.ToArray();

        public DmiEXLayer getLayerByIndex(int index)
        {
            DmiEXLayer layer = _layers.Find((l) => l.Index == index);
            if(layer == null) throw new ArgumentException("No Layer with that Index exists");
            return layer;
        }
        
        public BitmapImage getImage()
        {
            ImageFactory imgF = new ImageFactory();
            bool first = true;

            sortLayers();
            for (int i = 0; i < _layers.Count; i++)
            {
                DmiEXLayer dmiExLayer = _layers[i];
                if (!dmiExLayer.Visible) continue;
                if (first)
                {
                    imgF.Load(dmiExLayer.Bitmap);
                    first = false;
                    continue;
                }

                ImageLayer l = new ImageLayer();
                l.Image = dmiExLayer.Bitmap;
                imgF.Overlay(l);
            }

            imgF.Resolution(Width, Height)
                .Format(new PngFormat())
                .BackgroundColor(Color.Transparent);

            return BitmapUtils.ImageFactory2BitmapImage(imgF);
        }

        public object Clone()
        {
            DmiEXImage image = new DmiEXImage(Width, Height);
            image._layers = new List<DmiEXLayer>();
            foreach (var layer in _layers)
            {
                image.addLayer((DmiEXLayer)layer.Clone());
            }

            return image;
        }
    }
}