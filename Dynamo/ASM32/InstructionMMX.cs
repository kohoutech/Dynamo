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
    //- data transfer -------------------------------------------------------------

    //MMXMoveWord - MOVD/MOVQ
    public class MMXMoveWord : Instruction
    {
        public enum MODE { DOUBLE, QUAD }
        MODE mode;

        public MMXMoveWord(Operand _op1, Operand _op2, MODE _mode)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            mode = _mode;
        }

        public override string ToString()
        {
            return (mode == MODE.DOUBLE) ? "MOVD" : "MOVQ";
        }
    }

    //- conversion ----------------------------------------------------------------
    
    //MMXPack - PACKSSDW/PACKSSWB/PACKUSWB
    public class MMXPack : Instruction
    {
        public enum MODE { WB, DW }
        MODE mode;
        bool signed;

        public MMXPack(Operand _op1, Operand _op2, MODE _mode, bool _signed)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            mode = _mode;
            signed = _signed;
        }

        public override string ToString()
        {
            return (mode == MODE.WB) ? (signed ? "PACKSSWB" : "PACKUSWB") : "PACKSSDW";                
        }
    }

    //MMXUnpack - PUNPCKHBW/PUNPCKHDQ/PUNPCKHWD/PUNPCKLBW/PUNPCKLDQ/PUNPCKLWD
    public class MMXUnpack : Instruction
    {
        public enum MODE { BW, WD, DQ }
        MODE mode;
        bool high;

        public MMXUnpack(Operand _op1, Operand _op2, MODE _mode, bool _high)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            mode = _mode;
            high = _high;
        }

        public override string ToString()
        {
            return (mode == MODE.BW) ? (high ? "PUNPCKHBW" : "PUNPCKLBW") :
                (mode == MODE.WD) ? (high ? "PUNPCKHWD" : "PUNPCKLWD") :
                (high ? "PUNPCKHDQ" : "PUNPCKLDQ");
        }
    }

    //- arithmetic -------------------------------------------------------------

    //MMXAdd - PADDB/PADDW/PADDD/PADDSB/PADDSW/PADDUSB/PADDUSW
    public class MMXAdd : Instruction
    {
        public enum SIZE { BYTE, WORD, DOUBLE }
        SIZE size;
        bool saturation;
        bool signed;

        public MMXAdd(Operand _op1, Operand _op2, SIZE _size, bool _sat, bool _sign)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            size = _size;
            saturation = _sat;
            signed = _sign;
        }

        public override string ToString()
        {
            return !saturation ? ((size == SIZE.BYTE) ? "PADDB" : (size == SIZE.WORD) ? "PADDW" : "PADDD") :
                signed ? ((size == SIZE.BYTE) ? "PADDSB" : "PADDSW") :
                ((size == SIZE.BYTE) ? "PADDUSB" : "PADDUSW");
        }
    }

    //MMXSubtract - PSUBB/PSUBW/PSUBD/PSUBSB/PSUBSW/PSUBUSB/PSUBUSW
    public class MMXSubtract : Instruction
    {
        public enum SIZE { BYTE, WORD, DOUBLE }
        SIZE size;
        bool saturation;
        bool signed;

        public MMXSubtract(Operand _op1, Operand _op2, SIZE _size, bool _sat, bool _sign)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            size = _size;
            saturation = _sat;
            signed = _sign;
        }

        public override string ToString()
        {
            return !saturation ? ((size == SIZE.BYTE) ? "PSUBB" : (size == SIZE.WORD) ? "PSUBW" : "PSUBD") :
                signed ? ((size == SIZE.BYTE) ? "PSUBSB" : "PSUBSW") :
                ((size == SIZE.BYTE) ? "PSUBUSB" : "PSUBUSW");
        }
    }

    //MMXMult - PMULHW/PMULLW
    public class MMXMult : Instruction
    {
        public enum MODE { HIGH, LOW }
        MODE mode;

        public MMXMult(Operand _op1, Operand _op2, MODE _mode)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            mode = _mode;
        }

        public override string ToString()
        {
            return (mode == MODE.HIGH) ? "PMULHW" : "PMULLW";
        }
    }

    //MMXMultAdd - PMADDWD
    public class MMXMultAdd : Instruction
    {
        public MMXMultAdd(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "PMADDWD";
        }
    }

    //- comparison ----------------------------------------------------------------

    //MMXCompEqual - PCMPEQB/PCMPEQW/PCMPEQD
    public class MMXCompEqual : Instruction
    {
        public enum MODE { BYTE, WORD, DOUBLE }
        MODE mode;

        public MMXCompEqual(Operand _op1, Operand _op2, MODE _mode)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            mode = _mode;
        }

        public override string ToString()
        {
            return (mode == MODE.BYTE) ? "PCMPEQB" : (mode == MODE.WORD) ? "PCMPEQW" : "PCMPEQD";
        }
    }

    //MMXCompGtrThn - PCMPGTB/PCMPGTW/PCMPGTD
    public class MMXCompGtrThn : Instruction
    {
        public enum MODE { BYTE, WORD, DOUBLE }
        MODE mode;

        public MMXCompGtrThn(Operand _op1, Operand _op2, MODE _mode)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            mode = _mode;
        }

        public override string ToString()
        {
            return (mode == MODE.BYTE) ? "PCMPGTB" : (mode == MODE.WORD) ? "PCMPGTW" : "PCMPGTD";
        }
    }

    //- shift / rotate ------------------------------------------------------------

    //MMXShift - PSLLW/PSLLD/PSLLQ/PSRLW/PSRLD/PSRLQ/PSRAW/PSRAD/PSRAQ
    public class MMXShift : Instruction
    {
        public enum SIZE { WORD, DOUBLE, QUAD }
        public enum DIR { LEFT, RIGHT }
        SIZE size;
        DIR direction;
        bool arithmetic;

        public MMXShift(Operand _op1, Operand _op2, SIZE _size, DIR _dir, bool _arith)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            size = _size;
            direction = _dir;
            arithmetic = _arith;
        }

        public override string ToString()
        {
            return (direction == DIR.LEFT) ? ((size == SIZE.WORD) ? "PSLLW" : (size == SIZE.DOUBLE) ? "PSLLD" : "PSLLQ") :
                (arithmetic) ? ((size == SIZE.WORD) ? "PSRAW" : (size == SIZE.DOUBLE) ? "PSRAD" : "PSRAQ") :
                ((size == SIZE.WORD) ? "PSRLW" : (size == SIZE.DOUBLE) ? "PSRLD" : "PSRLQ");
        }
    }

    //- logical -------------------------------------------------------------------

    //MMXAnd - PAND
    public class MMXAnd : Instruction
    {
        public MMXAnd(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "PAND";
        }
    }

    //MMXAddNot - PANDN
    public class MMXAddNot : Instruction
    {
        public MMXAddNot(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "PANDN";
        }
    }

    //MMXOr - POR
    public class MMXOr : Instruction
    {
        public MMXOr(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "POR";
        }
    }

    //MMXXor - PXOR
    public class MMXXor : Instruction
    {
        public MMXXor(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "PXOR";
        }
    }

    //- state mgmt ----------------------------------------------------------------

    //MMXEmptyState  - EMMS
    public class MMXEmptyState : Instruction
    {
        public MMXEmptyState()
            : base()
        {
        }

        public override string ToString()
        {
            return "EMMS";
        }
    }

    //StoreMMXState - FXSAVE
    public class StoreMMXState : Instruction
    {
        public StoreMMXState(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "FXSAVE";
        }
    }

    //RestoreMMXState - FXRSTOR
    public class RestoreMMXState : Instruction
    {
        public RestoreMMXState(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "FXRSTOR";
        }
    }
}
