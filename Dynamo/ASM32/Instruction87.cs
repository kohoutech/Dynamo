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

//fpu instructions - section 5.2 Intel Architecture Manual (2018)

namespace Origami.Asm32
{

    //- data xfer -------------------------------------------------------------

    //FLoad - FLD
    public class FLoad : Instruction
    {
        public FLoad(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                Memory r1 = (Memory)op1;
                bytes.Add((byte)((r1.size == OPSIZE.DWord) ? 0xd9 : (r1.size == OPSIZE.TByte) ? 0xdb : 0xdd));
                List<byte> membytes = r1.getBytes(out mod, out rm);
                int reg = (r1.size == OPSIZE.TByte) ? 0x28 : 0x00;
                bytes.Add((byte)(mod * 0x40 + reg + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                bytes.Add(0xd9);
                bytes.Add((byte)(0xc0 + ((Register)op1).code));
            }
        }

        public override string ToString()
        {
            return "FLD";
        }
    }

    //FStore - FST/FSTP
    public class FStore : Instruction
    {
        bool pop;

        public FStore(Operand _op1, bool _pop)
            : base()
        {
            opcount = 1;
            op1 = _op1;
            pop = _pop;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                Memory r1 = (Memory)op1;
                bytes.Add((byte)((r1.size == OPSIZE.DWord) ? 0xd9 : (r1.size == OPSIZE.TByte) ? 0xdb : 0xdd));
                int reg = (r1.size == OPSIZE.TByte) ? 0x38 : (pop ? 0x18 : 0x10);
                List<byte> membytes = r1.getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + reg + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                bytes.Add((byte)(pop ? 0xd9 : 0xdd));
                bytes.Add((byte)((pop ? 0xd8 : 0xd0) + ((Register)op1).code));
            }
        }

        public override string ToString()
        {
            return (pop) ? "FSTP" : "FST";
        }
    }

    //FLoadInteger - FILD
    public class FLoadInteger : Instruction
    {
        public FLoadInteger(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                Memory r1 = (Memory)op1;
                bytes.Add((byte)((r1.size == OPSIZE.DWord) ? 0xdb : 0xdf));
                List<byte> membytes = r1.getBytes(out mod, out rm);
                int reg = (r1.size == OPSIZE.QWord) ? 0x28 : 0x00;
                bytes.Add((byte)(mod * 0x40 + reg + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                bytes.Add(0xdb);
                bytes.Add((byte)(0xc0 + ((Register)op1).code));
            }
        }

        public override string ToString()
        {
            return "FILD";
        }
    }

    //FStoreInteger - FIST/FISTP/FISTTP
    public class FStoreInteger : Instruction
    {
        bool pop;
        bool trunc;

        public FStoreInteger(Operand _op1, bool _pop, bool _trunc)
            : base()
        {
            opcount = 1;
            op1 = _op1;
            pop = _pop;
            trunc = _trunc;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                Memory r1 = (Memory)op1;
                bytes.Add((byte)((r1.size == OPSIZE.DWord) ? 0xdb : ((r1.size == OPSIZE.QWord && trunc) ? 0xdd : 0xdf)));
                int reg = trunc ? 0x08 : (pop ? ((r1.size == OPSIZE.QWord) ? 0x38 : 0x18) : 0x10);
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + reg + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                bytes.Add(0xdb);
                int reg = trunc ? 0x08 : (pop ? 0x18 : 0x10);
                bytes.Add((byte)(0xc0 + reg + ((Register)op1).code));
            }
        }

        public override string ToString()
        {
            return (trunc) ? "FISTTP" : (pop) ? "FISTP" : "FIST";
        }
    }

