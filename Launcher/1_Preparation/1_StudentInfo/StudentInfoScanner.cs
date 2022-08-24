using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Drawing;

using EDKv5;
using EDKv5.Utility;
using EDKv5.Utility.Scanners;
using System.Globalization;
using System.Windows;

namespace Launcher.Scanners
{
    public class StudentInfoScanner : ExcelScanner<Student>
    {
        public event EventHandler RequestDisplay;

        //constructor
        public StudentInfoScanner(IAskFirstRow afr, IManualMatch matcher)
        {
            this.askFirstRow = afr;
            this.matcher = matcher;
        }

        //fields
        Regex regStuId = new Regex(@"^[A-Za-z]?\d{4,}$", RegexOptions.IgnoreCase);
        Regex regCls = new Regex(@"[0-9][a-z]", RegexOptions.IgnoreCase);

        //Threads
        Semaphore semaphore = new Semaphore(0, 1);

        IAskFirstRow askFirstRow;
        IManualMatch matcher;

        protected override string[] getColumnNames()
        {
            return new string[]
            {
                "sid",
                "cls",
                "cls_no",
                "stu_name",
                "dob",
                "gender",
                "house",
            };
        }
        protected override string[] getColumnDisplayNames()
        {
            return new string[]
            {
                Properties.Resources.strStudentID,
                Properties.Resources.strClass,
                Properties.Resources.strClassNumber,
                Properties.Resources.strStudentName,
                Properties.Resources.strDOB,
                Properties.Resources.strGender,
                Properties.Resources.strHouse,
            };
        }
        protected override short[] optionalColumns { get { return new short[] { 0 }; } }

        protected override bool analyzeColumn(dynamic[][] row, ref short[] index)
        {
            //convert row to string
            string[] cols = new string[row[1].Length];
            for (int i = 0; i < row[1].Length; i++)
            {
                string val = Convert.ToString(row[1][i]);
                cols[i] = val;
            }

            for (short i = 0; i < cols.Length; i++)
            {
                if (null == cols[i]) continue;
                int intval;
                DateTime dob;

                //analyse student id
                if (index[0] < 0)
                {
                    if (regStuId.IsMatch(cols[i]))
                    {
                        index[0] = i;
                        continue;
                    }
                }

                //analyse class
                if (index[1] < 0 && cols[i].Length == 2)
                {
                    if (regCls.IsMatch(cols[i]))
                    {
                        index[1] = i;
                        continue;
                    }
                }

                //analyse class number
                if (index[2] < 0)
                {
                    if (cols[i].Length < 3 && Int32.TryParse(cols[i], out intval))
                    {
                        //
                        int x = Convert.ToInt32(row[0][i]);
                        int y = Convert.ToInt32(row[1][i]);
                        int z = Convert.ToInt32(row[2][i]);

                        if (x < y && y < z)
                        {
                            index[2] = i;
                            continue;
                        }
                    }
                }

                //analyse date of birth
                if (index[4] < 0 && DateTime.TryParse(cols[i], out dob) ||  // parse by system date format
                    DateTime.TryParseExact(cols[i], "dd/mm/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dob)    // protect against incorrect selection of system date format
                )
                {
                    index[4] = i;
                    continue;
                }

                //analyse gender
                if (index[5] < 0)
                {
                    if (cols[i] == "M" || cols[i] == "F")
                    {
                        index[5] = i;
                        continue;
                    }
                }

                //analyse house
                if (index[2] > -1 && index[3] > -1 && index[6] < 0 && cols[i].Length == 1)
                {
                    char c = cols[i][0];
                    if (c >= 'A' && c <= 'Z' || c >= '1' && c <= '9')
                    {
                        index[6] = i;
                        continue;
                    }
                }

                //analyse name
                if (index[3] < 0)
                {
                    if (cols[i].Length == 3)
                    {
                        index[3] = i;
                    }
                    else
                    {
                        //count space
                        intval = 0;
                        foreach (char c in cols[i])
                            if (c == ' ') intval++;

                        if (intval >= 2 && intval <= 3)
                            index[3] = i;
                    }
                }
            }

            //check index
            for (int i = 0; i < index.Length; i++)
            {
                if (i > 0 && index[i] < 0) return false;         // i > 0 ==> student ID is optional
                for (int j = i + 1; j < index.Length; j++)
                    if (index[i] == index[j]) return false;
            }

            return true;
        }


        protected override bool askFirstColumn(dynamic[,] range)
        {
            askFirstRow.Initialize(range, getColumnNames(), semaphore);

            if (null != RequestDisplay)
                this.RequestDisplay(askFirstRow, EventArgs.Empty);

            semaphore.WaitOne();
            return askFirstRow.FirstRowIsHeading;
        }

        protected override bool askLackColumn(string[] requiredCols, dynamic[] valueRow, ref short[] index)
        {
            Tuple<string, int>[] tps = new Tuple<string, int>[index.Length];

            //init
            for (int i = 0; i < tps.Length; i++)
            {
                tps[i] = new Tuple<string, int>(requiredCols[i], index[i]);
            }

            //copy convert
            string[] valRowStr = new string[valueRow.Length];
            for (int i = 0; i < valueRow.Length; i++)
            {
                if (valueRow[i] is DateTime)
                    valRowStr[i] = ((DateTime)valueRow[i]).ToString("d-MMM-yyyy");
                else
                    valRowStr[i] = Convert.ToString(valueRow[i]);
            }

            //init
            matcher.Initialize(tps, valRowStr, semaphore);

            
            if (null != RequestDisplay)
                this.RequestDisplay(matcher, EventArgs.Empty);
            

            //wait
            semaphore.WaitOne();
            index = matcher.Result;

            return !matcher.Cancelled;
        }

        protected override Student objectCreationCallback(dynamic[] range, short[] index)
        {
            try
            {
                string tmp;

                string sid = index[0] < 0 ? "" : range[index[0]];       // SID is optional
                string clsNm = range[index[1]];
                int clsNo = Convert.ToInt32(range[index[2]]);
                string name = range[index[3]];

                DateTime dob;
                if (range[index[4]] is DateTime)
                {
                    dob = range[index[4]];
                }
                else
                {
                    tmp = range[index[4]];
                    try { dob = DateTime.Parse(tmp); }
                    catch { dob = DateTime.ParseExact(tmp, "dd/mm/yyyy", CultureInfo.InvariantCulture); }
                }

                char gender = Convert.ToString(range[index[5]])[0];
                char hsid = Convert.ToString(range[index[6]])[0];

                //get instance by id
                Class cls;
                House house;
                Project project = Project.GetInstance();
                bool ba = project.TryGetClass(clsNm, out cls);
                bool bb = project.TryGetHouse(hsid, out house);
                if (!ba || !bb)      //get both 2 data first
                {
                    //create class
                    if (null == cls)
                    {
                        cls = project.CreateClass(
                            Convert.ToInt32(clsNm.Substring(0, 1)),
                            clsNm[1]
                        );
                    }

                    //create house
                    if (null == house)
                    {
                        house = project.CreateHouse(
                            hsid, "Noname House [" + hsid + "]",
                            Color.White
                        );
                    }

                }

                Student stu = new Student(sid, name, cls, clsNo, house, gender, dob);
                return stu;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                String.Format("clsNm:{0}, clsNo:{1}, name:{2}, dob:{3}, gender:{4}, hsid:{5}",
                    range[index[1]],
                    range[index[2]],
                    range[index[3]],
                    range[index[4]],
                    range[index[5]],
                    range[index[6]]
                ));
                throw ex;
            }
        }

    }

}
