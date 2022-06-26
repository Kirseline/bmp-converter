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
            int bytex = (int)img.Width / 8 + 1;
            byte bx = 0;
            UInt16 bmp_dimension = (UInt16)((bytex * img.Height) + 5);
            Color white = Color.FromArgb(255, 255, 255);
           
            int x = 0;
            int idx = 5; //data starting adress
            int byte_completition = 0;


            byte[] array = new byte[bmp_dimension];
            array[0] = 0xA7;
            array[1] = (byte)img.Width;
            array[2] = (byte)img.Height;
            array[3] = (byte)(bmp_dimension >> 8);
            array[4] = (byte)(bmp_dimension & 0x00ff);


            for (int y = 0; y < img.Height; y++)
            {
                
                do
                {
                    if (byte_completition == 8)
                    {
                        array[idx++] = bx;
                        bx = 0x00;
                        byte_completition = 0;
                    }

                    Color pixelColor = img.GetPixel(x, y);

                    if (!pixelColor.Equals(white))
                        bx |= (byte)(0x80 >> x % 8);
                    else
                        bx &= (byte)~(0x80 >> x % 8);

                    byte_completition++;
                    x++;
                }
                while (x < img.Width);
                array[idx++] = bx;

                byte_completition = 0;
                x = 0;
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

        public static void SaveByteArrayToFileWithBinaryWriter(byte[] data, string filePath)
        {
            using var writer = new BinaryWriter(File.OpenWrite(filePath));
            writer.Write(data);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (image1 != null)
            {
                byte[] vbc = write_vbc(image1);
                SaveFileDialog savefile = new SaveFileDialog();
                savefile.RestoreDirectory = true;
                savefile.InitialDirectory = "c:\\";
                //savefile.FileName = String.Format("{0}.vbc", txtidstruk.Text);
                savefile.DefaultExt = "*.vbc*";
                savefile.Filter = "vbc file|*.vbc";

                if (savefile.ShowDialog() == DialogResult.OK)
                {
                   // using (System.IO.StreamWriter sw = new System.IO.StreamWriter(savefile.FileName))

                        /*for (int i = 0; i < vbc.Length; i++)
                        {
                            System.Diagnostics.Debug.Write(vbc[i],"X");
                            sw.WriteAllBytes("{0:X4}", vbc[i]);
                            
                            System.Diagnostics.Debug.Write("\n");
                        }*/
                        SaveByteArrayToFileWithBinaryWriter(vbc, savefile.FileName);

                }
            }

            else
            {
                    MessageBox.Show("Nothing to save" + "\nOpen a file before saving");
            }


        }
    }
}