    //FLoadBCD - FBLD
    public class FLoadBCD : Instruction
    {
        public FLoadBCD(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                bytes.Add(0xdf);
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + 0x20 + rm));
                bytes.AddRange(membytes);
            }
        }

        public override string ToString()
        {
            return "FBLD";
        }
    }

    //FStoreBCD - FBSTP
    public class FStoreBCD : Instruction
    {
        public FStoreBCD(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                bytes.Add(0xdf);
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + 0x30 + rm));
                bytes.AddRange(membytes);
            }
        }

        public override string ToString()
        {
            return "FBSTP";
        }
    }

    //FExchange - FXCH
    public class FExchange : Instruction
    {
        public FExchange(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add(0xd9);
            bytes.Add((byte)(0xc8 + ((Register)op1).code));
        }

        public override string ToString()
        {
            return "FXCH";
        }
    }

    //FConditionalMove - FCMOVB/FCMOVBE/FCMOVE/FCMOVNB/FCMOVNBE/FCMOVNE/FCMOVNU/FCMOVU
    public class FConditionalMove : Instruction
    {
        public enum CONDIT { MOVB, MOVE, MOVBE, MOVU, MOVNB, MOVNE, MOVNBE, MOVNU };

        CONDIT condit;

        public FConditionalMove(Operand _op1, Operand _op2, CONDIT _condit)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            condit = _condit;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add((byte)((condit >= CONDIT.MOVNB) ? 0xdb : 0xda));
            int reg = (condit >= CONDIT.MOVNB) ? ((int)condit - 4) : (int)condit;
            bytes.Add((byte)(0xc0 + (reg * 8) + ((Register)op2).code));
        }

        public override string ToString()
        {
            String result = "???";
            switch (condit)
            {
                case CONDIT.MOVB:
                    result = "FCMOVB";
                    break;
                case CONDIT.MOVNB:
                    result = "FCMOVNB";
                    break;
                case CONDIT.MOVE:
                    result = "FCMOVE";
                    break;
                case CONDIT.MOVNE:
                    result = "FCMOVNE";
                    break;
                case CONDIT.MOVBE:
                    result = "FCMOVBE";
                    break;
                case CONDIT.MOVNBE:
                    result = "FCMOVNBE";
                    break;
                case CONDIT.MOVU:
                    result = "FCMOVU";
                    break;
                case CONDIT.MOVNU:
                    result = "FCMOVNU";
                    break;
            }
            return result;
        }
    }

    //- arithmetic ----------------------------------------------------------------

    //FAdd - FADD/FADDP/FIADD
    public class FAdd : Instruction
    {
        bool intop;
        bool pop;

        public FAdd(Operand _op1, Operand _op2, bool _intop, bool _pop)
            : base()
        {
            opcount = (_op2 != null) ? 2 : 1;
            op1 = _op1;
            op2 = _op2;
            intop = _intop;
            pop = _pop;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                byte opbyte = (byte)(((Memory)op1).size == OPSIZE.DWord ? (intop ? 0xda : 0xd8) : (intop ? 0xde : 0xdc));
                bytes.Add(opbyte);
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                if (pop)
                {
                    bytes.Add(0xde);
                    bytes.Add((byte)(0xc0 + ((Register)op1).code));
                }
                else
                {
                    bytes.Add((byte)((((Register)op1).code == 0) ? 0xd8 : 0xdc));
                    int rm = (((Register)op1).code == 0) ? ((Register)op2).code : ((Register)op1).code;
                    bytes.Add((byte)(0xc0 + rm));
                }
            }
        }

        public override string ToString()
        {
            return (intop ? "FIADD" : (pop) ? "FADDP" : "FADD");
        }
    }

    //FSubtract - FSUB/FSUBR/FSUBP/FSUBRP/FISUB/FISUBR
    public class FSubtract : Instruction
    {
        bool intop;
        bool pop;
        bool reverse;

        public FSubtract(Operand _op1, Operand _op2, bool _intop, bool _pop, bool _rev)
            : base()
        {
            opcount = (_op2 != null) ? 2 : 1;
            op1 = _op1;
            op2 = _op2;
            intop = _intop;
            pop = _pop;
            reverse = _rev;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                byte opbyte = (byte)(((Memory)op1).size == OPSIZE.DWord ? (intop ? 0xda : 0xd8) : (intop ? 0xde : 0xdc));
                bytes.Add(opbyte);
                int reg = reverse ? 0x28 : 0x20;
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + reg + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                if (pop)
                {
                    bytes.Add(0xde);
                    bytes.Add((byte)((reverse ? 0xe0 : 0xe8) + ((Register)op1).code));
                }
                else if (((Register)op1).code == 0)
                {
                    bytes.Add(0xd8);
                    bytes.Add((byte)(0xc0 + (reverse ? 0x28 : 0x20) + ((Register)op2).code));
                }
                else
                {
                    bytes.Add(0xdc);
                    bytes.Add((byte)(0xc0 + (reverse ? 0x20 : 0x28) + ((Register)op1).code));
                }
            }
        }

        public override string ToString()
        {
            String result = (intop ? "FISUB" : "FSUB");
            if (reverse)
            {
                result = result + "R";
            }
            if (pop)
            {
                result = result + "P";
            }
            return result;
        }
    }

    //FMultiply - FMUL/FMULP/FIMUL
    public class FMulitply : Instruction
    {
        bool intop;
        bool pop;

        public FMulitply(Operand _op1, Operand _op2, bool _intop, bool _pop)
            : base()
        {
            opcount = (_op2 != null) ? 2 : 1;
            op1 = _op1;
            op2 = _op2;
            intop = _intop;
            pop = _pop;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                byte opbyte = (byte)(((Memory)op1).size == OPSIZE.DWord ? (intop ? 0xda : 0xd8) : (intop ? 0xde : 0xdc));
                bytes.Add(opbyte);
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + 0x08 + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                if (pop)
                {
                    bytes.Add(0xde);
                    bytes.Add((byte)(0xc8 + ((Register)op1).code));
                }
                else
                {
                    bytes.Add((byte)((((Register)op1).code == 0) ? 0xd8 : 0xdc));
                    int rm = (((Register)op1).code == 0) ? ((Register)op2).code : ((Register)op1).code;
                    bytes.Add((byte)(0xc8 + rm));
                }
            }
        }

        public override string ToString()
        {
            return (intop ? "FIMUL" : (pop) ? "FMULP" : "FMUL");
        }
    }

    //FDivide - FDIV/FDIVP/FDIVR/FDIVRP/FIDIV/FIDIVR
    public class FDivide : Instruction
    {
        bool intop;
        bool pop;
        bool reverse;

        public FDivide(Operand _op1, Operand _op2, bool _intop, bool _pop, bool _rev)
            : base()
        {
            opcount = (_op2 != null) ? 2 : 1;
            op1 = _op1;
            op2 = _op2;
            intop = _intop;
            pop = _pop;
            reverse = _rev;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                byte opbyte = (byte)(((Memory)op1).size == OPSIZE.DWord ? (intop ? 0xda : 0xd8) : (intop ? 0xde : 0xdc));
                bytes.Add(opbyte);
                int reg = reverse ? 0x38 : 0x30;
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + reg + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                if (pop)
                {
                    bytes.Add(0xde);
                    bytes.Add((byte)((reverse ? 0xf0 : 0xf8) + ((Register)op1).code));
                }
                else if (((Register)op1).code == 0)
                {
                    bytes.Add(0xd8);
                    bytes.Add((byte)(0xc0 + (reverse ? 0x38 : 0x30) + ((Register)op2).code));
                }
                else
                {
                    bytes.Add(0xdc);
                    bytes.Add((byte)(0xc0 + (reverse ? 0x30 : 0x38) + ((Register)op1).code));
                }
            }
        }

        public override string ToString()
        {
            String result = (intop ? "FIDIV" : "FDIV");
            if (reverse)
            {
                result = result + "R";
            }
            if (pop)
            {
                result = result + "P";
            }
            return result;
        }
    }

    //FRemainder - FPREM/FPREM1
    public class FRemainder : Instruction
    {
        public enum MODE { ROUND0, ROUND1 };

        public MODE mode;

        public FRemainder(MODE _mode)
            : base()
        {
            mode = _mode;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, (byte)((mode == MODE.ROUND1) ? 0xf5 : 0xf8) };
        }

        public override string ToString()
        {
            return (mode == MODE.ROUND1) ? "FPREM1" : "FPREM";
        }
    }

    //FAbsolute - FABS
    public class FAbsolute : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xe1 };
        }

        public override string ToString()
        {
            return "FABS";
        }
    }

    //FChangeSign - FCHS
    public class FChangeSign : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xe0 };
        }

        public override string ToString()
        {
            return "FCHS";
        }
    }

    //FRound - FRNDINT
    public class FRound : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xfc };
        }

        public override string ToString()
        {
            return "FRNDINT";
        }
    }

    //FScale - FSCALE
    public class FScale : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xfd };
        }

        public override string ToString()
        {
            return "FSCALE";
        }
    }

    //FSquareRoot - FSQRT
    public class FSquareRoot : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xfa };
        }

        public override string ToString()
        {
            return "FSQRT";
        }
    }

    //FExtract - FXTRACT
    public class FExtract : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xf4 };
        }

        public override string ToString()
        {
            return "FXTRACT";
        }
    }

    //- comparison ----------------------------------------------------------------

    //FCompare - FCOM/FCOMI/FCOMIP/FCOMP/FCOMPP
    public class FCompare : Instruction
    {
        bool pop;
        bool doublepop;
        bool setflags;

        public FCompare(Operand _op1, Operand _op2, bool _pop, bool _doublepop, bool _flags)
            : base()
        {
            opcount = (_op2 != null) ? 2 : (_op1 != null) ? 1 : 0;
            op1 = _op1;
            op2 = _op2;
            pop = _pop;
            doublepop = _doublepop;
            if (doublepop) pop = true;          //you have to pop once before you can pop twice!
            setflags = _flags;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                byte opbyte = (byte)(((Memory)op1).size == OPSIZE.DWord ? 0xd8 : 0xdc);
                bytes.Add(opbyte);
                int reg = pop ? 0x18 : 0x10;
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + reg + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                if (doublepop)
                {
                    bytes = new List<byte>() { 0xde, 0xd9 };        //FCOMPP
                }
                else if (setflags)
                {
                    bytes.Add((byte)(pop ? 0xdf : 0xdb));
                    bytes.Add((byte)(0xf0 + ((Register)op2).code));             //FCOMI/FCOMIP
                }
                else
                {
                    bytes.Add(0xd8);
                    bytes.Add((byte)(0xc0 + (pop ? 0x18 : 0x10) + ((Register)op1).code));       //FCOM/FCOMP
                }
            }
        }

        public override string ToString()
        {
            String result = "FCOM";
            if (setflags)
            {
                result = result + "I";
            }
            if (pop)
            {
                result = result + "P";
            }
            if (doublepop)
            {
                result = result + "P";
            }
            return result;
        }
    }

    //FCompareInt - FICOM/FICOMP
    public class FCompareInt : Instruction
    {
        bool pop;

        public FCompareInt(Operand _op1, Operand _op2, bool _pop)
            : base()
        {
            opcount = (_op2 != null) ? 2 : 1;
            op1 = _op1;
            op2 = _op2;
            pop = _pop;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                byte opbyte = (byte)(((Memory)op1).size == OPSIZE.DWord ? 0xda : 0xde);
                bytes.Add(opbyte);
                int reg = pop ? 0x18 : 0x10;
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + reg + rm));
                bytes.AddRange(membytes);
            }
        }

        public override string ToString()
        {
            return ((pop) ? "FICOMP" : "FICOM");
        }
    }

    //FCompareUnordered - FUCOM/FUCOMI/FUCOMIP/FUCOMP/FUCOMPP
    public class FCompareUnordered : Instruction
    {
        bool pop;
        bool doublepop;
        bool setflags;

        public FCompareUnordered(Operand _op1, Operand _op2, bool _pop, bool _doublepop, bool _flags)
            : base()
        {
            opcount = (_op2 != null) ? 2 : (_op1 != null) ? 1 : 0;
            op1 = _op1;
            op2 = _op2;
            pop = _pop;
            doublepop = _doublepop;
            if (doublepop) pop = true;          //you have to pop once before you can pop twice!
            setflags = _flags;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (doublepop)
            {
                bytes = new List<byte>() { 0xda, 0xe9 };        //FUCOMPP
            }
            else
            {
                if (setflags)
                {
                    bytes.Add((byte)(pop ? 0xdf : 0xdb));
                    bytes.Add((byte)(0xe8 + ((Register)op2).code));             //FUCOMI/FUCOMIP
                }
                else
                {
                    bytes.Add(0xdd);
                    bytes.Add((byte)((pop ? 0xe8 : 0xe0) + ((Register)op1).code));      //FUCOM/FUCOMP
                }
            }
        }

        public override string ToString()
        {
            String result = "FUCOM";
            if (setflags)
            {
                result = result + "I";
            }
            if (pop)
            {
                result = result + "P";
            }
            if (doublepop)
            {
                result = result + "P";
            }
            return result;
        }
    }

    //FTest - FTST
    public class FTest : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xe4 };
        }

        public override string ToString()
        {
            return "FTST";
        }
    }

    //FExamine - FXAM
    public class FExamine : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xe5 };
        }

        public override string ToString()
        {
            return "FXAM";
        }
    }

    //- trig / log -------------------------------------------------------------

    //FSine - FSIN
    public class FSine : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xfe };
        }

        public override string ToString()
        {
            return "FSIN";
        }
    }

    //FCosine - FCOS
    public class FCosine : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xff };
        }

        public override string ToString()
        {
            return "FCOS";
        }
    }

    //FSineCosine - FSINCOS
    public class FSineCosine : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xfb };
        }

        public override string ToString()
        {
            return "FSINCOS";
        }
    }

    //FTangent - FPTAN
    public class FTangent : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xf2 };
        }

        public override string ToString()
        {
            return "FPTAN";
        }

    }

    //FArcTangent - FPATAN
    public class FArcTangent : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xf3 };
        }

        public override string ToString()
        {
            return "FPATAN";
        }

    }

    //F2XM1 - F2XM1
    public class F2XM1 : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xf0 };
        }

        public override string ToString()
        {
            return "F2XM1";
        }
    }

    //FYL2X - FYL2X
    public class FYL2X : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xf1 };
        }

        public override string ToString()
        {
            return "FYL2X";
        }
    }

    //FYL2XPi - FYL2XP1
    public class FYL2XP1 : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xf9 };
        }

        public override string ToString()
        {
            return "FYL2XP1";
        }
    }

    //- constants -------------------------------------------------------------

    //FLoadConstant - FLD1/FLDZ/FLDPI/FLDL2E/FLDLN2/FLDL2T/FLDLG2
    public class FLoadConstant : Instruction
    {
        public enum CONSTOP { ONE, LOG210, LOG2E, PI, LOG102, LOGE2, ZERO };

        CONSTOP constOp;

        public FLoadConstant(CONSTOP _op)
        {
            constOp = _op;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, (byte)(0xe8 + constOp) };
        }

        public override string ToString()
        {
            String result = "???";
            switch (constOp)
            {
                case CONSTOP.ONE:
                    result = "FLD1";
                    break;
                case CONSTOP.LOG210:
                    result = "FLDL2T";
                    break;
                case CONSTOP.LOG2E:
                    result = "FLDL2E";
                    break;
                case CONSTOP.PI:
                    result = "FLDPI";
                    break;
                case CONSTOP.LOG102:
                    result = "FLDLG2";
                    break;
                case CONSTOP.LOGE2:
                    result = "FLDLN2";
                    break;
                case CONSTOP.ZERO:
                    result = "FLDZ";
                    break;
            }
            return result;
        }
    }

    //- control operations --------------------------------------------------------

    //FIncrement - FINCSTP
    public class FIncrement : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xf7 };
        }

        public override string ToString()
        {
            return "FINCSTP";
        }
    }

    //FDecrement - FDECSTP
    public class FDecrement : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd9, 0xf6 };
        }

        public override string ToString()
        {
            return "FDECSTP";
        }
    }

    //FFreeRegister - FFREE/FFREEP
    public class FFreeRegister : Instruction
    {
        bool pop;

        public FFreeRegister(Operand _op1, bool _pop)
            : base()
        {
            opcount = 1;
            op1 = _op1;
            pop = _pop;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add((byte)(pop ? 0xdf : 0xdd));
            bytes.Add((byte)(0xc0 + ((Register)op1).code));
        }


        public override string ToString()
        {
            return (pop) ? "FFREEP" : "FFREE";
        }
    }


    //FInitialize - FINIT/FNINIT
    public class FInitialize : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xdb, 0xe3 };
        }

        public override string ToString()
        {
            return "FNINIT";
        }
    }

    //FClearExceptions - FCLEX/FNCLEX
    public class FClearExceptions : Instruction
    {
        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xdb, 0xe2 };
        }

        public override string ToString()
        {
            return "FNCLEX";
        }
    }

    //FStoreControlWord - FSTCW/FNSTCW
    public class FStoreControlWord : Instruction
    {
        public FStoreControlWord(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                bytes.Add(0xd9);
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + 0x38 + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                bytes.Add(0xd9);
                bytes.Add((byte)(0xc0 + 0x38 + ((Register)op2).code));
            }
        }

        public override string ToString()
        {
            return "FNSTCW";
        }
    }

    //FLoadControlWord - FLDCW
    public class FLoadControlWord : Instruction
    {
        public FLoadControlWord(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                bytes.Add(0xd9);
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + 0x28 + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                bytes.Add(0xd9);
                bytes.Add((byte)(0xc0 + 0x28 + ((Register)op1).code));
            }
        }

        public override string ToString()
        {
            return "FLDCW";
        }
    }

    //FStorenvironment - FSTENV/FNSTENV
    public class FStoreEnvironment : Instruction
    {
        public FStoreEnvironment(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                bytes.Add(0xd9);
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + 0x30 + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                bytes.Add(0xd9);
                bytes.Add((byte)(0xc0 + 0x30 + ((Register)op1).code));
            }
        }

        public override string ToString()
        {
            return "FNSTENV";
        }
    }

    //FLoadEnvironment - FLDENV
    public class FLoadEnvironment : Instruction
    {
        public FLoadEnvironment(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                bytes.Add(0xd9);
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + 0x20 + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                bytes.Add(0xd9);
                bytes.Add((byte)(0xc0 + 0x20 + ((Register)op1).code));
            }
        }

        public override string ToString()
        {
            return "FLDENV";
        }
    }

    //FSaveState - FSAVE/FNSAVE
    public class FSaveState : Instruction
    {
        public FSaveState(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                bytes.Add(0xdd);
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + 0x30 + rm));
                bytes.AddRange(membytes);
            }
        }

        public override string ToString()
        {
            return "FNSAVE";
        }
    }

    //FRestoreState - FRSTOR
    public class FRestoreState : Instruction
    {
        public FRestoreState(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                bytes.Add(0xdd);
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + 0x20 + rm));
                bytes.AddRange(membytes);
            }
        }

        public override string ToString()
        {
            return "FRSTOR";
        }
    }

    //FStoreStatusWord - FSTSW/FNSTSW 
    public class FStoreStatusWord : Instruction
    {
        public FStoreStatusWord(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Memory)
            {
                int mod;
                int rm;
                bytes.Add(0xdd);
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + 0x38 + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                bytes = new List<byte>() { 0xdf, 0xe0 };
            }
        }

        public override string ToString()
        {
            return "FNSTSW";
        }
    }

    //FNoOp - FNOP/FNDISI/FNENI/FNSETPM
    public class FNoOp : Instruction
    {
        public enum NOPTYPE { FNOP, FENI, FDISI, FSETPM };

        NOPTYPE nopType;

        public FNoOp()
        {
            nopType = NOPTYPE.FNOP;
        }

        public FNoOp(NOPTYPE type)
        {
            nopType = type;
        }

        int[] nopBytes = { 0xd0, 0xe0, 0xe1, 0xe4 };

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add((byte)((nopType == NOPTYPE.FNOP) ? 0xd9 : 0xdb));
            bytes.Add((byte)(nopBytes[(int)nopType]));
        }

        String[] nopTypeStr = { "FNOP", "FENI", "FDISI", "FSETPM" };

        public override string ToString()
        {
            return nopTypeStr[(int)nopType];
        }
    }
}
