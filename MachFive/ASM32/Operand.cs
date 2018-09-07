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
    public class Operand
    {
       public enum OPSIZE { Byte, SignedByte, Word, DWord, QWord, FWord, TByte, None };
    }

//- immediate ----------------------------------------------------------------

    public class Immediate : Operand
    {
        public uint val;
        public OPSIZE size;
        public bool isOffset;

        public Immediate(uint _val, OPSIZE _size)
        {
            val = _val;
            size = _size;
            isOffset = false;
        }

        public override string ToString()
        {
            String result = val.ToString("X");
            if (val > 0x09) result = result + "h";
            if (isOffset && size == OPSIZE.Byte)
            {
                uint b = val;
                bool negative = false;
                if ((b > 0x80))
                {
                    b = 0x100 - b;
                    negative = true;
                }
                result = b.ToString("X");
                if (b > 0x09) result = result + "h";
                if (negative)
                {
                    result = "-" + result;
                }
            }

            if (size == OPSIZE.SignedByte)
            {
                if (val >= 0x80)
                {
                    result = "FFFFFF" + result;
                }
            }

            if ((!isOffset) && (Char.IsLetter(result[0])))
            {
                result = "0" + result;
            }

            return result;
        }
    }

    public class Address : Operand
    {
        public uint val;

        public Address(uint _val)
        {
            val = _val;
        }

        public override string ToString()
        {
            return val.ToString("X8");
        }
    }

    public class Absolute : Operand
    {
        public uint seg;
        public uint addr;

        public Absolute(uint _seg, uint _addr)
        {
            seg = _seg;
            addr = _addr;            
        }

        public override string ToString()
        {
            return (seg.ToString("X4") + ':' + addr.ToString("X8"));
        }
    }

//- register ------------------------------------------------------------------

    public enum REG8 { AL, CL, DL, BL, AH, CH, DH, BH, None };
    public enum REG16 { AX, CX, DX, BX, SP, BP, SI, DI, None };
    public enum REG32 { EAX, ECX, EDX, EBX, ESP, EBP, ESI, EDI, None };

    //base class
    public class Register : Operand
    {
    }

    public class Register8 : Register
    {
        public REG8 reg8;

        public static Register8 AL = new Register8(REG8.AL);
        public static Register8 CL = new Register8(REG8.CL);
        public static Register8 DL = new Register8(REG8.DL);
        public static Register8 BL = new Register8(REG8.BL);
        public static Register8 AH = new Register8(REG8.AH);
        public static Register8 CH = new Register8(REG8.CH);
        public static Register8 DH = new Register8(REG8.DH);
        public static Register8 BH = new Register8(REG8.BH);

        public Register8(REG8 r8)
        {
            reg8 = r8;
        }

        String[] reg8s = { "AL", "CL", "DL", "BL", "AH", "CH", "DH", "BH", "None" };

        public override string ToString()
        {            
            return reg8s[(int)reg8];
        }
    }

    public class Register16 : Register
    {
        public REG16 reg16;

        public static Register16 AX = new Register16(REG16.AX);
        public static Register16 CX = new Register16(REG16.CX);
        public static Register16 DX = new Register16(REG16.DX);
        public static Register16 BX = new Register16(REG16.BX);
        public static Register16 SP = new Register16(REG16.SP);
        public static Register16 BP = new Register16(REG16.BP);
        public static Register16 SI = new Register16(REG16.SI);
        public static Register16 DI = new Register16(REG16.DI);

        public Register16(REG16 r16)
        {
            reg16 = r16;
        }

        String[] reg16s = { "AX", "CX", "DX", "BX", "SP", "BP", "SI", "DI", "None" };

        public override string ToString()
        {
            return reg16s[(int)reg16];
        }
    }

    public class Register32 : Register
    {
        public REG32 reg32;

        public static Register32 EAX = new Register32(REG32.EAX);
        public static Register32 ECX = new Register32(REG32.ECX);
        public static Register32 EDX = new Register32(REG32.EDX);
        public static Register32 EBX = new Register32(REG32.EBX);
        public static Register32 ESP = new Register32(REG32.ESP);
        public static Register32 EBP = new Register32(REG32.EBP);
        public static Register32 ESI = new Register32(REG32.ESI);
        public static Register32 EDI = new Register32(REG32.EDI);        

        public Register32(REG32 r32)
        {
            reg32 = r32;
        }

        String[] reg32s = { "EAX", "ECX", "EDX", "EBX", "ESP", "EBP", "ESI", "EDI", "None" };

        public override string ToString()
        {
            return reg32s[(int)reg32];
        }
    }

    public class Stack87 : Operand
    {
        int regnum;
        bool stackTop;

        public Stack87(int _regnum, bool _top)
        {
            regnum = _regnum;
            stackTop = _top;
        }

        public override string ToString()
        {
            return (stackTop) ? "st" : "st(" + regnum.ToString() + ")";
        }
    }

    public class RegisterMM : Operand
    {
        public enum REGMM { MM0, MM1, MM2, MM3, MM4, MM5, MM6, MM7, None };
        public enum REGXMM { XMM0, XMM1, XMM2, XMM3, XMM4, XMM5, XMM6, XMM7, None };

        public int size;
        public REGMM regmm;
        public REGXMM regxmm;

        public RegisterMM(REGMM rmm)
        {
            size = 1;
            regmm = rmm;
            regxmm = REGXMM.None;
        }

        public RegisterMM(REGXMM rxmm)
        {
            size = 2;
            regmm = REGMM.None;
            regxmm = rxmm;
        }

        String[] regMMs = { "MM0", "MM1", "MM2", "MM3", "MM4", "MM5", "MM6", "MM7", "None" };
        String[] regXMMs = { "XMM0", "XMM1", "XMM2", "XMM3", "XMM4", "XMM5", "XMM6", "XMM7", "None" };

        public override string ToString()
        {
            return (size == 1) ? regMMs[(int)regmm] : regXMMs[(int)regxmm];
            
        }
    }

