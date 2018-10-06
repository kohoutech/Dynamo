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
    //base class
    public class Register : Operand
    {
        //public enum REGTYPE { Byte, Word, DWord, FPU, MMX, XMMX };

        public String name;
        public OPSIZE size;
        public int code;

        public Register(String _name, int _code)
        {
            name = _name;
            size = OPSIZE.None;
            code = _code;
        }

        public override string ToString()
        {
            return name;
        }
    }

//- 8 bit ---------------------------------------------------------------------

    public class Register8 : Register
    {
        static List<Register8> regs;

        public enum REG8 { AL, CL, DL, BL, AH, CH, DH, BH };
        public REG8 reg8;

        public static Register8 AL = new Register8(REG8.AL, "AL", 0);
        public static Register8 CL = new Register8(REG8.CL, "CL", 1);
        public static Register8 DL = new Register8(REG8.DL, "DL", 2);
        public static Register8 BL = new Register8(REG8.BL, "BL", 3);
        public static Register8 AH = new Register8(REG8.AH, "AH", 4);
        public static Register8 CH = new Register8(REG8.CH, "CH", 5);
        public static Register8 DH = new Register8(REG8.DH, "DH", 6);
        public static Register8 BH = new Register8(REG8.BH, "BH", 7);

        static Register8()
        {
            regs = new List<Register8>();
            regs.Add(AL);
            regs.Add(CL);
            regs.Add(DL);
            regs.Add(BL);
            regs.Add(AH);
            regs.Add(CH);
            regs.Add(DH);
            regs.Add(BH);
        }

        static public Register8 getReg(int i)
        {
            return regs[i];
        }

        Register8(REG8 r8, String name, int code) : base(name, code)
        {
            reg8 = r8;
            size = OPSIZE.Byte;
        }
    }

//- 16 bit --------------------------------------------------------------------

    public class Register16 : Register
    {
        static List<Register16> regs;

        public enum REG16 { AX, CX, DX, BX, SP, BP, SI, DI };
        public REG16 reg16;

        public static Register16 AX = new Register16(REG16.AX, "AX", 0);
        public static Register16 CX = new Register16(REG16.CX, "CX", 1);
        public static Register16 DX = new Register16(REG16.DX, "DX", 2);
        public static Register16 BX = new Register16(REG16.BX, "BX", 3);
        public static Register16 SP = new Register16(REG16.SP, "SP", 4);
        public static Register16 BP = new Register16(REG16.BP, "BP", 5);
        public static Register16 SI = new Register16(REG16.SI, "SI", 6);
        public static Register16 DI = new Register16(REG16.DI, "DI", 7);

        static Register16()
        {
            regs = new List<Register16>();
            regs.Add(AX);
            regs.Add(CX);
            regs.Add(DX);
            regs.Add(BX);
            regs.Add(SP);
            regs.Add(BP);
            regs.Add(SI);
            regs.Add(DI);
        }

        static public Register16 getReg(int i)
        {
            return regs[i];
        }

        public Register16(REG16 r16, String name, int code) : base(name, code)
        {
            reg16 = r16;
            size = OPSIZE.Word;
        }
    }

//- 32 bit --------------------------------------------------------------------

    public class Register32 : Register
    {
        static List<Register32> regs;

        public enum REG32 { EAX, ECX, EDX, EBX, ESP, EBP, ESI, EDI };
        public REG32 reg32;

        public static Register32 EAX = new Register32(REG32.EAX, "EAX", 0);
        public static Register32 ECX = new Register32(REG32.ECX, "ECX", 1);
        public static Register32 EDX = new Register32(REG32.EDX, "EDX", 2);
        public static Register32 EBX = new Register32(REG32.EBX, "EBX", 3);
        public static Register32 ESP = new Register32(REG32.ESP, "ESP", 4);
        public static Register32 EBP = new Register32(REG32.EBP, "EBP", 5);
        public static Register32 ESI = new Register32(REG32.ESI, "ESI", 6);
        public static Register32 EDI = new Register32(REG32.EDI, "EDI", 7);

        static Register32()
        {
            regs = new List<Register32>();
            regs.Add(EAX);
            regs.Add(ECX);
            regs.Add(EDX);
            regs.Add(EBX);
            regs.Add(ESP);
            regs.Add(EBP);
            regs.Add(ESI);
            regs.Add(EDI);
        }

        static public Register32 getReg(int i)
        {
            return regs[i];
        }

        public Register32(REG32 r32, String name, int code) : base (name, code)
        {
            reg32 = r32;
            size = OPSIZE.DWord;
        }
    }

