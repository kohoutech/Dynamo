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

//https://en.wikibooks.org/wiki/X86_Disassembly/Windows_Executable_Files

namespace Origami.Win32
{
    public class Section
    {
        //section flag constants
        public const uint IMAGE_SCN_TYPE_NO_PAD = 0x00000008;

        public const uint IMAGE_SCN_CNT_CODE = 0x00000020;
        public const uint IMAGE_SCN_CNT_INITIALIZED_DATA = 0x00000040;
        public const uint IMAGE_SCN_CNT_UNINITIALIZED_DATA = 0x00000080;
        public const uint IMAGE_SCN_LNK_OTHER = 0x00000100;
        public const uint IMAGE_SCN_LNK_INFO = 0x00000200;
        public const uint IMAGE_SCN_LNK_REMOVE = 0x00000800;
        public const uint IMAGE_SCN_LNK_COMDAT = 0x00001000;
        public const uint IMAGE_SCN_NO_DEFER_SPEC_EXC = 0x00004000;
        public const uint IMAGE_SCN_GPREL = 0x00008000;

        public const uint IMAGE_SCN_MEM_PURGEABLE = 0x00020000;
        public const uint IMAGE_SCN_MEM_LOCKED = 0x00040000;
        public const uint IMAGE_SCN_MEM_PRELOAD = 0x00080000;

        //valid only for object files
        public const uint IMAGE_SCN_ALIGN_1BYTES = 0x00100000;
        public const uint IMAGE_SCN_ALIGN_2BYTES = 0x00200000;
        public const uint IMAGE_SCN_ALIGN_4BYTES = 0x00300000;
        public const uint IMAGE_SCN_ALIGN_8BYTES = 0x00400000;
        public const uint IMAGE_SCN_ALIGN_16BYTES = 0x00500000;
        public const uint IMAGE_SCN_ALIGN_32BYTES = 0x00600000;
        public const uint IMAGE_SCN_ALIGN_64BYTES = 0x00700000;
        public const uint IMAGE_SCN_ALIGN_128BYTES = 0x00800000;
        public const uint IMAGE_SCN_ALIGN_256BYTES = 0x00900000;
        public const uint IMAGE_SCN_ALIGN_512BYTES = 0x00A00000;
        public const uint IMAGE_SCN_ALIGN_1024BYTES = 0x00B00000;
        public const uint IMAGE_SCN_ALIGN_2048BYTES = 0x00C00000;
        public const uint IMAGE_SCN_ALIGN_4096BYTES = 0x00D00000;
        public const uint IMAGE_SCN_ALIGN_8192BYTES = 0x00E00000;

        public const uint IMAGE_SCN_LNK_NRELOC_OVFL = 0x01000000;
        public const uint IMAGE_SCN_MEM_DISCARDABLE = 0x02000000;
        public const uint IMAGE_SCN_MEM_NOT_CACHED = 0x04000000;
        public const uint IMAGE_SCN_MEM_NOT_PAGED = 0x08000000;

        public const uint IMAGE_SCN_MEM_SHARED = 0x10000000;
        public const uint IMAGE_SCN_MEM_EXECUTE = 0x20000000;
        public const uint IMAGE_SCN_MEM_READ = 0x40000000;
        public const uint IMAGE_SCN_MEM_WRITE = 0x80000000;

        public int secNum;
        public String secName;

        public uint memloc;                 //section addr in memory
        public uint memsize;                //section size in memory
        public uint fileloc;                //section addr in file
        public uint filesize;               //section size in file

        public uint pRelocations;
        public uint pLinenums;
        public int relocCount;
        public int linenumCount;

        public uint flags;

        public uint imageBase;
        public byte[] data;

        //cons
        public Section(int _secnum, String _sectionName, uint _memsize, uint _memloc, uint _filesize, uint _fileloc, 
            uint _pRelocations, uint _pLinenums, int _relocCount, int _linenumCount, uint _flags)
        {
            secNum = _secnum;
            secName = _sectionName;

            memsize = _memsize;
            memloc = _memloc;
            filesize = _filesize;
            fileloc = _fileloc;

            pRelocations = _pRelocations;
            pLinenums = _pLinenums;
            relocCount = _relocCount;
            linenumCount = _linenumCount;

            flags = _flags;

            imageBase = 0;
            data = null;
        }

//- flag methods --------------------------------------------------------------

        public bool isCode()
        {
            return (flags & IMAGE_SCN_CNT_CODE) != 0;
        }

        public bool isInitializedData()
        {
            return (flags & IMAGE_SCN_CNT_INITIALIZED_DATA) != 0;
        }

        public bool isUninitializedData()
        {
            return (flags & IMAGE_SCN_CNT_UNINITIALIZED_DATA) != 0;
        }

        public bool isDiscardable()
        {
            return (flags & IMAGE_SCN_MEM_DISCARDABLE) != 0;
        }

        public bool isExecutable()
        {
            return (flags & IMAGE_SCN_MEM_EXECUTE) != 0;
        }

        public bool isReadable()
        {
            return (flags & IMAGE_SCN_MEM_READ) != 0;
        }

        public bool isWritable()
        {
            return (flags & IMAGE_SCN_MEM_WRITE) != 0;
        }
        
//- displaying ---------------------------------------------------------------

        public String displayData()
        {
            StringBuilder dataStr = new StringBuilder();       //the whole thing as one LONG string
            StringBuilder ascii = new StringBuilder();      //the ascii representation of the bytes on one line

            int bpos = 0;
            uint loc = memloc;
            for (; bpos < data.Length; )
            {
                if (bpos % 16 == 0)
                {
                    dataStr.Append(loc.ToString("X8") + ": ");         //address field if at start of line
                }

                uint b = data[bpos];
                dataStr.Append(b.ToString("X2"));                                              //single byte value in hex
                dataStr.Append(" ");
                ascii.Append((b >= 0x20 && b <= 0x7E) ? ((char)b).ToString() : ".");        //and its ascii equivalent
                bpos++;
                loc++;

                if (bpos % 16 == 0)
                {
                    dataStr.AppendLine(ascii.ToString());      //ascii field if at end of line
                    ascii.Clear();
                }
            }
            if (bpos % 16 != 0)             //fill out partial last line
            {
                int remainder = (bpos % 16);
                for (; remainder < 16; remainder++)
                {
                    dataStr.Append("   ");                  //space over to line up ascii field
                    
                }
                dataStr.AppendLine(ascii.ToString());
            }
            return dataStr.ToString();
        }
    }
}
