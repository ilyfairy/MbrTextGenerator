using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;

namespace MbrTextGenerator;

public partial class BitmapWindow : Window
{
    public BitmapWindow()
    {
        InitializeComponent();
    }

    internal static MBRColor CurrentPenColor = MBRColor.xF;
    public UserControl[,] pixels = new UserControl[80, 30];
    readonly Random r = new Random();

    private void Paint(MouseEventArgs e)
    {
        var pos = e.GetPosition(ePixel);
        if (new Rect(ePixel.RenderSize).Contains(pos))
        {
            int x, y;
            x = (int)(pos.X / pixels[0, 0].ActualWidth);
            y = (int)(pos.Y / pixels[0, 0].ActualHeight);
            try
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    pixels[x, y].Tag = CurrentPenColor;
                    pixels[x, y].Background = new SolidColorBrush(MBR.ColorValue[(int)CurrentPenColor]);
                }
            }
            catch (Exception) { }
            this.Title = $"x:{x} y:{y}";
        }
    }

    private void Pen_Move(object sender, MouseEventArgs e)
    {
        Paint(e);
    }

    private void Pen_Down(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            Paint(e);
        }
    }



    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender == eRandomColor)
        {
            foreach (var pixel in pixels)
            {
                var color = r.Next(0, 0x10);
                pixel.Tag = (MBRColor)color;
                pixel.Background = new SolidColorBrush(MBR.ColorValue[color]);
            }
        }
        else if (sender == ClearColorButton)
        {
            foreach (var pixel in pixels)
            {
                pixel.Tag = CurrentPenColor;
                pixel.Background = new SolidColorBrush(MBR.ColorValue[(int)CurrentPenColor]);
            }
        }
        else if (sender == eBuild)
        {
            try
            {
                int IP = 0;
                int BaseAddress = 0x7C00;
                List<byte> bin = new List<byte>();
                List<byte> TEXTbin = new List<byte>();

                ASM.Screen(bin, ref IP);
                ASM.LoadDisk(bin, ref IP, 0x20);

                int STRINGADDRESS = IP + BaseAddress;
                int S0 = ASM.HexL(STRINGADDRESS); //L
                int S2 = ASM.HexH(STRINGADDRESS); //H
                TEXTbin.AddRange(new byte[] { 0xB8, 0x00, 0x00 }); //MOV ax,0x0000
                //IP += 2;
                TEXTbin.AddRange(new byte[] { 0x8E, 0xD8 }); //MOV ds,ax
                //IP += 2;
                TEXTbin.AddRange(new byte[] { 0x8E, 0xC0 }); //MOV es,ax
                //IP += 2;
                TEXTbin.AddRange(new byte[] { 0xB8, (byte)S0, (byte)S2 }); //MOV ax,0x0000
                //IP += 3;
                TEXTbin.AddRange(new byte[] { 0xB9, 0x00, 0x00 }); //MOV cx,0x0000
                //IP += 3;
                TEXTbin.AddRange(new byte[] { 0x89, 0xC5 }); //MOV bp,ax
                //IP += 2;
                TEXTbin.AddRange(new byte[] { 0xB4, 0x13 }); //MOV ah,0x13
                //IP += 2;
                TEXTbin.AddRange(new byte[] { 0xB0, 0x00 }); //MOV al,0x00
                //IP += 2;
                TEXTbin.AddRange(new byte[] { 0xB7, 0x00 }); //MOV bh,0x00
                //IP += 2;
                TEXTbin.AddRange(new byte[] { 0xB3, 0x0F }); //MOV bl,0x0F
                //IP += 2;
                TEXTbin.AddRange(new byte[] { 0xB6, 0x00 }); //MOV dh,0x00
                //IP += 2;
                TEXTbin.AddRange(new byte[] { 0xB2, 0x00 }); //MOV dl,0x00
                //IP += 2;
                //TEXTbin.AddRange(new byte[] { 0xCD, 0x10 }); //INT 0x13
                //IP += 2;

                int StringIP = 0;
                StringIP = IP;
                StringIP += BaseAddress;
                string ScreenString = "";
                int chatLength = 0;
                while (chatLength <= 500 - IP)
                {
                    for (int i = 0; i < MBRchar.Text.Length; i++)
                    {
                        ScreenString += MBRchar.Text.ToArray()[i];
                        chatLength++;
                        if (chatLength >= 80)
                        {
                            break;
                        }
                    }
                }
                bin.AddRange(Encoding.ASCII.GetBytes(ScreenString));

                MBRColor TempColor = MBRColor.x0; //黑色
                int colorSInt = 0;
                MBRColor colorTrue =  MBRColor.x0; //黑色
                //string ASMstring = "";
                Point point = new Point(0, 0);
                for (int j = 0; j < 30; j++)
                {
                    for (int i = 0; i < 80; i++)
                    {
                        if ((MBRColor)pixels[i, j].Tag != TempColor) //如果当前颜色不是上一次的颜色
                        {
                            if ((MBRColor)pixels[i, j].Tag != colorTrue)
                            {
                                if (colorSInt != 0)
                                {
                                    //Color COLORtemp = colorTrue;
                                    //ASMstring += COLORtemp.R.ToString() + "," + COLORtemp.G.ToString() + "," + COLORtemp.B.ToString() + ":" + colorSInt.ToString() + "\r\n"; //

                                    TEXTbin.AddRange(new byte[] { 0xB3, (byte)((0 * 16) + (int)colorTrue) }); // mov bl,0x0F     ;颜色2字节
                                    TEXTbin.AddRange(new byte[] { 0xBA, (byte)point.X, (byte)point.Y }); // mov dx,0x0000; 位置3字节
                                    TEXTbin.AddRange(new byte[] { 0xB1, (byte)colorSInt }); // mov cl,0x03; 长度2字节
                                    TEXTbin.AddRange(new byte[] { 0xCD, 0x10 }); // int 0x10; 绘画2字节
                                }
                            }
                            if ((MBRColor)pixels[i, j].Tag != MBRColor.x0) //如果当前颜色不是黑色
                            {
                                point = new Point(i, j);
                                colorTrue = (MBRColor)pixels[i, j].Tag; //temp
                                colorSInt = 1;
                                StringIP += 10; //String字节递增
                                TempColor = (MBRColor)pixels[i, j].Tag;
                            }
                            else
                            {
                                colorSInt = 0;
                                TempColor =  MBRColor.x0;
                            }
                        }
                        else
                        {
                            if ((MBRColor)pixels[i, j].Tag != MBRColor.x0) //如果当前颜色不是黑色
                            {
                                colorSInt++;
                            }
                        }
                    }
                }
                //MessageBox.Show(StringIP.ToString());
                //MessageBox.Show(ASMstring.ToString());

                ASM.End(TEXTbin);
                //TEXTbin.AddRange(new byte[] { 0xFA, 0xF4 });

                byte[] b = new byte[512];
                for (int i = 0; i < bin.Count; i++)
                {
                    b[i] = bin[i]; //HelloWorld
                }

                b[510] = 0x55;
                b[511] = 0xAA;

                List<byte> buildByte = new List<byte>();
                buildByte.AddRange(b);
                buildByte.AddRange(TEXTbin);

                var addToByte = (buildByte.Count / 512 + 1) * 512; //512字节对齐
                var addByte = addToByte - buildByte.Count; //需要添加的字节
                for (int i = 0; i < addByte; i++)
                {
                    buildByte.Add(0);
                }


                if (isCreateFile.IsChecked == true)
                {
                    FileStream file = new FileStream("MBR.BIN", FileMode.Create);
                    file.Write(buildByte.ToArray(), 0, buildByte.Count);
                    file.Close();
                }

                CodeWindow codeWindow = new CodeWindow();
                codeWindow.textBox.Text = "0x" + BitConverter.ToString(buildByte.ToArray()).Replace("-", ",0x");
                //codeWindow.textBox.Text = BitConverter.ToString(tempBYTE.ToArray()).Replace("-", "");
                codeWindow.ShowDialog();

            }
            catch (Exception Error)
            {
                MessageBox.Show(Error.Message);
            }

        }
    }

    private void SetColor(object sender, RoutedEventArgs e)
    {
        if (sender == C0)
        {
            CurrentPenColor = MBRColor.x0;
        }
        else if (sender == C1)
        {
            CurrentPenColor = MBRColor.x1;
        }
        else if (sender == C2)
        {
            CurrentPenColor = MBRColor.x2;
        }
        else if (sender == C3)
        {
            CurrentPenColor = MBRColor.x3;
        }
        else if (sender == C4)
        {
            CurrentPenColor = MBRColor.x4;
        }
        else if (sender == C5)
        {
            CurrentPenColor = MBRColor.x5;
        }
        else if (sender == C6)
        {
            CurrentPenColor = MBRColor.x6;
        }
        else if (sender == C7)
        {
            CurrentPenColor = MBRColor.x7;
        }
        else if (sender == C8)
        {
            CurrentPenColor = MBRColor.x8;
        }
        else if (sender == C9)
        {
            CurrentPenColor = MBRColor.x9;
        }
        else if (sender == CA)
        {
            CurrentPenColor = MBRColor.xA;
        }
        else if (sender == CB)
        {
            CurrentPenColor = MBRColor.xB;
        }
        else if (sender == CC)
        {
            CurrentPenColor = MBRColor.xC;
        }
        else if (sender == CD)
        {
            CurrentPenColor = MBRColor.xD;
        }
        else if (sender == CE)
        {
            CurrentPenColor = MBRColor.xE;
        }
        else if (sender == CF)
        {
            CurrentPenColor = MBRColor.xF;
        }
        eCurrentColor.Background = new SolidColorBrush(MBR.ColorValue[(int)CurrentPenColor]);
    }

    private void Bitmap_Loaded(object sender, RoutedEventArgs e)
    {
        for (int h = 0; h < 30; h++)
        {
            for (int w = 0; w < 80; w++)
            {
                UserControl pixel = new UserControl()
                {
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(0),
                    Background = new SolidColorBrush(Colors.Black),
                    Tag = MBRColor.x0,
                };
                pixel.SetValue(Grid.ColumnProperty, w);
                pixel.SetValue(Grid.RowProperty, h);
                pixels[w, h] = pixel;
                ePixel.Children.Add(pixel);

            }
        }

    }


}

