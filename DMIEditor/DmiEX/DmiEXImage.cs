using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;
using DMI_Parser.Utils;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;

namespace DMIEditor.DmiEX
{
    public class DmiEXImage : ICloneable
    {
        private List<DmiEXLayer> _layers = new List<DmiEXLayer>();
        public int Width { get; private set; }
        public int Height { get; private set; }
        public event EventHandler ImageChanged;
        public event EventHandler LayerListChanged;


        public DmiEXImage(int width, int height) : this(new Bitmap(width,height)) {}

        public DmiEXImage(Bitmap bm)
        {
            Width = bm.Width;
            Height = bm.Height;
            AddLayer(new DmiEXLayer(bm, 0));
            ImageChanged += (sender, e) => _bufferedImage = null;
        }

        public void SetLayerIndex(DmiEXLayer layer, int index)
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

        public void AddLayer(DmiEXLayer l)
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
            SortLayers();
            l.IndexChanged += SortLayers;
            
            l.Changed += (sender, e) => ImageChanged?.Invoke(this, EventArgs.Empty); //any change on the layer means a change on the image
            LayerListChanged?.Invoke(this, EventArgs.Empty);
        }
        
        public void AddLayer(int index) => AddLayer(new DmiEXLayer(new Bitmap(Width, Height), index));

        public void RemoveLayer(int index)
        {
            DmiEXLayer l = GetLayerByIndex(index);
            _layers.Remove(l);
            LayerListChanged?.Invoke(this, EventArgs.Empty);
        }
        
        private void SortLayers(object sender = null, EventArgs e = null)
            => _layers.Sort((l1,l2)=>l1.Index.CompareTo(l2.Index));

        public DmiEXLayer[] GetLayers() => _layers.ToArray();

        public DmiEXLayer GetLayerByIndex(int index)
        {
            DmiEXLayer layer = _layers.Find((l) => l.Index == index);
            if(layer == null) throw new ArgumentException("No Layer with that Index exists");
            return layer;
        }

        private BitmapImage _bufferedImage;
        public BitmapImage GetImage()
        {
            if (_bufferedImage != null) return _bufferedImage;
            
            ImageFactory imgF = new ImageFactory();
            bool first = true;

            SortLayers();
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

            _bufferedImage = BitmapUtils.ImageFactory2BitmapImage(imgF);
            return _bufferedImage;
        }

        public object Clone()
        {
            DmiEXImage image = new DmiEXImage(Width, Height);
            image._layers = new List<DmiEXLayer>();
            foreach (var layer in _layers)
            {
                image.AddLayer((DmiEXLayer)layer.Clone());
            }

            return image;
        }
    }
}