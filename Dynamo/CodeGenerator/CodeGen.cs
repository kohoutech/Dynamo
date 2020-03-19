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
        public List<Instruction> insns;

        public CodeGen(Dynamo _dynamo)
        {
            dynamo = _dynamo;
            insns = new List<Instruction>();
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
                insns.Add(new Pop(Register32.EAX));         //eax <--- return val
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
                    Immediate pImm = new Immediate(pofs, OPSIZE.Byte);
                    insns.Add(new Move(Register32.EAX, new Memory(Register32.EBP, null, Memory.Mult.NONE, pImm, OPSIZE.DWord, Segment.SEG.CS)));
                    break;

                case OILType.VarDecl:
                    uint vofs = 0x100 - ((CGVarDeclNode)(lsym.cgnode)).addr;
                    Immediate vImm = new Immediate(vofs, OPSIZE.Byte);
                    insns.Add(new Move(Register32.EAX, new Memory(Register32.EBP, null, Memory.Mult.NONE, vImm, OPSIZE.DWord, Segment.SEG.CS)));
                    break;

                default:
                    break;
            }
            insns.Add(new Push(Register32.EAX));       //var value --> top of stack
        }

        public void genArithmeticExpression(ArithmeticExprNode expr)
        {
            genExpression(expr.lhs);
            genExpression(expr.rhs);
            insns.Add(new Pop(Register32.EBX));         //rhs --> ebx
            insns.Add(new Pop(Register32.EAX));         //lhs --> eax

            switch (expr.op)
            {
                case ArithmeticExprNode.OPERATOR.ADD:
                    insns.Add(new Add(Register32.EAX, Register32.EBX, false));
                    break;

                case ArithmeticExprNode.OPERATOR.SUB:
                    insns.Add(new Subtract(Register32.EAX, Register32.EBX, false));
                    break;

                case ArithmeticExprNode.OPERATOR.MULT:
                    insns.Add(new Multiply(Register32.EAX, Register32.EBX));
                    break;

                case ArithmeticExprNode.OPERATOR.DIV:
                    insns.Add(new Divide(Register32.EAX, Register32.EBX));                    
                    break;

                case ArithmeticExprNode.OPERATOR.MOD:
                    insns.Add(new Divide(Register32.EAX, Register32.EBX));
                    insns.Add(new Move(Register32.EAX, Register32.EDX));        //remainder is in edx
                    break;

                case ArithmeticExprNode.OPERATOR.PLUS:
                    break;

                case ArithmeticExprNode.OPERATOR.MINUS:
                    break;

                case ArithmeticExprNode.OPERATOR.INC:
                    insns.Add(new Increment(Register32.EAX));
                    break;

                case ArithmeticExprNode.OPERATOR.DEC:
                    insns.Add(new Decrement(Register32.EAX));
                    break;
                
                default:
                    break;
            }
            insns.Add(new Push(Register32.EAX));      //result --> stack
        }

        //only supporting simple assignments (=) now, not compound assignments (ie +=, *= etc)
        public void genAssignmentExpression(AssignExprNode expr)
        {
            genExpression(expr.rhs);
            insns.Add(new Pop(Register32.EAX));       //rhs --> eax

            IdentExprNode lvar = (IdentExprNode)expr.lhs;
            OILNode lsym = lvar.idsym;
            switch (lsym.type)
            {
                case OILType.ParamDecl:
                    uint pofs = ((CGParamDeclNode)(lsym.cgnode)).addr;
                    Immediate pImm = new Immediate(pofs, OPSIZE.Byte);
                    insns.Add(new Move(new Memory(Register32.EBP, null, Memory.Mult.NONE, pImm, OPSIZE.DWord, Segment.SEG.CS), Register32.EAX));
                    break;

                case OILType.VarDecl:
                    uint vofs = 0x100 - ((CGVarDeclNode)(lsym.cgnode)).addr;
                    Immediate vImm = new Immediate(vofs, OPSIZE.Byte);
                    insns.Add(new Move(new Memory(Register32.EBP, null, Memory.Mult.NONE, vImm, OPSIZE.DWord, Segment.SEG.CS), Register32.EAX));
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
            insns.Add(new SectionDir("text"));
            foreach (FuncDefNode func in module.funcs)
            {
                CGFuncDefNode cgfunc = new CGFuncDefNode(func);

                insns.Add(new GlobalDir(new Symbol(func.name)));

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
                Symbol funcName = new Symbol(func.name);
                Instruction funcStart = new Push(Register32.EBP);
                funcName.def = funcStart;
                insns.Add(funcStart);
                insns.Add(new Move(Register32.EBP, Register32.ESP));
                insns.Add(new Subtract(Register32.ESP, new Immediate(cgfunc.stacksize, OPSIZE.Byte), false));

                foreach (StatementNode stmt in func.body)
                {
                    genStatement(stmt);
                }

                //func epilog
                insns.Add(new Move(Register32.ESP, Register32.EBP));
                insns.Add(new Pop(Register32.EBP));
                insns.Add(new Return(false));
            }
        }

        public List<Instruction> generate(Module module)
        {            
            genGlobalData(module);
            genFunctions(module);            
            return insns;
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the Wizard");