/* ----------------------------------------------------------------------------
LibOriAST - a library for working with abstract syntax trees
Copyright (C) 1997-2019  George E Greaney

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

namespace Origami.AST
{
    //base class
    public class Node
    {
        NodeType nodetype;
    }

    public class Operand
    {
    }

    public class Identifier : Operand
    {
        String id;

        public Identifier(String _id)
        {
            id = _id;
        }
    }

    public class IntConst : Operand
    {
        int val;

        public IntConst(int _val)
        {
            val = _val;
        }
    }

    public class IntVar : Operand
    {
        public int val;

        public IntVar()
        {
            val = 0;
        }
    }

    public enum NodeType
    {
        AssignStmt, IfStmt,ForStmt, WhileStmt
    }
}
