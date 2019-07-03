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

    //base statement node
    public class StmtNode : Node
    {
    }

    public class StmtBlockNode : StmtNode
    {
        public List<DeclarNode> decls;
        public List<StmtNode> statements;

        public StmtBlockNode()
        {
            decls = new List<DeclarNode>();
            statements = new List<StmtNode>();
        }

        public override Operand eval()
        {
            foreach (DeclarNode decl in decls)
            {
                decl.eval();
            }

            foreach (StmtNode stmt in statements)
            {
                stmt.eval();
            }
            return null;
        }
    }

    public class AssignNode : StmtNode
    {
        Identifier lhs;
        ExprNode rhs;

        public AssignNode(Identifier _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class IfNode : StmtNode
    {
    }

    public class IfElseNode : StmtNode
    {
    }

    public class WhileNode : StmtNode
    {
    }

    public class DoWhileNode : StmtNode
    {
    }

    public class SwitchNode : StmtNode
    {
    }

    public class CaseNode : StmtNode
    {
    }

    public class ForNode : StmtNode
    {
    }

    public class BreakNode : StmtNode
    {
    }

    public class ContinueNode : StmtNode
    {
    }

    public class ReturnNode : StmtNode
    {
    }

    public class PrintVarNode : StmtNode
    {
        Identifier id;

        public PrintVarNode(Identifier _id)
        {
            id = _id;
        }
    }
}
