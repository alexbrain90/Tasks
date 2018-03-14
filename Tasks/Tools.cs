using System;
using System.IO;

namespace Tasks
{
    static class Tools
    {
        #region Дата и время
        /* Формат строки
         * MMMMm - месяц полностью
         * MMMMM - месяц полностью с большой буквы
         * MMMm - месяц полностью
         * MMMM - месяц полностью с большой буквы
         * YYYY - год
         * MM - месяц числом (1 -> 01)
         * DD - день числом (1 -> 01)
         * M - месяц числом (1 -> 1)
         * D - день числом (1 -> 1)
         * 
         * mmm - милисекунды (1 -> 001)
         * hh - часы (1 -> 01)
         * mm - минуты (1 -> 01)
         * ss - секунды (1 -> 01)
         * h - часы
         * m - минуты
         * s - секунды
        */
        static public string DateToString(DateTime date, string format)
        {
            format = format.Replace("MMMMm", MonthToString2(date.Month, false));
            format = format.Replace("MMMMM", MonthToString2(date.Month, true));
            format = format.Replace("MMMM", MonthToString(date.Month, true));
            format = format.Replace("YYYY", date.Year.ToString("0000"));
            format = format.Replace("DD", date.Day.ToString("00"));
            format = format.Replace("MM", date.Month.ToString("00"));
            format = format.Replace("D", date.Day.ToString());
            format = format.Replace("M", date.Month.ToString());

            return format;
        }
        static public string DateToString(long date, string format)
        {
            return DateToString(new DateTime(date), format);
        }
        static public string TimeToString(DateTime time, string format)
        {
            format = format.Replace("mmm", time.Millisecond.ToString("000"));
            format = format.Replace("hh", time.Hour.ToString("00"));
            format = format.Replace("mm", time.Minute.ToString("00"));
            format = format.Replace("ss", time.Second.ToString("00"));
            format = format.Replace("h", time.Hour.ToString());
            format = format.Replace("m", time.Minute.ToString());
            format = format.Replace("s", time.Second.ToString());

            return format;
        }
        static public string TimeToString(long time, string format)
        {
            return TimeToString(new DateTime(time), format);
        }
        static public string MonthToString(int m, bool upper)
        {
            string result;
            switch (m)
            {
                case 1:
                    result = "январь";
                    break;
                case 2:
                    result = "февраль";
                    break;
                case 3:
                    result = "март";
                    break;
                case 4:
                    result = "апрель";
                    break;
                case 5:
                    result = "май";
                    break;
                case 6:
                    result = "июнь";
                    break;
                case 7:
                    result = "июль";
                    break;
                case 8:
                    result = "август";
                    break;
                case 9:
                    result = "сентябрь";
                    break;
                case 10:
                    result = "октябрь";
                    break;
                case 11:
                    result = "ноябрь";
                    break;
                case 12:
                    result = "декабрь";
                    break;

                default:
                    return "";
            }

            if (upper == true)
                result = result.Substring(0, 1).ToUpper() + result.Substring(1);

            return result;
        }
        static public string MonthToString2(int m, bool upper)
        {
            string result;
            switch (m)
            {
                case 1:
                    result = "января";
                    break;
                case 2:
                    result = "февраля";
                    break;
                case 3:
                    result = "марта";
                    break;
                case 4:
                    result = "апреля";
                    break;
                case 5:
                    result = "мая";
                    break;
                case 6:
                    result = "июня";
                    break;
                case 7:
                    result = "июля";
                    break;
                case 8:
                    result = "августа";
                    break;
                case 9:
                    result = "сентября";
                    break;
                case 10:
                    result = "октября";
                    break;
                case 11:
                    result = "ноября";
                    break;
                case 12:
                    result = "декабря";
                    break;

                default:
                    return "";
            }

            if (upper == true)
                result = result.Substring(0, 1).ToUpper() + result.Substring(1);

            return result;
        }

        static public string DateTimeToString(DateTime dateTime, string format)
        {
            format = DateToString(dateTime, format);
            format = TimeToString(dateTime, format);

            return format;
        }
        static public string DateTimeToString(long dateTime, string format)
        {
            return DateTimeToString(new DateTime(dateTime), format);
        }

