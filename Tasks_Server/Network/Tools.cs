using System;
using System.Net.Sockets;
using System.Threading;

namespace Tasks_Server
{
    partial class Network
    {
        static public int BytesR = 0, BytesS = 0, Requests = 0;

        static private byte[] StringToByte(string text)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
            byte[] result = new byte[data.Length + 4];
            BitConverter.GetBytes(data.Length).CopyTo(result, 0);
            data.CopyTo(result, 4);

            return result;
        }
        static private byte[] StringToByte(int value)
        {
            return StringToByte(value.ToString());
        }
        static private byte[] StringToByte(long value)
        {
            return StringToByte(value.ToString());
        }
        static private byte[] StringToByte(bool value)
        {
            if (value == true)
                return StringToByte("1");
            else
                return StringToByte("0");
        }
        static private string ByteToString(byte[] data)
        {
            int len = BitConverter.ToInt32(data, 0);
            return System.Text.Encoding.UTF8.GetString(data, 4, len);
        }

        static private byte[] ByteCut(byte[] data, int start)
        {
            byte[] result = new byte[data.Length - start];
            for (int i = start; i < data.Length; i++)
                result[i - start] = data[i];
            return result;
        }
        static private byte[] ByteCut(byte[] data)
        {
            data = ByteCut(data, BitConverter.ToInt32(data, 0) + 4);
            return data;
        }
        static private byte[] ByteAdd(byte[] list1, byte[] list2)
        {
            byte[] result = new byte[list1.Length + list2.Length];
            list1.CopyTo(result, 0);
            list2.CopyTo(result, list1.Length);
            return result;
        }

        static private string HashToString(byte[] data)
        {
            string result = "";
            for (int i = 0; i < data.Length; i++)
                result += data[i].ToString("X2");
            return result;
        }
        static private byte[] HashCompute(byte[] data)
        {
            return new System.Security.Cryptography.SHA1CryptoServiceProvider().ComputeHash(data);
        }

        static private string[,] StringAdd(string[,] list1, string[,] list2, int n)
        {
            int x1 = list1.Length / n;
            int x2 = list2.Length / n;

            string[,] result = new string[x1 + x2, n];
            for(int i = 0; i < x1; i++)
                for (int j = 0; j < n; j++)
                    result[i, j] = list1[i, j];
            for (int i = 0; i < x2; i++)
                for (int j = 0; j < n; j++)
                    result[x1 + i, j] = list2[i, j];

            return result;
        }

        static public string FioToShort(string text)
        {
            int n1 = text.IndexOf(" ");
            int n2 = text.IndexOf(" ", n1 + 1);
            string f = text.Substring(0, n1);
            string i = text.Substring(n1 + 1, n2 - n1);
            string o = text.Substring(n2 + 1);
            return f + " " + i.Substring(0, 1).ToUpper() + "." + o.Substring(0, 1).ToUpper() + ".";
        }

        static public string DateTimeToString(long ticks)
        {
            DateTime dt = new DateTime(ticks);
            return dt.Day.ToString("00") + "." + dt.Month.ToString("00") + "." + dt.Year.ToString("0000");
        }
    }
}