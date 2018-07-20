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
        byte[] outbuf;
        uint outlen;
        uint outpos;

        //for reading fields from a disk file
        public OutputFile(uint filesize)
        {
            outbuf = new byte[filesize];
            outlen = filesize;
            outpos = 0;
        }

        public void writeOut(String filename)
        {
            File.WriteAllBytes(filename, outbuf);            
        }

        public uint getPos()
        {
            return outpos;
        }

        public void setRange(byte[] bytes)
        {
            uint len = (uint)bytes.Length;
            Array.Copy(bytes, 0, outbuf, outpos, len);                
            outpos += len;            
        }

        public void setZeros(uint len)
        {
            for (int i = 0; i < len; i++)
            {
                outbuf[outpos++] = 0;
            }
        }

        public void setOne(uint val)
        {
            outbuf[outpos++] = (byte)(val % 0x100);            
        }

        public void setTwo(uint val)
        {
            byte a = (byte)(val % 0x100);
            val /= 0x100;
            byte b = (byte)(val % 0x100);
            outbuf[outpos++] = a;
            outbuf[outpos++] = b;       
        }

        public void setFour(uint val)
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

        //fixed len string
        public void setAsciiString(String str, int width)
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

        public void seek(uint pos)
        {
            outpos = pos;
        }    
    }
}
