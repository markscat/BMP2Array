

namespace BMP2Arry__page_
{
    partial class BMP2Array
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ReadFile = new Button();
            pictureBox1 = new PictureBox();
            numericUpDown1 = new NumericUpDown();
            checkBox1 = new CheckBox();
            textBox1 = new TextBox();
            panel1 = new Panel();
            pictureBoxPreview = new PictureBox();
            label1 = new Label();
            comboBox1 = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            SuspendLayout();
            // 
            // ReadFile
            // 
            ReadFile.Location = new Point(12, 12);
            ReadFile.Name = "ReadFile";
            ReadFile.Size = new Size(75, 23);
            ReadFile.TabIndex = 0;
            ReadFile.Text = "Read File";
            ReadFile.UseVisualStyleBackColor = true;
            ReadFile.Click += ReadFile_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(3, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(394, 390);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(208, 12);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(120, 23);
            numericUpDown1.TabIndex = 2;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(350, 12);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(50, 19);
            checkBox1.TabIndex = 3;
            checkBox1.Text = "反白";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(393, 3);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.ScrollBars = ScrollBars.Both;
            textBox1.Size = new Size(376, 390);
            textBox1.TabIndex = 4;
            // 
            // panel1
            // 
            panel1.Controls.Add(pictureBoxPreview);
            panel1.Controls.Add(pictureBox1);
            panel1.Controls.Add(textBox1);
            panel1.Location = new Point(3, 41);
            panel1.Name = "panel1";
            panel1.Size = new Size(1251, 413);
            panel1.TabIndex = 5;
            // 
            // pictureBoxPreview
            // 
            pictureBoxPreview.Location = new Point(775, 3);
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.Size = new Size(476, 390);
            pictureBoxPreview.TabIndex = 5;
            pictureBoxPreview.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(135, 16);
            label1.Name = "label1";
            label1.Size = new Size(67, 15);
            label1.TabIndex = 6;
            label1.Text = "黑白值標準";
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(629, 13);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(121, 23);
            comboBox1.TabIndex = 7;
            // 
            // BMP2Array
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1266, 455);
            Controls.Add(comboBox1);
            Controls.Add(label1);
            Controls.Add(panel1);
            Controls.Add(checkBox1);
            Controls.Add(numericUpDown1);
            Controls.Add(ReadFile);
            Name = "BMP2Array";
            Text = "SH1106/SSD1306 圖檔轉矩陣";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }



        #endregion

        private Button ReadFile;
        private PictureBox pictureBox1;
        private NumericUpDown numericUpDown1;
        private CheckBox checkBox1;
        private TextBox textBox1;
        private Panel panel1;
        private PictureBox pictureBoxPreview;
        private Label label1;
        private ComboBox comboBox1;
    }
}
