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
//https://docs.microsoft.com/en-us/windows/desktop/debug/pe-format

namespace Origami.Win32
{
    public class Win32Exe : Win32Coff
    {
        public String filename;

        public MsDosHeader dosHeader;

        //optional header fields
        public uint majorLinkerVersion;
        public uint minorLinkerVersion;
        public uint sizeOfCode;
        public uint sizeOfInitializedData;
        public uint sizeOfUninitializedData;
        public uint addressOfEntryPoint;
        public uint baseOfCode;
        public uint baseOfData;
        public uint imageBase;
        public uint sectionAlignment;
        public uint fileAlignment;
        public uint majorOSVersion;
        public uint minorOSVersion;
        public uint majorImageVersion;
        public uint minorImageVersion;
        public uint majorSubsystemVersion;
        public uint minorSubsystemVersion;
        public uint win32VersionValue;
        public uint sizeOfImage;
        public uint sizeOfHeaders;
        public uint checksum;
        public uint subsystem;
        public uint dLLCharacteristics;
        public uint sizeOfStackReserve;
        public uint sizeOfStackCommit;
        public uint sizeOfHeapReserve;
        public uint sizeOfHeapCommit;
        public uint loaderFlags;
        public uint numberOfRvaAndSizes;

        //dataDirectory entries
        public DataDirectory dExportTable;
        public DataDirectory dImportTable;
        public DataDirectory dResourceTable;
        public DataDirectory exceptionTable;
        public DataDirectory certificatesTable;
        public DataDirectory baseRelocationTable;
        public DataDirectory debugTable;
        public DataDirectory architecture;
        public DataDirectory globalPtr;
        public DataDirectory threadLocalStorageTable;
        public DataDirectory loadConfigurationTable;
        public DataDirectory boundImportTable;
        public DataDirectory importAddressTable;
        public DataDirectory delayImportDescriptor;
        public DataDirectory CLRRuntimeHeader;
        public DataDirectory reserved;

        //standard sections
        public ExportTable exportTable;
        public ImportTable importTable;
        public ResourceTable resourceTable;

        public Win32Exe() : base()
        {
            filename = null;

            dosHeader = null;

            //optional header fields
            majorLinkerVersion = 0;
            minorLinkerVersion = 0;
            sizeOfCode = 0;
            sizeOfInitializedData = 0;
            sizeOfUninitializedData = 0;
            addressOfEntryPoint = 0;
            baseOfCode = 0;
            baseOfData = 0;
            imageBase = 0;
            sectionAlignment = 0;
            fileAlignment = 0;
            majorOSVersion = 0;
            minorOSVersion = 0;
            majorImageVersion = 0;
            minorImageVersion = 0;
            majorSubsystemVersion = 0;
            minorSubsystemVersion = 0;
            win32VersionValue = 0;
            sizeOfImage = 0;
            sizeOfHeaders = 0;
            checksum = 0;
            subsystem = 0;
            dLLCharacteristics = 0;
            sizeOfStackReserve = 0;
            sizeOfStackCommit = 0;
            sizeOfHeapReserve = 0;
            sizeOfHeapCommit = 0;
            loaderFlags = 0;
            numberOfRvaAndSizes = 0;

            //data directory
            dExportTable = null;
            dImportTable = null;
            dResourceTable = null;
            exceptionTable = null;
            certificatesTable = null;
            baseRelocationTable = null;
            debugTable = null;
            architecture = null;
            globalPtr = null;
            threadLocalStorageTable = null;
            loadConfigurationTable = null;
            boundImportTable = null;
            importAddressTable = null;
            delayImportDescriptor = null;
            CLRRuntimeHeader = null;
            reserved = null;

            //standard sections
            exportTable = null;
            importTable = null;
            resourceTable = null;
        }

//- reading in ----------------------------------------------------------------

