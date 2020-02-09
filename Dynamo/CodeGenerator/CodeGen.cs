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
using Origami.Asm32;

namespace Dynamo
{
    class CodeGen
    {
        public Dynamo dynamo;
        public List<Instruction> insns;

        public CodeGen(Dynamo _dynamo)
        {
            dynamo = _dynamo;
            insns = new List<Instruction>();
        }

        internal void evalBlockStatement(OILNode node)
        {
            throw new NotImplementedException();
        }

        internal void evalAssignStatement(OILNode node)
        {
            throw new NotImplementedException();
        }

        internal void evalIfStatement(OILNode node)
        {
            throw new NotImplementedException();
        }

        internal void evalSwitchStatement(OILNode node)
        {
            throw new NotImplementedException();
        }

        internal void evalCaseStatement(OILNode node)
        {
            throw new NotImplementedException();
        }

        internal void evalwhileStatement(OILNode node)
        {
            throw new NotImplementedException();
        }

        internal void evalDoWhileStatement(OILNode node)
        {
            throw new NotImplementedException();
        }

        internal void evalForStatement(OILNode node)
        {
            throw new NotImplementedException();
        }

        internal void evalBreakStatement(OILNode node)
        {
            throw new NotImplementedException();
        }

        internal void evalContinueStatement(OILNode node)
        {
            throw new NotImplementedException();
        }

        internal void evalReturnStatement(OILNode node)
        {
            throw new NotImplementedException();
        }

        internal void evalPrintVarStatement(OILNode node)
        {
            throw new NotImplementedException();
        }

        internal void evalVarDeclaration(OILNode node)
        {
            throw new NotImplementedException();
        }

        internal void evalPrimaryId(OILNode node)
        {
            throw new NotImplementedException();
        }

        internal void evalPrimaryConst(OILNode node)
        {
            throw new NotImplementedException();
        }

        internal void evalAddExpression(OILNode node)
        {
            throw new NotImplementedException();
        }

        static void emit_data(Module module)
        {
        }

        void emit_text(Module module)
        {
        }

        public List<Instruction> generate(Module module)
        {
            emit_data(module);
            emit_text(module);

            
        //    Value val = null;
        //    switch (node.nodetype)
        //    {
        //        case NodeType.VarDeclar:
        //            codeGen.evalVarDeclaration(node);
        //            break;
        //        case NodeType.PrimaryId:
        //            codeGen.evalPrimaryId(node);
        //            break;
        //        case NodeType.PrimaryConst:
        //            codeGen.evalPrimaryConst(node);
        //            break;
        //        case NodeType.AddExpr:
        //            codeGen.evalAddExpression(node);
        //            break;
        //        case NodeType.BlockStmt:
        //            codeGen.evalBlockStatement(node);
        //            break;
        //        case NodeType.AssignStmt:
        //            codeGen.evalAssignStatement(node);
        //            break;
        //        case NodeType.IfStmt:
        //            codeGen.evalIfStatement(node);
        //            break;
        //        case NodeType.SwitchStmt:
        //            codeGen.evalSwitchStatement(node);
        //            break;
        //        case NodeType.CaseStmt:
        //            codeGen.evalCaseStatement(node);
        //            break;
        //        case NodeType.WhileStmt:
        //            codeGen.evalwhileStatement(node);
        //            break;
        //        case NodeType.DoWhileStmt:
        //            codeGen.evalDoWhileStatement(node);
        //            break;
        //        case NodeType.ForStmt:
        //            codeGen.evalForStatement(node);
        //            break;
        //        case NodeType.BreakStmt:
        //            codeGen.evalBreakStatement(node);
        //            break;
        //        case NodeType.ContinueStmt:
        //            codeGen.evalContinueStatement(node);
        //            break;
        //        case NodeType.ReturnStmt:
        //            codeGen.evalReturnStatement(node);
        //            break;
        //        case NodeType.PrintVarNode:
        //            codeGen.evalPrintVarStatement(node);
        //            break;
        //        default:
        //            break;
        //    }
            return insns;
        }
    }
}