//- fpu registers -------------------------------------------------------------

    public class Register87 : Register
    {
        static List<Register87> regs;

        public enum REG87 { ST0, ST1, ST2, ST3, ST4, ST5, ST6, ST7 };
        public REG87 reg87;

        public static Register87 ST0 = new Register87(REG87.ST0, "st", 0);
        public static Register87 ST1 = new Register87(REG87.ST1, "st(1)", 1);
        public static Register87 ST2 = new Register87(REG87.ST2, "st(2)", 2);
        public static Register87 ST3 = new Register87(REG87.ST3, "st(3)", 3);
        public static Register87 ST4 = new Register87(REG87.ST4, "st(4)", 4);
        public static Register87 ST5 = new Register87(REG87.ST5, "st(5)", 5);
        public static Register87 ST6 = new Register87(REG87.ST6, "st(6)", 6);
        public static Register87 ST7 = new Register87(REG87.ST7, "st(7)", 7);

        static Register87()
        {
            regs = new List<Register87>();
            regs.Add(ST0);
            regs.Add(ST1);
            regs.Add(ST2);
            regs.Add(ST3);
            regs.Add(ST4);
            regs.Add(ST5);
            regs.Add(ST6);
            regs.Add(ST7);
        }

        static public Register87 getReg(int i)
        {
            return regs[i];
        }

        public Register87(REG87 r87, String name, int code)
            : base(name, code)
        {
            reg87 = r87;
            size = OPSIZE.TByte;
        }
    }

//- mmx registers -------------------------------------------------------------

    public class RegisterMM : Register
    {
        static List<RegisterMM> regs;

        public enum REGMM { MM0, MM1, MM2, MM3, MM4, MM5, MM6, MM7};
        public REGMM regmm;

        public static RegisterMM MM0 = new RegisterMM(REGMM.MM0, "MM0", 0);
        public static RegisterMM MM1 = new RegisterMM(REGMM.MM1, "MM1", 1);
        public static RegisterMM MM2 = new RegisterMM(REGMM.MM2, "MM2", 2);
        public static RegisterMM MM3 = new RegisterMM(REGMM.MM3, "MM3", 3);
        public static RegisterMM MM4 = new RegisterMM(REGMM.MM4, "MM4", 4);
        public static RegisterMM MM5 = new RegisterMM(REGMM.MM5, "MM5", 5);
        public static RegisterMM MM6 = new RegisterMM(REGMM.MM6, "MM6", 6);
        public static RegisterMM MM7 = new RegisterMM(REGMM.MM7, "MM7", 7);

        static RegisterMM()
        {
            regs = new List<RegisterMM>();
            regs.Add(MM0);
            regs.Add(MM1);
            regs.Add(MM2);
            regs.Add(MM3);
            regs.Add(MM4);
            regs.Add(MM5);
            regs.Add(MM6);
            regs.Add(MM7);
        }

        static public RegisterMM getReg(int i)
        {
            return regs[i];
        }

        public RegisterMM(REGMM rmm, String name, int code)
            : base(name, code)
        {
            regmm = rmm;
            size = OPSIZE.MM;
        }
    }

//- xmmx registers ------------------------------------------------------------

    public class RegisterXMM : Register
    {
        static List<RegisterXMM> regs;

        public enum REGXMM { XMM0, XMM1, XMM2, XMM3, XMM4, XMM5, XMM6, XMM7};
        public REGXMM regxmm;

        public static RegisterXMM XMM0 = new RegisterXMM(REGXMM.XMM0, "XMM0", 0);
        public static RegisterXMM XMM1 = new RegisterXMM(REGXMM.XMM1, "XMM1", 1);
        public static RegisterXMM XMM2 = new RegisterXMM(REGXMM.XMM2, "XMM2", 2);
        public static RegisterXMM XMM3 = new RegisterXMM(REGXMM.XMM3, "XMM3", 3);
        public static RegisterXMM XMM4 = new RegisterXMM(REGXMM.XMM4, "XMM4", 4);
        public static RegisterXMM XMM5 = new RegisterXMM(REGXMM.XMM5, "XMM5", 5);
        public static RegisterXMM XMM6 = new RegisterXMM(REGXMM.XMM6, "XMM6", 6);
        public static RegisterXMM XMM7 = new RegisterXMM(REGXMM.XMM7, "XMM7", 7);

        static RegisterXMM()
        {
            regs = new List<RegisterXMM>();
            regs.Add(XMM0);
            regs.Add(XMM1);
            regs.Add(XMM2);
            regs.Add(XMM3);
            regs.Add(XMM4);
            regs.Add(XMM5);
            regs.Add(XMM6);
            regs.Add(XMM7);
        }

        static public RegisterXMM getReg(int i)
        {
            return regs[i];
        }

        public RegisterXMM(REGXMM rxmm, String name, int code)
            : base(name, code)
        {
            regxmm = rxmm;
            size = OPSIZE.XMM;
        }
    }

