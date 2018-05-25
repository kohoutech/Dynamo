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

//- pe header -----------------------------------------------------------------

    public class PEHeader
    {
        public uint pesig;
        public uint machine;
        public int sectionCount;
        public uint timeStamp;
        public uint pSymbolTable;
        public uint symbolcount;
        public uint optionalHeaderSize;
        public uint characteristics;


        public String getInfo()
        {
            String info =
                "machine = " + machine + "\r\n" +
                "sectioncount = " + sectionCount + "\r\n" +
                "timestamp = " + timeStamp + "\r\n" +
                "symbol tbl ptr = " + pSymbolTable + "\r\n" +
                "symbol count = " + symbolcount + "\r\n" +
                "optional header size = " + optionalHeaderSize + "\r\n" +
                "characteristics = " + characteristics;
            return info;
        }
    }

//- optional header -----------------------------------------------------------

    public class OptionalHeader
    {
        public uint signature;
        public uint MajorLinkerVersion;
        public uint MinorLinkerVersion;
        public uint SizeOfCode;
        public uint SizeOfInitializedData;
        public uint SizeOfUninitializedData;
        public uint AddressOfEntryPoint;
        public uint BaseOfCode;
        public uint BaseOfData;
        public uint imageBase;
        public uint SectionAlignment;
        public uint FileAlignment;
        public uint MajorOSVersion;
        public uint MinorOSVersion;
        public uint MajorImageVersion;
        public uint MinorImageVersion;
        public uint MajorSubsystemVersion;
        public uint MinorSubsystemVersion;
        public uint Win32VersionValue;
        public uint SizeOfImage;
        public uint SizeOfHeaders;
        public uint Checksum;
        public uint Subsystem;
        public uint DLLCharacteristics;
        public uint SizeOfStackReserve;
        public uint SizeOfStackCommit;
        public uint SizeOfHeapReserve;
        public uint SizeOfHeapCommit;
        public uint LoaderFlags;
        public uint NumberOfRvaAndSizes;
        public DataDirectory[] dataDirectory;

        public String getInfo()
        {
            String info =
                "signature = " + signature + "\r\n" +
                "MajorLinkerVersion = " + MajorLinkerVersion + "\r\n" +
                "MinorLinkerVersion = " + MinorLinkerVersion + "\r\n" +
                "SizeOfCode = " + SizeOfCode + "\r\n" +
                "SizeOfInitializedData = " + SizeOfInitializedData + "\r\n" +
                "SizeOfUninitializedData = " + SizeOfUninitializedData + "\r\n" +
                "AddressOfEntryPoint = " + AddressOfEntryPoint + "\r\n" +
                "BaseOfCode = " + BaseOfCode + "\r\n" +
                "BaseOfData = " + BaseOfData + "\r\n" +
                "ImageBase = " + imageBase.ToString("X") + "\r\n" +
                "SectionAlignment = " + SectionAlignment + "\r\n" +
                "FileAlignment = " + FileAlignment + "\r\n" +
                "MajorOSVersion = " + MajorOSVersion + "\r\n" +
                "MinorOSVersion = " + MinorOSVersion + "\r\n" +
                "MajorImageVersion = " + MajorImageVersion + "\r\n" +
                "MinorImageVersion = " + MinorImageVersion + "\r\n" +
                "MajorSubsystemVersion = " + MajorSubsystemVersion + "\r\n" +
                "MinorSubsystemVersion = " + MinorSubsystemVersion + "\r\n" +
                "Win32VersionValue = " + Win32VersionValue + "\r\n" +
                "SizeOfImage = " + SizeOfImage + "\r\n" +
                "SizeOfHeaders = " + SizeOfHeaders + "\r\n" +
                "Checksum = " + Checksum + "\r\n" +
                "Subsystem = " + Subsystem + "\r\n" +
                "DLLCharacteristics = " + DLLCharacteristics + "\r\n" +
                "SizeOfStackReserve = " + SizeOfStackReserve + "\r\n" +
                "SizeOfStackCommit = " + SizeOfStackCommit + "\r\n" +
                "SizeOfHeapReserve = " + SizeOfHeapReserve + "\r\n" +
                "SizeOfHeapCommit = " + SizeOfHeapCommit + "\r\n" +
                "LoaderFlags = " + LoaderFlags + "\r\n" +
                "NumberOfRvaAndSizes = " + NumberOfRvaAndSizes;
            return info;
        }
    }

//- data directory ------------------------------------------------------------

    public class DataDirectory
    {
        public static int IMAGE_DIRECTORY_ENTRY_EXPORT = 0;
        public static int IMAGE_DIRECTORY_ENTRY_IMPORT = 1;
        public static int IMAGE_DIRECTORY_ENTRY_RESOURCE = 2;

        public uint rva;
        public uint size;

        public DataDirectory(uint _rva, uint _size)
        {
            rva = _rva;
            size = _size;
        }
    }

//- obj sym table ------------------------------------------------------------

    public class ObjSymbolRecord
    {
        String name;
        uint value;
        uint sectionNum;
        uint type;
        uint storageClass;
        uint auxSymbolCount;
    }

}
