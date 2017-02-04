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
        public Bitmap MainImage;
        public Bitmap OutputImage;
        public List<BitmapPixel> MainImageBitmapPixels = new List<BitmapPixel>();
        public List<CachedImageData> CachedImageDatas = new List<CachedImageData>();


        public Form1()
        {

            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {


            Bitmap bmp = (Bitmap)Image.FromFile(textBox1.Text);


            // bmp.Save("test.bmp");

            label2.Text = "Main Image dimensions: " + bmp.Width.ToString() + " x " + bmp.Height.ToString();

            PushImageToImagePanel(bmp);

            MainImage = ResizeImage(bmp, 100,90);
            MainImage.Save("test.bmp");

            ProcessMainImage();

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


        public void ProcessMainImage()
        {

            for (int x = 0; x < MainImage.Width; x++)
            {
                for (int y = 0; y < MainImage.Height; y++)
                {
                    BitmapPixel bmpp = new BitmapPixel();
                    MainImageBitmapPixels.Add(bmpp);

                    bmpp.X = x;
                    bmpp.Y = y;
                    bmpp.MyColor = MainImage.GetPixel(x, y);
                   // this.BackColor = bmpp.MyColor;
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

            FindAllBestCachedImages();
        }

        public void FindBestCachedImageForPixel(BitmapPixel bmpp)
        {

            CachedImageData ClosestImage = CachedImageDatas[1];
            int BestScore = 10000;

            foreach(CachedImageData cachedImage in CachedImageDatas)
            {
                int curScore = ColorComparison(bmpp.MyColor, cachedImage.DominantColor);

                if(curScore < BestScore)
                {
                    BestScore = curScore;
                    bmpp.ClosestCachedImage = cachedImage;
                }
            }
        }

        public void FindAllBestCachedImages()
        {
            int n = 0;
            foreach(BitmapPixel  bmpp in MainImageBitmapPixels)
            {
                FindBestCachedImageForPixel(bmpp);
                n++;
                label5.Text = n.ToString();
            }

            CreateBigImage();
        }
        public int ColorComparison(Color ca, Color cb)
        {
            int score = 0;
            
            int  r =  Math.Abs(ca.R - cb.R);
            int  b = Math.Abs(ca.B - cb.B);
            int  g = Math.Abs(ca.G - cb.G);

            score = r + b + g;


            return score;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FindAllBestCachedImages();

           
        }


        public void CreateBigImage()
        {
            int xScale = 100;
            int yScale = 100;

            int xSize = xScale * MainImage.Width;
            int ySize = yScale * MainImage.Height;
            OutputImage = new Bitmap(xSize, ySize);



            foreach (BitmapPixel bmpp in MainImageBitmapPixels)
            {
                int localX = 0;
                int localY = 0;
                int startX = bmpp.X * xScale;
                int startY = bmpp.Y * yScale;
             //   if (bmpp.X < 50 && bmpp.Y < 50)
            //    {
                    //////////////////
                    using (Bitmap bmp = (Bitmap)Image.FromFile(bmpp.ClosestCachedImage.Name))

                    using (Bitmap currentPic = ResizeImage(bmp, xScale, yScale))
                    {

                        for (int x = 0; x < xScale; x++)
                        {
                            for (int y = 0; y < yScale; y++)
                            {
                                localX = startX + x;
                                localY = startY + y;


                                Color c = currentPic.GetPixel(x, y);

                                OutputImage.SetPixel(localX, localY, c);

                            }
                        }
                    }
                }
          //  }

            OutputImage.Save("output.bmp");


        }

        private void button4_Click(object sender, EventArgs e)
        {
            CreateBigImage();
        }
    }
}


