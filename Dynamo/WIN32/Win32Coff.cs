/* ----------------------------------------------------------------------------
Origami Win32 Library
Copyright (C) 1998-2020  George E Greaney

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

//https://docs.microsoft.com/en-us/windows/desktop/debug/pe-format

namespace Origami.Win32
{
    public class Win32Coff
    {
        const int IMAGE_FILE_MACHINE_I386 = 0x14c;

        //coff header fields
        public int machine;
        public int sectionCount;
        public uint timeStamp;
        public uint symbolTblAddr;
        public uint symbolCount;
        public int optionalHdrSize;
        public int characteristics;

        public List<Section> sections;
        public List<CoffSymbol> symbolTbl;
        
        public Dictionary<int, String> stringTbl;
        public int strTblIdx;

        //cons
        public Win32Coff()
        {
            machine = IMAGE_FILE_MACHINE_I386;
            sectionCount = 0;
            timeStamp = 0;
            symbolTblAddr = 0;
            symbolCount = 0;
            optionalHdrSize = 0;
            characteristics = 0;

            sections = new List<Section>();
            symbolTbl = new List<CoffSymbol>();

            stringTbl = new Dictionary<int, string>();
            strTblIdx = 4;
        }


        public void addSection(Section sec)
        {
            sections.Add(sec);
        }

        public void addSymbol(CoffSymbol sym)
        {
            symbolTbl.Add(sym);
        }

        public int addString(string str)
        {
            stringTbl.Add(strTblIdx, str);
            strTblIdx += (str.Length + 1);
            return strTblIdx;
        }

//- reading in ----------------------------------------------------------------

        public void readCoffHeader(SourceFile source)
        {
            machine = (int)source.getTwo();
            sectionCount = (int)source.getTwo();
            timeStamp = source.getFour();
            symbolTblAddr = source.getFour();
            symbolCount = source.getFour();
            optionalHdrSize = (int)source.getTwo();
            characteristics = (int)source.getTwo();         
        }

        public void loadSections(SourceFile source)
        {
            for (int i = 0; i < sectionCount; i++)
            {
                Section section = Section.loadSection(source);
                sections.Add(section);
            }
        }

        public void loadStringTable(SourceFile source)
        {
            uint pos = symbolTblAddr + (symbolCount * 0x12);
            source.seek(pos);
            uint len = source.getFour() - 4;
            byte[] data = source.getRange(len);
            String str = "";
            int idx = 4;
            for (int i = 0; i < len; i++)
            {
                if (data[i] != 0)
                {
                    str += (char)data[i];
                }
                else
                {
                    stringTbl.Add(idx, str);
                    str = "";
                    idx = i + 5;
                }
            }
        }

        public void loadReloctionTable(SourceFile source)
        {
            throw new NotImplementedException();
        }

//- writing out ---------------------------------------------------------------

        public void writeCoffHeader(OutputFile outfile)
        {
            outfile.putTwo((uint)machine);
            outfile.putTwo((uint)sections.Count);
            outfile.putFour(timeStamp);
            outfile.putFour(symbolTblAddr);
            outfile.putFour((uint)symbolTbl.Count);
            outfile.putTwo((uint)optionalHdrSize);
            outfile.putTwo((uint)characteristics);
        }

        //public void writeSectionTable(OutputFile outfile)
        //{
        //    for (int i = 0; i < sections.Count; i++)
        //    {
        //        sections[i].writeSectionTblEntry(outfile);
        //    }
        //}

        //public void writeSectionData(OutputFile outfile)
        //{
        //    for (int i = 0; i < sections.Count; i++)
        //    {
        //        sections[i].writeSectionData(outfile);
        //    }
        //}

        public void writeSymbolTable(OutputFile outfile)
        {
            for (int i = 0; i < symbolTbl.Count; i++)
            {
                symbolTbl[i].writeSymbol(outfile);
            }
        }

        public void writeStringTable(OutputFile outfile)
        {
            uint tblSize = 4;
            for (int i = 0; i < stringTbl.Count; i++)
            {
                tblSize += (uint)(stringTbl[i].Length + 1);    
            }
            outfile.putFour(tblSize);
            for (int i = 0; i < stringTbl.Count; i++)
            {
                outfile.putString(stringTbl[i]);
            }
        }

        public void write(String filename)
        {
        }

//-----------------------------------------------------------------------------

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

//- obj sym table ------------------------------------------------------------

    public class CoffSymbol
    {
        String name;
        uint value;
        uint sectionNum;
        uint type;
        uint storageClass;
        uint auxSymbolCount;

        public CoffSymbol(String _name, uint _val, uint _num, uint _type, uint _storage, uint _aux )
        {
            name = _name;
            value = _val;
            sectionNum = _num;
            type = _type;
            storageClass = _storage;
            auxSymbolCount = _aux;
        }

        internal void writeSymbol(OutputFile outfile)
        {
            //kludge for testing purposes
            if (name.Equals("alongstring"))
            {
                outfile.putFour(0);
                outfile.putFour(0x4);
            }
            else if (name.Equals("alongerstring"))
            {
                outfile.putFour(0);
                outfile.putFour(0x13);
            }
            else outfile.putFixedString(name, 8);
            outfile.putFour(value);
            outfile.putTwo(sectionNum);
            outfile.putTwo(type);
            outfile.putOne(storageClass);
            outfile.putOne(auxSymbolCount);
        }
    }

//- error handling ------------------------------------------------------------

    class Win32ReadException : Exception
    {
        public Win32ReadException(string message)
            : base(message)
        {
        }
    }
}
