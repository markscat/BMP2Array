using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace BMP2Arry__page_
{
    public partial class BMP2Array : Form
    {

        public BMP2Array()
        {
            InitializeComponent();

            //ReadFile.Click += ReadFile_Click; //<< �S������h���ŦX�e��eventhander
            numericUpDown1.Minimum = 0;
            numericUpDown1.Maximum = 255; // �G���H�ȫ�ĳ�d�� 0~255
            numericUpDown1.Value = 128;
            //�o����椧��|�X�{�H�U�����D
            //System.ArgumentOutOfRangeException: ''128' ���O 'Value' �����ĭȡC'Value' ���Ӥ��� 'Minimum' �P 'Maximum' �����C (Parameter 'value')
            //Actual value was 128.'

        }

        // ---------------- ���J BMP �óB�z ----------------

        private void ReadFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.bmp;*.jpg;*.png;*.gif";
                ofd.Title = "Select an Image File";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (Bitmap originalBmp = new Bitmap(ofd.FileName))
                        {
                            // ��ܭ��
                            pictureBox1.Image?.Dispose();
                            pictureBox1.Image = new Bitmap(originalBmp);

                            int width = originalBmp.Width;
                            int height = originalBmp.Height;

                            // �o�̰��] OLED �O 128x64
                            const int OLED_W = 128;
                            const int OLED_H = 64;

                            Bitmap workBmp;
                            if (width > OLED_W || height > OLED_H)
                            {
                                workBmp = new Bitmap(OLED_W, OLED_H);
                                using (Graphics g = Graphics.FromImage(workBmp))
                                {
                                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                    g.DrawImage(originalBmp, 0, 0, OLED_W, OLED_H);
                                }
                                width = OLED_W;
                                height = OLED_H;
                            }
                            else
                            {
                                workBmp = new Bitmap(originalBmp);
                            }
                            // ---- ��s��� ----
                            pictureBox1.Image?.Dispose();
                            pictureBox1.Image = new Bitmap(workBmp);

                            // �ഫ�� SH1106 byte[] �}�C
                            byte[] shArray = ConvertBmpToSH1106ByteArray(workBmp, (int)numericUpDown1.Value, checkBox1.Checked);

                            // �ͦ� C �}�C�r��
                            string cArray = ConvertByteArrayToCArrayString(shArray, "myBitmap", width, height);
                            textBox1.Text = cArray;

                            // ---- ��V��X�w�� ----
                            int paddedHeight = (height + 7) & ~7;
                            Bitmap preview = RenderSH1106Array(shArray, width, paddedHeight);

                            pictureBoxPreview.Image?.Dispose();
                            pictureBoxPreview.Image = preview;

                            // ����
                            workBmp.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("�L�kŪ���γB�z�Ϥ��ɮסC\n���~�T��: " + ex.Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        // ---------------- �N byte[] �ͦ� C �}�C�r�� ----------------
        // (�u��: �ϥ� StringBuilder �����Ĳv)
        private static string ConvertByteArrayToCArrayString(byte[] arr, string name, int width, int height)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"const uint8_t {name}[{arr.Length}] = {{");


            for (int i = 0; i < arr.Length; i++)
            {
                sb.Append("0x" + arr[i].ToString("X2") + ", ");
                if ((i + 1) % width == 0)
                    sb.AppendLine();
            }
            sb.AppendLine("};");
            return sb.ToString();

        }

        private static byte[] ConvertBmpToSH1106ByteArray(Bitmap bmp, int threshold, bool invert)
        {
            int width = bmp.Width;
            int originalHeight = bmp.Height;

            // --- �i���n�j���׸ɻ��� 8 ������ ---
            // OLED �ù��H 8 �ӹ��������@��(page)�Ӽg�J�ƾ�
            // �ҥH�ƾڪ����ץ����O 8 ������
            int paddedHeight = (originalHeight + 7) & ~7; // �󰪮Ī��g�k�A���P�� (originalHeight + 7) / 8 * 8

            byte[] data = new byte[width * paddedHeight / 8];

            // �M���ؼФؤo (�e�פ��ܡA���׬��ɻ��᪺����)
            for (int y = 0; y < paddedHeight; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // �u�b��l�Ϥ������׽d�򤺨���
                    if (y < originalHeight)
                    {
                        Color c = bmp.GetPixel(x, y);
                        int gray = (int)(c.R * 0.299 + c.G * 0.587 + c.B * 0.114);

                        bool isDark = gray < threshold;
                        bool pixelOn = invert ? !isDark : isDark;

                        if (pixelOn)
                        {
                            data[(y / 8) * width + x] |= (byte)(1 << (y % 8));
                        }
                    }
                    // �W�X��l���ת��ɻ��ϰ�A�O���� 0 (�զ�)
                    // �]�� byte[] ��l�ƮɴN�O 0�A�ҥH�o�̤��ݭn�g else
                }
            }
            return data;
        }


        // ---------------- �N SH1106 byte[] ��V�� Bitmap �w�� ----------------

        private static Bitmap RenderSH1106Array(byte[] arr, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            int pages = (height + 7) / 8;

            for (int page = 0; page < pages; page++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte data = arr[page * width + x];
                    for (int bit = 0; bit < 8; bit++)
                    {
                        int y = page * 8 + bit;
                        if (y < height)
                        {
                            bool pixelOn = (data & (1 << bit)) != 0;
                            bmp.SetPixel(x, y, pixelOn ? Color.Black : Color.White);
                        }
                    }
                }
            }
            return bmp;
        }


        /*
        // ---------------- �N byte[] �ͦ� C �}�C�r�� ----------------
        private static string ConvertBmpToSH1106Array(byte[] data)
        {
            string result = "const unsigned char bmp[] = {";
            for (int i = 0; i < data.Length; i++)
            {
                if (i % 16 == 0) result += "\r\n";
                result += $"0x{data[i]:X2}, ";
            }
            result = result.TrimEnd(',', ' ') + "\r\n};";
            return result;
        }




        private void ReadFile_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "BMP Files|*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // �ϥ� using �T�O Bitmap �귽�Q����
                    using (Bitmap originalBmp = new Bitmap(ofd.FileName)) {
                        // ��ܭ��
                        // ���F�קK��w�ɮסA���ƻs�@���A���
                        pictureBox1.Image = new Bitmap(originalBmp);

                        // �ഫ�� SH1106 byte[] �}�C
                        byte[] shArray = ConvertBmpToSH1106ByteArray(originalBmp, (int)numericUpDown1.Value, checkBox1.Checked);

                        // �ͦ� C �}�C�r��
                        string cArray = ConvertByteArrayToCArrayString(shArray, "bmp");
                        textBox1.Text = cArray;

                        // ��ܥͦ��}�C���w����
                        Bitmap preview = RenderSH1106Array(shArray, 128, 64); // SH1106 128x64

                        // �����ª��w���ϸ귽
                        pictureBoxPreview.Image?.Dispose();
                        pictureBoxPreview.Image = preview;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("�L�kŪ���γB�z�Ϥ��ɮסC\n���~�T��: " + ex.Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        */

        /*
        private string ConvertByteArrayToCArrayString(byte[] data, string  arrayName)
        {

            if (data == null || data.Length == 0)
            {
                return $"const unsigned char {arrayName}[] = {{}};\r\n";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append($"const unsigned char {arrayName}[] = {{\r\n  ");

            for (int i = 0; i < data.Length; i++)
            {
                sb.Append($"0x{data[i]:X2}, ");
                if ((i + 1) % 16 == 0 && (i + 1) < data.Length)
                {
                    sb.Append("\r\n  ");
                }
            }

            // �����̫�h�l���r���M�Ů�
            sb.Length -= 2;
            sb.Append("\r\n};");
            return sb.ToString();
        }

        */




        /*
        private byte[] ConvertBmpToSH1106ByteArray(Bitmap bmp, int threshold, bool invert)
        {
            int targetWidth = 128;
            int targetHeight = 64;
            byte[] data = new byte[targetWidth * targetHeight / 8];


            // �إߤ@�� 128x64 ���̲׵e���A�ΨӳB�z�M�ഫ
            using (Bitmap processedBmp = new Bitmap(targetWidth, targetHeight))
            using (Graphics g = Graphics.FromImage(processedBmp))
            {
                // --- �]�wø�ϫ~�� ---
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                // --- Step 1: ���Υզ�񺡾�ӭI�� ---
                g.Clear(Color.White);

                // --- Step 2: �ھڭ�l�Ϥ��j�p�M�wø�s�ؤo ---
                int drawWidth;
                int drawHeight;

                // �i���� 1�j�p�G��l�Ϥ���ؼеe���j�A�h������Y�p
                if (bmp.Width > targetWidth || bmp.Height > targetHeight)
                {
                    double ratioX = (double)targetWidth / bmp.Width;
                    double ratioY = (double)targetHeight / bmp.Height;
                    // �����p����ҡA�T�O��ӹϤ������i�h�B���ܧ�
                    double ratio = Math.Min(ratioX, ratioY);
                    drawWidth = (int)(bmp.Width * ratio);
                    drawHeight = (int)(bmp.Height * ratio);
                }
                // �i���� 2�j�p�G��l�Ϥ���ؼеe���p�ε���A�h�ϥέ�l�ؤo
                else
                {
                    drawWidth = bmp.Width;
                    drawHeight = bmp.Height;
                }

                // --- Step 3: �N�Ϥ�ø�s��e�������W�� (0, 0) ---
                // �i�D�n�ק�j�����F�m���p��A�����N�ؼЮy�г]�� 0, 0
                g.DrawImage(bmp, 0, 0, drawWidth, drawHeight);

                // --- Step 4: �N�B�z�n�� 128x64 �e���ഫ�� byte �}�C (�o�����޿褣��) ---
                for (int y = 0; y < processedBmp.Height; y++)
                {
                    for (int x = 0; x < processedBmp.Width; x++)
                    {
                        Color c = processedBmp.GetPixel(x, y);
                        int gray = (int)(c.R * 0.299 + c.G * 0.587 + c.B * 0.114);

                        bool isDark = gray < threshold;
                        bool pixelOn = invert ? !isDark : isDark;

                        if (pixelOn)
                        {
                            data[(y / 8) * targetWidth + x] |= (byte)(1 << (y % 8));
                        }
                    }
                }
            }
            return data;

        }


        */



        // ---------------- �ഫ BMP �� SH1106 byte[] ----------------
        /*
        private static byte[] ConvertBmpToSH1106ByteArray(Bitmap bmp, int threshold, bool invert)
        {
            int width = 128;
            int height = 64;
            byte[] data = new byte[width * height / 8];

            Bitmap resized = new Bitmap(width,height);

            if (bmp.Width > width || bmp.Height > height)
            {                
                using (Graphics g = Graphics.FromImage(resized))
                {
                    g.Clear(Color.White); // �I���ɥ�
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bmp, 0, 0, width, height);
                }
            }
            //Bitmap resized = new Bitmap(bmp, new Size(width, height));
            
            for (int y = 0; y < resized.Height; y++)
            {
                for (int x = 0; x < resized.Width; x++)
                {
                    Color c = resized.GetPixel(x, y);
                    int gray = (c.R + c.G + c.B) / 3;
                    bool pixelOn = invert ? gray < threshold : gray > threshold;

                    if (pixelOn)
                        data[(y / 8) * width + x] |= (byte)(1 << (y % 8));
                }
            }
            return data;
        }
        */



        /*
private void ReadFile_Click(object? sender, EventArgs e)
{
   using OpenFileDialog ofd = new OpenFileDialog();

   ofd.Filter = "BMP Files|*.bmp";

   if (ofd.ShowDialog() == DialogResult.OK)
   {
       Bitmap bmp = new(ofd.FileName);

       // ��ܭ��
       pictureBox1.Image = bmp;

       // �ͦ� SH1106 �}�C
       byte[] shArray = ConvertBmpToSH1106ByteArray(bmp); // �令�^�� byte[]
       string cArray = ConvertBmpToSH1106Array(bmp);      // �^�� C �}�C�r��
       textBox1.Text = cArray;

       // ��̲ܳ� SH1106 �w��
       Bitmap preview = RenderSH1106Array(shArray, 128, 64); // ���] SH1106 128x64
       pictureBoxPreview.Image = preview;

   }
}


*/


    }
}
