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

    //SSEMovePacked - MOVAPS/MOVUPS
    public class SSEMovePacked : Instruction
    {
        bool aligned;

        public SSEMovePacked(Operand _op1, Operand _op2, bool _aligned)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            aligned = _aligned;
        }

        public override string ToString()
        {
            return aligned ? "MOVAPS" : "MOVUPS";
        }
    }

    //SSEMoveHigh - MOVHPS/MOVLHPS
    public class SSEMoveHigh : Instruction
    {
        bool lowToHigh;

        public SSEMoveHigh(Operand _op1, Operand _op2, bool _lowToHigh)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            lowToHigh = _lowToHigh;
        }

        public override string ToString()
        {
            return lowToHigh ? "MOVLHPS" : "MOVHPS";
        }
    }

    //SSEMoveLow - MOVLPS/MOVHLPS
    public class SSEMoveLow : Instruction
    {
        bool highToLow;

        public SSEMoveLow(Operand _op1, Operand _op2, bool _highToLow)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            highToLow = _highToLow;
        }

        public override string ToString()
        {
            return highToLow ? "MOVHLPS" : "MOVLPS";
        }
    }

    //SSEExtract - MOVMSKPS
    public class SSEExtract : Instruction
    {
        public SSEExtract(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "MOVMSKPS";
        }
    }

    //SSEMoveScalar - MOVSS
    public class SSEMoveScalar : Instruction
    {
        public SSEMoveScalar(Operand _op1, Operand _op2)
            : base()
        {
            opcount = (_op2 != null) ? 2 : 1;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "MOVSS";
        }
    }

//- arithmetic -------------------------------------------------------------

    //SSEAdd - ADDPS/ADDSS
    public class SSEAdd : Instruction
    {
        bool packed;

        public SSEAdd(Operand _op1, Operand _op2, bool _packed)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            packed = _packed;
        }

        public override string ToString()
        {
            return packed ? "ADDPS" : "ADDSS";
        }
    }

    //SSESubtract - SUBPS/SUBSS
    public class SSESubtract : Instruction
    {
        bool packed;

        public SSESubtract(Operand _op1, Operand _op2, bool _packed)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            packed = _packed;
        }

        public override string ToString()
        {
            return packed ? "SUBPS" : "SUBSS";
        }
    }

    //SSEMult - MULPS/MULSS
    public class SSEMult : Instruction
    {
        bool packed;

        public SSEMult(Operand _op1, Operand _op2, bool _packed)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            packed = _packed;
        }

        public override string ToString()
        {
            return packed ? "MULPS" : "MULSS";
        }
    }

    //SSEDivide - DIVPS/DIVSS
    public class SSEDivide : Instruction
    {
        bool packed;

        public SSEDivide(Operand _op1, Operand _op2, bool _packed)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            packed = _packed;
        }

        public override string ToString()
        {
            return packed ? "DIVPS" : "DIVSS";
        }
    }

    //SSEReciprocal - RCPPS/RCPSS
    public class SSEReciprocal : Instruction
    {
        bool packed;

        public SSEReciprocal(Operand _op1, Operand _op2, bool _packed)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            packed = _packed;
        }

        public override string ToString()
        {
            return packed ? "RCPPS" : "RCPSS";
        }
    }

    //SSESqrt - SQRTPS/SQRTSS
    public class SSESqrt : Instruction
    {
        bool packed;

        public SSESqrt(Operand _op1, Operand _op2, bool _packed)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            packed = _packed;
        }

        public override string ToString()
        {
            return packed ? "SQRTPS" : "SQRTSS";
        }
    }

    //SSEReciprocalSqrt - RSQRTPS/RSQRTSS
    public class SSEReciprocalSqrt : Instruction
    {
        bool packed;

        public SSEReciprocalSqrt(Operand _op1, Operand _op2, bool _packed)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            packed = _packed;
        }

        public override string ToString()
        {
            return packed ? "RSQRTPS" : "RSQRTSS";
        }
    }

    //SSEMax - MAXPS/MAXSS
    public class SSEMax : Instruction
    {
        bool packed;

        public SSEMax(Operand _op1, Operand _op2, bool _packed)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            packed = _packed;
        }

        public override string ToString()
        {
            return packed ? "MAXPS" : "MAXSS";
        }
    }

    //SSEMin - MINPS/MINSS
    public class SSEMin : Instruction
    {
        bool packed;

        public SSEMin(Operand _op1, Operand _op2, bool _packed)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            packed = _packed;
        }

        public override string ToString()
        {
            return packed ? "MINPS" : "MINSS";
        }
    }

//- comparison ----------------------------------------------------------------

    //SSECompare - CMPPS/CMPSS
    public class SSECompare : Instruction
    {
        bool packed;        

        public SSECompare(Operand _op1, Operand _op2, Operand _op3, bool _packed)
            : base()
        {
            opcount = 3;
            op1 = _op1;
            op2 = _op2;
            op3 = _op3;
            packed = _packed;            
        }

        public override string ToString()
        {
            return packed ? "CMPPS" : "CMPSS";
        }
    }

    //SSECompareSetFlags - COMISS/UCOMISS
    public class SSECompareSetFlags : Instruction
    {
        bool unordered;

        public SSECompareSetFlags(Operand _op1, Operand _op2, bool _unordered)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            unordered = _unordered;
        }

        public override string ToString()
        {
            return unordered ? "UCOMISS" : "COMISS";
        }
    }

