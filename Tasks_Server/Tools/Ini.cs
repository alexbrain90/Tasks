using System;
using System.IO;
using System.Text;

namespace Tasks_Server
{
    /// <summary>
    /// Класс для работы с ini-файлами
    /// </summary>
    class Ini
    {
        /// <summary>
        /// Секция ini-файла
        /// </summary>
        private struct Section
        {
            public string Name;
            public string[] Param;
            public string[] Value;
        }
        /// <summary>
        /// Имя файла
        /// </summary>
        private string FileName;
        /// <summary>
        /// Массив секций
        /// </summary>
        private Section[] Sections = new Section[0];

        public Ini(string fileName)
        {
            FileName = fileName;
            string line, param, value, section = "";
            int n;

            if (File.Exists(fileName) == false)
                File.Create(fileName).Close();

            StreamReader sr = new StreamReader(FileName, Encoding.Default);
            while (sr.EndOfStream == false)
            {
                line = sr.ReadLine();
                if (line.Length == 0)
                    continue;

                if (line.Substring(0, 1) == "[")
                {
                    section = line.Substring(1, line.Length - 2);
                    Sections = AddSection(Sections, section);
                }
                else
                {
                    n = line.IndexOf(" = ");
                    param = line.Substring(0, n);
                    value = line.Substring(n + 3);
                    Sections = AddParamValue(Sections, section, param, value);
                }
            }
            sr.Close();
        }

        public string Read(string section, string param)
        {
            for (int i = 0; i < Sections.Length; i++)
                if (Sections[i].Name == section)
                    for (int j = 0; j < Sections[i].Param.Length; j++)
                        if (Sections[i].Param[j] == param)
                            return Sections[i].Value[j];

            return "";
        }
        public void Write(string section, string param, string value)
        {
            Sections = AddParamValue(Sections, section, param, value);
        }
        public bool Save()
        {
            try
            {
                StreamWriter sw = new StreamWriter(FileName + ".tmp", false, Encoding.Default);
                for (int i = 0; i < Sections.Length; i++)
                {
                    sw.WriteLine("[" + Sections[i].Name + "]");
                    for (int j = 0; j < Sections[i].Param.Length; j++)
                        if (Sections[i].Value[j] != "")
                            sw.WriteLine(Sections[i].Param[j] + " = " + Sections[i].Value[j]);
                }
                sw.Close();

                File.Copy(FileName + ".tmp", FileName, true);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private Section[] AddSection(Section[] list, string newSection)
        {
            for (int i = 0; i < list.Length; i++)
                if (list[i].Name == newSection)
                    return list;

            Section[] result = new Section[list.Length + 1];
            list.CopyTo(result, 0);
            result[list.Length].Name = newSection;
            result[list.Length].Param = new string[0];
            result[list.Length].Value = new string[0];

            return result;
        }
        private Section[] AddParamValue(Section[] list, string section, string param, string value)
        {
            int s = -1, p = -1;

            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].Name == section)
                {
                    s = i;
                    for (int j = 0; j < list[i].Param.Length; j++)
                    {
                        if (list[i].Param[j] == param)
                        {
                            p = j;
                            break;
                        }
                    }
                    break;
                }
            }

            if (s == -1)
            {
                list = AddSection(list, section);
                s = list.Length - 1;
            }
            if (p == -1)
            {
                list[s].Param = AddString(list[s].Param, param);
                list[s].Value = AddString(list[s].Value, value);
            }
            else
            {
                list[s].Value[p] = value;
            }

            return list;
        }
        private string[] AddString(string[] list, string value)
        {
            string[] result = new string[list.Length + 1];

            list.CopyTo(result, 0);
            result[list.Length] = value;

            return result;
        }
    }
}