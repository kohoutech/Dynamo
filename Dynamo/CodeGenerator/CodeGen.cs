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

//https://www.felixcloutier.com/x86/index.html

namespace Dynamo.CodeGenerator
{
    class CodeGen
    {
        public Dynamo dynamo;
        public Assembly assembly;

        public CodeGen(Dynamo _dynamo)
        {
            dynamo = _dynamo;
            assembly = new Assembly();
        }

        //- statement nodes ---------------------------------------------------

        public void genStatement(StatementNode stmt)
        {
            switch (stmt.type)
            {
                case OILType.ExpressionStmt:
                    genExpressionStatement((ExpressionStatementNode)stmt);
                    break;

                case OILType.ReturnStmt:
                    genReturnStatement((ReturnStatementNode)stmt);
                    break;

                default:
                    break;
            }
        }

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


        public void genExpressionStatement(ExpressionStatementNode stmt)
        {
            genExpression(stmt.expr);
        }

        public void genIfStatement(OILNode stmt)
        {
            throw new NotImplementedException();
        }

        public void genSwitchStatement(OILNode stmt)
        {
            throw new NotImplementedException();
        }

        public void genCaseStatement(OILNode stmt)
        {
            throw new NotImplementedException();
        }

        public void genWhileStatement(OILNode stmt)
        {
            throw new NotImplementedException();
        }

        public void genDoWhileStatement(OILNode stmt)
        {
            throw new NotImplementedException();
        }

        public void genForStatement(OILNode stmt)
        {
            throw new NotImplementedException();
        }

        public void genBreakStatement(OILNode stmt)
        {
            throw new NotImplementedException();
        }

        public void genContinueStatement(OILNode stmt)
        {
            throw new NotImplementedException();
        }

        public void genReturnStatement(ReturnStatementNode stmt)
        {
            if (stmt.retval != null)
            {
                genExpression(stmt.retval);
                assembly.addLine("pop eax");         //eax <--- return val
            }
        }

        //        case NodeType.PrintVarNode:
        //            codeGen.evalPrintVarStatement(node);
        //            break;

        //public void genPrintVarStatement(OILNode stmt)
        //{
        //    throw new NotImplementedException();
        //}

        //public void genVarDeclaration(OILNode stmt)
        //{
        //    throw new NotImplementedException();
        //}

        //- expression nodes --------------------------------------------------

        private void genExpression(ExprNode expr)
        {
            switch (expr.type)
            {
                case OILType.IdentExpr:
                    genIdentExpression((IdentExprNode)expr);
                    break;

                case OILType.ArithmeticExpr:
                    genArithmeticExpression((ArithmeticExprNode)expr);
                    break;

                case OILType.AssignExpr:
                    genAssignmentExpression((AssignExprNode)expr);
                    break;

                default:
                    break;
            }
        }

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


        public void genIdentExpression(IdentExprNode expr)
        {
            OILNode lsym = expr.idsym;
            switch (lsym.type)
            {
                case OILType.ParamDecl:
                    uint pofs = ((CGParamDeclNode)(lsym.cgnode)).addr;
                    assembly.addLine("mov eax, [ebp+" + pofs + "]");                        
                    break;

                case OILType.VarDecl:
                    uint vofs = ((CGVarDeclNode)(lsym.cgnode)).addr;
                    assembly.addLine("mov eax, [ebp-" + vofs + "]");
                    break;

                default:
                    break;
            }
            assembly.addLine("push eax");       //var value --> top of stack
        }

        public void genArithmeticExpression(ArithmeticExprNode expr)
        {
            genExpression(expr.lhs);
            genExpression(expr.rhs);
            assembly.addLine("pop ebx");         //rhs --> ebx
            assembly.addLine("pop eax");         //lhs --> eax

            switch (expr.op)
            {
                case ArithmeticExprNode.OPERATOR.ADD:
                    assembly.addLine("add eax,ebx");
                    break;

                case ArithmeticExprNode.OPERATOR.SUB:
                    assembly.addLine("sub eax,ebx");
                    break;

                case ArithmeticExprNode.OPERATOR.MULT:
                    assembly.addLine("mul eax,ebx");
                    break;

                case ArithmeticExprNode.OPERATOR.DIV:
                    assembly.addLine("div eax,ebx");
                    break;

                case ArithmeticExprNode.OPERATOR.MOD:
                    assembly.addLine("div eax, ebx");
                    assembly.addLine("mov eax, edx");        //remainder is in edx
                    break;

                case ArithmeticExprNode.OPERATOR.PLUS:
                    break;

                case ArithmeticExprNode.OPERATOR.MINUS:
                    break;

                case ArithmeticExprNode.OPERATOR.INC:
                    assembly.addLine("inc eax");
                    break;

                case ArithmeticExprNode.OPERATOR.DEC:
                    assembly.addLine("dec eax");
                    break;
                
                default:
                    break;
            }
            assembly.addLine("push eax");      //result --> stack
        }

        //only supporting simple assignments (=) now, not compound assignments (ie +=, *= etc)
        public void genAssignmentExpression(AssignExprNode expr)
        {
            genExpression(expr.rhs);
            assembly.addLine("pop eax");       //rhs --> eax

            IdentExprNode lvar = (IdentExprNode)expr.lhs;
            OILNode lsym = lvar.idsym;
            switch (lsym.type)
            {
                case OILType.ParamDecl:
                    uint pofs = ((CGParamDeclNode)(lsym.cgnode)).addr;
                    assembly.addLine("mov [ebp+" + pofs + "],eax");
                    break;

                case OILType.VarDecl:
                    uint vofs = ((CGVarDeclNode)(lsym.cgnode)).addr;
                    assembly.addLine("mov [ebp-" + vofs + "],eax");
                    break;

                default:
                    break;
            }
        }

        //---------------------------------------------------------------------

        static void genGlobalData(Module module)
        {
        }

        public void genFunctions(Module module)
        {
            assembly.addLine(".section text");
            foreach (FuncDefNode func in module.funcs)
            {
                CGFuncDefNode cgfunc = new CGFuncDefNode(func);

                assembly.addLine(".global " + func.name);

                uint paramofs = 8;
                for (int i = 0; i < func.paramList.Count; i++)
                {
                    ParamDeclNode para = func.paramList[i];
                    CGParamDeclNode cgpara = new CGParamDeclNode(para);
                    cgpara.addr = paramofs;
                    paramofs += 4;
                }

                //for now, the only type we handle are ints, so each local var is 4 bytes on the stack
                cgfunc.stacksize = 0;
                for (int i = 0; i < func.locals.Count; i++)
                {
                    VarDeclNode local = func.locals[i];
                    CGVarDeclNode cglocal = new CGVarDeclNode(local);
                    cgfunc.stacksize += 4;
                    cglocal.addr = cgfunc.stacksize;
                    cglocal.type = CGVarDeclNode.VarType.LOCAL;
                }                

                //func prolog
                assembly.addLine(func.name + ":");
                assembly.addLine("push ebp");
                assembly.addLine("mov ebp, esp");
                assembly.addLine("sub esp, " + cgfunc.stacksize);

                foreach (StatementNode stmt in func.body)
                {
                    genStatement(stmt);
                }

                //func epilog
                assembly.addLine("mov esp, ebp");
                assembly.addLine("pop ebp");
                assembly.addLine("ret");
            }
        }

        public Assembly generate(Module module)
        {            
            genGlobalData(module);
            genFunctions(module);            
            return assembly;
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the Wizard");