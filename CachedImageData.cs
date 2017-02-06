using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace MosaicOnDemand
{

    //These are made en masse during ProcessSourceImages()
    //Each one represents a source image. Doing it this way saves us from holding them all in RAM
   public  class CachedImageData
    {
        public string Name;                     //File location of the image, saves holdin the whole thing in RAM
       
        public Color DominantColor;             //The dominant color of the image after running GetDominantColor()
    }
}