internal class MBR
{
    
    public static Color[] ColorValue = new Color[0x10];

    static MBR()
    {
        ColorValue[0x0] = Color.FromRgb(0x00, 0x00, 0x00);
        ColorValue[0x1] = Color.FromRgb(0x00, 0x00, 0xAA);
        ColorValue[0x2] = Color.FromRgb(0x00, 0xAA, 0x00);
        ColorValue[0x3] = Color.FromRgb(0x00, 0xAA, 0xAA);
        ColorValue[0x4] = Color.FromRgb(0xAA, 0x00, 0x00);
        ColorValue[0x5] = Color.FromRgb(0xAA, 0x00, 0xAA);
        ColorValue[0x6] = Color.FromRgb(0xAA, 0x55, 0x00);
        ColorValue[0x7] = Color.FromRgb(0xAA, 0xAA, 0xAA);
        ColorValue[0x8] = Color.FromRgb(0x55, 0x55, 0x55);
        ColorValue[0x9] = Color.FromRgb(0x55, 0x55, 0xFF);
        ColorValue[0xA] = Color.FromRgb(0x55, 0xFF, 0x55);
        ColorValue[0xB] = Color.FromRgb(0x55, 0xFF, 0xFF);
        ColorValue[0xC] = Color.FromRgb(0xFF, 0x55, 0x55);
        ColorValue[0xD] = Color.FromRgb(0xFF, 0x55, 0xFF);
        ColorValue[0xE] = Color.FromRgb(0xFF, 0xFF, 0x55);
        ColorValue[0xF] = Color.FromRgb(0xFF, 0xFF, 0xFF);
    }

}

