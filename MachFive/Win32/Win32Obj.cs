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

//win32 obj files
//https://en.wikibooks.org/wiki/X86_Disassembly/Windows_Executable_Files


namespace Origami.Win32
{
    class Win32Obj : Win32Coff
    {
        public String filename;

        public Win32Obj() : base()
        {
            filename = null;
        }

//- reading in ----------------------------------------------------------------

        public void readFromFile(String _filename)
        {
            filename = _filename;

            SourceFile source = new SourceFile(filename);

            readCoffHeader(source);
            loadSections(source);
            loadReloctionTable(source);
            loadStringTable(source);
        }

//- writing out ---------------------------------------------------------------

        public void writeToFile(String filename)
        {
            //layout .obj file 
            uint filepos = 0x14;                               //coff hdr size

            //sections
            filepos += (uint)sections.Count * 0x28;            //add sec tbl size
            for (int i = 0; i < sections.Count; i++)            //add section data sizes
            {
                if (sections[i].data.Length > 0)
                {
                    sections[i].fileloc = filepos;
                    sections[i].filesize = (uint)(sections[i].data.Length);
                    filepos += sections[i].filesize;
                    uint relocsize = (uint)(sections[i].relocCount * 0x0a);
                    if (relocsize > 0)
                    {
                        sections[i].pRelocations = filepos;
                        filepos += relocsize;
                    }
                }
            }

            symbolTblAddr = filepos;                           
            filepos += (uint)symbolTbl.Count * 0x12;           //add symbol tbl size
            filepos += 0x04;
            for (int i = 0; i < stringTbl.Count; i++)
            {
                filepos += (uint)(stringTbl[i].Length + 1);    //add string tbl size
            }

            //then write to .obj file
            OutFile outfile = new OutFile(filename, filepos);
            writeCoffHeader(outfile);
            writeSectionTable(outfile);
            writeSectionData(outfile);
            writeSymbolTable(outfile);
            writeStringTable(outfile);

            outfile.write();
        }
    }


}
