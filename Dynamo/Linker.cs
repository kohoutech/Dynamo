/* ----------------------------------------------------------------------------
Dynamo - a backend code generator
Copyright (C) 1997-2018  George E Greaney

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

using Origami.Win32;

namespace Dynamo
{


    public class Linker
    {
        public Linker()
        {

        }

        public void loadLinkFiles(List<string> linkfiles)
        {
            throw new NotImplementedException();
        }

        public void setModules(List<Module> modules)
        {
            throw new NotImplementedException();
        }
        
        public void BuildExecutable()
        {
            Win32Exe exeImage = new Win32Exe();
            exeImage.layoutImage();
            exeImage.writeFile("test.exe");
        }
    }
}

//Console.WriteLine("done!");