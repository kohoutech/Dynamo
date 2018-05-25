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
    class Win32Obj
    {
        public String filename;
        public PEHeader peHeader;
        public List<Section> sections;
        public List<ObjSymbolRecord> symbolTable;

        public Win32Obj()
        {
            filename = null;
            symbolTable = new List<ObjSymbolRecord>();
        }

//- reading in ----------------------------------------------------------------

        public void readFile(String _filename)
        {
            filename = _filename;

            SourceFile source = new SourceFile(filename);

            readWinHeader(source);
            loadSections(source);
        }

        private void readWinHeader(SourceFile source)
        {
            peHeader = new PEHeader();

            peHeader.machine = source.getTwo();
            peHeader.sectionCount = (int)source.getTwo();
            peHeader.timeStamp = source.getFour();
            peHeader.pSymbolTable = source.getFour();
            peHeader.symbolcount = source.getFour();
            peHeader.optionalHeaderSize = source.getTwo();
            peHeader.characteristics = source.getTwo();

        }

        private void loadSections(SourceFile source)
        {
            int sectionCount = peHeader.sectionCount;

            sections = new List<Section>(sectionCount);
            for (int i = 0; i < sectionCount; i++)
            {
                Section section = loadSection(source, i + 1);
                sections.Add(section);
            }
        }

        private Section loadSection(SourceFile source, int num)
        {

            Section section = new Section();
            section.secName = source.getAsciiString(8);

            section.memsize = source.getFour();
            section.memloc = source.getFour();
            section.filesize = source.getFour();
            section.fileloc = source.getFour();

            section.pRelocations = source.getFour();
            section.pLinenums = source.getFour();
            section.relocCount = (int)source.getTwo();
            section.linenumCount = (int)source.getTwo();
            section.flags = source.getFour();
            //section.imageBase = optHeader.imageBase;
            section.data = source.getRange(section.fileloc, section.filesize);          //load section data

            return section;
        }


    }
}