internal enum MBRColor
{
    x0,
    x1,
    x2,
    x3,
    x4,
    x5,
    x6,
    x7,
    x8,
    x9,
    xA,
    xB,
    xC,
    xD,
    xE,
    xF,
}

/*

try
{
int IP = 0;
int BaseAddress = 0x7C00;
List<byte> bin = new List<byte>();
List<byte> TEXTbin = new List<byte>();

ASM.Screen(bin, ref IP);
ASM.LoadDisk(bin, ref IP, 0x20);

int STRINGADDRESS = IP + BaseAddress;
int S0 = ASM.HexL(STRINGADDRESS); //L
int S2 = ASM.HexH(STRINGADDRESS); //H
TEXTbin.AddRange(new byte[] { 0xB8, 0x00, 0x00 }); //MOV ax,0x0000
//IP += 2;
TEXTbin.AddRange(new byte[] { 0x8E, 0xD8 }); //MOV ds,ax
//IP += 2;
TEXTbin.AddRange(new byte[] { 0x8E, 0xC0 }); //MOV es,ax
//IP += 2;
TEXTbin.AddRange(new byte[] { 0xB8, (byte)S0, (byte)S2 }); //MOV ax,0x0000
//IP += 3;
TEXTbin.AddRange(new byte[] { 0xB9, 0x00, 0x00 }); //MOV cx,0x0000
//IP += 3;
TEXTbin.AddRange(new byte[] { 0x89, 0xC5 }); //MOV bp,ax
//IP += 2;
TEXTbin.AddRange(new byte[] { 0xB4, 0x13 }); //MOV ah,0x13
//IP += 2;
TEXTbin.AddRange(new byte[] { 0xB0, 0x00 }); //MOV al,0x00
//IP += 2;
TEXTbin.AddRange(new byte[] { 0xB7, 0x00 }); //MOV bh,0x00
//IP += 2;
TEXTbin.AddRange(new byte[] { 0xB3, 0x0F }); //MOV bl,0x0F
//IP += 2;
TEXTbin.AddRange(new byte[] { 0xB6, 0x00 }); //MOV dh,0x00
//IP += 2;
TEXTbin.AddRange(new byte[] { 0xB2, 0x00 }); //MOV dl,0x00
//IP += 2;
//TEXTbin.AddRange(new byte[] { 0xCD, 0x10 }); //INT 0x13
//IP += 2;

int StringIP = 0;
StringIP = IP;
StringIP += BaseAddress;
string ScreenString = "";
int chatLength = 0;
while (chatLength <= 500 - IP)
{
    for (int i = 0; i < MBRchar.Text.Length; i++)
    {
        ScreenString += MBRchar.Text.ToArray()[i];
        chatLength++;
        if (chatLength >= 80)
        {
            break;
        }
    }
}
bin.AddRange(Encoding.ASCII.GetBytes(ScreenString));

Color TempColor = Color.FromRgb(0, 0, 0);
int colorSInt = 0;
Color colorTrue = Color.FromRgb(0, 0, 0);
string ASMstring = "";
Point point = new Point(0, 0);
for (int j = 0; j < 30; j++)
{
    for (int i = 0; i < 80; i++)
    {
        if ((pixels[i, j].Background as SolidColorBrush).Color != TempColor) //如果当前颜色不是上一次的颜色
        {
            if ((pixels[i, j].Background as SolidColorBrush).Color != colorTrue)
            {
                if (colorSInt != 0)
                {
                    Color COLORtemp = colorTrue;
                    ASMstring += COLORtemp.R.ToString() + "," + COLORtemp.G.ToString() + "," + COLORtemp.B.ToString() + ":" + colorSInt.ToString() + "\r\n"; //

                    TEXTbin.AddRange(new byte[] { 0xB3, (byte)((0 * 16) + (MBR.ColorToBin(colorTrue))) }); // mov bl,0x0F     ;颜色2字节
                    TEXTbin.AddRange(new byte[] { 0xBA, (byte)point.X, (byte)point.Y }); // mov dx,0x0000; 位置3字节
                    TEXTbin.AddRange(new byte[] { 0xB1, (byte)colorSInt }); // mov cl,0x03; 长度2字节
                    TEXTbin.AddRange(new byte[] { 0xCD, 0x10 }); // int 0x10; 绘画2字节
                }
            }
            if ((pixels[i, j].Background as SolidColorBrush).Color != Color.FromRgb(0, 0, 0)) //如果当前颜色不是黑色
            {
                point = new Point(i, j);
                colorTrue = (pixels[i, j].Background as SolidColorBrush).Color; //temp
                colorSInt = 1;
                StringIP += 10; //String字节递增
                TempColor = (pixels[i, j].Background as SolidColorBrush).Color;
            }
            else
            {
                colorSInt = 0;
                TempColor = Color.FromRgb(0, 0, 0);
            }
        }
        else
        {
            if ((pixels[i, j].Background as SolidColorBrush).Color != Color.FromRgb(0, 0, 0)) //如果当前颜色不是黑色
            {
                colorSInt++;
            }
        }
    }
}
//MessageBox.Show(StringIP.ToString());
//MessageBox.Show(ASMstring.ToString());

ASM.End(TEXTbin);
//TEXTbin.AddRange(new byte[] { 0xFA, 0xF4 });

byte[] b = new byte[512];
for (int i = 0; i < bin.Count; i++)
{
    b[i] = bin[i]; //HelloWorld
}
b[510] = 0x55;
b[511] = 0xAA;
List<byte> tempBYTE = new List<byte>();
tempBYTE.AddRange(b);
tempBYTE.AddRange(TEXTbin);

if (isCreateFile.IsChecked == true)
{
    FileStream file = new FileStream("MBR.BIN", FileMode.Create);
    file.Write(tempBYTE.ToArray(), 0, tempBYTE.Count);
    file.Close();
}

CodeWindow codeWindow = new CodeWindow();
codeWindow.textBox.Text = "0x" + BitConverter.ToString(tempBYTE.ToArray()).Replace("-", ",0x");
codeWindow.ShowDialog();

}
catch (Exception Error)
{
MessageBox.Show(Error.Message);
}

*/