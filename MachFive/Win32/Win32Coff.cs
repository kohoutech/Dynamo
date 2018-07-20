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

namespace Origami.Win32
{
    public class Win32Coff
    {
        const uint IMAGE_FILE_MACHINE_I386 = 0x14c;

        //coff header fields
        public uint machine;
        public int sectionCount;
        public uint timeStamp;
        public uint symbolTableAddr;
        public uint symbolcount;
        public uint optionalHeaderSize;
        public uint characteristics;

        public List<Section> sections;        

        //cons
        public Win32Coff()
        {
            machine = IMAGE_FILE_MACHINE_I386;
            sectionCount = 0;
            timeStamp = 0;
            symbolTableAddr = 0;
            symbolcount = 0;
            optionalHeaderSize = 0;
            characteristics = 0;

            sections = new List<Section>();
        }

        public void readCoffHeader(SourceFile source)
        {
            machine = source.getTwo();
            sectionCount = (int)source.getTwo();
            timeStamp = source.getFour();
            symbolTableAddr = source.getFour();
            symbolcount = source.getFour();
            optionalHeaderSize = source.getTwo();
            characteristics = source.getTwo();         
        }

        public void loadSections(SourceFile source)
        {
            for (int i = 0; i < sectionCount; i++)
            {
                Section section = Section.loadSection(source);
                sections.Add(section);
            }
        }

        public Section findSection(uint memloc)
        {
            Section sec = null;
            for (int i = 0; i < sections.Count; i++)
            {
                if ((memloc >= sections[i].memloc) && (memloc < (sections[i].memloc + sections[i].memsize)))
                {
                    sec = sections[i];
                    break;
                }
            }
            return sec;
        }
    }
}
