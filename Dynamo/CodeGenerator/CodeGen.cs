/* ----------------------------------------------------------------------------
Dynamo - a backend code generator
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

namespace Dynamo.CodeGenerator
{
    class CodeGen
    {
        private Dynamo dynamo;

        public CodeGen(Dynamo _dynamo)
        {
            // TODO: Complete member initialization
            dynamo = _dynamo;
        }

        internal void evalBlockStatement(Origami.AST.Node node)
        {
            throw new NotImplementedException();
        }

        internal void evalAssignStatement(Origami.AST.Node node)
        {
            throw new NotImplementedException();
        }

        internal void evalIfStatement(Origami.AST.Node node)
        {
            throw new NotImplementedException();
        }

        internal void evalSwitchStatement(Origami.AST.Node node)
        {
            throw new NotImplementedException();
        }

        internal void evalCaseStatement(Origami.AST.Node node)
        {
            throw new NotImplementedException();
        }

        internal void evalwhileStatement(Origami.AST.Node node)
        {
            throw new NotImplementedException();
        }

        internal void evalDoWhileStatement(Origami.AST.Node node)
        {
            throw new NotImplementedException();
        }

        internal void evalForStatement(Origami.AST.Node node)
        {
            throw new NotImplementedException();
        }

        internal void evalBreakStatement(Origami.AST.Node node)
        {
            throw new NotImplementedException();
        }

        internal void evalContinueStatement(Origami.AST.Node node)
        {
            throw new NotImplementedException();
        }

        internal void evalReturnStatement(Origami.AST.Node node)
        {
            throw new NotImplementedException();
        }

        internal void evalPrintVarStatement(Origami.AST.Node node)
        {
            throw new NotImplementedException();
        }

        internal void evalVarDeclaration(Origami.AST.Node node)
        {
            throw new NotImplementedException();
        }

        internal void evalPrimaryId(Origami.AST.Node node)
        {
            throw new NotImplementedException();
        }

        internal void evalPrimaryConst(Origami.AST.Node node)
        {
            throw new NotImplementedException();
        }

        internal void evalAddExpression(Origami.AST.Node node)
        {
            throw new NotImplementedException();
        }
    }
}
