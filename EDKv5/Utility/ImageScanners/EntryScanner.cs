using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDKv5.Utility.Scanners
{
    public class EntryImageScanner
    {
        public EntryImageScanner(IScanOutput output)
        {
            if (null == output) throw new NullReferenceException();
            this.output = output;
        }

        private int fno;
        private IScanOutput output;

        private double refLnRad = 0.0185d;  //ReferenceLineRadian

        public int Compensation = 80; //recommended max = 80, extra max = 100
        public int CellSample = 45;   //number of pixels to be chosen for each cell

        public int ErrorCount { get; set; }

        public double ReferenceLineRadian
        {
            get { return refLnRad; }
            set
            {
                while (value > Math.PI) value -= Math.PI;
                while (value < 0) value += Math.PI;
                refLnRad = 0;
            }
        }
        public double ReferenceLineDegree
        {
            get { return refLnRad / Math.PI * 180d; }
            set { ReferenceLineRadian = value / 180d * Math.PI; }
        }

        public bool Scan(Image image, int fileNumber)
        {
            this.fno = fileNumber;

            //prepare image copy in grayscale
            Bitmap img = createGrayscale(image);

            //get black reference
            int refBlack = sampleImageBackground(img);

            //get lines
            List<List<Point>> lines = analyzeLines(img, refBlack);
            if (lines.Count <= 0)
                throw new Exception("No any line is found");


            //search barcode reference line
            Point lineStart, lineEnd;
            if (!findBarcodeReference(lines, img.Width, out lineStart, out lineEnd))
                throw new Exception("No any line seems to be a barcode reference line");

            //read barcode
            float imgScale;
            double imgRotate;
            byte[] code;
            if (!verifyBarcode(img, lineStart, lineEnd, refBlack, out imgScale, out imgRotate, out code))
                throw new Exception("Incorrect barcode, verification did not pass");

            //barcode success
            if (output.SetCode(code))
            {
                //start read cells
                readCells(img, lineStart, refBlack, imgScale, imgRotate);
                return true;
            }
            return false;
        }

        private Bitmap createGrayscale(Image image)
        {
            Bitmap img = new Bitmap(image.Width, image.Height);
            Graphics g = Graphics.FromImage(img);

            ColorMatrix colorMatrix = new ColorMatrix(new float[][] {
                new float[] {.3f, .3f, .3f, 0, 0},
                new float[] {.59f, .59f, .59f, 0, 0},
                new float[] {.11f, .11f, .11f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            });
            ImageAttributes attr = new ImageAttributes();
            attr.SetColorMatrix(colorMatrix);

            GraphicsUnit gunit = GraphicsUnit.Pixel;
            g.DrawImage(image, Rectangle.Round(img.GetBounds(ref gunit)), 0, 0, image.Width, image.Height, gunit, attr);
            return img;
        }

        private List<List<Point>> analyzeLines(Bitmap img, int refBlack)
        {
            //find left-top
            //get possible start point
            List<Point> lspt = new List<Point>();
            int lstH = img.Width / 6;

            int wvMin = 255;

            for (int x = 1; x < lstH; x++)
            {
                for (int y = 1; y < lstH; y++)
                {
                    int wv = img.GetPixel(x, y).R;
                    if (wv < refBlack)
                    {
                        lspt.Add(new Point(x, y));
                        break;
                    }
                    if (wv < wvMin) wvMin = wv;
                } //end for top down
            } //end for left right

            //search lines
            List<List<Point>> lsLine = new List<List<Point>>();
            foreach (Point p in lspt)
            {
                //search for consecutive pixel - meaning a line
                Point k = p;

                //remove redundant lines
                bool flag = false;
                foreach (List<Point> lastLn in lsLine)
                    if (lastLn.Contains(k)) { flag = true; break; }
                if (flag) continue;

                //init line
                List<Point> line = new List<Point>();
                line.Add(k);

                do
                {
                    k.X++;                                                      //move right
                    int wv = 0;
                    if ((wv = img.GetPixel(k.X, k.Y - 1).R) < refBlack)         //check top
                        k.Y--;
                    else if ((wv = img.GetPixel(k.X, k.Y).R) < refBlack) { }    //check right
                    else if ((wv = img.GetPixel(k.X, k.Y + 1).R) < refBlack)    //check bottom
                        k.Y++;
                    else                                                        //end line
                        break;
                    line.Add(k);                                                //add point and move next point
                } while (k.Y > 0);

                //a line at least length 50
                //and it should not exceed half of the paper
                if (line.Count > 50 && line.Count < img.Width / 2) lsLine.Add(line);
            }

            //sort lines
            lsLine.Sort((x, y) => { return y.Count.CompareTo(x.Count); });

            //return
            return lsLine;
        }

        private int sampleImageBackground(Bitmap img)
        {
            //sample image background
            Color[] cs = new Color[] {
                img.GetPixel(0, 0), img.GetPixel(0, 20), img.GetPixel(20, 0), img.GetPixel(0, 20), img.GetPixel(10, 10), img.GetPixel(5, 15), img.GetPixel(15, 5),
                img.GetPixel(img.Width - 1, 0), img.GetPixel(img.Width - 1, 20), img.GetPixel(img.Width - 21, 0), img.GetPixel(img.Width - 10, 10), img.GetPixel(img.Width - 5, 15), img.GetPixel(img.Width - 15, 5),
                img.GetPixel(0, img.Height - 1), img.GetPixel(0, img.Height - 21), img.GetPixel(20, img.Height - 1), img.GetPixel(10, img.Height - 10), img.GetPixel(5, img.Height - 15), img.GetPixel(15, img.Height - 5),
                img.GetPixel(img.Width - 1, img.Height - 1), img.GetPixel(img.Width - 1, img.Height - 21), img.GetPixel(img.Width - 21, img.Height - 1), img.GetPixel(img.Width - 10, img.Height - 10), img.GetPixel(img.Width - 5, img.Height - 15), img.GetPixel(img.Width - 15, img.Height - 5)
            };

            //avg sample background 
            float sum = 0f;
            int min = 256;
            for (int i = 0; i < cs.Length; i++)
            {
                sum += cs[i].R;
                if (cs[i].R < min) min = cs[i].R;
            }

            float avg = sum / cs.Length;
            int refBlack = 355 - (int)avg;
            return refBlack;
        }

        private bool findBarcodeReference(List<List<Point>> lines, float imgWidth, out Point lineStart, out Point lineEnd)
        {
            //search for a line with ratio to image width 0.1536 ~ large range 0.1505
            lineStart = Point.Empty;                            //left starting point
            lineEnd = Point.Empty;                              //right end point
            bool bcRefFound = false;                            //one-way flag, bar code line found

            //get straight line angle
            foreach (List<Point> line in lines)
            {
                //first last point line slope
                double flAngle = Math.Atan2(line.Last().Y - line[0].Y, line.Last().X - line[0].X);

                //filter out non-linear line
                int splitUnit = line.Count / 4;
                bool skip = false;                           // one-way flag
                for (int i = 0; i < 3; i++)
                {
                    int xn = splitUnit * i;
                    double kAngle = Math.Atan2(line[xn + splitUnit].Y - line[xn].Y, line[xn + splitUnit].X - line[xn].X);
                    double diff = Math.Abs(flAngle - kAngle);
                    if (diff > refLnRad)  // 1 deg. = 0.017453292 rad. // 1.0deg. = 0.0185d
                    {
                        skip = true;
                        break;
                    }
                }

                //skip the line
                if (skip) continue;

                //check width ratio
                float ratio = (float)(line.Last().X - line.First().X) / imgWidth;
                if (ratio > 0.145f && ratio < 0.155f)
                {
                    lineStart = line.First();
                    lineEnd = line.Last();
                    bcRefFound = true;
                }

            }

            return bcRefFound;
        }

        private bool verifyBarcode(Bitmap img, Point lineStart, Point lineEnd, int refBlack, out float imgScale, out double imgRotate, out byte[] code)
        {
            const float C_codeBit = 64;
            float dx = lineEnd.X - lineStart.X;
            float dy = lineEnd.Y - lineStart.Y;
            double bcLength = Math.Sqrt(dx * dx + dy * dy);

            //scan scale
            imgScale = (float)bcLength / 128f;

            //start vectors shift
            PointF vec = new PointF(dx / C_codeBit, dy / C_codeBit);    //bit vector
            PointF vDn = new PointF(3, 0);                              //move down vector
            vDn = vDn.Multiply(imgScale);
            vDn = vDn.RotateDeg(90);

            imgRotate = Math.Atan2(vec.Y, vec.X);                       //rotation radian
            vDn = vDn.RotateRad(imgRotate);                             //set move down rotation

            PointF curPt = lineStart;                                   //init position
            curPt = curPt.Add(vDn);                                     //move down on code block
            curPt = curPt.Add(vec.Multiply(0.33f));                     //move right starting point (to center of code block)

            PointF hshPt = curPt;                                       //init hash
            hshPt = hshPt.Add(vDn).Add(vDn);                            //move down on code block

            code = new byte[(int)C_codeBit / 8];
            byte[] hash = new byte[(int)C_codeBit / 8];

            for (int i = 0; i < C_codeBit / 8; i++)
            {
                int tmpCode = 0;
                int tmpHash = 0;
                for (int j = 0; j < 8; j++)
                {
                    //get code
                    int wv = 0;

                    //get code
                    wv = img.GetPixel((int)curPt.X, (int)curPt.Y).R;
                    if (wv < refBlack)
                        tmpCode |= 1 << j;


                    //get hash
                    wv = img.GetPixel((int)hshPt.X, (int)hshPt.Y).R;
                    if (wv < refBlack)
                        tmpHash |= 1 << j;

                    //move
                    curPt = curPt.Add(vec);
                    hshPt = hshPt.Add(vec);
                }

                //revert last shift
                code[i] = (byte)tmpCode;
                hash[i] = (byte)tmpHash;


            }

            //verify code
            bool verifyCode = true;                             //one-way flag
            int iHash = hash[0];
            for (int i = 1; i < code.Length; i++)
            {
                iHash ^= code[i];
                if (iHash != hash[i]) { verifyCode = false; break; }
            }

            return verifyCode;
        }

        private void readCells(Bitmap img, Point lineStart, int refBlack, float imgScale, double imgRotate)
        {
            //start read
            PointF vS = new PointF(210f, 42f);                  // start point vector
            vS = vS.RotateRad(imgRotate).Multiply(imgScale);    // transform
            vS = new PointF(lineStart.X, lineStart.Y).Add(vS);  // move

            PointF vR = new PointF(24f, 0f);                    // move right vector
            vR = vR.RotateRad(imgRotate).Multiply(imgScale);    // transform
            PointF vD = new PointF(0, 24f);                     // move down vector
            vD = vD.RotateRad(imgRotate).Multiply(imgScale);    // transform

            PointF vRow = vS;                                   // location on row
            PointF vCur = vS;                                   // vector on current cell

            Random r = new Random();                            // sample randomizer

            //release black reference for pencil and color pens
            refBlack += Compensation;

            //loop row
            int colCount = output.GetColumnCount();
            bool[] colsTmp = new bool[colCount];
            while (output.Next())
            {
                //loop column
                for (int k = 0; k < colCount; k++)
                {
                    float avg = 0f;
                    //15 samples
                    for (int smi = 0; smi < CellSample; smi++)
                    {
                        const int C_RMin = 4;
                        int C_RMax = (int)(20f * imgScale) + C_RMin;

                        //get samples
                        float sx = (r.Next(C_RMin, C_RMax) + r.Next(C_RMin, C_RMax) + r.Next(C_RMin, C_RMax)) / 3f;
                        float sy = (r.Next(C_RMin, C_RMax) + r.Next(C_RMin, C_RMax) + r.Next(C_RMin, C_RMax)) / 3f;
                        PointF sample = new PointF(sx, sy);                                 //sample vector
                        sample = sample.RotateRad(imgRotate);                               //transform sample vector

                        sample = vCur.Add(sample);                                          //get actual sample location
                        int wv = img.GetPixel((int)sample.X, (int)sample.Y).R;              //get pixel

                        avg += wv;                                                          //accumulate average
                    }

                    //get average value
                    avg /= CellSample;

                    //set cell boolean
                    colsTmp[k] = avg < refBlack;

                    //move right
                    vCur = vCur.Add(vR);
                }
                ErrorCount += output.SetRow(colsTmp);

                // move vectors
                vRow = vRow.Add(vD);
                vCur = vRow;

            } // end while read next line
        } // end void readCells
    }
}
