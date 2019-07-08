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
    public class ExprNode : Node
    {
    }

    public class PrimaryId : ExprNode
    {
        String id;

        public PrimaryId(String _id)
        {
            nodetype = NodeType.PrimaryId;
            id = _id;
        }
    }

    public class PrimaryIntConst : ExprNode
    {
        int constVal;

        public PrimaryIntConst(int _constVal)
        {
            nodetype = NodeType.PrimaryConst;
            constVal = _constVal;
        }
    }

    //- operation nodes -----------------------------------------------------------

    public class AddOpNode : ExprNode
    {
        ExprNode op1;
            ExprNode op2;

        public AddOpNode(ExprNode _op1, ExprNode _op2)
        {
            nodetype = NodeType.AddExpr;
            op1 = _op1;
            op2 = _op2;
        }
    }

    public class SubtractOpNode : ExprNode
    {
    }

    public class MultiplyOpNode : ExprNode
    {
    }

    public class DivideOpNode : ExprNode
    {
    }

    public class ModOpNode : ExprNode
    {
    }

    public class NotOpNode : ExprNode
    {
    }

    public class AndOpNode : ExprNode
    {
    }

    public class OrOpNode : ExprNode
    {
    }

    public class XorOpNode : ExprNode
    {
    }

    public class ShiftLeftOpNode : ExprNode
    {
    }

    public class ShiftRightOpNode : ExprNode
    {
    }

    public class IncrementOpNode : ExprNode
    {
    }

    public class DecrementOpNode : ExprNode
    {
    }
    
    //- conditional nodes -----------------------------------------------------------

    public class EqualConditNode : ExprNode
    {
    }

    public class NotEqualConditNode : ExprNode
    {
    }

    public class LessThanConditNode : ExprNode
    {
    }

    public class LessEqualConditNode : ExprNode
    {
    }

    public class GreaterThanConditNode : ExprNode
    {
    }

    public class GreaterEqualConditNode : ExprNode
    {
    }
}
