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

//general instructions - section 5.20 Intel Architecture Manual (2018)

namespace Origami.Asm32
{
    //- prefix instruction ---------------------------------------------------------------

    public class Wait : Instruction
    {
        public Wait()
            : base()
        {
        }

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0x9b };
        }

        public override string ToString()
        {
            return "WAIT";
        }
    }

    //- data transfer ---------------------------------------------------------------

    public class Move : Instruction
    {
        public Move(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override void generateBytes()
        {
            OpMode mode;
            OPSIZE size;

            bytes = new List<byte>();
            if (op2 is Segment)
            {
                if (op1 is Memory)
                {
                    int mod;
                    int rm;
                    bytes.Add(0x8c);
                    List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                    Segment.SEG seg = ((Segment)op2).seg;
                    bytes.Add((byte)(mod * 0x40 + (int)seg * 8 + rm));
                    bytes.AddRange(membytes);
                }
                else
                {
                    bytes.Add(0x8c);
                    int rm = ((Register)op1).code;
                    Segment.SEG seg = ((Segment)op2).seg;
                    bytes.Add((byte)(0xc0 + (int)seg * 8 + rm));
                }
            }
            else if (op2 is Immediate)
            {
                Immediate imm = (Immediate)op2;
                if (op1 is Register)
                {
                    bytes.Add((byte)(((imm.size == OPSIZE.Byte) ? 0xb0 : 0xb8) + ((Register)op1).code));
                    bytes.AddRange(imm.getBytes());
                }
                else
                {
                    int mod;
                    int rm;
                    bytes.Add((byte)((imm.size == OPSIZE.Byte) ? 0xc6 : 0xc7));
                    List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                    bytes.Add((byte)(mod * 0x40 + rm));
                    bytes.AddRange(membytes);
                    bytes.AddRange(imm.getBytes());
                }
            } 
            else
            {
                if (op1 is Segment)
                {
                    if (op2 is Memory)
                    {
                        int mod;
                        int rm;
                        bytes.Add(0x8e);
                        List<byte> membytes = ((Memory)op2).getBytes(out mod, out rm);
                        Segment.SEG seg = ((Segment)op1).seg;
                        bytes.Add((byte)(mod * 0x40 + (int)seg * 8 + rm));
                        bytes.AddRange(membytes);
                    }
                    else
                    {
                        bytes.Add(0x8e);
                        int rm = ((Register)op2).code;
                        Segment.SEG seg = ((Segment)op1).seg;
                        bytes.Add((byte)(0xc0 + (int)seg * 8 + rm));
                    }
                }
                else
                {
                    if ((op1 is Register) && ((Register)op1).code == 0 && (op2 is Memory) && ((Memory)op2).isImmediate())
                    {
                        int mod;
                        int rm;
                        bytes.Add((byte)((((Register)op1).size == OPSIZE.Byte) ? 0xa0 : 0xa1));
                        List<byte> membytes = ((Memory)op2).getBytes(out mod, out rm);
                        bytes.AddRange(membytes);
                    }
                    else if ((op2 is Register) && ((Register)op2).code == 0 && (op1 is Memory) && ((Memory)op1).isImmediate())
                    {
                        int mod;
                        int rm;
                        bytes.Add((byte)((((Register)op2).size == OPSIZE.Byte) ? 0xa2 : 0xa3));
                        List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                        bytes.AddRange(membytes);
                    }
                    else
                    {
                        //88, 89, 8a, 8b - reg,reg / reg,mem / mem,reg 
                        byte[] opbyte = { 0x88, 0x8a, 0x88 };
                        List<byte> modrm = getModrm(op1, op2, out mode, out size);
                        bytes.Add(opbyte[(int)mode]);
                        if (size == OPSIZE.DWord)
                        {
                            bytes[0] += 1;
                        }
                        bytes.AddRange(modrm);
                    }
                }
            }
        }

        public override string ToString()
        {
            return "MOV";
        }
    }

    //ConditionalMove - CMOV#
    public class ConditionalMove : Instruction
    {
        public enum CONDIT
        {
            CMOVO, CMOVNO, CMOVB, CMOVAE, CMOVE, CMOVNE, CMOVBE, CMOVA,
            CMOVS, CMOVNS, CMOVP, CMOVNP, CMOVL, CMOVGE, CMOVLE, CMOVG
        };

        public CONDIT condit;

        public ConditionalMove(Operand _op1, Operand _op2, CONDIT _condit)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            condit = _condit;
        }

        String[] condits = { "CMOVO", "CMOVNO", "CMOVB", "CMOVAE", "CMOVE", "CMOVNE", "CMOVBE", "CMOVA", 
                           "CMOVS", "CMOVNS", "CMOVP", "CMOVNP", "CMOVL", "CMOVGE", "CMOVLE", "CMOVG" };

        public override string ToString()
        {
            return condits[(int)condit];
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

        public override void generateBytes()
        {
            OpMode mode;
            OPSIZE size;

            bytes = new List<byte>();

            List<byte> modrm;
            if (op1 is Register && op2 is Register && ((Register)op1).code == 0)
            {
                bytes.Add((byte)(0x90 + ((Register)op2).code));
            }
            else
            {             
                if (op2 is Register)
                {
                    {
                        modrm = getModrm(op2, op1, out mode, out size);     //xchg reverses op1 & op2 order for mode 3
                    }
                }
                else
                {
                    modrm = getModrm(op1, op2, out mode, out size);
                }
                bytes.Add(0x86);
                if (size == OPSIZE.DWord)
                {
                    bytes[0] += 1;
                }
                bytes.AddRange(modrm);
            }
    }

        public override string ToString()
        {
            return "XCHG";
        }
    }

    //ByteSwap - BSWAP
    public class ByteSwap : Instruction
    {
        public ByteSwap(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "BSWAP";
        }
    }

    //ExchangeAdd - XADD
    public class ExchangeAdd : Instruction
    {
        public ExchangeAdd(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "XADD";
        }
    }

    //CompareExchange - CMPXCHG/CMPXCHG8B
    public class CompareExchange : Instruction
    {
        bool wide;

        public CompareExchange(Operand _op1, Operand _op2, bool _wide)
            : base()
        {
            opcount = (_op2 != null) ? 2 : 1;
            op1 = _op1;
            op2 = _op2;
            wide = _wide;
        }

        public override string ToString()
        {
            return wide ? "CMPXCHG8B" : "CMPXCHG";
        }
    }

    public class Push : Instruction
    {
        public Push(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Segment)
            {
                if (((Segment)op1).seg <= Segment.SEG.DS)
                {
                    bytes.Add((byte)(0x06 + ((int)((Segment)op1).seg * 8)));
                }
            }
            else if (op1 is Register)
            {
                bytes.Add((byte)(0x50 + ((Register)op1).code));
            }
            else if (op1 is Immediate)
            {
                Immediate imm = (Immediate)op1;
                bytes.Add((byte)(imm.size == OPSIZE.DWord ? 0x68 : 0x6a));
                bytes.AddRange(imm.getBytes());
            }
            else
            {
                int mod;
                int rm;
                bytes.Add(0xff);
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + + 0x30 + rm));
                bytes.AddRange(membytes);
            }
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

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Segment)
            {
                if (((Segment)op1).seg <= Segment.SEG.DS)
                {
                    bytes.Add((byte)(0x07 + ((int)((Segment)op1).seg * 8)));
                }
            }
            else if (op1 is Register)
            {
                bytes.Add((byte)(0x58 + ((Register)op1).code));
            }
            else
            {
                int mod;
                int rm;
                bytes.Add(0x8f);
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + rm));
                bytes.AddRange(membytes);
            }
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

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0x60 };
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

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0x61 };
        }

        public override string ToString()
        {
            return "POPAD";
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

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add((byte)((mode == MODE.CWDE) ? 0x98 : 0x99));
        }

        public override string ToString()
        {
            return (mode == MODE.CWDE) ? "CWDE" : "CDQ";
        }
    }

    //MoveExtend - MOVSX/MOVZX
    public class MoveExtend : Instruction
    {
        public enum MODE { SIGN, ZERO }
        MODE mode;

        public MoveExtend(Operand _op1, Operand _op2, MODE _mode)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            mode = _mode;
        }

        public override string ToString()
        {
            return (mode == MODE.SIGN) ? "MOVSX" : "MOVZX";
        }
    }

    //- arithmetic ----------------------------------------------------------------

    //Add - ADC/ADD
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

        public override void generateBytes()
        {
            OpMode mode;
            OPSIZE size;

            bytes = new List<byte>();
            if (op2 is Immediate)
            {
                Immediate reg2 = (Immediate)op2;
                if (op1 is Register)
                {
                    Register reg1 = (Register)op1;
                    if (reg1.code == 0)
                    {
                        //04, 05 - AL/EAX, imm
                        bytes.Add((byte)((reg1.size == OPSIZE.Byte) ? 0x4 : 0x5));
                        if (carry)
                        {
                            bytes[0] += 0x10;       //ADC = 14, 15
                        }                        
                    }
                    else
                    {
                        //80 - 83
                        bytes.Add((byte)(reg2.size == OPSIZE.Byte ? 0x80 : (reg2.size == OPSIZE.DWord ? 0x81 : 0x83)));
                        bytes.Add((byte)((carry ? 0xd0 : 0xc0) + reg1.code));                        
                    }
                }
                else
                {
                    //80 - 83
                    int mod;
                    int rm;
                    bytes.Add((byte)(reg2.size == OPSIZE.Byte ? 0x80 : (reg2.size == OPSIZE.DWord ? 0x81 : 0x83))); 
                    List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                    bytes.Add((byte)(mod * 0x40 + (carry ? 0x10 : 0 ) + rm));
                    bytes.AddRange(membytes);                    
                }                
                bytes.AddRange(reg2.getBytes());
            }
            else
            {
                //00, 01, 02, 03 - reg,reg / reg,reg / mem,reg 
                byte[] opbyte = { 0x00, 0x02, 0x00 };
                List<byte> modrm = getModrm(op1, op2, out mode, out size);
                bytes.Add(opbyte[(int)mode]);
                if (carry)
                {
                    bytes[0] += 0x10;       //ADC = 10, 11, 12, 13
                }
                if (size == OPSIZE.DWord)
                {
                    bytes[0] += 1;          
                }
                bytes.AddRange(modrm);
            }
        }

        public override string ToString()
        {
            return (carry ? "ADC" : "ADD");
        }
    }

    //Subtract - SBB/SUB
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

        public override void generateBytes()
        {
            OpMode mode;
            OPSIZE size;

            bytes = new List<byte>();
            if (op2 is Immediate)
            {
                Immediate reg2 = (Immediate)op2;
                if (op1 is Register)
                {
                    Register reg1 = (Register)op1;
                    if (((Register)op1).code == 0)
                    {
                        bytes.Add((byte)((((Register)op1).size == OPSIZE.Byte) ? 0x1c : 0x1d));
                        if (!borrow)
                        {
                            bytes[0] += 0x10;
                        }
                        bytes.AddRange(((Immediate)op2).getBytes());
                    }
                    else
                    {
                        //80 - 83
                        bytes.Add((byte)(reg2.size == OPSIZE.Byte ? 0x80 : (reg2.size == OPSIZE.DWord ? 0x81 : 0x83)));
                        bytes.Add((byte)((borrow ? 0xd8 : 0xe8) + reg1.code));
                    }
                }
                else
                {
                    //80 - 83
                    int mod;
                    int rm;
                    bytes.Add((byte)(reg2.size == OPSIZE.Byte ? 0x80 : (reg2.size == OPSIZE.DWord ? 0x81 : 0x83)));
                    List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                    bytes.Add((byte)(mod * 0x40 + (borrow ? 0x18 : 0x28) + rm));
                    bytes.AddRange(membytes);
                }
                bytes.AddRange(((Immediate)op2).getBytes());
            }
            else
            {
                byte[] opbyte = { 0x18, 0x1a, 0x18 };
                List<byte> modrm = getModrm(op1, op2, out mode, out size);
                bytes.Add(opbyte[(int)mode]);
                if (!borrow)
                {
                    bytes[0] += 0x10;
                }
                if (size == OPSIZE.DWord)
                {
                    bytes[0] += 1;
                }
                bytes.AddRange(modrm);
            }
        }

        public override string ToString()
        {
            return (borrow ? "SBB" : "SUB");
        }
    }

    //IntMultiply - IMUL
    public class IntMultiply : Instruction
    {
        public IntMultiply(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public IntMultiply(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public IntMultiply(Operand _op1, Operand _op2, Operand _op3)
            : base()
        {
            opcount = 3;
            op1 = _op1;
            op2 = _op2;
            op3 = _op3;
        }

        public override void generateBytes()
        {
            OpMode mode;
            OPSIZE size;

            bytes = new List<byte>();
            if (op3 != null)
            {
                Immediate imm = (Immediate)op3;
                bytes.Add((byte)(imm.size == OPSIZE.DWord ? 0x69 : 0x6b));
                List<byte> modrm;
                if (op2 is Register)
                {
                    modrm = getModrm(op2, op1, out mode, out size);     //imul reverses op1 & op2 order for mode 3
                }
                else
                {
                    modrm = getModrm(op1, op2, out mode, out size);
                }
                bytes.AddRange(modrm);
                bytes.AddRange(imm.getBytes());
            }
            else if (op2 == null)
            {
                if (op1 is Memory)
                {
                    int mod;
                    int rm;
                    bytes.Add((byte)((((Memory)op1).size == OPSIZE.Byte) ? 0xf6 : 0xf7));
                    List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                    bytes.Add((byte)(mod * 0x40 + 0x28 + rm));
                    bytes.AddRange(membytes);
                }
                else
                {
                    bytes.Add((byte)((((Register)op1).size == OPSIZE.Byte) ? 0xf6 : 0xf7));
                    int rm = ((Register)op1).code;
                    bytes.Add((byte)(0xe8 + rm));
                }
            }
        }

        public override string ToString()
        {
            return "IMUL";
        }
    }

    //Multiply - MUL
    public class Multiply : Instruction
    {
        public Multiply(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add((byte)((((Register)op1).size == OPSIZE.Byte) ? 0xf6 : 0xf7));
            if (op2 is Memory)
            {
                int mod;
                int rm;
                List<byte> membytes = ((Memory)op2).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + 0x20 + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                int rm = ((Register)op2).code;
                bytes.Add((byte)(0xe0 + rm));
            }
        }

        public override string ToString()
        {
            return "MUL";
        }
    }

    //IntDivide - IDIV
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

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op2 != null)
            {
                bytes.Add(0xf7);
                if (op2 is Memory)
                {
                    int mod;
                    int rm;
                    List<byte> membytes = ((Memory)op2).getBytes(out mod, out rm);
                    bytes.Add((byte)(mod * 0x40 + 0x38 + rm));
                    bytes.AddRange(membytes);
                }
                else
                {
                    int rm = ((Register)op2).code;
                    bytes.Add((byte)(0xf8 + rm));
                }
            }
            else
            {
                bytes.Add(0xf6);
                if (op1 is Memory)
                {
                    int mod;
                    int rm;
                    List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                    bytes.Add((byte)(mod * 0x40 + 0x38 + rm));
                    bytes.AddRange(membytes);
                }
                else
                {
                    int rm = ((Register)op1).code;
                    bytes.Add((byte)(0xf8 + rm));
                }
            }
        }

        public override string ToString()
        {
            return "IDIV";
        }
    }

    //Divide - DIV
    public class Divide : Instruction
    {
        public Divide(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add((byte)((((Register)op1).size == OPSIZE.Byte) ? 0xf6 : 0xf7));
            if (op2 is Memory)
            {
                int mod;
                int rm;
                List<byte> membytes = ((Memory)op2).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + 0x30 + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                int rm = ((Register)op2).code;
                bytes.Add((byte)(0xf0 + rm));
            }
        }

        public override string ToString()
        {
            return "DIV";
        }
    }

    //Increment - INC
    public class Increment : Instruction
    {
        public Increment(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Register)
            {
                Register reg1 = (Register)op1;
                if (reg1.size == OPSIZE.DWord)
                {
                    bytes.Add((byte)(0x40 + ((Register)op1).code));
                }
                else
                {
                    bytes.Add(0xfe);
                    bytes.Add((byte)(0xc0 + reg1.code));
                }

            }
            else
            {
                int mod;
                int rm;
                bytes.Add((byte)(((Memory)op1).size == OPSIZE.Byte ? 0xfe : 0xff));
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + rm));
                bytes.AddRange(membytes);
            }
        }

        public override string ToString()
        {
            return "INC";
        }
    }

    //Decrement - DEC
    public class Decrement : Instruction
    {
        public Decrement(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Register)
            {
                Register reg1 = (Register)op1;
                if (reg1.size == OPSIZE.DWord)
                {
                    bytes.Add((byte)(0x48 + ((Register)op1).code));
                }
                else
                {
                    bytes.Add(0xfe);                    
                    bytes.Add((byte)(0xc8 + reg1.code));
                }
            }
            else
            {
                int mod;
                int rm;
                bytes.Add((byte)(((Memory)op1).size == OPSIZE.Byte ? 0xfe : 0xff));
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + 0x08 + rm));
                bytes.AddRange(membytes);
            }
        }

            public override string ToString()
        {
            return "DEC";
        }
    }

    //Negate - NEG
    public class Negate : Instruction
    {
        public Negate(Operand _op1)
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
                bytes.Add((byte)((((Memory)op1).size == OPSIZE.Byte) ? 0xf6 : 0xf7));
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + 0x18 + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                bytes.Add((byte)((((Register)op1).size == OPSIZE.Byte) ? 0xf6 : 0xf7));
                int rm = ((Register)op1).code;
                bytes.Add((byte)(0xd8 + rm));
            }
        }

        public override string ToString()
        {
            return "NEG";
        }
    }

    //Compare - CMP
    public class Compare : Instruction
    {
        public Compare(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override void generateBytes()
        {
            OpMode mode;
            OPSIZE size;

            bytes = new List<byte>();
            if (op2 is Immediate)
            {
                Immediate reg2 = (Immediate)op2;
                if (op1 is Register)
                {
                    Register reg1 = (Register)op1;
                    if (((Register)op1).code == 0)
                    {
                        bytes.Add((byte)((((Register)op1).size == OPSIZE.Byte) ? 0x3c : 0x3d));
                        bytes.AddRange(((Immediate)op2).getBytes());
                    }
                    else
                    {
                        //80 - 83
                        bytes.Add((byte)(reg2.size == OPSIZE.Byte ? 0x80 : (reg2.size == OPSIZE.DWord ? 0x81 : 0x83)));
                        bytes.Add((byte)(0xf8 + reg1.code));
                    }
                }
                else
                {
                    //80 - 83
                    int mod;
                    int rm;
                    bytes.Add((byte)(reg2.size == OPSIZE.Byte ? 0x80 : (reg2.size == OPSIZE.DWord ? 0x81 : 0x83)));
                    List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                    bytes.Add((byte)(mod * 0x40 + 0x38 + rm));
                    bytes.AddRange(membytes);
                }
                bytes.AddRange(((Immediate)op2).getBytes());
            }
            else
            {
                byte[] opbyte = { 0x38, 0x3a, 0x38 };
                List<byte> modrm = getModrm(op1, op2, out mode, out size);
                bytes.Add(opbyte[(int)mode]);
                if (size == OPSIZE.DWord)
                {
                    bytes[0] += 1;
                }
                bytes.AddRange(modrm);
            }
        }

        public override string ToString()
        {
            return "CMP";
        }
    }

    //- decimal ---------------------------------------------------------------

    //AsciiAdjust - AAA/AAS/AAM/AAD
    public class AsciiAdjust : Instruction
    {
        public enum MODE { Add, Sub, Mult, Div }

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

        public override void generateBytes()
        {
            byte bass;
            switch (mode)
            {
                case MODE.Add:
                    bytes = new List<byte>() { 0x37 };
                    break;

                case MODE.Sub:
                    bytes = new List<byte>() { 0x3f };
                    break;

                case MODE.Mult:
                    bass = (op1 != null) ? (byte)((Immediate)op1).val : (byte)0x0a;
                    bytes = new List<byte>() { 0xd4, bass};
                    break;

                case MODE.Div:
                    bass = (op1 != null) ? (byte)((Immediate)op1).val : (byte)0x0a;
                    bytes = new List<byte>() { 0xd5, bass };
                    break;
            }
        }

        String[] modes = { "AAA", "AAS", "AAM", "AAD" };

        public override string ToString()
        {
            String result = modes[(int)mode];
            if ((mode == MODE.Mult || mode == MODE.Div) && (op1 != null))
            {
                result = result + "B";
            }
            return result;
        }
    }

    //DecimalAdjust - DAA/DAS
    public class DecimalAdjust : Instruction
    {
        public enum MODE { Add, Sub }

        MODE mode;

        public DecimalAdjust(MODE _mode)
            : base()
        {
            mode = _mode;
        }

        public override void generateBytes()
        {
            switch (mode)
            {
                case MODE.Add:
                    bytes = new List<byte>() { 0x27 };
                    break;

                case MODE.Sub:
                    bytes = new List<byte>() { 0x2f };
                    break;
            }
        }

        public override string ToString()
        {
            return (mode == MODE.Add) ? "DAA" : "DAS";
        }
    }

    //- logical ----------------------------------------------------------------

    //And - AND
    public class And : Instruction
    {
        public And(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override void generateBytes()
        {
            OpMode mode;
            OPSIZE size;

            bytes = new List<byte>();
            if (op2 is Immediate)
            {
                Immediate reg2 = (Immediate)op2;
                if (op1 is Register)
                {
                    Register reg1 = (Register)op1;
                    if (((Register)op1).code == 0)
                    {
                        bytes.Add((byte)((((Register)op1).size == OPSIZE.Byte) ? 0x24 : 0x25));
                        bytes.AddRange(((Immediate)op2).getBytes());
                    }
                    else
                    {
                        //80 - 83
                        bytes.Add((byte)(reg2.size == OPSIZE.Byte ? 0x80 : (reg2.size == OPSIZE.DWord ? 0x81 : 0x83)));
                        bytes.Add((byte)(0xe0 + reg1.code));
                    }
                }
                else
                {
                    //80 - 83
                    int mod;
                    int rm;
                    bytes.Add((byte)(reg2.size == OPSIZE.Byte ? 0x80 : (reg2.size == OPSIZE.DWord ? 0x81 : 0x83)));
                    List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                    bytes.Add((byte)(mod * 0x40 + 0x20 + rm));
                    bytes.AddRange(membytes);
                }
                bytes.AddRange(((Immediate)op2).getBytes());
            }
            else
            {
                byte[] opbyte = { 0x20, 0x22, 0x20 };
                List<byte> modrm = getModrm(op1, op2, out mode, out size);
                bytes.Add(opbyte[(int)mode]);
                if (size == OPSIZE.DWord)
                {
                    bytes[0] += 1;
                }
                bytes.AddRange(modrm);
            }
        }

        public override string ToString()
        {
            return "AND";
        }
    }


    //Or - OR
    public class Or : Instruction
    {
        public Or(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override void generateBytes()
        {
            OpMode mode;
            OPSIZE size;

            bytes = new List<byte>();
            if (op2 is Immediate)
            {
                Immediate reg2 = (Immediate)op2;
                if (op1 is Register)
                {
                    Register reg1 = (Register)op1;
                    if (((Register)op1).code == 0)
                    {
                        bytes.Add((byte)((((Register)op1).size == OPSIZE.Byte) ? 0xc : 0xd));
                        bytes.AddRange(((Immediate)op2).getBytes());
                    }
                    else
                    {
                        //80 - 83
                        bytes.Add((byte)(reg2.size == OPSIZE.Byte ? 0x80 : (reg2.size == OPSIZE.DWord ? 0x81 : 0x83)));
                        bytes.Add((byte)(0xc8 + reg1.code));
                    }
                }
                else
                {
                    //80 - 83
                    int mod;
                    int rm;
                    bytes.Add((byte)(reg2.size == OPSIZE.Byte ? 0x80 : (reg2.size == OPSIZE.DWord ? 0x81 : 0x83)));
                    List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                    bytes.Add((byte)(mod * 0x40 + 0x08 + rm));
                    bytes.AddRange(membytes);
                }
                bytes.AddRange(((Immediate)op2).getBytes());
            }
            else
            {
                byte[] opbyte = { 0x08, 0x0a, 0x08 };
                List<byte> modrm = getModrm(op1, op2, out mode, out size);
                bytes.Add(opbyte[(int)mode]);
                if (size == OPSIZE.DWord)
                {
                    bytes[0] += 1;
                }
                bytes.AddRange(modrm);
            }
        }

        public override string ToString()
        {
            return "OR";
        }
    }

    //Xor - XOR
    public class Xor : Instruction
    {
        public Xor(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override void generateBytes()
        {
            OpMode mode;
            OPSIZE size;

            bytes = new List<byte>();
            if (op2 is Immediate)
            {
                Immediate reg2 = (Immediate)op2;
                if (op1 is Register)
                {
                    Register reg1 = (Register)op1;
                    if (((Register)op1).code == 0)
                    {
                        bytes.Add((byte)((((Register)op1).size == OPSIZE.Byte) ? 0x34 : 0x35));
                        bytes.AddRange(((Immediate)op2).getBytes());
                    }
                    else
                    {
                        //80 - 83
                        bytes.Add((byte)(reg2.size == OPSIZE.Byte ? 0x80 : (reg2.size == OPSIZE.DWord ? 0x81 : 0x83)));
                        bytes.Add((byte)(0xf0 + reg1.code));                        
                    }
                }
                else
                {
                    //80 - 83
                    int mod;
                    int rm;
                    bytes.Add((byte)(reg2.size == OPSIZE.Byte ? 0x80 : (reg2.size == OPSIZE.DWord ? 0x81 : 0x83)));
                    List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                    bytes.Add((byte)(mod * 0x40 + 0x30 + rm));
                    bytes.AddRange(membytes);                    
                }
                bytes.AddRange(((Immediate)op2).getBytes());

            }
            else
            {
                byte[] opbyte = { 0x30, 0x32, 0x30 };
                List<byte> modrm = getModrm(op1, op2, out mode, out size);
                bytes.Add(opbyte[(int)mode]);
                if (size == OPSIZE.DWord)
                {
                    bytes[0] += 1;
                }
                bytes.AddRange(modrm);
            }
        }

        public override string ToString()
        {
            return "XOR";
        }
    }

    //Not - NOT
    public class Not : Instruction
    {
        public Not(Operand _op1)
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
                bytes.Add((byte)((((Memory)op1).size == OPSIZE.Byte) ? 0xf6 : 0xf7));
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + 0x10 + rm));
                bytes.AddRange(membytes);
            }
            else
            {
                bytes.Add((byte)((((Register)op1).size == OPSIZE.Byte) ? 0xf6 : 0xf7));
                int rm = ((Register)op1).code;
                bytes.Add((byte)(0xd0 + rm));
            }
        }

        public override string ToString()
        {
            return "NOT";
        }
    }

    //- shift/rotate ----------------------------------------------------------

    //Shift - SAL/SAR/SHL/SHR
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

        public override void generateBytes()
        {
            bytes = new List<byte>();
            int shf = (mode == MODE.LEFT ? (arthimetic ? 2 : 0) : (arthimetic ? 3 : 1));
            if (op2 is Immediate)
            {
                int shfop = (((Immediate)op2).val != 1) ? 0xc0 : 0xd0;
                if (op1 is Memory)
                {
                    int mod;
                    int rm;

                    if (((Memory)op1).size == OPSIZE.DWord)
                    {
                        shfop++;
                    }
                    bytes.Add((byte)shfop);
                    List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                    bytes.Add((byte)(mod * 0x40 + (shf * 8 + 0x20) + rm));
                    bytes.AddRange(membytes);
                }
                else
                {
                    if (((Register)op1).size == OPSIZE.DWord)
                    {
                        shfop++;
                    }
                    bytes.Add((byte)shfop);
                    int rm = ((Register)op1).code;
                    bytes.Add((byte)(0xe0 + (shf * 8) + rm));
                }
                if (shfop < 0xd0)
                {
                    bytes.AddRange(((Immediate)op2).getBytes());
                }
            }
            else if (op2 is Register && ((Register)op2).code == 1)
            {
                int shfop = 0xd2;
                if (op1 is Memory)
                {
                    int mod;
                    int rm;

                    if (((Memory)op1).size == OPSIZE.DWord)
                    {
                        shfop++;
                    }
                    bytes.Add((byte)shfop);
                    List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                    bytes.Add((byte)(mod * 0x40 + (shf * 8 + 0x20) + rm));
                    bytes.AddRange(membytes);
                }
                else
                {
                    if (((Register)op1).size == OPSIZE.DWord)
                    {
                        shfop++;
                    }
                    bytes.Add((byte)shfop);
                    int rm = ((Register)op1).code;
                    bytes.Add((byte)(0xe0 + (shf * 8) + rm));
                }
            }
        }

        public override string ToString()
        {
            return (arthimetic) ? ((mode == MODE.LEFT) ? "SAL" : "SAR") : ((mode == MODE.LEFT) ? "SHL" : "SHR");
        }
    }

    //DoublePrecShift - SHLD/SHRD
    public class DoublePrecShift : Instruction
    {

        public enum MODE { LEFT, RIGHT }
        MODE mode;

        public DoublePrecShift(Operand _op1, Operand _op2, Operand _op3, MODE _mode)
            : base()
        {
            opcount = 3;
            op1 = _op1;
            op2 = _op2;
            op3 = _op3;
            mode = _mode;
        }

        public override string ToString()
        {
            return (mode == MODE.LEFT) ? "SHLD" : "SHRD";
        }
    }

    //Rotate - RCL/RCR/ROL/ROR
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

        public override void generateBytes()
        {
            bytes = new List<byte>();
            int rot = (mode == MODE.LEFT ? (withCarry ? 2 : 0) : (withCarry ? 3 : 1));
            if (op2 is Immediate)
            {
                int rotop = (((Immediate)op2).val != 1) ? 0xc0 : 0xd0;
                if (op1 is Memory)
                {
                    int mod;
                    int rm;

                    if (((Memory)op1).size == OPSIZE.DWord)
                    {
                        rotop++;
                    }
                    bytes.Add((byte)rotop);
                    List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                    bytes.Add((byte)(mod * 0x40 + (rot * 8) + rm));
                    bytes.AddRange(membytes);
                }
                else
                {
                    if (((Register)op1).size == OPSIZE.DWord)
                    {
                        rotop++;
                    }
                    bytes.Add((byte)rotop);
                    int rm = ((Register)op1).code;
                    bytes.Add((byte)(0xc0 + (rot * 8) + rm));
                }
                if (rotop < 0xd0)
                {
                    bytes.AddRange(((Immediate)op2).getBytes());
                }
            }
            else if (op2 is Register && ((Register)op2).code == 1)
            {
                int rotop = 0xd2;
                if (op1 is Memory)
                {
                    int mod;
                    int rm;

                    if (((Memory)op1).size == OPSIZE.DWord)
                    {
                        rotop++;
                    }
                    bytes.Add((byte)rotop);
                    List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                    bytes.Add((byte)(mod * 0x40 + (rot * 8) + rm));
                    bytes.AddRange(membytes);
                }
                else
                {
                    if (((Register)op1).size == OPSIZE.DWord)
                    {
                        rotop++;
                    }
                    bytes.Add((byte)rotop);
                    int rm = ((Register)op1).code;
                    bytes.Add((byte)(0xc0 + (rot * 8) + rm));
                }
            }
        }

        public override string ToString()
        {
            return (withCarry) ? ((mode == MODE.LEFT) ? "RCL" : "RCR") : ((mode == MODE.LEFT) ? "ROL" : "ROR");
        }
    }

    //- bit operations --------------------------------------------------------

    //BitTest - BT/BTC/BTR/BTS
    public class BitTest : Instruction
    {
        public enum MODE { BT, BTS, BTR, BTC }
        MODE mode;

        public BitTest(Operand _op1, Operand _op2, MODE _mode)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            mode = _mode;
        }

        public override string ToString()
        {
            return (mode == MODE.BT) ? "BT" : (mode == MODE.BTC) ? "BTC" : (mode == MODE.BTR) ? "BTR" : "BTS";
        }
    }

    //BitScan - BSF/BSR
    public class BitScan : Instruction
    {
        public enum MODE { BSF, BSR }
        MODE mode;

        public BitScan(Operand _op1, Operand _op2, MODE _mode)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            mode = _mode;
        }

        public override string ToString()
        {
            return (mode == MODE.BSF) ? "BSF" : "BSR";
        }
    }

    //SetByte - SET#
    public class SetByte : Instruction
    {
        public enum CONDIT
        {
            SETO, SETNO, SETB, SETAE, SETE, SETNE, SETBE, SETA,
            SETS, SETNS, SETP, SETNP, SETL, SETGE, SETLE, SETG
        };

        public CONDIT condit;

        public SetByte(Operand _op1, CONDIT _condit)
            : base()
        {
            opcount = 1;
            op1 = _op1;
            condit = _condit;
        }

        String[] condits = { "SETO", "SETNO", "SETB", "SETAE", "SETE", "SETNE", "SETBE", "SETA", 
                           "SETS", "SETNS", "SETP", "SETNP", "SETL", "SETGE", "SETLE", "SETG" };

        public override string ToString()
        {
            return condits[(int)condit];
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

        public override void generateBytes()
        {
            OpMode mode;
            OPSIZE size;

            bytes = new List<byte>();
            if (op2 is Immediate)
            {
                Immediate reg2 = (Immediate)op2;
                if (op1 is Register)
                {
                    Register reg1 = (Register)op1;
                    if (reg1.code == 0)
                    {
                        //a8, a9 - AL/EAX, imm
                        bytes.Add((byte)((reg1.size == OPSIZE.Byte) ? 0xa8 : 0xa9));                        
                    }
                    else
                    {
                        //f6, f7 - mem, imm
                        bytes.Add((byte)((((Register)op1).size == OPSIZE.Byte) ? 0xf6 : 0xf7));
                        bytes.Add((byte)(0xc0 + reg1.code));
                    }
                }
                else
                {
                    //f6, f7
                    int mod;
                    int rm;
                    bytes.Add((byte)((((Memory)op1).size == OPSIZE.Byte) ? 0xf6 : 0xf7));
                    List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                    bytes.Add((byte)(mod * 0x40 + rm));
                    bytes.AddRange(membytes);
                }
                bytes.AddRange(reg2.getBytes());
            }
            else
            {
                //84, 85
                List<byte> modrm = getModrm(op1, op2, out mode, out size);
                bytes.Add(0x84);
                if (size == OPSIZE.DWord)
                {
                    bytes[0] += 1;
                }
                bytes.AddRange(modrm);
            }
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

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Absolute)
            {
                bytes.Add(0xea);
                bytes.AddRange(((Absolute)op1).getBytes());
            }
            else if (op1 is Memory)
            {
                int mod;
                int rm;
                bytes.Add(0xff);
                int reg = (((Memory)op1).size == OPSIZE.DWord) ? 4 : 5;
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + (reg * 8) + rm));
                bytes.AddRange(membytes);
            }
            else if (op1 is Register)
            {
                bytes.Add(0xff);
                bytes.Add((byte)(0xe0 + ((Register)op1).code));
            }
            else
            {
                bytes.Add((byte)((((Relative)op1).size == OPSIZE.Byte) ? 0xeb : 0xe9));
                bytes.AddRange(((Relative)op1).getOffset());
            }
        }

        public override string ToString()
        {
            return "JMP";
        }
    }

    public class JumpConditional : Instruction
    {
        public enum CONDIT { JO, JNO, JB, JAE, JE, JNE, JBE, JA, JS, JNS, JP, JNP, JL, JGE, JLE, JG };

        public CONDIT condit;

        public JumpConditional(Operand _op1, CONDIT _condit)
            : base()
        {
            condit = _condit;
            op1 = _op1;
            opcount = 1;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add((byte)(0x70 + (int)condit));
            bytes.AddRange(((Relative)op1).getOffset());
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
        public enum MODE { LOOPNE, LOOPE, LOOP, JECXZ }

        MODE mode;

        public Loop(Operand _op1, MODE _mode)
            : base()
        {
            opcount = 1;
            op1 = _op1;
            mode = _mode;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add((byte)(0xe0 + (int)mode));
            bytes.AddRange(((Relative)op1).getOffset());
        }

        String[] modes = { "LOOPNE", "LOOPE", "LOOP", "JECXZ" };

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

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Absolute)
            {
                bytes.Add(0x9a);
                bytes.AddRange(((Absolute)op1).getBytes());
            }
            else if (op1 is Memory)
            {
                int mod;
                int rm;
                bytes.Add(0xff);
                int reg = (((Memory)op1).size == OPSIZE.DWord) ? 2 : 3;
                List<byte> membytes = ((Memory)op1).getBytes(out mod, out rm);
                bytes.Add((byte)(mod * 0x40 + (reg * 8) + rm));
                bytes.AddRange(membytes);
            }
            else if (op1 is Register)
            {
                bytes.Add(0xff);
                bytes.Add((byte)(0xd0 + ((Register)op1).code));
            }
            else
            {
                bytes.Add(0xe8);
                bytes.AddRange(((Relative)op1).getOffset());
            }
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

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add((byte)(far ? ((op1 != null) ? 0xca : 0xcb) : ((op1 != null) ? 0xc2 : 0xc3)));
            if (op1 != null)
            {
                bytes.AddRange(((Immediate)op1).getBytes());
            }
        }

        public override string ToString()
        {
            return (far) ? "RETF" : "RET";
        }
    }

    public class IReturn : Instruction
    {
        public IReturn()
            : base()
        {
        }

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xcf };
        }

        public override string ToString()
        {
            return "IRETD";
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

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xcd };
            bytes.AddRange(((Immediate)op1).getBytes());
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
            op1 = new Immediate(3, OPSIZE.Byte);
        }

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xcc };
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

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xce };
        }

        public override string ToString()
        {
            return "INTO";
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

        public override void generateBytes()
        {
            OpMode mode;
            OPSIZE size;

            bytes = new List<byte>();
            List<byte> modrm = getModrm(op1, op2, out mode, out size);
            bytes.Add(0x62);
            bytes.AddRange(modrm);
        }
        
        public override string ToString()
        {
            return "BOUND";
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

        public override void generateBytes()
        {
            bytes = new List<byte>() {0xc8};
            bytes.AddRange(((Immediate)op1).getBytes());
            bytes.AddRange(((Immediate)op2).getBytes());
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

        public override void generateBytes()
        {
            bytes = new List<byte>(){0xc9};
        }

        public override string ToString()
        {
            return "LEAVE";
        }
    }

    //- string operations -----------------------------------------------------------

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

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add((byte)((((Memory)op1).size == OPSIZE.Byte) ? 0xa4 : 0xa5));
        }

        public override string ToString()
        {
            String prefixStr = (prefix == LOOPPREFIX.REP) ? "REP " : ((prefix == LOOPPREFIX.REPNE) ? "REPNE " : "");
            return prefixStr + "MOVS";
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

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add((byte)((((Memory)op1).size == OPSIZE.Byte) ? 0xa6 : 0xa7));
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

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add((byte)((((Memory)op1).size == OPSIZE.Byte) ? 0xae : 0xaf));
        }

        public override string ToString()
        {
            String prefixStr = (prefix == LOOPPREFIX.REP) ? "REPE " : ((prefix == LOOPPREFIX.REPNE) ? "REPNE " : "");
            return prefixStr + "SCAS";
        }
    }

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

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add((byte)((((Memory)op1).size == OPSIZE.Byte) ? 0xac : 0xad));
        }

        public override string ToString()
        {
            String prefixStr = (prefix == LOOPPREFIX.REP) ? "REP " : ((prefix == LOOPPREFIX.REPNE) ? "REPNE " : "");
            return prefixStr + "LODS";
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

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add((byte)((((Memory)op1).size == OPSIZE.Byte) ? 0xaa : 0xab));
        }

        public override string ToString()
        {
            String prefixStr = (prefix == LOOPPREFIX.REP) ? "REP " : ((prefix == LOOPPREFIX.REPNE) ? "REPNE " : "");
            return prefixStr + "STOS";
        }
    }

    //- i/o operations --------------------------------------------------------

    public class Input : Instruction
    {
        public Input(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op2 is Immediate)
            {
                bytes.Add((byte)(((Register)op1).size == OPSIZE.Byte ? 0xe4 : 0xe5));
                bytes.AddRange(((Immediate)op2).getBytes());
            }
            else if (op2 is Register && ((Register)op2).code == 2)
            {
                bytes.Add((byte)(((Register)op1).size == OPSIZE.Byte ? 0xec : 0xed));
            }            
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

        public override void generateBytes()
        {
            bytes = new List<byte>();
            if (op1 is Immediate)
            {
                bytes.Add((byte)(((Register)op2).size == OPSIZE.Byte ? 0xe6 : 0xe7));
                bytes.AddRange(((Immediate)op1).getBytes());
            }
            else if (op1 is Register && ((Register)op1).code == 2)
            {
                bytes.Add((byte)(((Register)op2).size == OPSIZE.Byte ? 0xee : 0xef));
            }
        }

        public override string ToString()
        {
            return "OUT";
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

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add((byte)(((Memory)op1).size == OPSIZE.Byte ? 0x6c : 0x6d));
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

        public override void generateBytes()
        {
            bytes = new List<byte>();
            bytes.Add((byte)(((Memory)op2).size == OPSIZE.Byte ? 0x6e : 0x6f));
        }

        public override string ToString()
        {
            String prefixStr = (prefix == LOOPPREFIX.REP) ? "REP " : ((prefix == LOOPPREFIX.REPNE) ? "REPNE " : "");
            return prefixStr + "OUTS";
        }
    }

    //- flag operations -----------------------------------------------------------

    public class SetFlag : Instruction
    {
        public enum FLAG { Carry, Int, Dir }

        FLAG flag;

        public SetFlag(FLAG _flag)
            : base()
        {
            flag = _flag;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>() { (byte)(0xf9 + ((int)flag * 2)) };
        }

        String[] flags = { "STC", "STI", "STD" };

        public override string ToString()
        {
            return flags[(int)flag];
        }
    }

    public class ClearFlag : Instruction
    {
        public enum FLAG { Carry, Int, Dir }

        FLAG flag;

        public ClearFlag(FLAG _flag)
            : base()
        {
            flag = _flag;
        }

        public override void generateBytes()
        {
            bytes = new List<byte>() { (byte)(0xf8 + ((int)flag * 2)) };
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

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xf5 };
        }

        public override string ToString()
        {
            return "CMC";
        }
    }

    public class LoadFlags : Instruction
    {
        public LoadFlags()
            : base()
        {
        }

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0x9f };
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

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0x9e };
        }

        public override string ToString()
        {
            return "SAHF";
        }
    }

    public class PushFlags : Instruction
    {
        public PushFlags()
            : base()
        {
        }

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0x9c };
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

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0x9d };
        }

        public override string ToString()
        {
            return "POPFD";
        }
    }

    //- segment operations -----------------------------------------------------

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

        public override void generateBytes()
        {
            OpMode mod;
            OPSIZE size;
            bytes = new List<byte>();
            List<byte> modrm = getModrm(op1, op2, out mod, out size);
            bytes.Add((byte)((mode == MODE.LES) ? 0xc4 : 0xc5));
            bytes.AddRange(modrm);
        }


        public override string ToString()
        {
            return (mode == MODE.LDS) ? "LDS" : "LES";
        }
    }

    //LoadFarPointer - LFS/LGS/LSS
    public class LoadFarPointer : Instruction
    {
        public enum SEG { SS, FS, GS }
        SEG seg;

        public LoadFarPointer(Operand _op1, Operand _op2, SEG _seg)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
            seg = _seg;
        }

        public override string ToString()
        {
            return (seg == SEG.FS) ? "LFS" : (seg == SEG.GS) ? "LGS" : "LSS";
        }
    }

    //- miscellaneous -------------------------------------------------------------

    public class LoadEffAddress : Instruction
    {
        public LoadEffAddress(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override void generateBytes()
        {
            OpMode mode;
            OPSIZE size;

            bytes = new List<byte>();

            List<byte> modrm = getModrm(op1, op2, out mode, out size);
            bytes.Add(0x8d);
            bytes.AddRange(modrm);
        }


        public override string ToString()
        {
            return "LEA";
        }
    }

    public class NoOp : Instruction
    {
        public NoOp()
            : base()
        {
        }

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0x90 };
        }

        public override string ToString()
        {
            return "NOP";
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

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xd7 };
        }

        public override string ToString()
        {
            String prefixStr = (prefix == LOOPPREFIX.REP) ? "REP " : ((prefix == LOOPPREFIX.REPNE) ? "REPNE " : "");
            return prefixStr + "XLAT";
        }
    }

    //CpuId - CPUID
    public class CpuId : Instruction
    {
        public CpuId()
            : base()
        {
        }

        public override string ToString()
        {
            return "CPUID";
        }
    }

    //CacheFlush - CLFLUSH/CLFLUSHOPT
    public class CacheFlush : Instruction
    {
        bool optimized;

        public CacheFlush(Operand _op1, bool _optimized)
            : base()
        {
            opcount = 1;
            op1 = _op1;
            optimized = _optimized;
        }

        public override string ToString()
        {
            return optimized ? "CLFLUSHOPT" : "CLFLUSH";
        }
    }

    //UndefinedOp - UD0/UD1/UD2
    public class UndefinedOp : Instruction
    {
        int level;

        public UndefinedOp(int _level)
            : base()
        {
            level = _level;
        }

        public override string ToString()
        {
            return ("UD" + level.ToString());
        }
    }
}
