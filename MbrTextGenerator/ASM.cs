using System;
using System.Collections.Generic;

namespace MbrTextGenerator;

class ASM
{
    public static void Screen(List<byte> bin, ref int IP)
    {
        bin.AddRange(new byte[] { 0xB4, 0x00 }); //MOV ah,0x00
        IP += 2;
        bin.AddRange(new byte[] { 0xB0, 0x12 }); //MOV al,0x12
        IP += 2;
        bin.AddRange(new byte[] { 0xCD, 0x10 }); //INT 0x10
        IP += 2;
    }

    public static void LoadDisk(List<byte> bin, ref int IP, int Length = 0x0A, int Address = 0x8000)
    {
        int A0 = ASM.HexH(Address);
        int A2 = ASM.HexL(Address);

        bin.AddRange(new byte[] { 0xB8, 0x00, 0x00 }); //MOV ax,0x0000
        IP += 3;
        bin.AddRange(new byte[] { 0x8E, 0xD8 }); //MOV es,ax
        IP += 2;
        bin.AddRange(new byte[] { 0x8E, 0xC0 }); //MOV es,ax
        IP += 2;
        bin.AddRange(new byte[] { 0xBB, (byte)A2, (byte)A0 }); //MOV bx,0x8000
        IP += 3;
        bin.AddRange(new byte[] { 0xB4, 0x02 }); //MOV ah,0x02
        IP += 2;
        bin.AddRange(new byte[] { 0xB0, (byte)Length }); //MOV al,0x0A
        IP += 2;
        bin.AddRange(new byte[] { 0xB5, 0x00 }); //MOV ch,0x00
        IP += 2;
        bin.AddRange(new byte[] { 0xB1, 0x02 }); //MOV cl,0x02
        IP += 2;
        bin.AddRange(new byte[] { 0xB6, 0x00 }); //MOV dh,0x00
        IP += 2;
        bin.AddRange(new byte[] { 0xB2, 0x80 }); //MOV dl,0x80
        IP += 2;
        bin.AddRange(new byte[] { 0xCD, 0x13 }); //INT 0x13
        IP += 2;
        bin.AddRange(new byte[] { 0xEA, (byte)A2, (byte)A0, 0x00, 0x00 }); //
        IP += 5;
    }

    public static void End(List<byte> bin, ref int IP)
    {
        End(bin);
        IP += 2;
    }

    public static void End(List<byte> bin)
    {
        bin.AddRange(new byte[] { 0xFA, 0xF4 });
    }

    public static int HexL(int Hex)
    {
        try
        {
            return int.Parse(Convert.ToInt32((Convert.ToString(Hex, 16).PadLeft(4, '0')).Substring(2, 2), 16).ToString());
        }
        catch
        {
            return 0;
        }
    }

    public static int HexH(int Hex)
    {
        try
        {
            return int.Parse(Convert.ToInt32((Convert.ToString(Hex, 16).PadLeft(4, '0')).Substring(0, 2), 16).ToString());
        }
        catch
        {
            return 0;
        }
    }

    public static int ToHex(int L,int H)
    {
        return L * 16 + H;
    }

    public static int StringToInt32(string str)
    {
        try
        {
            return int.Parse(str);
        }
        catch
        {

        }
        if (str.Substring(0, 2).ToUpper() == "0B")
        {
            return Convert.ToInt32(str.Substring(2, str.Length - 2), 2); //2进制转10进制
        }
        if (str.Substring(0, 2).ToUpper() == "0X")
        {
            return Convert.ToInt32(str.Substring(2, str.Length - 2), 16); //16进制转10进制
        }
        throw new Exception("转换错误!");
    }

    public static int String2ByteToInt32H(int Hex)
    {
        return int.Parse(Convert.ToInt32(Convert.ToString(Hex, 16).Substring(0, 1), 16).ToString());
    }

    public static int String2ByteToInt32L(int Hex)
    {
        if (Hex <= 0xF)
        {
            return int.Parse(Convert.ToInt32(("0" + Convert.ToString(Hex, 16)).Substring(1, 1), 16).ToString());
        }
        return int.Parse(Convert.ToInt32(Convert.ToString(Hex, 16).Substring(1, 1), 16).ToString());
    }

}
