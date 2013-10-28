using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Color[] colors = new Color[0];
        Button[] colorButtons = new Button[0];
        TextBox[] colorNumber = new TextBox[0];
        Color thisStitch;
        Byte[,] output = new Byte[0, 0];
        int MaxColoursPerRow = 0;

        Bitmap bmp; // = (Bitmap)Bitmap.FromFile("C:\\Users\\kevin.blain\\Documents\\jumper_back_final.bmp");

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                bmp = (Bitmap)Bitmap.FromFile(openFileDialog1.FileName);


                pictureBox1.Height = bmp.Height;
                pictureBox1.Width = bmp.Width;
                pictureBox1.Image = bmp;

                if (pictureBox1.Height > 240)
                {
                    this.Height = pictureBox1.Top + pictureBox1.Height + 60;
                }
                this.Width = pictureBox1.Left + pictureBox1.Width + 40;

                for (int row = 0; row < bmp.Height; row++)
                {
                    int coloursThisRow = 0;
                    Color[] rowColors = new Color[0];

                    for (int stitch = 0; stitch < bmp.Width; stitch++)
                    {

                        thisStitch = bmp.GetPixel(stitch, row);

                        if (!colors.Contains(thisStitch))
                        {
                            Array.Resize(ref colors, colors.Length + 1);
                            colors[colors.Length - 1] = thisStitch;
                        }

                        if (!rowColors.Contains(thisStitch))
                        {
                            Array.Resize(ref rowColors, rowColors.Length + 1);
                            rowColors[rowColors.Length - 1] = thisStitch;
                            coloursThisRow++;
                        }

                        if (coloursThisRow > MaxColoursPerRow)
                        {
                            MaxColoursPerRow = coloursThisRow;
                        }
                    }

                }

                for (int i = 0; i < colors.Length; i++)
                {
                    Array.Resize(ref colorButtons, colorButtons.Length + 1);
                    colorButtons[colorButtons.Length - 1] = new Button();
                    colorButtons[colorButtons.Length - 1].Left = 10;
                    colorButtons[colorButtons.Length - 1].Top = i * (colorButtons[colorButtons.Length - 1].Height + 3) + 60;
                    colorButtons[colorButtons.Length - 1].BackColor = colors[i];
                    this.Controls.Add(colorButtons[colorButtons.Length - 1]);

                    Array.Resize(ref colorNumber, colorNumber.Length + 1);
                    colorNumber[colorNumber.Length - 1] = new TextBox();
                    colorNumber[colorNumber.Length - 1].Left = colorButtons[colorButtons.Length - 1].Left + colorButtons[colorButtons.Length - 1].Width + 10;
                    colorNumber[colorNumber.Length - 1].Top = colorButtons[colorButtons.Length - 1].Top;
                    colorNumber[colorNumber.Length - 1].Text = (i + 1).ToString();

                    this.Controls.Add(colorNumber[colorNumber.Length - 1]);
                }

                label1.Text = "Max colours per row: " + MaxColoursPerRow.ToString();
                label2.Text = "Rows: " + bmp.Height.ToString();
                label3.Text = "Stitches: " + bmp.Width.ToString();

                // button1.Text = colors.Length.ToString();

                if (MaxColoursPerRow > 2)
                {
                    buttonConvert.Visible = true;
                    button1.Visible = false;
                }
                else
                {
                    buttonConvert.Visible = false;
                    button1.Visible = true;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int colour;
            int i;

            output = new Byte[bmp.Height, bmp.Width];

            for (int row = 0; row < bmp.Height; row++)
            {
                Color[] rowColors = new Color[MaxColoursPerRow];
                int colorCount = 0;

                for (int stitch = 0; stitch < bmp.Width; stitch++)
                {
                    thisStitch = bmp.GetPixel(stitch, row);

                    if (!rowColors.Contains(thisStitch))
                    {
                        rowColors[colorCount++] = thisStitch;
                    }
                }

                for (int stitch = 0; stitch < bmp.Width; stitch++)
                {
                    thisStitch = bmp.GetPixel(stitch, row);

                    for (i = 0; i < colors.Length; i++)
                    {
                        if (thisStitch == colors[i])
                        {
                            colour = Convert.ToInt32(colorNumber[i].Text);
                            output[row, stitch] = (Byte)colour;
                            break;
                        }
                    }
                }
            }

            // Array 'output' now contains one byte per pixel (stitch), with number representing colour of yarn to use. Let's dump it to a file for reference.

            System.IO.StreamWriter stream = new System.IO.StreamWriter("C:\\Knitulator\\Frontproof.txt");


            for (int row = 0; row < bmp.Height; row++)
            {
                for (int stitch = 0; stitch < bmp.Width; stitch++)
                {
                    stream.Write(output[row, stitch].ToString());
                }
                stream.WriteLine();
            }

            stream.Close();

            // Now lets create a Multicolour pattern, where each row contains a boolean indicating do or don't use the colour.
            // A separate array is required, for each row to indicate which colour to use.

            //TOD: Make sure patternRowColour is multiple of 2 as colours are stored as nibbles

            Byte[] patternRowColour = new Byte[bmp.Height * MaxColoursPerRow]; // Array to hold colour of yarn to use on each Multicolour row

            int widthInBits = (int)(8 * Math.Round(bmp.Width / (double)8, MidpointRounding.AwayFromZero));    // must be multiple of 8 bits

            Byte[,] pattern = new Byte[bmp.Height * MaxColoursPerRow, widthInBits]; // Array to hold pattern data = 1 byte represents 8 stitches

            System.IO.StreamWriter stream2 = new System.IO.StreamWriter("C:\\Knitulator\\Frontmc.txt");

            int n = bmp.Height * MaxColoursPerRow;

            stream2.WriteLine("Row : Disp : Col : Row Pattern");

            for (int row = 0; row < bmp.Height; row++)
            {
                // Work out which colours to knit:
                Byte[] rowColours = new Byte[MaxColoursPerRow];
                int colourCount = 0;

                for (int stitch = 0; stitch < bmp.Width; stitch++)
                {
                    if (!rowColours.Contains(output[row, stitch]))
                    {
                        rowColours[colourCount++] = output[row, stitch];
                    }
                }

                if (rowColours[1] == 0)
                {
                    rowColours[1] = 8;
                    rowColours[2] = 9;
                }

                if (rowColours[2] == 0)
                {
                    rowColours[2] = 9;
                }

                for (i = 0; i < MaxColoursPerRow; i++)
                {
                    stream2.Write((bmp.Height - row).ToString("D3") + " : ");
                    stream2.Write(n.ToString("D3") + "  : ");
                    n--;

                    stream2.Write(rowColours[i].ToString() + "   : ");
                    patternRowColour[row * MaxColoursPerRow + i] = rowColours[i];

                    for (int stitch = 0; stitch < bmp.Width; stitch++)
                    {
                        if (output[row, stitch] == rowColours[i])
                        {
                            stream2.Write("X");
                            pattern[MaxColoursPerRow * row + i, stitch] = 1;
                        }
                        else
                        {
                            stream2.Write(".");
                            pattern[MaxColoursPerRow * row + i, stitch] = 0;
                        }
                    }
                    stream2.WriteLine();
                }
            }

            stream2.Close();

            // Now create binary array in brother format, 1 bit per pixel
            Byte[,] patternOut = new Byte[bmp.Height * MaxColoursPerRow, widthInBits / 8];

            for (int row = 0; row < bmp.Height * MaxColoursPerRow; row++)
            {
                Byte bit = 0;
                for (int stitch = 0; stitch < widthInBits; stitch++)
                {
                    bit = (Byte)(stitch & 0x07);
                    bit = (Byte)(0x80 >> bit);
                    if (pattern[row, stitch] != 0)
                    {
                        patternOut[row, stitch / 8] = (Byte)(patternOut[row, stitch / 8] | ( bit) );
                    }
//                    patternOut[row, stitch / 8] = (Byte)(patternOut[row, stitch / 8] | ((pattern[row, stitch] == 0) ? 0 : (2 ^ bit) ));
                }
            }

            System.IO.FileStream fs = new System.IO.FileStream("C:\\Knitulator\\track-01.dat", System.IO.FileMode.OpenOrCreate);
            System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(fs);

            binaryWriter.Seek(0, System.IO.SeekOrigin.Begin);

            binaryWriter.Write((Byte)0x01);
            binaryWriter.Write((Byte)0x20);

            Byte heightHundreds = (Byte)((bmp.Height * MaxColoursPerRow) / 100);
            Byte heightTens = (Byte)(((bmp.Height * MaxColoursPerRow) / 10) - (10 * heightHundreds));
            Byte heightUnits = (Byte)((bmp.Height * MaxColoursPerRow) - (10 * heightTens) - (100 * heightHundreds));

            Byte widthHundreds = (Byte)((widthInBits) / 100);
            Byte widthTens = (Byte)(((widthInBits) / 10) - (10 * widthHundreds));
            Byte widthUnits = (Byte)((widthInBits) - (10 * widthTens) - (100 * widthHundreds));

            //binaryWriter.Write((Byte)0x77);//774 high
            //binaryWriter.Write((Byte)0x41);
            //binaryWriter.Write((Byte)0x74);// 174 wide

            binaryWriter.Write((Byte)((heightHundreds << 4) + heightTens));
            binaryWriter.Write((Byte)((heightUnits << 4) + widthHundreds));
            binaryWriter.Write((Byte)((widthTens << 4) + widthUnits));

            binaryWriter.Write((Byte)0x89);// 89
            binaryWriter.Write((Byte)0x01);//01 = pattern mode 8 number 901
            binaryWriter.Write((Byte)0x00);
            binaryWriter.Write((Byte)0x00);
            binaryWriter.Write((Byte)0x00);
            binaryWriter.Write((Byte)0x00);
            binaryWriter.Write((Byte)0x00);
            binaryWriter.Write((Byte)0x09);
            binaryWriter.Write((Byte)0x02);

            //fs.Position = 0x7ee0 - (patternRowColour.Length / 2) - (bmp.Height * MaxColoursPerRow * widthInBits / 8);
            //binaryWriter = new System.IO.BinaryWriter(fs);

            binaryWriter.Seek(0x7ee0 - (patternRowColour.Length / 2) - (bmp.Height * MaxColoursPerRow * widthInBits / 8), System.IO.SeekOrigin.Begin);

            for (int row = 0; row < bmp.Height * MaxColoursPerRow; row++)
            {
                for (int stitchBlock = 0; stitchBlock < widthInBits / 8; stitchBlock++)
                {
                    binaryWriter.Write(patternOut[row, stitchBlock]);
                    Console.Write(patternOut[row, stitchBlock].ToString("X2") + " ");
                }
                Console.WriteLine();
            }

            //binaryWriter.Close();
            //fs.Close();

            //System.IO.FileStream fs2 = new System.IO.FileStream("C:\\Users\\kevin.blain\\Documents\\track-01.dat", System.IO.FileMode.OpenOrCreate);
            //fs2.Position = 0x7ee0 - (patternRowColour.Length/2);
            binaryWriter = new System.IO.BinaryWriter(fs);
            for (int row = 0; row < bmp.Height * MaxColoursPerRow; row += 2)
            {
                Byte colourInfo = 0;

                if (patternRowColour[row] == 0)
                {
                    colourInfo = 1 << 4;
                }
                else
                {
                    colourInfo = (Byte)(patternRowColour[row] << 4);
                }


                if (patternRowColour[row + 1] == 0)
                {
                    colourInfo += 1;
                }
                else
                {
                    colourInfo += (Byte)(patternRowColour[row + 1]);
                }

                binaryWriter.Write(colourInfo);
            }

            binaryWriter.Close();
            fs.Close();

            MessageBox.Show("Conversion Complete.");
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            int colour;
            int i;

            output = new Byte[bmp.Height, bmp.Width];

            for (int row = 0; row < bmp.Height; row++)
            {
                Color[] rowColors = new Color[MaxColoursPerRow];
                int colorCount = 0;

                for (int stitch = 0; stitch < bmp.Width; stitch++)
                {
                    thisStitch = bmp.GetPixel(stitch, row);

                    if (!rowColors.Contains(thisStitch))
                    {
                        rowColors[colorCount++] = thisStitch;
                    }
                }

                for (int stitch = 0; stitch < bmp.Width; stitch++)
                {
                    thisStitch = bmp.GetPixel(stitch, row);

                    for (i = 0; i < colors.Length; i++)
                    {
                        if (thisStitch == colors[i])
                        {
                            colour = Convert.ToInt32(colorNumber[i].Text);
                            output[row, stitch] = (Byte)colour;
                            break;
                        }
                    }
                }
            }

            // Array 'output' now contains one byte per pixel (stitch), with number representing colour of yarn to use. Let's dump it to a file for reference.

            System.IO.StreamWriter stream = new System.IO.StreamWriter("C:\\Knitulator\\proof.txt");

            for (int row = 0; row < bmp.Height; row++)
            {
                for (int stitch = 0; stitch < bmp.Width; stitch++)
                {
                    stream.Write(output[row, stitch].ToString());
                }
                stream.WriteLine();
            }

            stream.Close();

            // Now lets create a 2 colour pattern, where each row contains a boolean indicating do or don't use the contrast yarn.

            int widthInBits = (int)(8 * Math.Round(bmp.Width / (double)8, MidpointRounding.AwayFromZero));    // must be multiple of 8 bits

            Byte[,] pattern = new Byte[bmp.Height, widthInBits]; // Array to hold pattern data = 1 byte represents 8 stitches

            System.IO.StreamWriter stream2 = new System.IO.StreamWriter("C:\\Knitulator\\2col.txt");

            int n = bmp.Height;

            stream2.WriteLine("Row : Row Pattern");

            for (int row = 0; row < bmp.Height; row++)
            {
                stream2.Write((bmp.Height - row).ToString("D3") + " : ");
                n--;

                for (int stitch = 0; stitch < bmp.Width; stitch++)
                {
                    if (output[row, stitch] == 2)
                    {
                        stream2.Write("X");
                        pattern[row, stitch] = 1;
                    }
                    else
                    {
                        stream2.Write(".");
                        pattern[row, stitch] = 0;
                    }
                }
                stream2.WriteLine();
            }

            stream2.Close();

            // Now create binary array in brother format, 1 bit per pixel
            Byte[,] patternOut = new Byte[bmp.Height, widthInBits / 8];

            for (int row = 0; row < bmp.Height; row++)
            {
                Byte bit = 0;
                for (int stitch = 0; stitch < widthInBits; stitch++)
                {
                    bit = (Byte)(stitch & 0x07);
                    bit = (Byte)(0x80 >> bit);

                    patternOut[row, stitch / 8] = (Byte)(patternOut[row, stitch / 8] | ((2 ^ bit) * pattern[row, stitch]));
                }
            }

            System.IO.FileStream fs = new System.IO.FileStream("C:\\Knitulator\\track-01.dat", System.IO.FileMode.OpenOrCreate);
            System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(fs);

            binaryWriter.Seek(0, System.IO.SeekOrigin.Begin);

            binaryWriter.Write((Byte)0x01);
            binaryWriter.Write((Byte)0x20);

            Byte heightHundreds = (Byte)((bmp.Height) / 100);
            Byte heightTens = (Byte)(((bmp.Height) / 10) - (10 * heightHundreds));
            Byte heightUnits = (Byte)((bmp.Height) - (10 * heightTens) - (100 * heightHundreds));

            Byte widthHundreds = (Byte)((widthInBits) / 100);
            Byte widthTens = (Byte)(((widthInBits) / 10) - (10 * widthHundreds));
            Byte widthUnits = (Byte)((widthInBits) - (10 * widthTens) - (100 * widthHundreds));

            binaryWriter.Write((Byte)((heightHundreds << 4) + heightTens));
            binaryWriter.Write((Byte)((heightUnits << 4) + widthHundreds));
            binaryWriter.Write((Byte)((widthTens << 4) + widthUnits));

            binaryWriter.Write((Byte)0x49);// 49
            binaryWriter.Write((Byte)0x01);//01 = pattern mode 8 number 901
            binaryWriter.Write((Byte)0x00);
            binaryWriter.Write((Byte)0x00);
            binaryWriter.Write((Byte)0x00);
            binaryWriter.Write((Byte)0x00);
            binaryWriter.Write((Byte)0x00);
            binaryWriter.Write((Byte)0x09);
            binaryWriter.Write((Byte)0x02);

            binaryWriter.Seek(0x7ee0 - (bmp.Height * widthInBits / 8), System.IO.SeekOrigin.Begin);

            for (int row = 0; row < bmp.Height ; row++)
            {
                for (int stitchBlock = 0; stitchBlock < widthInBits / 8; stitchBlock++)
                {
                    binaryWriter.Write(patternOut[row, stitchBlock]);
                    Console.Write(patternOut[row, stitchBlock].ToString("X2") + " ");
                }
                Console.WriteLine();
            }

            binaryWriter.Close();
            fs.Close();

            MessageBox.Show("Conversion Complete.");
        }
    }
}