        public void readFile(String _filename)
        {
            filename = _filename;

            SourceFile source = new SourceFile(filename);

            dosHeader = MsDosHeader.readMSDOSHeader(source);
            source.seek(dosHeader.e_lfanew);
            uint pesig = source.getFour();
            if (pesig != 0x00004550)
            {
                throw new Win32Exception("this is not a valid win32 executable file");
            }

            readCoffHeader(source);
            readOptionalHeader(source);
            loadSections(source);
            //getResourceTable(source);
        }

        private void readOptionalHeader(SourceFile source)
        {
            uint signature = source.getTwo();

            majorLinkerVersion = source.getOne();
            minorLinkerVersion = source.getOne();
            sizeOfCode = source.getFour();
            sizeOfInitializedData = source.getFour();
            sizeOfUninitializedData = source.getFour();
            addressOfEntryPoint = source.getFour();
            baseOfCode = source.getFour();
            baseOfData = source.getFour();
            imageBase = source.getFour();
            sectionAlignment = source.getFour();
            fileAlignment = source.getFour();
            majorOSVersion = source.getTwo();
            minorOSVersion = source.getTwo();
            majorImageVersion = source.getTwo();
            minorImageVersion = source.getTwo();
            majorSubsystemVersion = source.getTwo();
            minorSubsystemVersion = source.getTwo();
            win32VersionValue = source.getFour();
            sizeOfImage = source.getFour();
            sizeOfHeaders = source.getFour();
            checksum = source.getFour();
            subsystem = source.getTwo();
            dLLCharacteristics = source.getTwo();
            sizeOfStackReserve = source.getFour();
            sizeOfStackCommit = source.getFour();
            sizeOfHeapReserve = source.getFour();
            sizeOfHeapCommit = source.getFour();
            loaderFlags = source.getFour();
            numberOfRvaAndSizes = source.getFour();

            dExportTable = DataDirectory.readDataDirectory(source);
            dImportTable = DataDirectory.readDataDirectory(source);
            dResourceTable = DataDirectory.readDataDirectory(source);
            exceptionTable = DataDirectory.readDataDirectory(source);
            certificatesTable = DataDirectory.readDataDirectory(source);
            baseRelocationTable = DataDirectory.readDataDirectory(source);
            debugTable = DataDirectory.readDataDirectory(source);
            architecture = DataDirectory.readDataDirectory(source);
            globalPtr = DataDirectory.readDataDirectory(source);
            threadLocalStorageTable = DataDirectory.readDataDirectory(source);
            loadConfigurationTable = DataDirectory.readDataDirectory(source);
            boundImportTable = DataDirectory.readDataDirectory(source);
            importAddressTable = DataDirectory.readDataDirectory(source);
            delayImportDescriptor = DataDirectory.readDataDirectory(source);
            CLRRuntimeHeader = DataDirectory.readDataDirectory(source);
            reserved = DataDirectory.readDataDirectory(source);
        }

        //private void getResourceTable(SourceFile source)
        //{
        //    if (optHeader.dataDirectory[DataDirectory.IMAGE_DIRECTORY_ENTRY_RESOURCE].size > 0)
        //    {
        //        uint resOfs = optHeader.dataDirectory[DataDirectory.IMAGE_DIRECTORY_ENTRY_RESOURCE].rva;
        //        uint resSize = optHeader.dataDirectory[DataDirectory.IMAGE_DIRECTORY_ENTRY_RESOURCE].size;
        //        Section resSec = findSection(resOfs);
        //        if (resSec != null)
        //        {
        //            SourceFile secData = new SourceFile(resSec.data);
        //            resourceTable = new ResourceTable();
        //            resourceTable.imageBase = imageBase;
        //            resourceTable.resourceRVA = resOfs;
        //            resourceTable.data = secData.getRange(resOfs - resSec.memloc, resSize);
        //        }
        //    }
        //}

//- writing out ----------------------------------------------------------------

        public void layoutImage()
        {
            dosHeader = new MsDosHeader();
            dosHeader.e_lfanew = 0x200;
        }