        static public string DateIntervalToID(long dt1, long dt2)
        {
            return DateIntervalToID(new DateTime(dt1), new DateTime(dt2));
        }
        static public string DateIntervalToID(DateTime dt1, DateTime dt2)
        {
            if (dt1.Year != dt2.Year)
                return "";

            DateTime tmp1 = dt1.AddMonths(1), tmp2 = dt2.AddDays(1);
            if (dt1.Month == 1 && dt1.Day == 1 && dt2.Month == 12 && dt2.Day == 31)
                return "Y" + dt1.Year.ToString("0000");

            else if (dt1.Day == 1 && tmp2.Day == 1)
            {
                if (dt1.Month == 1 && dt2.Month == 3)
                    return "Q1Y" + dt1.Year.ToString("0000");
                else if (dt1.Month == 4 && dt2.Month == 6)
                    return "Q2Y" + dt1.Year.ToString("0000");
                else if (dt1.Month == 7 && dt2.Month == 9)
                    return "Q3Y" + dt1.Year.ToString("0000");
                else if (dt1.Month == 10 && dt2.Month == 12)
                    return "Q4Y" + dt1.Year.ToString("0000");
                else if (dt1.Month == dt2.Month)
                    return "M" + dt1.Month.ToString("00") + "Y" + dt1.Year.ToString("0000");
            }

            else if (dt1.Month == dt2.Month && dt1.Day != dt2.Day)
            {
                if (dt1.Day == 1 && dt2.Day == 10)
                    return "D1M" + dt1.Month.ToString("00") + "Y" + dt1.Year.ToString("0000");
                else if (dt1.Day == 11 && dt2.Day == 20)
                    return "D2M" + dt1.Month.ToString("00") + "Y" + dt1.Year.ToString("0000");
                else if (dt1.Day == 21 && tmp2.Day == 1)
                    return "D3M" + dt1.Month.ToString("00") + "Y" + dt1.Year.ToString("0000");
            }
            
            else if (dt1.Day == dt2.Day)
            {
                return "S" + dt1.Day.ToString("00")+ "M" + dt1.Month.ToString("00") + "Y" + dt1.Year.ToString("0000");
            }

            return "";
        }
        static public string DateIntervalToString(long dt1, long dt2)
        {
            return DateIntervalToString(DateIntervalToID(dt1, dt2));
        }
        static public string DateIntervalToString(string interval)
        {
            if (interval == "")
                return "";

            string id = interval.Substring(0, 1);
            string value = interval.Substring(1);
            string result = "";

            if (id == "Y")
                return value + " год";
            else if (id == "Q")
            {
                id = value.Substring(0, 1);
                value = value.Substring(1);
                if (id == "1")
                    result = "I квартал";
                else if (id == "2")
                    result = "II квартал";
                else if (id == "3")
                    result = "III квартал";
                else if (id == "4")
                    result = "IV квартал";
                else
                    return "";

                id = value.Substring(0, 1);
                value = value.Substring(1);
                if (id != "Y")
                    return "";
                result += " " + value;
            }
            else if (id == "M")
            {
                id = value.Substring(0, 2);
                value = value.Substring(2);
                result = MonthToString(Convert.ToInt32(id), true);

                id = value.Substring(0, 1);
                value = value.Substring(1);
                if (id != "Y")
                    return "";
                result += " " + value;
            }
            else if (id == "D")
            {
                id = value.Substring(0, 1);
                value = value.Substring(1);
                result = id + " декада";

                id = value.Substring(0, 1);
                value = value.Substring(1);
                if (id != "M")
                    return "";
                id = value.Substring(0, 2);
                value = value.Substring(2);
                result += " " + MonthToString2(Convert.ToInt32(id), true);

                id = value.Substring(0, 1);
                value = value.Substring(1);
                if (id != "Y")
                    return "";
                result += " " + value;
            }
            else if (id == "S")
            {
                id = value.Substring(0, 2);
                value = value.Substring(2);
                result = Convert.ToInt32(id).ToString();

                id = value.Substring(0, 1);
                value = value.Substring(1);
                if (id != "M")
                    return "";
                id = value.Substring(0, 2);
                value = value.Substring(2);
                result += " " + MonthToString2(Convert.ToInt32(id), true);

                id = value.Substring(0, 1);
                value = value.Substring(1);
                if (id != "Y")
                    return "";
                result += " " + value;
            }

            return result;
        }
        #endregion

        static public string FioToShort(string text)
        {
            int n1 = text.IndexOf(" ");
            int n2 = text.IndexOf(" ", n1 + 1);
            string f = text.Substring(0, n1);
            string i = text.Substring(n1 + 1, n2 - n1);
            string o = text.Substring(n2 + 1);
            return f + " " + i.Substring(0, 1).ToUpper() + "." + o.Substring(0, 1).ToUpper() + ".";
        }

