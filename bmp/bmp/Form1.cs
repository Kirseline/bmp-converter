using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace bmp
{
    public partial class Form1 : Form
    {
        Bitmap image1;
        Bitmap prew;
        public Form1()
        {
            InitializeComponent();
        }

        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.AssumeLinear;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.None;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private byte[] write_vbc(Bitmap img)
        {
            int bytex = (int)img.Width / 8;
            byte bx = 0;
            short bmp_dimension = (short)(bytex * image1.Height);

            int idx = 5; //data starting adress

            byte[] array = new byte[bmp_dimension + 5];
            array[0] = 0xA7;
            array[1] = (byte)img.Width;
            array[2] = (byte)img.Height;
            array[3] = (byte)(bmp_dimension >> 8);
            array[4] = (byte)bmp_dimension;

            for (int y = 0; y < image1.Height; y++)
            {
                for (int x = 0; x < bytex; x++)
                {
                    array[idx] = 0x00;

                    for (int i = 0; i < 8; i++)
                    {
                        Color pixelColor = image1.GetPixel(i + (8 * x), y);
                        Color white = Color.FromArgb(255, 255, 255);

                        if (!pixelColor.Equals(white))
                        {
                            image1.SetPixel(x, y, Color.FromArgb(0, 255, 0));
                            bx |= (byte)(0x80 >> i);
                        }
                        else
                        {
                            image1.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                            bx &= (byte)~(0x80 >> i);
                        }
                    }
                    array[idx++] = bx;
                }
            }

            return array;




            }
         
        private void button1_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "bmp (*.bmp)|*.bmp|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }
            }

            try
            {
                // Retrieve the image.
                image1 = new Bitmap(filePath, true);
                prew = new Bitmap(image1);

                int x, y;

                // Loop through the images pixels to reset color.

                for (y = 0; y < image1.Height; y++)
                {

                        for ( x = 0; x < image1.Width; x++)
                        {
                            Color pixelColor = image1.GetPixel(x, y);
                            Color white = Color.FromArgb(255, 255, 255);

                            if (!pixelColor.Equals(white))
                                prew.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                            else
                                prew.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                        }

                }

                // Set the PictureBox to display the image.
                pictureBox1.Image = ResizeImage(prew, prew.Width*2, prew.Height * 2);

                // Display the pixel format in Label1.
                //Label1.Text = "Pixel format: " + image1.PixelFormat.ToString();
            }
            catch (ArgumentException)
            {
                MessageBox.Show("There was an error." +
                    "Check the path to the image file.");
            }

           // System.Diagnostics.Debug.WriteLine();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (System.Windows.Forms.Application.MessageLoop)
            {
                // WinForms app
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                // Console app
                System.Environment.Exit(1);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (image1 != null)
            {
                byte[] vbc = write_vbc(image1);
                SaveFileDialog savefile = new SaveFileDialog();
                savefile.RestoreDirectory = true;
                savefile.InitialDirectory = "c:\\";
                // savefile.FileName = String.Format("{0}.vbc", txtidstruk.Text);
                savefile.DefaultExt = "*.vbc*";
                savefile.Filter = "vbc file|*.vbc";

                if (savefile.ShowDialog() == DialogResult.OK)
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(savefile.FileName))
                        for(int i = 0; i < vbc.Length;i++)
                            sw.Write(vbc[i]);
                }
            }

            else
            {
                    MessageBox.Show("Nothing to save" + "\nOpen a file before saving");
            }


        }
    }
}


