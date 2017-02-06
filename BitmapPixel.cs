using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace MosaicOnDemand
{

    //BitmapPixel represents each pixel from the resized main image,
    //and are generated en masse during the ProcessMainImage()
    public  class BitmapPixel
    {
        public int X;                                          //x position in the MainImage bitmap
         
        public int Y;                                          //y position in the MainImage bitmap                                         

        public Color MyColor;                                  // the color of MainImage.GetPixel(x,y)

        public CachedImageData ClosestCachedImage;            // a reference to the closest match from the source images
    }                                                         // assigned from FindClosestCachedImage()
}
