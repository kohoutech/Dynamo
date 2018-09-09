/* ----------------------------------------------------------------------------
Origami Win32 Library
Copyright (C) 1998-2018  George E Greaney

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
----------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Origami.Win32
{
    public class OutputFile
    {
        String filename;
        byte[] outbuf;
        uint outlen;
        uint outpos;

        //for writing fields to a disk file
        public OutputFile(String _filename, uint size)
        {
            filename = _filename;
            outlen = size;
            outbuf = new byte[outlen];
            outpos = 0;
        }

        public uint getPos()
        {
            return outpos;
        }

        public void putOne(uint val)
        {
            outbuf[outpos++] = (byte)(val % 0x100);            
        }

        public void putTwo(uint val)
        {
            byte a = (byte)(val % 0x100);
            val /= 0x100;
            byte b = (byte)(val % 0x100);
            outbuf[outpos++] = a;
            outbuf[outpos++] = b;       
        }

        public void putFour(uint val)
        {
            byte d = (byte)(val % 0x100);
            val /= 0x100;
            byte c = (byte)(val % 0x100);
            val /= 0x100;
            byte b = (byte)(val % 0x100);
            val /= 0x100;
            byte a = (byte)(val % 0x100);
            outbuf[outpos++] = a;
            outbuf[outpos++] = b;
            outbuf[outpos++] = c;
            outbuf[outpos++] = d;
        }

        //asciiz string
        public void putString(String s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                outbuf[outpos++] = (byte)s[i];
            }
            outbuf[outpos++] = 0x00;
        }

        //fixed len string
        public void putFixedString(String str, int width)
        {
            for (int i = 0; i < width; i++)
            {
                if (i < str.Length)
                {
                    outbuf[outpos++] = (byte)str[i];
                }
                else
                {
                    outbuf[outpos++] = 0;
                }
            }
        }

        public void putRange(byte[] bytes)
        {
            uint len = (uint)bytes.Length;
            Array.Copy(bytes, 0, outbuf, outpos, len);
            outpos += len;
        }

        public void putZeros(uint len)
        {
            for (int i = 0; i < len; i++)
            {
                outbuf[outpos++] = 0;
            }
        }

        public void seek(uint pos)
        {
            outpos = pos;
        }

        public void writeOut()
        {
            File.WriteAllBytes(filename, outbuf);
        }
    }
}

//Console.WriteLine("there's no sun in the shadow of the wizard");