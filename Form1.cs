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

            //ReadFile.Click += ReadFile_Click; //<< 沒有任何多載符合委派eventhander
            numericUpDown1.Minimum = 0;
            numericUpDown1.Maximum = 255; // 亮度閾值建議範圍 0~255
            numericUpDown1.Value = 128;
            //這邊執行之後會出現以下的問題
            //System.ArgumentOutOfRangeException: ''128' 不是 'Value' 的有效值。'Value' 應該介於 'Minimum' 與 'Maximum' 之間。 (Parameter 'value')
            //Actual value was 128.'

        }

        // ---------------- 載入 BMP 並處理 ----------------

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
                            // 顯示原圖
                            pictureBox1.Image?.Dispose();
                            pictureBox1.Image = new Bitmap(originalBmp);

                            int width = originalBmp.Width;
                            int height = originalBmp.Height;

                            // 這裡假設 OLED 是 128x64
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
                            // ---- 更新顯示 ----
                            pictureBox1.Image?.Dispose();
                            pictureBox1.Image = new Bitmap(workBmp);

                            // 轉換成 SH1106 byte[] 陣列
                            byte[] shArray = ConvertBmpToSH1106ByteArray(workBmp, (int)numericUpDown1.Value, checkBox1.Checked);

                            // 生成 C 陣列字串
                            string cArray = ConvertByteArrayToCArrayString(shArray, "myBitmap", width, height);
                            textBox1.Text = cArray;

                            // ---- 渲染輸出預覽 ----
                            int paddedHeight = (height + 7) & ~7;
                            Bitmap preview = RenderSH1106Array(shArray, width, paddedHeight);

                            pictureBoxPreview.Image?.Dispose();
                            pictureBoxPreview.Image = preview;

                            // 釋放
                            workBmp.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("無法讀取或處理圖片檔案。\n錯誤訊息: " + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        // ---------------- 將 byte[] 生成 C 陣列字串 ----------------
        // (優化: 使用 StringBuilder 提高效率)
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

            // --- 【重要】高度補齊到 8 的倍數 ---
            // OLED 螢幕以 8 個像素高為一頁(page)來寫入數據
            // 所以數據的高度必須是 8 的倍數
            int paddedHeight = (originalHeight + 7) & ~7; // 更高效的寫法，等同於 (originalHeight + 7) / 8 * 8

            byte[] data = new byte[width * paddedHeight / 8];

            // 遍歷目標尺寸 (寬度不變，高度為補齊後的高度)
            for (int y = 0; y < paddedHeight; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 只在原始圖片的高度範圍內取色
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
                    // 超出原始高度的補齊區域，保持為 0 (白色)
                    // 因為 byte[] 初始化時就是 0，所以這裡不需要寫 else
                }
            }
            return data;
        }


        // ---------------- 將 SH1106 byte[] 渲染成 Bitmap 預覽 ----------------

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
        // ---------------- 將 byte[] 生成 C 陣列字串 ----------------
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
                    // 使用 using 確保 Bitmap 資源被釋放
                    using (Bitmap originalBmp = new Bitmap(ofd.FileName)) {
                        // 顯示原圖
                        // 為了避免鎖定檔案，先複製一份再顯示
                        pictureBox1.Image = new Bitmap(originalBmp);

                        // 轉換成 SH1106 byte[] 陣列
                        byte[] shArray = ConvertBmpToSH1106ByteArray(originalBmp, (int)numericUpDown1.Value, checkBox1.Checked);

                        // 生成 C 陣列字串
                        string cArray = ConvertByteArrayToCArrayString(shArray, "bmp");
                        textBox1.Text = cArray;

                        // 顯示生成陣列的預覽圖
                        Bitmap preview = RenderSH1106Array(shArray, 128, 64); // SH1106 128x64

                        // 釋放舊的預覽圖資源
                        pictureBoxPreview.Image?.Dispose();
                        pictureBoxPreview.Image = preview;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("無法讀取或處理圖片檔案。\n錯誤訊息: " + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            // 移除最後多餘的逗號和空格
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


            // 建立一個 128x64 的最終畫布，用來處理和轉換
            using (Bitmap processedBmp = new Bitmap(targetWidth, targetHeight))
            using (Graphics g = Graphics.FromImage(processedBmp))
            {
                // --- 設定繪圖品質 ---
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                // --- Step 1: 先用白色填滿整個背景 ---
                g.Clear(Color.White);

                // --- Step 2: 根據原始圖片大小決定繪製尺寸 ---
                int drawWidth;
                int drawHeight;

                // 【條件 1】如果原始圖片比目標畫布大，則按比例縮小
                if (bmp.Width > targetWidth || bmp.Height > targetHeight)
                {
                    double ratioX = (double)targetWidth / bmp.Width;
                    double ratioY = (double)targetHeight / bmp.Height;
                    // 取較小的比例，確保整個圖片都能放進去且不變形
                    double ratio = Math.Min(ratioX, ratioY);
                    drawWidth = (int)(bmp.Width * ratio);
                    drawHeight = (int)(bmp.Height * ratio);
                }
                // 【條件 2】如果原始圖片比目標畫布小或等於，則使用原始尺寸
                else
                {
                    drawWidth = bmp.Width;
                    drawHeight = bmp.Height;
                }

                // --- Step 3: 將圖片繪製到畫布的左上角 (0, 0) ---
                // 【主要修改】移除了置中計算，直接將目標座標設為 0, 0
                g.DrawImage(bmp, 0, 0, drawWidth, drawHeight);

                // --- Step 4: 將處理好的 128x64 畫布轉換為 byte 陣列 (這部分邏輯不變) ---
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



        // ---------------- 轉換 BMP 成 SH1106 byte[] ----------------
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
                    g.Clear(Color.White); // 背景補白
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

       // 顯示原圖
       pictureBox1.Image = bmp;

       // 生成 SH1106 陣列
       byte[] shArray = ConvertBmpToSH1106ByteArray(bmp); // 改成回傳 byte[]
       string cArray = ConvertBmpToSH1106Array(bmp);      // 回傳 C 陣列字串
       textBox1.Text = cArray;

       // 顯示最終 SH1106 預覽
       Bitmap preview = RenderSH1106Array(shArray, 128, 64); // 假設 SH1106 128x64
       pictureBoxPreview.Image = preview;

   }
}


*/


    }
}
