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
using System.Text.RegularExpressions;

//using Origami.Asm32;

namespace Origami.Win32
{
    
    class CodeSection : Section
    { 
        const int BYTESFIELDWIDTH = 6;              //in bytes = each byte takes up 3 spaces
        const int OPCODEFIELDWIDTH = 12;            //in actual spaces

        List<String> codeList;

        public CodeSection()
            : base()
        {
            codeList = null;
        }

        //public CodeSection(int _secnum, String _sectionName, uint _memsize, uint _memloc, uint _filesize, uint _fileloc, 
        //    uint _pRelocations, uint _pLinenums, int _relocCount, int _linenumCount, uint _flags)
        //    : base(_secnum, _sectionName, _memsize, _memloc, _filesize, _fileloc, 
        //    _pRelocations, _pLinenums, _relocCount, _linenumCount, _flags)
        //{            
        //}

//-----------------------------------------------------------------------------

        public void getAddrList()
        {
            Regex regex = new Regex("[0-9A-F]{8}");
            uint codestart = memloc;
            uint codeend = codestart + memsize;
            
            foreach (String line in codeList)
            {
                if (line.Length < 44) continue;
                Match match = regex.Match(line, 32);
                if (match.Success)
                {
                    uint val = Convert.ToUInt32(match.Value, 16);
                    if ((val >= codestart) && (val <= codeend)) {
                    Console.Out.WriteLine(val.ToString("X8"));
                    }
                }
            }
        }        
    }
}
