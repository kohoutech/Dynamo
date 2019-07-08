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

    //compound stmt
    public class BlockStmt : StmtNode
    {
        public List<DeclarNode> decls;
        public List<StmtNode> statements;

        public BlockStmt()
        {
            nodetype = NodeType.BlockStmt;
            decls = new List<DeclarNode>();
            statements = new List<StmtNode>();
        }
    }

    //expression stmt
    public class AssignStmt : StmtNode
    {
        Identifier lhs;
        ExprNode rhs;

        public AssignStmt(Identifier _lhs, ExprNode _rhs)
        {
            nodetype = NodeType.AssignStmt;
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    //selection stmt
    public class IfStmt : StmtNode    
    {
        public IfStmt()
        {
            nodetype = NodeType.IfStmt;
        }
    }

    public class SwitchStmt : StmtNode
    {
        public SwitchStmt()
        {
            nodetype = NodeType.SwitchStmt;
        }
    }

    public class CaseStmt : StmtNode
    {
        public CaseStmt()
        {
            nodetype = NodeType.CaseStmt;
        }
    }

    //iteration stmt
    public class WhileStmt : StmtNode
    {
        public WhileStmt()
        {
            nodetype = NodeType.WhileStmt;
        }
    }

    public class DoWhileStmt : StmtNode
    {
        public DoWhileStmt()
        {
            nodetype = NodeType.DoWhileStmt;
        }
    }

    public class ForStmt : StmtNode
    {
        public ForStmt()
        {
            nodetype = NodeType.ForStmt;
        }
    }

    //jump stmt
    public class BreakStmt : StmtNode
    {
        public BreakStmt()
        {
            nodetype = NodeType.BreakStmt;
        }
    }

    public class ContinueStmt : StmtNode
    {
        public ContinueStmt()
        {
            nodetype = NodeType.ContinueStmt;
        }
    }

    public class ReturnStmt : StmtNode
    {
        public ReturnStmt()
        {
            nodetype = NodeType.ReturnStmt;
        }
    }

    //built in output stmt
    //may been removed at some future time
    public class PrintVarStmt : StmtNode
    {
        Identifier id;

        public PrintVarStmt(Identifier _id)
        {
            nodetype = NodeType.PrintVarNode;
            id = _id;
        }
    }
}