        static public void MakePlan(DateTime dt1, DateTime dt2)
        {
            //DateTime dt0 = new DateTime(dt1.Year, dt1.Month, dt1.Day);
            //dt0 = dt0.AddTicks(-1);

            string fileName = Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".xlsx";

            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            fs.Write(Tasks.Properties.Resources.blank_Plan, 0, Tasks.Properties.Resources.blank_Plan.Length);
            fs.Close();

            ClosedXML.Excel.XLWorkbook xL = new ClosedXML.Excel.XLWorkbook(fileName);
            string[,] list = Network.Task_List_Plan(dt1.Ticks, dt2.Ticks);
            if (list.Length == 0)
            {
                System.Windows.Forms.MessageBox.Show("Нет задач для отображения в плане за указанный период", "Формирование плана", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                return;
            }

            xL.Worksheet("План").Cell(1, 1).Value = "Рабочий план на " + Tools.DateTimeToString(dt1, "MMMM YYYY").ToUpper() + " года";
            xL.Worksheet("План").Cell(2, 1).Value = Program.user_Fio;
            xL.Worksheet("План").Cell(3, 1).Value = Program.user_Post;

            int n = 0;
            for (int i = 0; i < list.Length / 4; i++)
            {
                if (n != 0)
                {
                    xL.Worksheet("План").Row(5 + n).InsertRowsBelow(1);
                    for (int j = 1; j <= 4; j++)
                        xL.Worksheet("План").Cell(6 + n, j).Style = xL.Worksheet("План").Cell(5 + n, j).Style;
                }
                n++;

                xL.Worksheet("План").Cell(6 + i, 1).Value = (i + 1).ToString();
                xL.Worksheet("План").Cell(6 + i, 2).Value = list[i, 0].Replace("%newline%", "\r\n");
                xL.Worksheet("План").Cell(6 + i, 3).Value = "с " + Tools.DateToString(Convert.ToInt64(list[i, 1]), "D MMMMm") + "\r\nпо " + Tools.DateToString(Convert.ToInt64(list[i, 2]), "D MMMMm");
                xL.Worksheet("План").Cell(6 + i, 4).Value = list[i, 3];
            }

            xL.Worksheet("План").Cell(list.Length / 4 + 7, 1).Value = Tools.FioToShort(Program.user_Fio) + "     ____________________     ";

            xL.Save();
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = fileName;
            p.StartInfo.UseShellExecute = true;
            p.Start();
            System.Threading.Thread.Sleep(1000);
        }
        static public void MakeReport(DateTime dt1, DateTime dt2)
        {
            string fileName = Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".xlsx";
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            fs.Write(Tasks.Properties.Resources.blank_Report, 0, Tasks.Properties.Resources.blank_Report.Length);
            fs.Close();

            ClosedXML.Excel.XLWorkbook xL = new ClosedXML.Excel.XLWorkbook(fileName);
            string[,] list = Network.Task_List_Report(dt1.Ticks, dt2.Ticks);
            if (list.Length == 0)
            {
                System.Windows.Forms.MessageBox.Show("Нет задач для отображения в отчете за указанный период", "Формирование отчета", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                return;
            }

            xL.Worksheet("Отчет").Cell(1, 1).Value = "Отчет по рабочему плану за " + Tools.DateTimeToString(dt1, "MMMM YYYY").ToUpper() + " года";
            xL.Worksheet("Отчет").Cell(2, 1).Value = Program.user_Fio;
            xL.Worksheet("Отчет").Cell(3, 1).Value = Program.user_Post;
            int n = 0;
            for (int i = 0; i < list.Length / 10; i++)
            {
                long taskA = Convert.ToInt64(list[i, 3]);
                if (taskA >= dt1.Ticks)
                    continue;

                if (n != 0)
                {
                    xL.Worksheet("Отчет").Row(5 + n).InsertRowsBelow(1);
                    for (int j = 1; j <= 6; j++)
                        xL.Worksheet("Отчет").Cell(6 + n, j).Style = xL.Worksheet("Отчет").Cell(5 + n, j).Style;
                }
                n++;

                xL.Worksheet("Отчет").Cell(5 + n, 1).Value = n.ToString();
                xL.Worksheet("Отчет").Cell(5 + n, 2).Value = list[i, 0].Replace("%newline%", "\r\n");
                xL.Worksheet("Отчет").Cell(5 + n, 3).Value = "с " + Tools.DateToString(Convert.ToInt64(list[i, 1]), "D MMMMm") + "\r\nпо " + Tools.DateToString(Convert.ToInt64(list[i, 2]), "D MMMMm");
                xL.Worksheet("Отчет").Cell(5 + n, 5).Value = list[i, 5];

                string progress = "", progressCom = "";
                long taskE = Convert.ToInt64(list[i, 2]), taskF = Convert.ToInt64(list[i, 4]);
                if (taskF != 0)
                    progress = "Выполнено" + "\r\n" + Tools.DateToString(taskF, "D MMMMm");
                else if (list[i, 6] != "0")
                {
                    progress = "Завершено на " + list[i, 6] + "%";
                    progressCom = "Выполнено:\r\n" + list[i, 7].Replace("%ns%", "\r\n") + "\r\n\r\nВ работе:\r\n" + list[i, 8].Replace("%ns%", "\r\n");
                }
                else if (taskE > DateTime.Now.Ticks)
                    progress = "В работе";
                else
                    progress = "Не выполнено";
                xL.Worksheet("Отчет").Cell(5 + n, 4).Value = progress;
                if (progressCom != "")
                {
                    xL.Worksheet("Отчет").Cell(5 + n, 4).Comment.AddText(progressCom);
                    xL.Worksheet("Отчет").Cell(5 + n, 4).Comment.Style.Alignment.SetAutomaticSize();
                }

                if (list[i, 9] != "")
                {
                    int k = list[i, 9].LastIndexOf("%nm%");
                    if (k == -1)
                        xL.Worksheet("Отчет").Cell(5 + n, 6).Value = list[i, 9];
                    else
                    {
                        xL.Worksheet("Отчет").Cell(5 + n, 6).Value = list[i, 9].Substring(k + 4);
                        xL.Worksheet("Отчет").Cell(5 + n, 6).Comment.AddText(list[i, 9].Replace("%nm%", "\r\n\r\n"));
                        xL.Worksheet("Отчет").Cell(5 + n, 6).Comment.Style.Alignment.SetAutomaticSize();
                    }
                }
            }

            n++;
            if (n == 1)
                n++;
            int nt = n;
            for (int i = 0; i < list.Length / 10; i++)
            {
                long taskA = Convert.ToInt64(list[i, 3]);
                if (taskA < dt1.Ticks)
                    continue;

                if (n != nt)
                {
                    xL.Worksheet("Отчет").Row(n + 5).InsertRowsBelow(1);
                    for (int j = 1; j <= 5; j++)
                        xL.Worksheet("Отчет").Cell(n + 6, j).Style = xL.Worksheet("Отчет").Cell(n + 5, j).Style;
                }
                n++;

                xL.Worksheet("Отчет").Cell(n + 5, 1).Value = (i + 1).ToString();
                xL.Worksheet("Отчет").Cell(n + 5, 2).Value = list[i, 0].Replace("%newline%", "\r\n");
                xL.Worksheet("Отчет").Cell(n + 5, 3).Value = "с " + Tools.DateToString(Convert.ToInt64(list[i, 1]), "D MMMMm") + "\r\nпо " + Tools.DateToString(Convert.ToInt64(list[i, 2]), "D MMMMm");
                xL.Worksheet("Отчет").Cell(n + 5, 5).Value = list[i, 5];

                string progress = "", progressCom = "";
                long taskE = Convert.ToInt64(list[i, 2]), taskF = Convert.ToInt64(list[i, 4]);
                if (taskF != 0)
                    progress = "Выполнено" + "\r\n" + Tools.DateToString(taskF, "D MMMMm");
                else if (list[i, 6] != "0")
                {
                    progress = "Завершено на " + list[i, 6] + "%";
                    progressCom = "Выполнено:\r\n" + list[i, 7].Replace("%ns%", "\r\n") + "\r\n\r\nВ работе:\r\n" + list[i, 8].Replace("%ns%", "\r\n");
                }
                else
                    progress = "Не выполнено";
                xL.Worksheet("Отчет").Cell(n + 5, 4).Value = progress;
                if (progressCom != "")
                {
                    xL.Worksheet("Отчет").Cell(5 + n, 4).Comment.AddText(progressCom);
                    xL.Worksheet("Отчет").Cell(5 + n, 4).Comment.Style.Alignment.SetAutomaticSize();
                }

                if (list[i, 9] != "")
                {
                    int k = list[i, 9].LastIndexOf("%nm%");
                    if (k == -1)
                        xL.Worksheet("Отчет").Cell(5 + n, 6).Value = list[i, 9];
                    else
                    {
                        xL.Worksheet("Отчет").Cell(5 + n, 6).Value = list[i, 9].Substring(k + 4);
                        xL.Worksheet("Отчет").Cell(5 + n, 6).Comment.AddText(list[i, 9].Replace("%nm%", "\r\n\r\n"));
                        xL.Worksheet("Отчет").Cell(5 + n, 6).Comment.Style.Alignment.SetAutomaticSize();
                    }
                }
            }

            xL.Worksheet("Отчет").Cell(list.Length / 7 + 9, 1).Value = Tools.FioToShort(Program.user_Fio) + "     ____________________     ";

            xL.Save();
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = fileName;
            p.StartInfo.UseShellExecute = true;
            p.Start();
            System.Threading.Thread.Sleep(1000);
        }
    }
}
