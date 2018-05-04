/* ----------------------------------------------------------------------------
MachFive - a backend code generator
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

namespace MachFive
{
    class Node
    {
        public virtual void GenerateCode () {
        }
    }

//- operation nodes -----------------------------------------------------------

    class AddIntegerNode : Node
    {
    }

    class SubtractIntegerNode : Node
    {
    }

    class MultiplyIntegerNode : Node
    {
    }

    class DivideIntegerNode : Node
    {
    }

    class ModIntegerNode : Node
    {
    }

    class NegateIntegerNode : Node
    {
    }

    class NotIntegerNode : Node
    {
    }

    class AndIntegerNode : Node
    {
    }

    class OrIntegerNode : Node
    {
    }

    class XorIntegerNode : Node
    {
    }

    class ShiftLeftIntegerNode : Node
    {
    }

    class ShiftRightIntegerNode : Node
    {
    }

    class IncrementIntegerNode : Node
    {
    }

    class DecrementIntegerNode : Node
    {
    }

    class AddFloatNode : Node
    {
    }

    class SubtractFloatNode : Node
    {
    }

    class MulitplyFloatNode : Node
    {
    }

    class DivideFloatNode : Node
    {
    }

    class NegateFloatNode : Node
    {
    }

//- statement nodes -----------------------------------------------------------

    class StatementListNode : Node
    {
        List<Node> statementList;

        public override void GenerateCode()
        {
            foreach (Node stmt in statementList)
            {
                stmt.GenerateCode();
            }
        }
    }

    class IfNode : Node
    {
    }

    class IfElseNode : Node
    {
    }

    class WhileNode : Node
    {
    }

    class DoWhileNode : Node
    {
    }

    class SwitchNode : Node
    {
    }

    class ForNode : Node
    {
    }
}
