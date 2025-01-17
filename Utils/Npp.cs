﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using EasySaveAsAdmin.PluginInfrastructure;

namespace EasySaveAsAdmin.Utils
{
    /// <summary>
    /// contains connectors to Scintilla (editor) and Notepad++ (notepad)
    /// </summary>
    public static class Npp
    {
        public static readonly IScintillaGateway Editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
        
        public static readonly INotepadPpGateway Notepad = new NotepadPPGateway();
        
        public static readonly Random Random = new Random();

        private static readonly int[] NppVersion = Notepad.GetNppVersion();

        public static readonly bool NppVersionAtLeast8 = NppVersion[0] >= 8;
        
        public static readonly string PluginDllDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        /// <summary>
        /// get all text starting at position start in the current document
        /// and ending at position end in the current document
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static string GetSlice(int start, int end)
        {
            int len = end - start;
            IntPtr rangePtr = Editor.GetRangePointer(start, len);
            string ansi = Marshal.PtrToStringAnsi(rangePtr, len);
            // TODO: figure out a way to do this that involves less memcopy for non-ASCII
            if (ansi.Any(c => c >= 128))
                return Encoding.UTF8.GetString(Encoding.Default.GetBytes(ansi));
            return ansi;
        }

        private static readonly string[] newlines = new string[] { "\r\n", "\r", "\n" };

        /// <summary>0: CRLF, 1: CR, 2: LF<br></br>
        /// Anything less than 0 or greater than 2: LF</summary>
        public static string GetEndOfLineString(int eolType)
        {
            if (eolType < 0 || eolType >= 3)
                return "\n";
            return newlines[eolType];
        }

        private static string NppVersionString(bool include32bitVs64bit)
        {
            int[] nppVer = Notepad.GetNppVersion();
            string nppVerStr = $"{nppVer[0]}.{nppVer[1]}.{nppVer[2]}";
            return include32bitVs64bit ? $"{nppVerStr} {IntPtr.Size * 8}bit" : nppVerStr;
        }

        /// <summary>
        /// appends the JSON representation of char c to a StringBuilder.<br></br>
        /// for most characters, this just means appending the character itself, but for example '\n' would become "\\n", '\t' would become "\\t",<br></br>
        /// and most other chars less than 32 would be appended as "\\u00{char value in hex}" (e.g., '\x14' becomes "\\u0014")
        /// </summary>
        public static void CharToSb(StringBuilder sb, char c)
        {
            switch (c)
            {
            case '\\': sb.Append("\\\\"); break;
            case '"': sb.Append("\\\""); break;
            case '\x01': sb.Append("\\u0001"); break;
            case '\x02': sb.Append("\\u0002"); break;
            case '\x03': sb.Append("\\u0003"); break;
            case '\x04': sb.Append("\\u0004"); break;
            case '\x05': sb.Append("\\u0005"); break;
            case '\x06': sb.Append("\\u0006"); break;
            case '\x07': sb.Append("\\u0007"); break;
            case '\x08': sb.Append("\\b"); break;
            case '\x09': sb.Append("\\t"); break;
            case '\x0A': sb.Append("\\n"); break;
            case '\x0B': sb.Append("\\v"); break;
            case '\x0C': sb.Append("\\f"); break;
            case '\x0D': sb.Append("\\r"); break;
            case '\x0E': sb.Append("\\u000E"); break;
            case '\x0F': sb.Append("\\u000F"); break;
            case '\x10': sb.Append("\\u0010"); break;
            case '\x11': sb.Append("\\u0011"); break;
            case '\x12': sb.Append("\\u0012"); break;
            case '\x13': sb.Append("\\u0013"); break;
            case '\x14': sb.Append("\\u0014"); break;
            case '\x15': sb.Append("\\u0015"); break;
            case '\x16': sb.Append("\\u0016"); break;
            case '\x17': sb.Append("\\u0017"); break;
            case '\x18': sb.Append("\\u0018"); break;
            case '\x19': sb.Append("\\u0019"); break;
            case '\x1A': sb.Append("\\u001A"); break;
            case '\x1B': sb.Append("\\u001B"); break;
            case '\x1C': sb.Append("\\u001C"); break;
            case '\x1D': sb.Append("\\u001D"); break;
            case '\x1E': sb.Append("\\u001E"); break;
            case '\x1F': sb.Append("\\u001F"); break;
            default: sb.Append(c); break;
            }
        }

        /// <summary>
        /// the string representation of a JSON string
        /// if not quoted, this will not have the enclosing quotes a JSON string normally has
        /// </summary>
        public static string StrToString(string s, bool quoted)
        {
            int slen = s.Length;
            int ii = 0;
            for (; ii < slen; ii++)
            {
                char c = s[ii];
                if (c < 32 || c == '\\' || c == '"')
                    break;
            }
            if (ii == slen)
                return quoted ? $"\"{s}\"" : s;
            var sb = new StringBuilder();
            if (quoted)
                sb.Append('"');
            if (ii > 0)
            {
                ii--;
                sb.Append(s, 0, ii);
            }
            for (; ii < slen; ii++)
                CharToSb(sb, s[ii]);
            if (quoted)
                sb.Append('"');
            return sb.ToString();
        }
        
    }
}
