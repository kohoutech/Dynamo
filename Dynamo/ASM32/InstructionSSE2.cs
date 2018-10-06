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

    //SSE2MoveAligned - MOVAPD
    public class SSE2MoveAligned : Instruction
    {
        public SSE2MoveAligned(Operand _op1, Operand _op2)
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

    //SSE2MoveHigh - MOVHPD
    public class SSE2MoveHigh : Instruction
    {
        public SSE2MoveHigh(Operand _op1, Operand _op2)
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

    //SSE2MoveLow - MOVLPD
    public class SSE2MoveLow : Instruction
    {
        public SSE2MoveLow(Operand _op1, Operand _op2)
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

    //SSE2Extract - MOVMSKPD
    public class SSE2Extract : Instruction
    {
        public SSE2Extract(Operand _op1, Operand _op2)
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

    //SSE2MoveScalar - MOVSD
    public class SSE2MoveScalar : Instruction
    {
        public SSE2MoveScalar(Operand _op1, Operand _op2)
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

    //- arithmetic -------------------------------------------------------------

    //SSE2Add - ADDPD/ADDSD
    public class SSE2Add : Instruction
    {
        public SSE2Add(Operand _op1, Operand _op2)
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

    //SSE2Sub - SUBPD/SUBSD
    public class SSE2Sub : Instruction
    {
        public SSE2Sub(Operand _op1, Operand _op2)
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

    //SSE2Mult - MULPD/MULSD
    public class SSE2Mult : Instruction
    {
        public SSE2Mult(Operand _op1, Operand _op2)
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

    //SSE2Divide - DIVPD/DIVSD
    public class SSE2Divide : Instruction
    {
        public SSE2Divide(Operand _op1, Operand _op2)
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

    //SSE2Sqrt - SQRTPD/SQRTSD
    public class SSE2Sqrt : Instruction
    {
        public SSE2Sqrt(Operand _op1, Operand _op2)
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

    //SSE2Max - MAXPD/MAXSD
    public class SSE2Max : Instruction
    {
        public SSE2Max(Operand _op1, Operand _op2)
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

    //SSE2Min - MINPD/MINSD
    public class SSE2Min : Instruction
    {
        public SSE2Min(Operand _op1, Operand _op2)
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

    //- logical -------------------------------------------------------------

    //SSE2And - ANDPD
    public class SSE2And : Instruction
    {
        public SSE2And(Operand _op1, Operand _op2)
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

    //SSE2Nand - ANDNPD
    public class SSE2Nand : Instruction
    {
        public SSE2Nand(Operand _op1, Operand _op2)
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

    //SSE2Or - ORPD
    public class SSE2Or : Instruction
    {
        public SSE2Or(Operand _op1, Operand _op2)
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

    //SSE2Xor - XORPD
    public class SSE2Xor : Instruction
    {
        public SSE2Xor(Operand _op1, Operand _op2)
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

    //- comparison -------------------------------------------------------------

    //SSE2Compare - CMPPD/CMPSD
    public class SSE2Compare : Instruction
    {
        public SSE2Compare(Operand _op1, Operand _op2)
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

    //SSE2CompareSetFlags - COMISD/UCOMISD
    public class SSE2CompareSetFlags : Instruction
    {
        public SSE2CompareSetFlags(Operand _op1, Operand _op2)
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

    //- shuffle / unpack -------------------------------------------------------------

    //SSE2Shuffle - SHUFPD
    public class SSE2Shuffle : Instruction
    {
        public SSE2Shuffle(Operand _op1, Operand _op2)
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

    //SSE2Unpack - UNPCKHPD/UNPCKLPD
    public class SSE2Unpack : Instruction
    {
        public SSE2Unpack(Operand _op1, Operand _op2)
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

    //- conversion -------------------------------------------------------------
    
    //SSEConvertPacked - CVTPD2PI/CVTTPD2PI/CVTPI2PD
    public class SSEConvertPacked : Instruction
    {
        public SSEConvertPacked(Operand _op1, Operand _op2)
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

    //SSE2ConvertScalar - CVTSD2SI/CVTTSD2SI/CVTSI2SD
    public class SSE2ConvertScalar : Instruction
    {
        public SSE2ConvertScalar(Operand _op1, Operand _op2)
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

    //SSE2ConvertSingle - CVTPS2DQ/CVTTPS2DQ/CVTDQ2PS
    public class SSE2ConvertSingle : Instruction
    {
        public enum DIR { SINGLETODOUBLE, DOUBLETOSINGLE }
        DIR direction;
        bool truncate;

        public SSE2ConvertSingle(Operand _op1, Operand _op2, DIR _dir, bool _trunc)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            direction = _dir;
            truncate = _trunc;
        }

        public override string ToString()
        {
            return (direction == DIR.SINGLETODOUBLE) ? (truncate ? "CVTTPS2DQ" : "CVTPS2DQ") : "CVTDQ2PS";
        }
    }

    //SSE2ConvertDouble - CVTPD2DQ/CVTTPD2DQ/CVTDQ2PD
    public class SSE2ConvertDouble : Instruction
    {
        public SSE2ConvertDouble(Operand _op1, Operand _op2)
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

    //SSE2ConvertPrecision - CVTPS2PD/CVTPD2PS/CVTSS2SD/CVTSD2SS
    public class SSE2ConvertPrecision : Instruction
    {
        public enum DIR { SINGLETODOUBLE, DOUBLETOSINGLE }
        DIR direction;
        bool packed;

        public SSE2ConvertPrecision(Operand _op1, Operand _op2, DIR _dir, bool _packed)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            direction = _dir;
            packed = _packed;
        }

        public override string ToString()
        {
            return (direction == DIR.SINGLETODOUBLE) ?
                (packed ? "CVTPS2PD" : "CVTSS2SD") :
                (packed ? "CVTPD2PS" : "CVTSD2SS");
        }
    }

    //- 128-bit -------------------------------------------------------------

    //SSE2MovePacked128 - MOVDQA,VMOVDQA32/64/MOVDQU,VMOVDQU8/16/32/64
    public class SSE2MovePacked128 : Instruction
    {
        public SSE2MovePacked128(Operand _op1, Operand _op2)
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

    //SSE2MoveQuad128 - MOVQ2DQ/MOVDQ2Q
    public class SSE2MoveQuad128 : Instruction
    {
        public SSE2MoveQuad128(Operand _op1, Operand _op2)
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

    //SSE2Mult128 - PMULUDQ
    public class SSE2Mult128 : Instruction
    {
        public SSE2Mult128(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "PMULUDQ";
        }
    }

    //SSE2Add128 - PADDQ
    public class SSE2Add128 : Instruction
    {
        public SSE2Add128(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "PADDQ";
        }
    }

    //SSE2Subtract128 - PSUBQ
    public class SSE2Subtract128 : Instruction
    {
        public SSE2Subtract128(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "PSUBQ";
        }
    }

    //SSE2Shuffle128 - PSHUFLW/PSHUFHW/PSHUFD
    public class SSE2Shuffle128 : Instruction
    {
        public SSE2Shuffle128(Operand _op1, Operand _op2)
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

    //SSE2Shift128 - PSLLDQ/PSRLDQ
    public class SSE2Shift128 : Instruction
    {
        public SSE2Shift128(Operand _op1, Operand _op2)
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

    //SSE2Unpack128 - PUNPCKHQDQ/PUNPCKLQDQ
    public class SSE2Unpack128 : Instruction
    {
        public SSE2Unpack128(Operand _op1, Operand _op2)
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

    //- control -------------------------------------------------------------

    //SSE2LoadFence - LFENCE
    public class SSE2LoadFence : Instruction
    {
        public SSE2LoadFence()
            : base()
        {
        }

        public override string ToString()
        {
            return "LFENCE";
        }
    }

    //SSE2MemoryFence - MFENCE
    public class SSE2MemoryFence : Instruction
    {
        public SSE2MemoryFence()
            : base()
        {
        }

        public override string ToString()
        {
            return "MFENCE";
        }
    }

    //SSE2Pause - PAUSE
    public class SSE2Pause : Instruction
    {
        public SSE2Pause(Operand _op1, Operand _op2)
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

    //SSE2StoreQuadBytes - MASKMOVDQU
    public class SSE2StoreQuadBytes : Instruction
    {
        public SSE2StoreQuadBytes(Operand _op1, Operand _op2)
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

    //SSE2StorePacked - MOVNTPD/MOVNTDQ
    public class SSE2StorePacked : Instruction
    {
        public SSE2StorePacked(Operand _op1, Operand _op2)
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

    //SSE2StoreInt - MOVNTI
    public class SSE2StoreInt : Instruction
    {
        public SSE2StoreInt(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "MOVNTI";
        }
    }
}
