/* ----------------------------------------------------------------------------
Dynamo - a backend code generator
Copyright (C) 1997-2020  George E Greaney

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

using Origami.OIL;

//code generator nodes that are attached to the matching OIL nodes & assist in the code generation for that node

namespace Dynamo.CodeGenerator
{
    class CGFuncDefNode : CGNode
    {
        public FuncDefNode funcdef;
        public uint stacksize;

        public CGFuncDefNode(FuncDefNode _funcdef)
        {
            funcdef = _funcdef;
            funcdef.cgnode = this;
        }
    }

    class CGParamDeclNode : CGNode
    {
        public enum VarType { GLOBAL, LOCAL };

        public ParamDeclNode paramdecl;
        public uint addr;
        public VarType type;

        public CGParamDeclNode(ParamDeclNode _paramdecl)
        {
            paramdecl = _paramdecl;
            paramdecl.cgnode = this;
        }
    }

    class CGVarDeclNode : CGNode
    {
        public enum VarType { GLOBAL, LOCAL };

        public VarDeclNode vardecl;
        public uint addr;
        public VarType type;

        public CGVarDeclNode(VarDeclNode _vardecl)
        {
            vardecl = _vardecl;
            vardecl.cgnode = this;
        }
    }
}
