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

//win32 exe model
//https://en.wikibooks.org/wiki/X86_Disassembly/Windows_Executable_Files

namespace Origami.Win32
{
    public class Win32Exe
    {
        public String filename;

        public MsDosHeader dosHeader;
        public PEHeader peHeader;
        public OptionalHeader optHeader;
        public List<Section> sections;

        public uint imageBase;
        public ExportTable exportSection;
        public ImportTable importSection;
        public ResourceTable resourceTable;

        public Win32Exe()
        {
            filename = null;
            peHeader = null;
            optHeader = null;
            sections = new List<Section>();

            imageBase = 0;
            exportSection = null;
            importSection = null;
            resourceTable = null;
        }

//- ms dos header -------------------------------------------------------------

        public class MsDosHeader
        {
        }

//- reading in ----------------------------------------------------------------

        public void readFile(String _filename)
        {
            filename = _filename;

            SourceFile source = new SourceFile(filename);

            readMSDOSHeader(source);
            readWinHeader(source);
            loadSections(source);
            getResourceTable(source);
        }

        private void readMSDOSHeader(SourceFile source)
        {
            uint e_magic = source.getFour();
            source.seek(0x3c);
            uint e_lfanew = source.getFour();
            source.seek(e_lfanew);
        }

        private void readWinHeader(SourceFile source)
        {
            peHeader = new PEHeader();

            peHeader.pesig = source.getFour();
            peHeader.machine = source.getTwo();
            peHeader.sectionCount = (int)source.getTwo();
            peHeader.timeStamp = source.getFour();
            peHeader.pSymbolTable = source.getFour();
            peHeader.symbolcount = source.getFour();
            peHeader.optionalHeaderSize = source.getTwo();
            peHeader.characteristics = source.getTwo();

            readOptionalHeader(source);
        }

        private void readOptionalHeader(SourceFile source)
        {
            optHeader = new OptionalHeader();

            optHeader.signature = source.getTwo();
            optHeader.MajorLinkerVersion = source.getOne();
            optHeader.MinorLinkerVersion = source.getOne();
            optHeader.SizeOfCode = source.getFour();
            optHeader.SizeOfInitializedData = source.getFour();
            optHeader.SizeOfUninitializedData = source.getFour();
            optHeader.AddressOfEntryPoint = source.getFour();
            optHeader.BaseOfCode = source.getFour();
            optHeader.BaseOfData = source.getFour();
            optHeader.imageBase = source.getFour();
            optHeader.SectionAlignment = source.getFour();
            optHeader.FileAlignment = source.getFour();
            optHeader.MajorOSVersion = source.getTwo();
            optHeader.MinorOSVersion = source.getTwo();
            optHeader.MajorImageVersion = source.getTwo();
            optHeader.MinorImageVersion = source.getTwo();
            optHeader.MajorSubsystemVersion = source.getTwo();
            optHeader.MinorSubsystemVersion = source.getTwo();
            optHeader.Win32VersionValue = source.getFour();
            optHeader.SizeOfImage = source.getFour();
            optHeader.SizeOfHeaders = source.getFour();
            optHeader.Checksum = source.getFour();
            optHeader.Subsystem = source.getTwo();
            optHeader.DLLCharacteristics = source.getTwo();
            optHeader.SizeOfStackReserve = source.getFour();
            optHeader.SizeOfStackCommit = source.getFour();
            optHeader.SizeOfHeapReserve = source.getFour();
            optHeader.SizeOfHeapCommit = source.getFour();
            optHeader.LoaderFlags = source.getFour();
            optHeader.NumberOfRvaAndSizes = source.getFour();

            optHeader.dataDirectory = new DataDirectory[optHeader.NumberOfRvaAndSizes];
            for (int i = 0; i < optHeader.NumberOfRvaAndSizes; i++)
            {
                uint rva = source.getFour();
                uint size = source.getFour();
                optHeader.dataDirectory[i] = new DataDirectory(rva, size);
            }

            imageBase = optHeader.imageBase;
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
            section.imageBase = optHeader.imageBase;
            section.data = source.getRange(section.fileloc, section.filesize);          //load section data

            return section;
        }

        private void getResourceTable(SourceFile source)
        {
            if (optHeader.dataDirectory[DataDirectory.IMAGE_DIRECTORY_ENTRY_RESOURCE].size > 0)
            {
                uint resOfs = optHeader.dataDirectory[DataDirectory.IMAGE_DIRECTORY_ENTRY_RESOURCE].rva;
                uint resSize = optHeader.dataDirectory[DataDirectory.IMAGE_DIRECTORY_ENTRY_RESOURCE].size;
                Section resSec = findSection(resOfs);
                if (resSec != null)
                {
                    SourceFile secData = new SourceFile(resSec.data);
                    resourceTable = new ResourceTable();
                    resourceTable.imageBase = imageBase;
                    resourceTable.resourceRVA = resOfs;
                    resourceTable.data = secData.getRange(resOfs - resSec.memloc, resSize);
                }
            }
        }

        private Section findSection(uint memloc)
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

//- writing out ----------------------------------------------------------------

        public void writeFile(String _filename)
        {
        }

    }

//-----------------------------------------------------------------------------

    public class ExportTable
    {
    }

//-----------------------------------------------------------------------------

    public class ImportTable
    {
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

//Console.WriteLine("there's no sun in the shadow of the wizard");