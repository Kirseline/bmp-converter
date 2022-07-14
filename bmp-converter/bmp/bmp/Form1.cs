using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace bmp
{
    public partial class Form1 : Form
    {
        Bitmap image1;
        Bitmap prew;
        int bytex;
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
            bytex = (int)img.Width / 8 +1 ;
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
        private void save_struct(string name, byte[] vbc)
        {

        }
        private void button2_Click(object sender, EventArgs e)
        {
            string txt_name = "";
            string txt_out = "";

            
            if (prew != null)
            {
                byte[] vbc = write_vbc(prew);
                SaveFileDialog savefile = new SaveFileDialog();
                savefile.RestoreDirectory = true;
                savefile.InitialDirectory = "c:\\";
                savefile.DefaultExt = "*.vbc*";
                savefile.Filter = "vbc file|*.vbc";
                


                if (savefile.ShowDialog() == DialogResult.OK)
                {
                    txt_name = Path.GetFileName(savefile.FileName).Replace(".vbc", "");
                    using StreamWriter file = new(savefile.FileName.Replace(".vbc", "") + "_struct" + ".txt");

                    txt_out += "//-------vbc_file start-------\n\n";
                    txt_out += "vbc_file "+ txt_name + ";\n\n";
                    
                    txt_out += txt_name + ".header.id = " + ("0x" + vbc[0].ToString("x2") + ";\n");
                    txt_out += txt_name + ".header.width = " + ("0x" + vbc[1].ToString("x2") + ";\n");
                    txt_out += txt_name + ".header.height = " + ("0x" + vbc[2].ToString("x2") + ";\n"); ;
                    txt_out += txt_name + ".header.byte_cnt_h = " + ("0x" + vbc[3].ToString("x2") + ";\n"); ;
                    txt_out += txt_name + ".header.byte_cnt_l = " + ("0x" + vbc[4].ToString("x2") + ";\n\n"); ;

                    txt_out += "uint8_t " + txt_name + "_sv" + "[" + (vbc.Length - 5).ToString() + "] = \n{\n\t";  // sv_ support vector

                    int cnt = 0;
                    for(int p=5; p < vbc.Length; p++)
                    {
                        if (cnt == bytex)
                        {
                            txt_out += "\n\t";
                            cnt = 0;
                        }

                        txt_out += ("0x" + vbc[p].ToString("x2") + ", ");
                        
                        cnt++;
                    }

                    txt_out = txt_out.Remove(txt_out.Length-2);
                    txt_out += "\n};\n\n";

                    txt_out += txt_name + ".body = " + txt_name + "_sv;\n\n";

                    txt_out += "//-------vbc_file stop -------";

                    file.WriteLine(txt_out);

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


