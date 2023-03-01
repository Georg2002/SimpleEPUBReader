using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace EPUBRenderer
{
    public static class Win32Func
    {
        [DllImport("gdi32.dll")]
        public static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart, string lpString, int cbString);
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("GDI32.dll")]
        public static extern bool DeleteObject(IntPtr objectHandle);
        [DllImport("GDI32.dll")]
        public static extern bool Rectangle(IntPtr hdc, int left, int top, int right, int bottom);
        [DllImport("GDI32.dll")]
        public static extern IntPtr CreateSolidBrush(int color);
        [DllImport("GDI32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr CreateFontA(int cHeight, int cWidth, int cEscapement, int cOrientation, int cWeight, ushort bItalic, ushort bUnderline,
            ushort bStrikeOut, ushort iCharSet, ushort iOutPrecision, ushort iClipPrecision, ushort iQuality, ushort iPitchAndFamily, string pszFaceName); 
    }

    // if we specify CharSet.Auto instead of CharSet.Ansi, then the string will be unreadable
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class LOGFONT
    {
        public int lfHeight;
        public int lfWidth;
        public int lfEscapement;
        public int lfOrientation;
        public int lfWeight;
        [MarshalAs(UnmanagedType.U1)]
        public bool lfItalic;
        [MarshalAs(UnmanagedType.U1)]
        public bool lfUnderline;
        [MarshalAs(UnmanagedType.U1)]
        public bool lfStrikeOut;
        public byte lfCharSet;
        public byte lfOutPrecision;
        public byte lfClipPrecision;
        public byte lfQuality;
        public byte lfPitchAndFamily;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string lfFaceName;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("LOGFONT\n");
            sb.AppendFormat("   lfHeight: {0}\n", lfHeight);
            sb.AppendFormat("   lfWidth: {0}\n", lfWidth);
            sb.AppendFormat("   lfEscapement: {0}\n", lfEscapement);
            sb.AppendFormat("   lfOrientation: {0}\n", lfOrientation);
            sb.AppendFormat("   lfWeight: {0}\n", lfWeight);
            sb.AppendFormat("   lfItalic: {0}\n", lfItalic);
            sb.AppendFormat("   lfUnderline: {0}\n", lfUnderline);
            sb.AppendFormat("   lfStrikeOut: {0}\n", lfStrikeOut);
            sb.AppendFormat("   lfCharSet: {0}\n", lfCharSet);
            sb.AppendFormat("   lfOutPrecision: {0}\n", lfOutPrecision);
            sb.AppendFormat("   lfClipPrecision: {0}\n", lfClipPrecision);
            sb.AppendFormat("   lfQuality: {0}\n", lfQuality);
            sb.AppendFormat("   lfPitchAndFamily: {0}\n", lfPitchAndFamily);
            sb.AppendFormat("   lfFaceName: {0}\n", lfFaceName);

            return sb.ToString();
        }
    }
}
