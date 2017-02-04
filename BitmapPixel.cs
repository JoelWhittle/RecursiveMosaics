using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace MosaicOnDemand
{
    public  class BitmapPixel
    {
        public int X;
        public int Y;

        public Color MyColor;

        public CachedImageData ClosestCachedImage;
    }
}
