﻿/* ----------------------------------------------------------------------------
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

//https://en.wikibooks.org/wiki/X86_Disassembly/Windows_Executable_Files

namespace Origami.Win32
{
    public class Win32Reader
    {
        public static Win32Exe readExe(String filename)
        {
            Win32Exe winexe = new Win32Exe();
            winexe.setSourceFile(filename);

            SourceFile source = new SourceFile(filename);

            readMSDOSHeader(source, winexe);
            readWinHeader(source, winexe);
            loadSections(source, winexe);

            winexe.imageBase = winexe.optHeader.imageBase;
            winexe.exports = findSection(winexe, DataDirectory.IMAGE_DIRECTORY_ENTRY_EXPORT);
            winexe.imports = findSection(winexe, DataDirectory.IMAGE_DIRECTORY_ENTRY_IMPORT);
            winexe.resources = findSection(winexe, DataDirectory.IMAGE_DIRECTORY_ENTRY_RESOURCE);

            return winexe;
        }

        public static void readMSDOSHeader(SourceFile source, Win32Exe exefile)
        {
            uint e_magic = source.getFour();
            source.seek(0x3c);
            uint e_lfanew = source.getFour();
            source.seek(e_lfanew);
        }

        public static void readWinHeader(SourceFile source, Win32Exe exefile)
        {
            exefile.peHeader = new PEHeader();

            exefile.peHeader.pesig = source.getFour();
            exefile.peHeader.machine = source.getTwo();
            exefile.peHeader.sectionCount = (int)source.getTwo();
            exefile.peHeader.timeStamp = source.getFour();
            exefile.peHeader.pSymbolTable = source.getFour();
            exefile.peHeader.symbolcount = source.getFour();
            exefile.peHeader.optionalHeaderSize = source.getTwo();
            exefile.peHeader.characteristics = source.getTwo(); 
       
            readOptionalHeader(source, exefile);
        }

        public static void readOptionalHeader(SourceFile source, Win32Exe exefile)
        {
            OptionalHeader header = new OptionalHeader();

            header.signature = source.getTwo();
            header.MajorLinkerVersion = source.getOne();
            header.MinorLinkerVersion = source.getOne();
            header.SizeOfCode = source.getFour();
            header.SizeOfInitializedData = source.getFour();
            header.SizeOfUninitializedData = source.getFour();
            header.AddressOfEntryPoint = source.getFour();
            header.BaseOfCode = source.getFour();
            header.BaseOfData = source.getFour();
            header.imageBase = source.getFour();
            header.SectionAlignment = source.getFour();
            header.FileAlignment = source.getFour();
            header.MajorOSVersion = source.getTwo();
            header.MinorOSVersion = source.getTwo();
            header.MajorImageVersion = source.getTwo();
            header.MinorImageVersion = source.getTwo();
            header.MajorSubsystemVersion = source.getTwo();
            header.MinorSubsystemVersion = source.getTwo();
            header.Win32VersionValue = source.getFour();
            header.SizeOfImage = source.getFour();
            header.SizeOfHeaders = source.getFour();
            header.Checksum = source.getFour();
            header.Subsystem = source.getTwo();
            header.DLLCharacteristics = source.getTwo();
            header.SizeOfStackReserve = source.getFour();
            header.SizeOfStackCommit = source.getFour();
            header.SizeOfHeapReserve = source.getFour();
            header.SizeOfHeapCommit = source.getFour();
            header.LoaderFlags = source.getFour();
            header.NumberOfRvaAndSizes = source.getFour();

            header.dataDirectory = new DataDirectory[header.NumberOfRvaAndSizes];
            for (int i = 0; i < header.NumberOfRvaAndSizes; i++)
            {
                uint rva = source.getFour();
                uint size = source.getFour();
                header.dataDirectory[i] = new DataDirectory(rva, size);
            }

            exefile.optHeader = header;
        }

        public static void loadSections(SourceFile source, Win32Exe exefile)
        {
            int sectionCount = exefile.peHeader.sectionCount;

            exefile.sections = new List<Section>(sectionCount);
            for (int i = 0; i < sectionCount; i++)
            {
                Section section = loadSection(source, exefile, i+1);
                exefile.sections.Add(section);
            }
        }

        public static Section loadSection(SourceFile source, Win32Exe exefile, int num)
        {
            String sectionName = source.getAsciiString(8);   
         
            uint memsize = source.getFour();
            uint memloc = source.getFour();
            uint filesize = source.getFour();
            uint fileloc = source.getFour();

            uint prelocations = source.getFour();
            uint plinenums = source.getFour();
            int reloccount = (int) source.getTwo();
            int linenumcount = (int) source.getTwo();
            uint flags = source.getFour();

            Section section;
            if ((flags & Section.IMAGE_SCN_CNT_CODE) != 0)
            {
                section = new CodeSection(num, sectionName, memsize, memloc, filesize, fileloc, 
                    prelocations, plinenums, reloccount, linenumcount, flags);
            }
            else
            {
                section = new Section(num, sectionName, memsize, memloc, filesize, fileloc, 
                    prelocations, plinenums, reloccount, linenumcount, flags);
            }

            section.imageBase = exefile.optHeader.imageBase;
            section.memloc += section.imageBase;
            section.data = source.getRange(fileloc, filesize);          //load section data

            return section;
        }

        public static Section findSection(Win32Exe exefile, int index)
        {
            Section result = null;

            DataDirectory datadir = exefile.optHeader.dataDirectory[index];
            foreach (Section section in exefile.sections)
            {
                if ((section.memloc - section.imageBase) == datadir.rva)
                {
                    result = section;
                    break;
                }
            }
            return result;
        }
    }

//- file i/o ------------------------------------------------------------------

    public class SourceFile
    {
        Byte[] srcbuf;
        uint srclen;
        uint srcpos;

        public SourceFile(String se1name)
        {            
            srcbuf = File.ReadAllBytes(se1name);
            srclen = (uint) srcbuf.Length;
            srcpos = 0;
        }

        public uint getPos()
        {
            return srcpos;
        }

        public byte[] getRange(uint ofs, uint len)
        {
            byte[] result = new byte[len];
            Array.Copy(srcbuf, ofs, result, 0, len);
            return result;
        }

        public uint getOne()
        {
            byte a = srcbuf[srcpos++];
            uint result = (uint)(a);
            return result;
        }

        public uint getTwo()
        {
            byte b = srcbuf[srcpos++];
            byte a = srcbuf[srcpos++];
            uint result = (uint)(a * 256 + b);
            return result;
        }

        public uint getFour()
        {
            byte d = srcbuf[srcpos++];
            byte c = srcbuf[srcpos++];
            byte b = srcbuf[srcpos++];
            byte a = srcbuf[srcpos++];
            uint result = (uint)(a * 256 + b);
            result = (result * 256 + c);
            result = (result * 256 + d);
            return result;
        }

        //fixed len string
        public String getAsciiString(int width)
        {
            String result = "";
            for (int i = 0; i < width; i++)
            {
                byte a = srcbuf[srcpos++];
                if ((a >= 0x20) && (a <= 0x7E))
                {
                    result += (char)a;
                }
            }
            return result;
        }

        public void skip(uint delta)
        {
            srcpos += delta;
        }

        public void seek(uint pos)
        {
            srcpos = pos;
        }
    }

//- error handling ------------------------------------------------------------

    class Win32Exception : Exception
    {
        public Win32Exception(string message)
            : base(message)
        {
        }
    }
}
