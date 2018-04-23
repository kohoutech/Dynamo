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

//- arithmetic ----------------------------------------------------------------

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

        public override string ToString()
        {
            return (intop ? "FIADD" : (pop) ? "FADDP" : "FADD");
        }
    }

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

        public override string ToString()
        {
            return (intop ? "FIMUL" : (pop) ? "FMULP" : "FMUL");
        }
    }

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

    public class FSquareRoot : Instruction
    {
        public override string ToString()
        {
            return "FSQRT";
        }
    }

    public class F2XM1 : Instruction
    {
        public override string ToString()
        {
            return "F2XM1";
        }
    }

    public class FYL2X : Instruction
    {
        public override string ToString()
        {
            return "FYL2X";
        }
    }

    public class FYL2XP1 : Instruction
    {
        public override string ToString()
        {
            return "FYL2XP1";
        }
    }

//- trignometric -------------------------------------------------------------

    public class FSine : Instruction
    {
        public override string ToString()
        {
            return "FSIN";
        }
    }

    public class FCosine : Instruction
    {
        public override string ToString()
        {
            return "FCOS";
        }
    }

    public class FSineCosine : Instruction
    {
        public override string ToString()
        {
            return "FSINCOS";
        }
    }

    public class FTangent : Instruction
    {
        public override string ToString()
        {
            return "FPTAN";
        }

    }

    public class FArcTangent : Instruction
    {
        public override string ToString()
        {
            return "FPATAN";
        }

    }

//- numeric ----------------------------------------------------------------

    public class FChangeSign : Instruction
    {
        public override string ToString()
        {
            return "FCHS";
        }
    }

    public class FAbsolute : Instruction
    {
        public override string ToString()
        {
            return "FABS";
        }
    }

    public class FRound : Instruction
    {
        public override string ToString()
        {
            return "FRNDINT";
        }
    }

    public class FScale : Instruction
    {
        public override string ToString()
        {
            return "FSCALE";
        }
    }

    public class FExtract : Instruction
    {
        public override string ToString()
        {
            return "FXTRACT";
        }
    }

    public class FRemainder : Instruction
    {
        public override string ToString()
        {
            return "FPREM";
        }
    }

//- stack operations ----------------------------------------------------------

    public class FIncrement : Instruction
    {
        public override string ToString()
        {
            return "FINCSTP";
        }
    }

    public class FDecrement : Instruction
    {
        public override string ToString()
        {
            return "FDECSTP";
        }
    }

//- comparison ----------------------------------------------------------------

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

        public override string ToString()
        {
            return ((pop) ? "FICOMP" : "FICOM");            
        }
    }

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

    public class FTest : Instruction
    {
        public override string ToString()
        {
            return "FTST";
        }
    }

    public class FExamine : Instruction
    {
        public override string ToString()
        {
            return "FXAM";
        }
    }

//- data operations -----------------------------------------------------------

    public class FExchange : Instruction
    {
        public FExchange(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "FXCH";
        }
    }

    public class FConditionalMove : Instruction
    {
        public enum CONDIT { MOVB, MOVNB, MOVE, MOVNE, MOVBE, MOVNBE, MOVU, MOVNU };

        CONDIT condit;

        public FConditionalMove(Operand _op1, Operand _op2, CONDIT _condit)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            condit = _condit;
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

    public class FLoad : Instruction
    {
        public FLoad(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "FLD";
        }
    }

    public class FLoadInteger : Instruction
    {
        public FLoadInteger(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "FILD";
        }
    }

    public class FLoadBCD : Instruction
    {
        public FLoadBCD(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "FBLD";
        }
    }

    public class FLoadConstant : Instruction
    {
        public enum CONSTOP { ONE, LOG210, LOG2E, PI, LOG102, LOGE2, ZERO };

        CONSTOP constOp;

        public FLoadConstant(CONSTOP _op)
        {
            constOp = _op;
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

        public override string ToString()
        {
            return (pop) ? "FSTP" : "FST";
        }
    }

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

        public override string ToString()
        {
            return (trunc) ? "FISTTP" : (pop) ? "FISTP" : "FIST";
        }
    }

    public class FStoreBCD : Instruction
    {
        public FStoreBCD(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "FBSTP";
        }
    }

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

        public override string ToString()
        {
            return (pop) ? "FFREEP" : "FFREE";
        }
    }

//- control operations --------------------------------------------------------

    public class FInitialize : Instruction
    {
        public override string ToString()
        {
            return "FNINIT";
        }
    }

    public class FClearExceptions : Instruction
    {
        public override string ToString()
        {
            return "FNCLEX";
        }
    }

    public class FLoadEnvironment : Instruction
    {
        public FLoadEnvironment(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "FLDENV";
        }
    }

    public class FStoreEnvironment : Instruction
    {
        public FStoreEnvironment(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "FNSTENV";
        }
    }

    public class FLoadControlWord : Instruction
    {
        public FLoadControlWord(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "FLDCW";
        }
    }

    public class FStoreControlWord : Instruction
    {
        public FStoreControlWord(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "FNSTCW";
        }
    }

    public class FStoreStatusWord : Instruction
    {
        public FStoreStatusWord(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "FNSTSW";
        }
    }

    public class FSaveState : Instruction
    {
        public FSaveState(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "FNSAVE";
        }
    }

    public class FRestoreState : Instruction
    {
        public FRestoreState(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "FRSTOR";
        }
    }

//- miscellaneous -------------------------------------------------------------

    public class FNoOp : Instruction
    {
        public enum NOPTYPE {  FNOP, FENI, FDISI, FSETPM };

        NOPTYPE nopType;

        public FNoOp()
        {
            nopType = NOPTYPE.FNOP;
        }

        public FNoOp(NOPTYPE type)
        {
            nopType = type;
        }

        String[] nopTypeStr = { "FNOP", "FENI", "FDISI", "FSETPM" };

        public override string ToString()
        {
            return nopTypeStr[(int)nopType];
        }
    }
}