//- control registers ------------------------------------------------------------

    public class RegisterCR : Register
    {
        static List<RegisterCR> regs;

        public enum REGCR { CR0, CR1, CR2, CR3, CR4, CR5, CR6, CR7 };
        public REGCR regcr;

        public static RegisterCR CR0 = new RegisterCR(REGCR.CR0, "CR0", 0);
        public static RegisterCR CR1 = new RegisterCR(REGCR.CR1, "CR1", 1);
        public static RegisterCR CR2 = new RegisterCR(REGCR.CR2, "CR2", 2);
        public static RegisterCR CR3 = new RegisterCR(REGCR.CR3, "CR3", 3);
        public static RegisterCR CR4 = new RegisterCR(REGCR.CR4, "CR4", 4);
        public static RegisterCR CR5 = new RegisterCR(REGCR.CR5, "CR5", 5);
        public static RegisterCR CR6 = new RegisterCR(REGCR.CR6, "CR6", 6);
        public static RegisterCR CR7 = new RegisterCR(REGCR.CR7, "CR7", 7);

        static RegisterCR()
        {
            regs = new List<RegisterCR>();
            regs.Add(CR0);
            regs.Add(CR1);
            regs.Add(CR2);
            regs.Add(CR3);
            regs.Add(CR4);
            regs.Add(CR5);
            regs.Add(CR6);
            regs.Add(CR7);
        }

        static public RegisterCR getReg(int i)
        {
            return regs[i];
        }

        public RegisterCR(REGCR rcr, String name, int code)
            : base(name, code)
        {
            regcr = rcr;
            size = OPSIZE.CR;
        }
    }

//- debug registers ------------------------------------------------------------

    public class RegisterDR : Register
    {
        static List<RegisterDR> regs;

        public enum REGDR { DR0, DR1, DR2, DR3, DR4, DR5, DR6, DR7};
        public REGDR regdr;

        public static RegisterDR DR0 = new RegisterDR(REGDR.DR0, "DR0", 0);
        public static RegisterDR DR1 = new RegisterDR(REGDR.DR1, "DR1", 1);
        public static RegisterDR DR2 = new RegisterDR(REGDR.DR2, "DR2", 2);
        public static RegisterDR DR3 = new RegisterDR(REGDR.DR3, "DR3", 3);
        public static RegisterDR DR4 = new RegisterDR(REGDR.DR4, "DR4", 4);
        public static RegisterDR DR5 = new RegisterDR(REGDR.DR5, "DR5", 5);
        public static RegisterDR DR6 = new RegisterDR(REGDR.DR6, "DR6", 6);
        public static RegisterDR DR7 = new RegisterDR(REGDR.DR7, "DR7", 7);

        static RegisterDR()
        {
            regs = new List<RegisterDR>();
            regs.Add(DR0);
            regs.Add(DR1);
            regs.Add(DR2);
            regs.Add(DR3);
            regs.Add(DR4);
            regs.Add(DR5);
            regs.Add(DR6);
            regs.Add(DR7);
        }

        static public RegisterDR getReg(int i)
        {
            return regs[i];            
        }

        public RegisterDR(REGDR rdr, String name, int code)
            : base(name, code)
        {
            regdr = rdr;
            size = OPSIZE.DR;
        }
    }

//- seqgment ------------------------------------------------------------------

    public class Segment : Operand
    {
        static List<Segment> segs;

        public enum SEG { ES, CS, SS, DS, FS, GS};
        public SEG seg;
        public String name;

        public static Segment ES = new Segment(SEG.ES, "ES");
        public static Segment CS = new Segment(SEG.CS, "CS");
        public static Segment SS = new Segment(SEG.SS, "SS");
        public static Segment DS = new Segment(SEG.DS, "DS");
        public static Segment FS = new Segment(SEG.FS, "FS");
        public static Segment GS = new Segment(SEG.GS, "GS");

        static Segment()
        {
            segs = new List<Segment>();
            segs.Add(ES);
            segs.Add(CS);
            segs.Add(SS);
            segs.Add(DS);
            segs.Add(FS);
            segs.Add(GS);
        }

        static public Segment getSeg(int i)
        {
            return segs[i];
        }

        public Segment(SEG _seg, String _name)
        {
            seg = _seg;
            name = _name;        
        }

        public override string ToString()
        {
            return name;
        }
    }
}
