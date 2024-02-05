using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Text.RegularExpressions;

namespace MbrTextGenerator;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    private void button_Click(object sender, RoutedEventArgs e)
    {

        //try
        //{
        string ALLCode = textBox.Text.Replace("\r", "");
        string[] CodeLine = ALLCode.Split(new string[] { "\n" }, StringSplitOptions.None); //分割换行

        int IP = 0;
        int BaseAddress = 0x7C00;
        List<byte> bin = new List<byte>();

        ASM.Screen(bin, ref IP); //Screen

        bin.AddRange(new byte[] { 0x8C, 0xC8 }); //MOV ax,cs
        IP += 2;
        bin.AddRange(new byte[] { 0x8E, 0xD8 }); //MOV ds,ax
        IP += 2;
        bin.AddRange(new byte[] { 0x8E, 0xC0 }); //MOV es,ax
        IP += 2;
        //bin.AddRange(new byte[] { 0xB8, 0x00, 0x00 }); //MOV ax,0x0000
        //IP += 3;
        //bin.AddRange(new byte[] { 0xB9, 0x00, 0x00 }); //MOV cx,0x0000
        //IP += 3;
        //bin.AddRange(new byte[] { 0x89, 0xC5 }); //MOV bp,ax
        //IP += 2;
        bin.AddRange(new byte[] { 0xB4, 0x13 }); //MOV ah,0x13
        IP += 2;
        bin.AddRange(new byte[] { 0xB0, 0x00 }); //MOV al,0x00
        IP += 2;
        bin.AddRange(new byte[] { 0xB7, 0x00 }); //MOV bh,0x00
        IP += 2;
        bin.AddRange(new byte[] { 0xB3, 0x0F }); //MOV bl,0x0F
        IP += 2;
        bin.AddRange(new byte[] { 0xB6, 0x00 }); //MOV dh,0x00
        IP += 2;
        bin.AddRange(new byte[] { 0xB2, 0x00 }); //MOV dl,0x00
        IP += 2;
        int StringIP = IP;
        List<byte> StringBin = new List<byte>();
        //
        foreach (string temp in CodeLine)
        {
            if (temp.ToUpper() == "START()")
            {
                StringIP += 5;
            }
            else if (temp.Length >= 9 && temp.ToUpper().Substring(0, 7) == "WRITE(\"" && temp.ToUpper().Substring(temp.Length - 2, 2) == "\")") //write("")
            {
                StringIP += 8;
            }
            else if (temp.Length >= 7 && temp.Substring(0, 6).ToUpper() == "POINT(" && temp.Substring(temp.Length - 1, 1) == ")") //
            {
                StringIP += 3;
            }
            else if (temp.Length >= 3 && temp.Substring(0, 2).ToUpper() == "X(" && temp.Substring(temp.Length - 1, 1) == ")")
            {
                StringIP += 2;
            }
            else if (temp.Length >= 3 && temp.Substring(0, 2).ToUpper() == "Y(" && temp.Substring(temp.Length - 1, 1) == ")")
            {
                StringIP += 2;
            }
            else if (temp.Length >= 7 && temp.Substring(0, 6).ToUpper() == "COLOR(" && temp.Substring(temp.Length - 1, 1) == ")")
            {
                StringIP += 2;
            }
        }
        StringIP += 2; //FA F4
        StringIP += BaseAddress; //0x7C00
        int Line = 0;
        foreach (string code in CodeLine)
        {
            Line++;
            if (code.Length >= 9 && code.ToUpper().Substring(0, 7) == "WRITE(\"" && code.Substring(code.Length - 2, 2) == "\")") //WRITE("")
            {
                string Parameter;
                try
                {
                    Parameter = Regex.Unescape(code.Substring(7, code.Length - 9));
                }
                catch (Exception UnescapeError)
                {
                    Console.WriteLine("转义错误\n" + UnescapeError.Message);
                    return;
                }


                int Foront = ASM.HexH(StringIP); //0~2
                int After = ASM.HexL(StringIP); //2~2

                string strbin = Parameter; //write bin
                                           //MessageBox.Show("Write:" + strbin);
                int length = Encoding.ASCII.GetBytes(strbin).Length; //write长度
                StringIP += length;
                int LH = ASM.HexH(length); //H
                int LL = ASM.HexL(length); //L

                bin.AddRange(new byte[] { 0xB8, (byte)After, (byte)Foront }); //MOV ax,0x7C00 + StringIP
                bin.AddRange(new byte[] { 0x89, 0xC5 }); //MOV bp,ax
                bin.AddRange(new byte[] { 0xB9, (byte)LL, (byte)LH }); //MOV cx,Length

                StringBin.AddRange(Encoding.ASCII.GetBytes(strbin));
                //Console.WriteLine("Write: " + Parameter);
            }
            else if (code.ToUpper() == "START()") //START()
            {
                bin.AddRange(new byte[] { 0xB8, 0x00, 0x13 });
                bin.AddRange(new byte[] { 0xCD, 0x10 });
                //Console.WriteLine("Start:");
            }
            else if (code.Length >= 3 && code.Substring(0, 2).ToUpper() == "X(" && code.Substring(code.Length - 1, 1) == ")") //X()
            {
                try
                {
                    string Parameter = code.Substring(2, code.Length - 3);
                    bin.AddRange(new byte[] { 0xB2, (byte)ASM.StringToInt32(Parameter) });
                    //Console.WriteLine("X: " + Parameter);
                }
                catch (Exception Error)
                {
                    MessageBox.Show(string.Format("第{0}行出现错误\n{1}", Line, code));
                    return;
                }

            }
            else if (code.Length >= 3 && code.Substring(0, 2).ToUpper() == "Y(" && code.Substring(code.Length - 1, 1) == ")") //Y()
            {
                try
                {
                    string Parameter = code.Substring(2, code.Length - 3);
                    bin.AddRange(new byte[] { 0xB6, (byte)ASM.StringToInt32(Parameter) });
                    //Console.WriteLine("Y: " + Parameter);
                }
                catch (Exception Error)
                {
                    MessageBox.Show(string.Format("第{0}行出现错误\n{1}", Line, code));
                    return;
                }
            }
            else if (code.Length >= 7 && code.Substring(0, 6).ToUpper() == "COLOR(" && code.Substring(code.Length - 1, 1) == ")") //COLOR()
            {
                string Parameter = code.Substring(6, code.Length - 7);
                if (Parameter.Length > 2)
                {
                    MessageBox.Show(string.Format("第{0}行出现错误\n{1}", Line, code));
                    return;
                }
                try
                {
                    bin.AddRange(new byte[] { 0xB3, (byte)(ASM.String2ByteToInt32L(Convert.ToInt32(Parameter, 16)) + ASM.String2ByteToInt32H(Convert.ToInt32(Parameter, 16)) * 16) });
                }
                catch (Exception Error)
                {
                    MessageBox.Show(string.Format("第{0}行出现错误\n{1}", Line, code));
                    return;
                }

                //Console.WriteLine("Color: " + Parameter);
            }
            else if (code.Length >= 7 && code.Substring(0, 6).ToUpper() == "POINT(" && code.Substring(code.Length - 1, 1) == ")") //POINT()
            {
                string[] Parameter = code.Substring(6, code.Length - 7).Split(',');
                if (Parameter.Length > 2)
                {
                    MessageBox.Show(string.Format("第{0}行出现错误\n{1}", Line, code));
                    return;
                }
                int H = Convert.ToInt32(Parameter[1], 10);
                int L = Convert.ToInt32(Parameter[0], 10);
                bin.AddRange(new byte[] { 0xBA, (byte)L, (byte)H });
                //Console.WriteLine("Color: " + Parameter);
            }
            else if (code.Trim() == "")
            {

            }
            else
            {
                MessageBox.Show(string.Format("第{0}行出现错误\n{1}", Line, code), "qwq");
                return;
                //return;
            }
        }
        //?????????emmm
        try
        {
            bin.AddRange(new byte[] { 0xFA, 0xF4 });
            bin.AddRange(StringBin.ToArray());

            byte[] b = new byte[512];
            for (int i = 0; i < bin.Count; i++)
            {
                b[i] = bin[i];
            }
            b[510] = 0x55;
            b[511] = 0xAA;

            if (checkBox.IsChecked == true)
            {
                FileStream file = new FileStream("MBR.BIN", FileMode.Create);
                file.Write(b, 0, 512);
                file.Close();
            }


            string Hex = "";
            //Hex += string.Join(",", b.AsEnumerable().Select(v => "0x" + v.ToString("X2")));
            Hex += "0x" + BitConverter.ToString(b).Replace("-", ",0x");

            CodeWindow codeWindow = new CodeWindow();
            codeWindow.textBox.Text = Hex;
            codeWindow.ShowDialog();

        }
        catch (Exception E)
        {
            MessageBox.Show(E.Message);
        }
        //}
        //catch (Exception Error)
        //{
        //    MessageBox.Show(Error.Message);
        //}


    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
        BitmapWindow bitmapWindow = new BitmapWindow();
        bitmapWindow.ShowDialog();
    }

    private void Grid_Loaded(object sender, RoutedEventArgs e)
    {

    }

    private void textBox_TextChanged(object sender, TextChangedEventArgs e)
    {

    }
}