//- seqgment ------------------------------------------------------------------

        //    readonly String[] seg16 = { "es", "cs", "ss", "ds", "fs", "gs", "??", "??" };    

    public class Segment : Operand
    {
        public enum SEG { ES, CS, SS, DS, FS, GS, None };

        public SEG seg;

        public Segment(SEG _seg)
        {
            seg = _seg;
        }

        String[] segstr = { "ES", "CS", "SS", "DS", "FS", "GS", "None" };

        public override string ToString()
        {
            return segstr[(int)seg];
        }

    }

//- memory --------------------------------------------------------------------

    public class Memory : Operand
    {
        public Register f1;
        public Register f2;
        public int mult;
        public Immediate f3;
        public OPSIZE size;
        public Segment.SEG seg;

        public Memory(Register _f1, Register _f2, int _mult, Immediate _f3, OPSIZE _size, Segment.SEG _seg)
        {
            f1 = _f1;
            f2 = _f2;
            mult = _mult;
            f3 = _f3;
            if (f3 != null)
            {
                f3.isOffset = true;
            }
            size = _size;
            seg = _seg;
        }

        public String getSizePtrStr(Operand.OPSIZE size)
        {
           //if (operandSizeOverride && (size == Operand.OPSIZE.DWord)) size = Operand.OPSIZE.Word;

            String result = "???";
            if (size == Operand.OPSIZE.Byte) result = "byte ptr ";
            if (size == Operand.OPSIZE.Word) result = "word ptr ";
            if (size == Operand.OPSIZE.DWord) result = "dword ptr ";
            if (size == Operand.OPSIZE.QWord) result = "qword ptr ";
            if (size == Operand.OPSIZE.FWord) result = "fword ptr ";
            if (size == Operand.OPSIZE.TByte) result = "tbyte ptr ";
            //if (size == Operand.OPSIZE.MM) result = "mmword ptr ";
            //if (size == Operand.OPSIZE.XMM) result = "xmmword ptr ";
            if (size == Operand.OPSIZE.None) result = "";
            return result;
        }

        public override string ToString()
        {
            String result = "";

            //the address part
            if (f1 != null)
            {
                result = f1.ToString();
            }
            if (f2 != null)
            {
                if (result.Length > 0)
                {
                    result += "+";
                }
                result += f2.ToString();
            }
            if (mult > 1)
            {
                result += ("*" + mult.ToString());
            }
            if ((f3 != null) && (f3.val > 0))
            {
                String immStr = f3.ToString();
                if ((result.Length > 0) && (immStr[0] != '-'))
                {
                    result += "+";
                }
                result += immStr;
            }

            //the decorations
            result = "[" + result + "]";
            if ((seg != Segment.SEG.DS) || (f1 == null && f2 == null))
            {
                result = seg.ToString() + ":" + result;
            }
            result = getSizePtrStr(size) + result;
            return result;
        }
    }

}
