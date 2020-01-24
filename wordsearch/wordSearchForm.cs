/*
 * Class: wordSearchForm
 * 
 * Operates an interactive form to define a word search puzzle. 
 * 
 * */

using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace wordsearch
{
    public partial class wordSearchForm : Form
    {

        private Color boxBackgroundColor = new Color();
        private Color letterColor = new Color();
        private static int xOffset = 310;
        private static int yOffset = 142;
        private static int cellSize = 22;
        private static int printCellSize = 32;
        private static int tryLimit = 500;
        private int numberOfWords;
        private string[] words = new string[24];  // array of the non-blanks words that the user filled in
        private int[] wordCon = new int[24];      // array from 1 to numberOfWords, which will be randomized
        private int[] wordLocation = new int[24]; // array of which input field (1-24) that a word is typed into
        private Random rand = new Random(System.DateTime.Now.Millisecond);
        private char[,] masterGrid = new char[24, 24];
        private char[,] tempGrid = new char[24, 24];
        private int incX;
        private int incY;
        private int[] directions = new int[8];
        private int numberPlaced = 0;
        private string saveFileName = "";
        private bool[] failedToPlace = new bool[25]; // needs to be one bigger because we use 1-24 as the index not 0-23
        private static readonly Regex sWhitespace = new Regex(@"\s+");


        public wordSearchForm()
        {
            InitializeComponent();
        }

        /*
         * wordSearchForm_Load()
         * Initialize some variables.
         * build the grid
         * */
        private void wordSearchForm_Load(object sender, EventArgs e)
        {
            titleTextBox.Text = "";
            toolStripStatusLabel.Text = "Ready";
            xComboBox.SelectedIndex = 16;
            yComboBox.SelectedIndex = 16;
            eastCheckBox.Checked = true;
            southCheckBox.Checked = true;
            westCheckBox.Checked = true;
            northCheckBox.Checked = true;
            northEastCheckBox.Checked = true;
            southEastCheckBox.Checked = true;
            southWestCheckBox.Checked = true;
            northWestCheckBox.Checked = true;
            for (int d = 0; d <= 7; d++) { directions[d] = d; }
            boxBackgroundColor = Color.FromArgb(200, 255, 255, 255);
            letterColor = Color.FromArgb(200, 0, 0, 0);
            buildGrid();
        }

        /*
         * directionAllowed()
         * given a direction ID (0-7), return true if that direction is allowed for
         * word placement. Otherwise return false.
         * Also set the incX and incY vars, which determine which directions we
         * need to move when placing a word.
         * */
        private bool directionAllowed(int d)
        {
            switch (d)
            {
                case 0: if (eastCheckBox.Checked == true) { incX = 1; incY = 0; return true; } break;
                case 1: if (southCheckBox.Checked == true) { incX = 0; incY = 1; return true; } break;
                case 2: if (westCheckBox.Checked == true) { incX = -1; incY = 0; return true; } break;
                case 3: if (northCheckBox.Checked == true) { incX = 0; incY = -1; return true; } break;
                case 4: if (northEastCheckBox.Checked == true) { incX = 1; incY = -1; return true; } break;
                case 5: if (southEastCheckBox.Checked == true) { incX = 1; incY = 1; return true; } break;
                case 6: if (southWestCheckBox.Checked == true) { incX = -1; incY = 1; return true; } break;
                case 7: if (northWestCheckBox.Checked == true) { incX = -1; incY = -1; return true; } break;
            }
            return false;
        }

        /*
         * countWords()
         * - count up how many words the user has typed in the 24 possible blanks. (numberOfWords)
         * - define arrays: words, wordLocation, wordCon
         * - resize those 3 arrays so the number of elements matches how many inputs the user made.
         */
        private void countWords()
        {
            string textString = "";
            Array.Resize(ref words, 24);
            Array.Resize(ref wordLocation, 24);
            Array.Resize(ref wordCon, 24);

            numberOfWords = 0;
            for (int i = 1; i <= 24; i++)
            {
                failedToPlace[i - 1] = false;
                Control textBox = this.Controls["wordTextBox" + i];
                textString = removeWhiteSpace(textBox.Text, "");
                if (textString.Length == 0) { }
                else
                {
                    words[numberOfWords] = textString.ToUpper();
                    wordLocation[numberOfWords] = i;
                    wordCon[numberOfWords] = numberOfWords;
                    numberOfWords++;
                }
            }

            Array.Resize(ref words, numberOfWords);
            Array.Resize(ref wordLocation, numberOfWords);
            Array.Resize(ref wordCon, numberOfWords);

        }

        public static string removeWhiteSpace(string input, string replacement)
        {
            return sWhitespace.Replace(input, replacement);
        }

        private void buildGrid()
        {
            // create label controls in a grid defined by the X and Y comboboxes

            Point location = new Point();

            for (int row = 0; row <= 19; row++) // Convert.ToInt16(xComboBox.SelectedItem)
            {
                for (int col = 0; col <= 19; col++) // Convert.ToInt16(yComboBox.SelectedItem)
                {
                    location.X = xOffset + row * cellSize;
                    location.Y = yOffset + col * cellSize;
                    Label lbl = new Label();
                    lbl.Name = row.ToString() + "_" + col.ToString();
                    lbl.BorderStyle = BorderStyle.Fixed3D;
                    lbl.Location = location;
                    lbl.Width = cellSize;
                    lbl.Height = cellSize;
                    lbl.TextAlign = ContentAlignment.MiddleCenter;
                    lbl.BackColor = boxBackgroundColor;
                    lbl.ForeColor = letterColor;
                    lbl.Tag = 0;
                    this.Controls.Add(lbl);
                }
            }

            clearWordLabelImages();

        }

        private void reSizeGrid()
        {

            for (int n = 0; n <= 19; n++)
            {
                for (int m = 0; m <= 19; m++)
                {
                    masterGrid[n, m] = Convert.ToChar(" "); // clears data in grid variables
                    tempGrid[n, m] = Convert.ToChar(" ");

                    Control lbl = this.Controls[n + "_" + m]; // find the control
                    if (lbl != null)                          // if the control exists
                    {
                        lbl.Text = " ";                       // clear the box
                        lbl.Tag = 0;
                        if (n < Convert.ToInt16(xComboBox.SelectedItem) & m < Convert.ToInt16(yComboBox.SelectedItem))  // any box outside the range, make invisible
                        {
                            lbl.Visible = true;

                        } else
                        {
                            lbl.Visible = false;
                        }
                        
                    }
                }
            }

            for (int n = 1; n <= 24; n++)
            {
                Label lbl = (Label)this.Controls["wordLabel" + n];
                lbl.Image = null;
            }


        }
        private void xComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            reSizeGrid();
            fillButton.Enabled = false;
            unFillButton.Enabled = false;
        }

        private void yComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            reSizeGrid();
            fillButton.Enabled = false;
            unFillButton.Enabled = false;
        }

        private void fillButton_Click(object sender, EventArgs e)
        {
            for (int n = 0; n < Convert.ToInt16(xComboBox.SelectedItem); n++)
            {
                for (int m = 0; m < Convert.ToInt16(yComboBox.SelectedItem); m++)
                {
                    Control lbl = this.Controls[n + "_" + m];
                    if (lbl.Tag.Equals(0))
                    {
                        lbl.Text = Convert.ToString(Convert.ToChar(rand.Next(65, 90)));
                    }
                }
            }
            toolStripStatusLabel.Text = "Filled with random letters";
        }

        private void unFillButton_Click(object sender, EventArgs e)
        {
            for (int n = 0; n < Convert.ToInt16(xComboBox.SelectedItem); n++)
            {
                for (int m = 0; m < Convert.ToInt16(yComboBox.SelectedItem); m++)
                {
                    Control lbl = this.Controls[n + "_" + m];
                    if (lbl.Tag.Equals(0))
                    {
                        lbl.Text = " ";
                    }

                }
            }
            toolStripStatusLabel.Text = "Removed random letters";
        }

        private void clearMasterGrid()
        {
            for (int n = 0; n < Convert.ToInt16(xComboBox.SelectedItem); n++)
            {
                for (int m = 0; m < Convert.ToInt16(yComboBox.SelectedItem); m++)
                {
                    masterGrid[n, m] = Convert.ToChar(" ");
                }
            }
        }

        private void clearTempGrid()
        {
            for (int n = 0; n < Convert.ToInt16(xComboBox.SelectedItem); n++)
            {
                for (int m = 0; m < Convert.ToInt16(yComboBox.SelectedItem); m++)
                {
                    tempGrid[n, m] = Convert.ToChar(" ");
                }
            }
        }

        private void overlayTempOnMaster()
        {
            for (int n = 0; n < Convert.ToInt16(xComboBox.SelectedItem); n++)
            {
                for (int m = 0; m < Convert.ToInt16(yComboBox.SelectedItem); m++)
                {
                    if (tempGrid[n, m].Equals(Convert.ToChar(" "))) { }
                    else
                    {
                        masterGrid[n, m] = tempGrid[n, m];
                        Control lbl = this.Controls[n + "_" + m];
                        lbl.Text = Convert.ToString(masterGrid[n, m]);
                        lbl.Tag = 1;
                    }
                }
            }
        }

        private void placeWordsButton_Click(object sender, EventArgs e)
        {
            int x;
            int y;
            char thisChar;
            int currX;
            int currY;
            bool placed;
            int tries;

            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;

            clearGridButton_Click(sender, e);
            countWords();
            clearMasterGrid();
            clearWordLabelImages();

            // shuffle the order of the words
            utility.shuffle(wordCon);

            // loop through all words
            foreach (int w in wordCon)
            {
                placed = false;
                tries = 0;

                // shuffle the directions
                utility.shuffle(directions);

                do
                {
                    // choose a random starting coordinate
                    x = rand.Next(0, Convert.ToInt16(xComboBox.SelectedItem));
                    y = rand.Next(0, Convert.ToInt16(yComboBox.SelectedItem));

                    // if the starting spot is empty, or it contains the right letter
                    if (masterGrid[x, y].Equals(Convert.ToChar(" ")) || masterGrid[x, y].Equals((Convert.ToChar(words[w].Substring(0, 1)))))
                    {
                        // go thru all directions
                        foreach (int d in directions)
                        {
                            // if this direction is selected on the compass
                            if (directionAllowed(d))
                            {
                                // clear the temporary grid
                                clearTempGrid();
                                // loop through all chars in the word
                                for (int c = 0; c < words[w].Length; c++)
                                {
                                    // determine the character and the curr coordinate
                                    thisChar = Convert.ToChar(words[w].Substring(c, 1));
                                    currX = x + (c * incX);
                                    currY = y + (c * incY);

                                    // if the curr coordinate is out of bounds, fail
                                    if (currX < 0 || currX > Convert.ToInt16(xComboBox.SelectedItem) - 1 || currY < 0 || currY > Convert.ToInt16(yComboBox.SelectedItem) - 1)
                                    {
                                        break;
                                    }

                                    // if the curr coordinate is empty, mark this char on the temporary grid
                                    if (masterGrid[currX, currY].Equals(Convert.ToChar(" ")))
                                    {
                                        tempGrid[currX, currY] = thisChar;
                                    }
                                    else
                                    {
                                        // ...if not empty, but it contains the right letter,  mark this char on the temporary grid
                                        if (masterGrid[currX, currY].Equals(thisChar))
                                        {
                                            tempGrid[currX, currY] = thisChar;
                                        }
                                        else
                                        {
                                            break;  // fail
                                        }
                                    }

                                    // if we just got thru the last letter, then it is placed
                                    if (c == words[w].Length - 1)
                                    {
                                        // copy the temp grid onto the master grid
                                        overlayTempOnMaster();
                                        // put the green check mark to the left of the number
                                        Label lbl = (Label)this.Controls["wordLabel" + wordLocation[w]];
                                        lbl.Image = Properties.Resources.tick;
                                        lbl.ImageAlign = ContentAlignment.MiddleLeft;
                                        placed = true;
                                    }

                                }

                                // once it is placed, no longer need to just other directions or starting points
                                if (placed)
                                {
                                    break;
                                }

                            }
                        }
                    }

                    // if we could not place it, we'lll keep trying until we reach a limit
                    if (!placed)
                    {
                        tries++;
                        // do the try limit check
                        if (tries > tryLimit)
                        {
                            // failed to place within the limit number of tries, put an X next to the wordTextBox
                            Label lbl = (Label)this.Controls["wordLabel" + wordLocation[w]];
                            lbl.Image = Properties.Resources.cross;
                            lbl.ImageAlign = ContentAlignment.MiddleLeft;
                            failedToPlace[wordLocation[w]] = true;
                        }
                    }


                } while (placed == false && (tries <= tryLimit)); // end of do loop

                // keep a count of how many got placed
                if (placed)
                {
                    numberPlaced++;
                }

            } // end loop of all words

            // update message in status area
            toolStripStatusLabel.Text = "Placed " + numberPlaced + " out of " + words.Length;

            // enable buttons for fill and unfill
            if (numberPlaced > 0)
            {
                fillButton.Enabled = true;
                unFillButton.Enabled = true;
            }

            this.Cursor = System.Windows.Forms.Cursors.Default;

        }

        private void clearGridButton_Click(object sender, EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;

            numberPlaced = 0;
            for (int n = 0; n < Convert.ToInt16(xComboBox.SelectedItem); n++)
            {
                for (int m = 0; m < Convert.ToInt16(yComboBox.SelectedItem); m++)
                {
                    masterGrid[n, m] = Convert.ToChar(" ");
                    tempGrid[n, m] = Convert.ToChar(" ");
                    Control lbl = this.Controls[n + "_" + m];
                    lbl.Text = " ";
                    lbl.Tag = 0;
                }
            }
            for (int n = 1; n <= 24; n++)
            {
                Label lbl = (Label)this.Controls["wordLabel" + n];
                lbl.Image = null;
            }

            fillButton.Enabled = false;
            unFillButton.Enabled = false;

            toolStripStatusLabel.Text = "Grid cleared";
            this.Cursor = System.Windows.Forms.Cursors.Default;
        }

        private void clearWordListButton_Click(object sender, EventArgs e)
        {
            for (int n = 1; n <= 24; n++)
            {
                Control tb = this.Controls["wordTextBox" + n];
                tb.Text = "";
                Label lbl = (Label)this.Controls["wordLabel" + n];
                lbl.Image = null;
            }
            clearGridButton_Click(sender, e);
            toolStripStatusLabel.Text = "Word List cleared";
        }

        private void clearWordLabelImages()
        {
            for (int n = 1; n <= 24; n++)
            {
                Label lbl = (Label)this.Controls["wordLabel" + n];
                lbl.Image = null;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aboutForm f = new aboutForm();
            f.ShowDialog();
        }


        private void pageSetupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PageSetupDialog p = new PageSetupDialog();
            p.PageSettings = new System.Drawing.Printing.PageSettings();
            p.ShowDialog();

        }

        private void printSetupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintDialog p = new PrintDialog();
            p.ShowDialog();
        }

        private void wordTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsLetter(e.KeyChar)) { }
            else
            {
                if (e.KeyChar == (Char)Keys.Back) { }
                else
                {
                    e.Handled = true;
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void printPuzzle(System.Object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            int x = 100;
            int y = 100;
            int x2;
            int y2;
            int yWordList;
            string textString;
            int xPrintOffset;
            int numberWordsPrinted = 0;

            Rectangle rc = new Rectangle(60, 60, 700, 80);
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            FontFamily ff = new FontFamily("Arial");
            Font ft16 = new Font(ff, 16, FontStyle.Bold);
            Font ft12 = new Font(ff, 12, FontStyle.Regular);
            Font ft10 = new Font(ff, 10, FontStyle.Italic);

            e.Graphics.DrawString(titleTextBox.Text, ft16, Brushes.Black, rc, sf);
            xPrintOffset = ((20 - Convert.ToInt16(xComboBox.SelectedItem)) * 15);
            x = 100 + xPrintOffset;
            x2 = Convert.ToInt16(xComboBox.SelectedItem) * printCellSize + 100 + xPrintOffset;
            y2 = Convert.ToInt16(yComboBox.SelectedItem) * printCellSize + 100;

            for (int n = 0; n <= Convert.ToInt16(yComboBox.SelectedItem); n++)
            {
                e.Graphics.DrawLine(Pens.Black, x, y, x2, y);
                y += printCellSize;
            }
            yWordList = y;
            x = 100 + xPrintOffset;
            y = 100;

            for (int m = 0; m <= Convert.ToInt16(xComboBox.SelectedItem); m++)
            {
                e.Graphics.DrawLine(Pens.Black, x, y, x, y2);
                x += printCellSize;
            }
            for (int n = 0; n <= Convert.ToInt16(xComboBox.SelectedItem) - 1; n++)
            {
                for (int m = 0; m <= Convert.ToInt16(yComboBox.SelectedItem) - 1; m++)
                {
                    Control lbl = this.Controls[n + "_" + m];
                    e.Graphics.DrawString(lbl.Text, ft12, Brushes.Black, 108 + (n * printCellSize) + xPrintOffset, 108 + (m * printCellSize));
                }
            }

            yWordList += 20;
            x = 80;

            for (int i = 1; i <= 24; i++)
            {
                Control tb = this.Controls["wordTextBox" + i];
                textString = removeWhiteSpace(tb.Text, "");
                if (textString.Length == 0) { }
                else
                {
                    if (failedToPlace[i]) { }
                    else
                    {
                        if (numberWordsPrinted > 2)
                        {
                            yWordList += 28;
                            x = 80;
                            numberWordsPrinted = 0;
                        }
                        e.Graphics.DrawString(textString.ToUpper(), ft12, Brushes.Black, x, yWordList);
                        numberWordsPrinted++;
                        x += 270;
                    }
                }
            }

            Rectangle rc2 = new Rectangle(60, 1050, 700, 1050);
            StringFormat sf2 = new StringFormat();
            sf2.Alignment = StringAlignment.Far;
            e.Graphics.DrawString("Creating with Word Search Maker. http://tiger84.com", ft10, Brushes.Black, rc2, sf2);

        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.DialogResult answer = new System.Windows.Forms.DialogResult();
            answer = printDialog.ShowDialog();
            if (answer == System.Windows.Forms.DialogResult.OK)
            {
                printDocument.Print();
            }

        }

        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printPreviewDialog.ShowDialog();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            savePuzzle(false);
        }
        

        private void savePuzzle(bool saveAs)
        {
            string cb;

            if (saveFileName.Equals(String.Empty) || saveAs)
            {
                SaveFileDialog sd = new SaveFileDialog();
                sd.Filter = "WSF files (*.wsf)|*.wsf|All files|*.*";
                sd.FilterIndex = 1;
                sd.RestoreDirectory = false;
                if (sd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    saveFileName = sd.FileName;
                }
                else
                {
                    return;
                }
            }


            try
            {

                if (File.Exists(saveFileName))
                {
                    File.Delete(saveFileName);
                }
                File.WriteAllText(saveFileName, titleTextBox.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, xComboBox.SelectedIndex.ToString() + Environment.NewLine);
                File.AppendAllText(saveFileName, yComboBox.SelectedIndex.ToString() + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox1.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox2.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox3.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox4.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox5.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox6.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox7.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox8.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox9.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox10.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox11.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox12.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox13.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox14.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox15.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox16.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox17.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox18.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox19.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox20.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox21.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox22.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox23.Text + Environment.NewLine);
                File.AppendAllText(saveFileName, wordTextBox24.Text + Environment.NewLine);
                if (eastCheckBox.Checked) { cb = "T"; } else { cb = "F"; }
                File.AppendAllText(saveFileName, cb + Environment.NewLine);
                if (northEastCheckBox.Checked) { cb = "T"; } else { cb = "F"; }
                File.AppendAllText(saveFileName, cb + Environment.NewLine);
                if (northCheckBox.Checked) { cb = "T"; } else { cb = "F"; }
                File.AppendAllText(saveFileName, cb + Environment.NewLine);
                if (northWestCheckBox.Checked) { cb = "T"; } else { cb = "F"; }
                File.AppendAllText(saveFileName, cb + Environment.NewLine);
                if (westCheckBox.Checked) { cb = "T"; } else { cb = "F"; }
                File.AppendAllText(saveFileName, cb + Environment.NewLine);
                if (southWestCheckBox.Checked) { cb = "T"; } else { cb = "F"; }
                File.AppendAllText(saveFileName, cb + Environment.NewLine);
                if (southCheckBox.Checked) { cb = "T"; } else { cb = "F"; }
                File.AppendAllText(saveFileName, cb + Environment.NewLine);
                if (southEastCheckBox.Checked) { cb = "T"; } else { cb = "F"; }
                File.AppendAllText(saveFileName, cb + Environment.NewLine);

            }
            catch (Exception)
            {
                throw;
            }

            toolStripStatusLabel.Text = "Saved file " + saveFileName;

        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            savePuzzle(true);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();
            od.Filter = "WSF files (*.wsf)|*.wsf|All files (*.*)|*.*";
            od.FilterIndex = 1;
            od.RestoreDirectory = false;
            if (od.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] lines = File.ReadAllLines(od.FileName);
                int lineCount = 1;
                foreach (string s in lines)
                {
                    switch (lineCount)
                    {
                        case 1: titleTextBox.Text = s; break;
                        case 2: xComboBox.SelectedIndex = Convert.ToInt16(s); break;
                        case 3: yComboBox.SelectedIndex = Convert.ToInt16(s); break;
                        case 4: wordTextBox1.Text = s; break;
                        case 5: wordTextBox2.Text = s; break;
                        case 6: wordTextBox3.Text = s; break;
                        case 7: wordTextBox4.Text = s; break;
                        case 8: wordTextBox5.Text = s; break;
                        case 9: wordTextBox6.Text = s; break;
                        case 10: wordTextBox7.Text = s; break;
                        case 11: wordTextBox8.Text = s; break;
                        case 12: wordTextBox9.Text = s; break;
                        case 13: wordTextBox10.Text = s; break;
                        case 14: wordTextBox11.Text = s; break;
                        case 15: wordTextBox12.Text = s; break;
                        case 16: wordTextBox13.Text = s; break;
                        case 17: wordTextBox14.Text = s; break;
                        case 18: wordTextBox15.Text = s; break;
                        case 19: wordTextBox16.Text = s; break;
                        case 20: wordTextBox17.Text = s; break;
                        case 21: wordTextBox18.Text = s; break;
                        case 22: wordTextBox19.Text = s; break;
                        case 23: wordTextBox20.Text = s; break;
                        case 24: wordTextBox21.Text = s; break;
                        case 25: wordTextBox22.Text = s; break;
                        case 26: wordTextBox23.Text = s; break;
                        case 27: wordTextBox24.Text = s; break;
                        case 28: if (s.Equals("T")) { eastCheckBox.Checked = true; } else { eastCheckBox.Checked = false; } break;
                        case 29: if (s.Equals("T")) { northEastCheckBox.Checked = true; } else { northEastCheckBox.Checked = false; } break;
                        case 30: if (s.Equals("T")) { northCheckBox.Checked = true; } else { northCheckBox.Checked = false; } break;
                        case 31: if (s.Equals("T")) { northWestCheckBox.Checked = true; } else { northWestCheckBox.Checked = false; } break;
                        case 32: if (s.Equals("T")) { westCheckBox.Checked = true; } else { westCheckBox.Checked = false; } break;
                        case 33: if (s.Equals("T")) { southWestCheckBox.Checked = true; } else { southWestCheckBox.Checked = false; } break;
                        case 34: if (s.Equals("T")) { southCheckBox.Checked = true; } else { southCheckBox.Checked = false; } break;
                        case 35: if (s.Equals("T")) { southEastCheckBox.Checked = true; } else { southEastCheckBox.Checked = false; } break;
                    }
                    lineCount++;
                }
            }

            fillButton.Enabled = false;
            unFillButton.Enabled = false;

            toolStripStatusLabel.Text = "Opened file " + od.FileName;
            saveFileName = od.FileName; // so attempting to save will automatically have the opened filename
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wordSearchForm_Load(sender, e);
            clearWordListButton_Click(sender, e);
            saveFileName = "";
            toolStripStatusLabel.Text = "";
            toolStripStatusLabel.Text = "New puzzle created";
        }

        private void CreatingPuzzlesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            creatingPuzzlesForm f = new creatingPuzzlesForm();
            f.ShowDialog();
        }

    }
}
