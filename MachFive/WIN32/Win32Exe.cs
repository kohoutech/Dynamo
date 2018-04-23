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
        public Section exports;
        public Section imports;
        public Section resources;

        public Win32Exe()
        {
            filename = null;
            peHeader = null;
            optHeader = null;
            sections = new List<Section>();

            imageBase = 0;
            exports = null;
            imports = null;
            resources = null;
        }

        public void setSourceFile(String fname) 
        {
            filename = fname;
        }
    }
}
