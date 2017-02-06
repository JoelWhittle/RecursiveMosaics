using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MosaicOnDemand
{
    public partial class Form1 : Form
    {
        public Bitmap MainImage;                                                          //Saves the resized image that is to be recreated

        public Bitmap OutputImage;                                                        //The outputted color image resulting from the CreateBigImage method.
                                                                                          //Holding a global var for it saves us a headache when the user decides
                                                                                          //they dont like the sepia filter they applied and wants to revert back

        public List<BitmapPixel> MainImageBitmapPixels = new List<BitmapPixel>();         //List of BitmapPixels which is filled during ProcessMainImage()   
        public List<CachedImageData> CachedImageDatas = new List<CachedImageData>();      //Filled during ProcessSourceImages()   

        public  _Orientations SelectedOrientation = _Orientations.Freemode;               //Should be assigned in the GUI and passed when processing MainImage() or CreateBigImage()
        public int SmallImageScale = 100;


        public Form1()
        {

            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {


           

            ProcessMainImage((Bitmap)Image.FromFile(textBox1.Text),SelectedOrientation);

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public void PushImageToImagePanel(Bitmap bmp)
        {
            pictureBox1.Image = bmp;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }


        public Color GetDominantColor(Bitmap bmp)
        {

            //Used for tally
            int r = 0;
            int g = 0;
            int b = 0;

            int total = 0;

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color clr = bmp.GetPixel(x, y);

                    r += clr.R;
                    g += clr.G;
                    b += clr.B;

                    total++;
                }
            }

            //Calculate average
            r /= total;
            g /= total;
            b /= total;


            Color domColour = Color.FromArgb(r, g, b);
            return domColour;

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            String searchFolder = textBox2.Text;
            var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            var files = GetFilesFrom(searchFolder, filters, false);

            PreProcessSourceFiles(files);
        }

        public String[] GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
        {
            List<String> filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
            }

            label4.Text = "Files found: " + filesFound.Count.ToString();
            return filesFound.ToArray();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }



        public Bitmap ResizeImage(Image image, int width, int height)
        {
            Rectangle destRect = new Rectangle(0, 0, width, height);

            Bitmap destImage = new Bitmap(width, height);



            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            var graphics = Graphics.FromImage(destImage);
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (ImageAttributes wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }


            return destImage;
        }


        public void ProcessMainImage(Bitmap bmp,  _Orientations Orientation)
        {

            

            label2.Text = "Main Image dimensions: " + bmp.Width.ToString() + " x " + bmp.Height.ToString();

            PushImageToImagePanel(bmp);

            //Resize the image according to the recommended Orientation dimensions divided by the smaller
            //pictures size
            Vector2 v = ReturnRecommendedDimensions(Orientation);

            MainImage = ResizeImage(bmp, (int)v.x / SmallImageScale, (int)v.y / SmallImageScale);


        
            //Save the image for testing purposes
            MainImage.Save("test.bmp");

            for (int x = 0; x < MainImage.Width; x++)
            {
                for (int y = 0; y < MainImage.Height; y++)
                {
                    BitmapPixel bmpp = new BitmapPixel();
                    MainImageBitmapPixels.Add(bmpp);

                    bmpp.X = x;
                    bmpp.Y = y;
                    bmpp.MyColor = MainImage.GetPixel(x, y);
                }
            }
        }

        public void PreProcessSourceFiles(String[] files)
        {
            /////////////
            foreach (string File in files)
            {
                CachedImageData cid = new CachedImageData();
                using (Bitmap bmp = (Bitmap)Image.FromFile(File))


                using (Bitmap TempImage = ResizeImage(bmp, 10, 10))
                {

                    Color TempColor = GetDominantColor(TempImage);


                    cid.Name = File;
                    cid.DominantColor = TempColor;

                    CachedImageDatas.Add(cid);
                }



            }

            FindAllBestCachedImages(MainImageBitmapPixels, CachedImageDatas);
        }


        
        public void FindBestCachedImageForPixel(BitmapPixel bmpp, List<CachedImageData> cachedImages)
        {

            CachedImageData ClosestImage = cachedImages[1];
            int BestScore = 10000;

            foreach(CachedImageData cachedImage in cachedImages)
            {
                int curScore = ColorComparison(bmpp.MyColor, cachedImage.DominantColor);

                if(curScore < BestScore)
                {
                    BestScore = curScore;
                    bmpp.ClosestCachedImage = cachedImage;
                }
            }
        }

        //Runs FindBestCachedImages a list of Bitmap pixels
        public void FindAllBestCachedImages(List<BitmapPixel> bitmapPixels, List<CachedImageData> cachedImages)
        {
            foreach(BitmapPixel  bmpp in MainImageBitmapPixels)
            {
                FindBestCachedImageForPixel(bmpp, cachedImages);
              
            }

            CreateBigImage("output.bmp", SmallImageScale, MainImageBitmapPixels);
        }
        //Returns an int which gives a score on the difference between 2 colors.
        // a score of 0 indicates a perfect match, whilst 200 is way out
        public int ColorComparison(Color ca, Color cb)
        {
            //Takes the difference between the 2 colors R channel. G channel and B channel and returns the sum
            int score = 0;
            
            int  r =  Math.Abs(ca.R - cb.R);
            int  b = Math.Abs(ca.B - cb.B);
            int  g = Math.Abs(ca.G - cb.G);

            score = r + b + g;


            return score;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FindAllBestCachedImages(MainImageBitmapPixels, CachedImageDatas);

           
        }


        //Requires the list of BitmapPixel objects and a scale, from that it will produce a colorised mosaic
        //image
        public void CreateBigImage(string OutputName, int Scale, List<BitmapPixel> BmpPixels)
        {
            
            //First create the new bitmap according to Scale * the low res main image
            //this should give us a size closer to  a 10mega pixel camera
            int xSize = Scale * MainImage.Width;
            int ySize = Scale * MainImage.Height;
            OutputImage = new Bitmap(xSize, ySize);


            //Roll through each BitmapPixel and stitch the bitmap together
            foreach (BitmapPixel bmpp in BmpPixels)
            {
                int localX = 0;
                int localY = 0;
                int startX = bmpp.X * Scale;
                int startY = bmpp.Y * Scale;
         
                    using (Bitmap bmp = (Bitmap)Image.FromFile(bmpp.ClosestCachedImage.Name))

                    using (Bitmap currentPic = ResizeImage(bmp, Scale, Scale))
                    {

                        for (int x = 0; x < Scale; x++)
                        {
                            for (int y = 0; y < Scale; y++)
                            {
                                localX = startX + x;
                                localY = startY + y;


                                Color c = currentPic.GetPixel(x, y);

                                OutputImage.SetPixel(localX, localY, c);

                            }
                        }
                    }
                }
      
            //Save the output image
            OutputImage.Save(OutputName);


        }

        private void button4_Click(object sender, EventArgs e)
        {
            CreateBigImage("output.bmp", SmallImageScale, MainImageBitmapPixels);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ConvertToSepia(OutputImage);
        }

        //Converts an image to Greyscale by cycling through each pixel and setting it to the
        //average of the sum of its R, G and B channels. Returns the new Bitmap.
        public Bitmap ConvertToGreyscale(Bitmap bmp)
        {

                Bitmap greyscaleBmp = new Bitmap(bmp.Width,bmp.Height);

                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {

                    Color c = bmp.GetPixel(x, y);
                    int n = (c.R + c.G + c.B) / 3;

                    Color newC = new Color();
                    newC = Color.FromArgb(n, n, n);

                    greyscaleBmp.SetPixel(x, y, newC);

                    }
                }
            
            greyscaleBmp.Save("greyscale.bmp");
            return greyscaleBmp;
        }

        //Converts an image to Sepia by cycling through each pixel and setting it to a 
        //(recommended by Microsoft) weighted sum of each of channels. Returns the new Bitmap
    
        public Bitmap ConvertToSepia(Bitmap bmp)
        {

            Bitmap sepiaBmp = new Bitmap(bmp.Width, bmp.Height);

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {

                    Color c = bmp.GetPixel(x, y);

                    int inputRed = c.R;
                    int inputGreen = c.G;
                    int inputBlue = c.B;

                    int outputRed = Convert.ToInt32((inputRed * .393f) + (inputGreen * .769f) + (inputBlue * .189f)) ;
                    int outputGreen = Convert.ToInt32((inputRed * .349) + (inputGreen * .686) + (inputBlue * .168));
                    int outputBlue = Convert.ToInt32((inputRed * .272) + (inputGreen * .534) + (inputBlue * .131));
                  
                        
                      if(outputRed > 255)
                    {
                        outputRed = 255;
                    }
                    if (outputBlue > 255)
                    {
                        outputBlue = 255;
                    }
                    if (outputGreen > 255)
                    {
                        outputGreen = 255;
                    }

                    Color newC = new Color();
                    newC = Color.FromArgb(outputRed, outputGreen, outputBlue);

                    sepiaBmp.SetPixel(x, y, newC);

                }
            }

            sepiaBmp.Save("sepia.bmp");
            return sepiaBmp;

        }

        //Different Orientation Options, pass an Orientation to the ReturnRecommendedDimensions class to
        //recieve a Vector2(x,y) to be passed to any ResizeImage calls
        public enum _Orientations
        {
            Freemode,             //Returns user submitted vector2, prone to huge file sizes and possible crashes
            Landscape,           
            Portrait
        }


        //Pass an Orientation and it will return to you a Vector2 containing the dimensions according to
        //a 10 mega pixel camera
        public Vector2 ReturnRecommendedDimensions(_Orientations orientation)
        {
            Vector2 v = new Vector2();
            switch (orientation)
            {
                case _Orientations.Freemode:

                    v.x = 10000;
                    v.y = 9000;
                    break;
                    case _Orientations.Portrait:

                    v.y = 3872;
                    v.x = 2592;
                    break;
                    case _Orientations.Landscape:

                    v.x = 3872;
                    v.y = 2592;
                    break;

            }
            return v;


        }
    }



 
}


