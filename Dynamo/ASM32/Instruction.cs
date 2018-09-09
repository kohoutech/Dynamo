/* ----------------------------------------------------------------------------
Origami Asm32 Library
Copyright (C) 1998-2018  George E Greaney

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

namespace Origami.Asm32
{
    public class Instruction
    {
        public enum LOOPPREFIX { REP, REPNE, None };

        public Operand op1;
        public Operand op2;
        public Operand op3;

        public bool lockprefix;

        public int opcount;
        public List<byte> bytes;

        public Instruction () 
        {
            opcount = 0;
            op1 = null;
            op2 = null;
            op3 = null;
            lockprefix = false;
            bytes = null;
        }

        public List<byte> getBytes()
        {
            return bytes;
        }
    }

//- arithmetic ----------------------------------------------------------------

    public class Add : Instruction
    {
        bool carry;

        public Add(Operand _op1, Operand _op2, bool _carry)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            carry = _carry;
        }

        public override string ToString()
        {
            return (carry ? "ADC" : "ADD");
        }
    }

    public class Subtract : Instruction
    {
        bool borrow;

        public Subtract(Operand _op1, Operand _op2, bool _borrow)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            borrow = _borrow;
        }

        public override string ToString()
        {
            return (borrow ? "SBB" : "SUB");
        }
    }

    public class Multiply : Instruction
    {
        public Multiply(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;        
        }

        public override string ToString()
        {
            return "MUL";
        }
    }

    public class IntMultiply : Instruction
    {
        public IntMultiply(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public IntMultiply(Operand _op1, Operand _op2, Operand _op3)
            : base()
        {
            opcount = 3;
            op1 = _op1;
            op2 = _op2;
            op3 = _op3;
        }

        public override string ToString()
        {
            return "IMUL";
        }
    }

    public class Divide : Instruction
    {
        public Divide(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "DIV";
        }
    }

    public class IntDivide : Instruction
    {
        public IntDivide(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public IntDivide(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;        
        }

        public override string ToString()
        {
            return "IDIV";
        }
    }

    public class Negate : Instruction
    {
        public Negate(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;            
        }

        public override string ToString()
        {
            return "NEG";
        }
    }

    public class Increment : Instruction
    {
        public Increment(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;            
        }

        public override string ToString()
        {
            return "INC";
        }
    }

    public class Decrement : Instruction
    {
        public Decrement(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "DEC";
        }
    }

//- boolean ----------------------------------------------------------------

    public class Not : Instruction
    {
        public Not(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;            
        }

        public override string ToString()
        {
            return "NOT";
        }
    }

    public class And : Instruction
    {
        public And(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "AND";
        }
    }

    public class Or : Instruction
    {
        public Or(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "OR";
        }
    }

    public class Xor : Instruction
    {
        public Xor(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "XOR";
        }
    }

//- bit operations ----------------------------------------------------------------

    public class Rotate : Instruction
    {
        public enum MODE { LEFT, RIGHT }

        MODE mode;
        bool withCarry;

        public Rotate(Operand _op1, Operand _op2, MODE _mode, bool _withCarry)
            : base() 
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            mode = _mode;
            withCarry = _withCarry;
        }

        public override string ToString()
        {
            return (withCarry) ? ((mode == MODE.LEFT) ? "RCL" : "RCR") : ((mode == MODE.LEFT) ? "ROL" : "ROR");
        }
    }

    public class Shift : Instruction
    {
        public enum MODE { LEFT, RIGHT }

        MODE mode;
        bool arthimetic;

        public Shift(Operand _op1, Operand _op2, MODE _mode, bool _arthimetic)
            : base() 
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            mode = _mode;
            arthimetic = _arthimetic;
        }

        public override string ToString()
        {
            return (arthimetic) ? ((mode == MODE.LEFT) ? "SAL" : "SAR") : ((mode == MODE.LEFT) ? "SHL" : "SHR");
        }
    }

    public class ConvertSize : Instruction
    {
        public enum MODE { CWDE, CDQ }

        MODE mode;

        public ConvertSize(MODE _mode)
            : base() 
        {
            mode = _mode;
        }

        public override string ToString()
        {
            return (mode == MODE.CWDE) ? "CWDE" : "CDQ";
        }
    }

    public class AsciiAdjust : Instruction
    {
        public enum MODE {Add, Sub, Mult, Div}

        MODE mode;

        //for mode = add / subtract
        public AsciiAdjust(MODE _mode)
            : base() 
        {
            mode = _mode;
        }

        //for mode = multiply / divide, op1 gives the base
        public AsciiAdjust(MODE _mode, Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
            mode = _mode;
        }

        String[] modes = { "AAA", "AAS", "AAM", "AAD"};

        public override string ToString()
        {
            String result = modes[(int)mode];
            if ((mode == MODE.Mult || mode == MODE.Div) && (op1 !=  null))
            {
                result = result + "B";
            }
            return result;
        }
    }

    public class DecimalAdjust : Instruction
    {
        public enum MODE {Add, Sub}

        MODE mode;

        public DecimalAdjust(MODE _mode)
            : base() 
        {
            mode = _mode;
        }

        public override string ToString()
        {
            return (mode == MODE.Add) ? "DAA" : "DAS";
        }
    }

//- stack operations ----------------------------------------------------------

    public class Push : Instruction
    {
        public Push(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "PUSH";
        }
    }

    public class Pop : Instruction
    {
        public Pop(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "POP";
        }
    }

    public class PushRegisters : Instruction
    {
        public PushRegisters()
            : base()
        {
        }

        public override string ToString()
        {
            return "PUSHAD";
        }
    }

    public class PopRegisters : Instruction
    {
        public PopRegisters()
            : base()
        {
        }

        public override string ToString()
        {
            return "POPAD";
        }
    }

    public class PushFlags : Instruction
    {
        public PushFlags()
            : base()
        {
        }

        public override string ToString()
        {
            return "PUSHFD";
        }
    }

    public class PopFlags : Instruction
    {
        public PopFlags()
            : base()
        {
        }

        public override string ToString()
        {
            return "POPFD";
        }
    }

//- comparison ----------------------------------------------------------------

    public class Compare : Instruction
    {
        public Compare(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "CMP";
        }
    }

    public class Test : Instruction
    {
        public Test(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "TEST";
        }
    }

//- branching -----------------------------------------------------------------

    public class Jump : Instruction
    {
        public Jump(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;            
        }

        public override string ToString()
        {
            return "JMP";
        }
    }

    public class JumpConditional : Instruction
    {
        public enum CONDIT { JO, JNO, JB, JAE, JE, JNE, JBE, JA, JS, JNS, JP, JNP, JL, JGE, JLE, JG};

        public CONDIT condit;

        public JumpConditional(CONDIT _condit, Operand _op1)
            : base()
        {
            condit = _condit;
            op1 = _op1;
            opcount = 1;
        }

        String[] condits = { "JO", "JNO", "JB", "JAE", "JE", "JNE", "JBE", "JA", 
                           "JS", "JNS", "JP", "JNP", "JL", "JGE", "JLE", "JG" };

        public override string ToString()
        {
            return condits[(int)condit];
        }
    }

    public class Loop : Instruction
    {
        public enum MODE {LOOP, LOOPE, LOOPNE, JECXZ}

        MODE mode;

        public Loop(Operand _op1, MODE _mode)
            : base() 
        {
            opcount = 1;
            op1 = _op1;
            mode = _mode;
        }

        String[] modes = { "LOOP", "LOOPE", "LOOPNE", "JECXZ" };

        public override string ToString()
        {
            return modes[(int)mode];
        }
    }

    public class Call : Instruction
    {
        public Call(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;            
        }

        public override string ToString()
        {
            return "CALL";
        }
    }

    public class Return : Instruction
    {
        bool far;

        public Return(bool _far)
            : base()
        {
            far = _far;
        }

        public Return(Operand _op1, bool _far)
            : base()
        {
            opcount = 1;
            op1 = _op1;
            far = _far;
        }

        public override string ToString()
        {
            return (far) ? "RETF" : "RET";
        }
    }

    public class Enter : Instruction
    {
        public Enter(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "ENTER";
        }
    }

    public class Leave : Instruction
    {
        public Leave()
            : base()
        {
        }

        public override string ToString()
        {
            return "LEAVE";
        }
    }

    public class Interrupt : Instruction
    {
        public Interrupt(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;            
        }

        public override string ToString()
        {
            return "INT";
        }
    }

    public class InterruptDebug : Instruction
    {
        public InterruptDebug()
            : base()
        {
            opcount = 1;
            op1 = new Immediate(3, Operand.OPSIZE.Byte);
        }

        public override string ToString()
        {
            return "INT";
        }
    }

    public class InterruptOverflow : Instruction
    {
        public InterruptOverflow()
            : base()
        {
        }

        public override string ToString()
        {
            return "INTO";
        }
    }
    
    public class IReturn : Instruction
    {
        public IReturn()
            : base()
        {
        }

        public override string ToString()
        {
            return "IRETD";
        }
    }

//- flag operations -----------------------------------------------------------

    public class LoadFlags : Instruction
    {
        public LoadFlags()
            : base()
        {
        }

        public override string ToString()
        {
            return "LAHF";
        }
    }

    public class StoreFlags : Instruction
    {
        public StoreFlags()
            : base()
        {
        }

        public override string ToString()
        {
            return "SAHF";
        }
    }

    public class SetFlag : Instruction
    {
        public enum FLAG {Carry, Int, Dir}

        FLAG flag;

        public SetFlag(FLAG _flag)
            : base() 
        {
            flag = _flag;
        }

        String[] flags = { "STC", "STI", "STD" };

        public override string ToString()
        {
            return flags[(int)flag];
        }
    }

    public class ClearFlag : Instruction
    {
        public enum FLAG {Carry, Int, Dir}

        FLAG flag;

        public ClearFlag(FLAG _flag)
            : base() 
        {
            flag = _flag;
        }
        
        String[] flags = { "CLC", "CLI", "CLD" };

        public override string ToString()
        {
            return flags[(int)flag];
        }
    }

    public class ComplementCarry : Instruction
    {
        public ComplementCarry()
            : base()
        {
        }

        public override string ToString()
        {
            return "CMC";
        }
    }

//- data operations -----------------------------------------------------------

    public class Move : Instruction
    {
        public Move(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "MOV";
        }
    }

    public class Exchange : Instruction
    {
        public Exchange(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "XCHG";
        }
    }

    public class Input : Instruction
    {
        public Input(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;        
        }

        public override string ToString()
        {
            return "IN";
        }
    }

    public class Output : Instruction
    {
        public Output(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;        
        }

        public override string ToString()
        {
            return "OUT";
        }
    }

//- string operations -----------------------------------------------------------

    public class LoadString : Instruction
    {
        LOOPPREFIX prefix;

        public LoadString(Operand _op1, LOOPPREFIX _prefix)
            : base()
        {
            opcount = 1;
            op1 = _op1;
            prefix = _prefix;
        }

        public override string ToString()
        {
            String prefixStr = (prefix == LOOPPREFIX.REP) ? "REP " : ((prefix == LOOPPREFIX.REPNE) ? "REPNE " : "");
            return prefixStr + "LODS";
        }
    }

    public class MoveString : Instruction
    {
        LOOPPREFIX prefix;

        public MoveString(Operand _op1, Operand _op2, LOOPPREFIX _prefix)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            prefix = _prefix;
        }

        public override string ToString()
        {
            String prefixStr = (prefix == LOOPPREFIX.REP) ? "REP " : ((prefix == LOOPPREFIX.REPNE) ? "REPNE " : "");
            return prefixStr + "MOVS";
        }
    }

    public class StoreString : Instruction
    {
        LOOPPREFIX prefix;

        public StoreString(Operand _op1, LOOPPREFIX _prefix)
            : base()
        {
            opcount = 1;
            op1 = _op1;
            prefix = _prefix;
        }

        public override string ToString()
        {
            String prefixStr = (prefix == LOOPPREFIX.REP) ? "REP " : ((prefix == LOOPPREFIX.REPNE) ? "REPNE " : "");
            return prefixStr + "STOS";
        }
    }

    public class CompareString : Instruction
    {
        LOOPPREFIX prefix;

        public CompareString(Operand _op1, Operand _op2, LOOPPREFIX _prefix)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            prefix = _prefix;
        }

        public override string ToString()
        {
            String prefixStr = (prefix == LOOPPREFIX.REP) ? "REP " : ((prefix == LOOPPREFIX.REPNE) ? "REPNE " : "");
            return prefixStr + "CMPS";
        }
    }

    public class ScanString : Instruction
    {
        LOOPPREFIX prefix;

        public ScanString(Operand _op1, LOOPPREFIX _prefix)
            : base()
        {
            opcount = 1;
            op1 = _op1;
            prefix = _prefix;
        }

        public override string ToString()
        {
            String prefixStr = (prefix == LOOPPREFIX.REP) ? "REPE " : ((prefix == LOOPPREFIX.REPNE) ? "REPNE " : "");
            return prefixStr + "SCAS";
        }
    }

    public class XlateString : Instruction
    {
        LOOPPREFIX prefix;

        public XlateString(Operand _op1, LOOPPREFIX _prefix)
            : base()
        {
            opcount = 1;
            op1 = _op1;
            prefix = _prefix;
        }

        public override string ToString()
        {
            String prefixStr = (prefix == LOOPPREFIX.REP) ? "REP " : ((prefix == LOOPPREFIX.REPNE) ? "REPNE " : "");
            return prefixStr + "XLAT";
        }
    }

    public class InputString : Instruction
    {
        LOOPPREFIX prefix;

        public InputString(Operand _op1, Operand _op2, LOOPPREFIX _prefix)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            prefix = _prefix;
        }

        public override string ToString()
        {
            String prefixStr = (prefix == LOOPPREFIX.REP) ? "REP " : ((prefix == LOOPPREFIX.REPNE) ? "REPNE " : "");
            return prefixStr + "INS";
        }
    }

    public class OutputString : Instruction
    {
        LOOPPREFIX prefix;

        public OutputString(Operand _op1, Operand _op2, LOOPPREFIX _prefix)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            prefix = _prefix;
        }

        public override string ToString()
        {
            String prefixStr = (prefix == LOOPPREFIX.REP) ? "REP " : ((prefix == LOOPPREFIX.REPNE) ? "REPNE " : "");
            return prefixStr + "OUTS";
        }
    }

//- addressing ----------------------------------------------------------------

    public class LoadPtr : Instruction
    {
        public enum MODE { LDS, LES }

        MODE mode;

        public LoadPtr(Operand _op1, Operand _op2, MODE _mode)
            : base() 
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            mode = _mode;        
        }

        public override string ToString()
        {
            return (mode == MODE.LDS) ? "LDS" : "LES";
        }
    }

    public class LoadEffAddress : Instruction
    {
        public LoadEffAddress(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "LEA";
        }
    }

//- miscellaneous -------------------------------------------------------------

    public class Wait : Instruction
    {
        public Wait()
            : base()
        {            
        }

        public override string ToString()
        {
            return "WAIT";
        }
    }

    public class Arpl : Instruction
    {
        public Arpl(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "ARPL";
        }
    }

    public class Bound : Instruction
    {
        public Bound(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "BOUND";
        }
    }

    public class Halt : Instruction
    {
        public Halt()
            : base()
        {            
        }

        public override string ToString()
        {
            return "HALT";
        }
    }

    public class NoOp : Instruction
    {
        public NoOp()
            : base()
        {            
        }

        public override string ToString()
        {
            return "NOP";
        }
    }

    public class UnknownOp : Instruction
    {
        public UnknownOp()
            : base()
        {            
        }

        public override string ToString()
        {
            return "???";
        }
    }
}