        public void writeFile(String filename)
        {
            OutputFile outfile = new OutputFile(0x200);
            dosHeader.writeOut(outfile);
            outfile.setZeros(0x200 - 0x40);
            outfile.writeOut(filename);
        }
    }

//- ms dos header -------------------------------------------------------------

        //the dos header is only used in reading in / writing out win32 exe files
        public class MsDosHeader
        {
            public uint signature;
            public uint lastsize;
            public uint nblocks;
            public uint nreloc;
            public uint hdrsize;
            public uint minalloc;
            public uint maxalloc;
            public uint ss;
            public uint sp;
            public uint checksum;
            public uint ip;
            public uint cs;
            public uint relocpos;
            public uint noverlay;
            public byte[] reserved1;
            public uint oem_id;
            public uint oem_info;
            public byte[] reserved2;
            public uint e_lfanew;         // Offset to the 'PE\0\0' signature relative to the beginning of the file

            public MsDosHeader()
            {
                signature = 0x5a4d;
                lastsize = 0;
                nblocks = 0;
                nreloc = 0;
                hdrsize = 0;
                minalloc = 0;
                maxalloc = 0;
                ss = 0;
                sp = 0;
                checksum = 0;
                ip = 0;
                cs = 0;
                relocpos = 0;
                noverlay = 0;
                reserved1 = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                oem_id = 0;
                oem_info = 0;
                reserved2 = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                e_lfanew = 0;
            }

            static public MsDosHeader readMSDOSHeader(SourceFile source)
            {
                MsDosHeader dosHeader = new MsDosHeader();

                dosHeader.signature = source.getTwo();
                if (dosHeader.signature != 0x5a4d)
                {
                    throw new Win32Exception("this is not a valid win32 executable file");
                }

                dosHeader.lastsize = source.getTwo();
                dosHeader.nblocks = source.getTwo();
                dosHeader.nreloc = source.getTwo();
                dosHeader.hdrsize = source.getTwo();
                dosHeader.minalloc = source.getTwo();
                dosHeader.maxalloc = source.getTwo();
                dosHeader.ss = source.getTwo();
                dosHeader.sp = source.getTwo();
                dosHeader.checksum = source.getTwo();
                dosHeader.ip = source.getTwo();
                dosHeader.cs = source.getTwo();
                dosHeader.relocpos = source.getTwo();
                dosHeader.noverlay = source.getTwo();
                dosHeader.reserved1 = source.getRange(8);
                dosHeader.oem_id = source.getTwo();
                dosHeader.oem_info = source.getTwo();
                dosHeader.reserved2 = source.getRange(20);
                dosHeader.e_lfanew = source.getFour();

                return dosHeader;
            }

            public void writeOut(OutputFile outfile)
            {

                outfile.setTwo(signature);

                outfile.setTwo(lastsize);
                outfile.setTwo(nblocks);
                outfile.setTwo(nreloc);
                outfile.setTwo(hdrsize);
                outfile.setTwo(minalloc);
                outfile.setTwo(maxalloc);
                outfile.setTwo(ss);
                outfile.setTwo(sp);
                outfile.setTwo(checksum);
                outfile.setTwo(ip);
                outfile.setTwo(cs);
                outfile.setTwo(relocpos);
                outfile.setTwo(noverlay);
                outfile.setZeros(8);
                outfile.setTwo(oem_id);
                outfile.setTwo(oem_info);
                outfile.setZeros(20);
                outfile.setFour(e_lfanew);                
            }
        }


//- data directory ------------------------------------------------------------

        public class DataDirectory
        {
            public uint rva;
            public uint size;

            public DataDirectory(uint _rva, uint _size)
            {
                rva = _rva;
                size = _size;
            }

            static public DataDirectory readDataDirectory(SourceFile source) 
            {
                uint rva = source.getFour();
                uint size = source.getFour();
                return new DataDirectory(rva, size);
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

//Console.WriteLine("there's no sun in the shadow of the wizard");