//- logical -------------------------------------------------------------------

    //SSEAnd - ANDPS
    public class SSEAnd : Instruction
    {
        public SSEAnd(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "ANDPS";
        }
    }

    //SSENand - ANDNPS
    public class SSENand : Instruction
    {
        public SSENand(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "ANDNPS";
        }
    }

    //SSEOr - ORPS
    public class SSEOr : Instruction
    {
        public SSEOr(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "ORPS";
        }
    }

    //SSEXor - XORPS
    public class SSEXor : Instruction
    {
        public SSEXor(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "XORPS";
        }
    }

//- shuffle/unpack ------------------------------------------------------------

    //SSEShuffle - SHUFPS
    public class SSEShuffle : Instruction
    {
        bool intop;
        bool pop;

        public SSEShuffle(Operand _op1, Operand _op2, Operand _op3)
            : base()
        {
            opcount = 3;
            op1 = _op1;
            op2 = _op2;
            op3 = _op3;
        }

        public override string ToString()
        {
            return "SHUFPS";
        }
    }

    //SSEUnpack - UNPCKHPS/UNPCKLPS
    public class SSEUnpack : Instruction
    {
        public enum MODE { HIGH, LOW }

        MODE mode;
        
        public SSEUnpack(Operand _op1, Operand _op2, MODE _mode)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            mode = _mode;
        }

        public override string ToString()
        {
            return (mode == MODE.HIGH) ? "UNPCKHPS" : "UNPCKLPS";
        }
    }

    //- conversion ----------------------------------------------------------------

    //SSEConvertFromInt - CVTPI2PS/CVTSI2SS
    public class SSEConvertFromInt : Instruction
    {
        bool packed;        

        public SSEConvertFromInt(Operand _op1, Operand _op2, bool _packed)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            packed = _packed;
        }

        public override string ToString()
        {
            return packed ? "CVTPI2PS" : "CVTSI2SS";
        }
    }

    //SSEConvertPackedToInt - CVTPS2PI/CVTTPS2PI
    public class SSEConvertPackedToInt : Instruction
    {
        bool truncate;

        public SSEConvertPackedToInt(Operand _op1, Operand _op2, bool _truncate)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            truncate = _truncate;
        }

        public override string ToString()
        {
            return truncate ? "CVTTPS2PI" : "CVTPS2PI";
        }
    }

    //SSEConvertScalarToInt - CVTSS2SI/CVTTSS2SI
    public class SSEConvertScalarToInt : Instruction
    {
        bool truncate;

        public SSEConvertScalarToInt(Operand _op1, Operand _op2, bool _truncate)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            truncate = _truncate;
        }

        public override string ToString()
        {
            return truncate ? "CVTTSS2SI" : "CVTSS2SI";
        }
    }

    //- state mgmt ----------------------------------------------------------------

    //SSELoadState - LDMXCSR
    public class SSELoadState : Instruction
    {
        public SSELoadState(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "LDMXCSR";
        }
    }

    //SSEStoreState - STMXCSR
    public class SSEStoreState : Instruction
    {
        public SSEStoreState(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "STMXCSR";
        }
    }

    //- control -------------------------------------------------------------------

    //SSEStoreQuadBytes - MASKMOVQ
    public class SSEStoreQuadBytes : Instruction
    {
        bool intop;
        bool pop;

        public SSEStoreQuadBytes(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "MASKMOVQ";
        }
    }

    //SSEStoreQuad - MOVNTQ
    public class SSEStoreQuad : Instruction
    {
        public SSEStoreQuad(Operand _op1, Operand _op2)
            : base()
        {
            opcount = (_op2 != null) ? 2 : 1;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "MOVNTQ";
        }
    }

    //SSEStorePacked - MOVNTPS
    public class SSEStorePacked : Instruction
    {
        public SSEStorePacked(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "MOVNTPS";
        }
    }

    //SSEPrefetchData - PREFETCHNTA/PREFETCHT0/PREFETCHT1/PREFETCHT2
    public class SSEPrefetchData : Instruction
    {
        public enum MODE { NTA, T0, T1, T2 }

        MODE mode;

        public SSEPrefetchData(Operand _op1, MODE _mode)
            : base()
        {
            opcount = 1;
            op1 = _op1;
            mode = _mode;
        }

        String[] modeStr = { "NTA", "T0", "T1", "T2" };

        public override string ToString()
        {
            return "PREFETCH" + modeStr[(int)mode];
        }
    }

    //SSEStoreFence - SFENCE
    public class SSEStoreFence : Instruction
    {

        public SSEStoreFence()
            : base()
        {
        }

        public override string ToString()
        {
            return "SFENCE";
        }
    }
}
