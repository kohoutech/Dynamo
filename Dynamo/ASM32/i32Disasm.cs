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

//Intel 64 and IA-32 Architectures Software Developer’s Manual. Volume 2A - 2D
//specfically Appendix A - Opcode Maps

namespace Origami.Asm32
{
    class i32Disasm
    {        
        public byte[] srcBuf;           //the bytes being disassembled
        public uint srcpos;             //cur pos in source buf
        public uint codeaddr;           //cur addr of instr in mem
        public List<byte> instrBytes;    //the bytes that have been decoded for this instruction        
        
        public Operand op1;
        public Operand op2;
        public Operand op3;
        
        public Segment.SEG segprefix;
        public Instruction.LOOPPREFIX loopprefix;
        public bool lockprefix;
        public bool operandSizeOverride;
        public bool addressSizeOverride;
        
        private bool useModrm32;

        public i32Disasm(byte[] _source, uint _srcpos)
        {
            srcBuf = _source;           //set source buf + pos in buf when we start disassembling
            srcpos = _srcpos;           //source pos can be changed later if we need to skip over some bytes

            instrBytes = new List<byte>();            

            codeaddr = 0;

            segprefix = Segment.SEG.DS;
            loopprefix = Instruction.LOOPPREFIX.None;
            lockprefix = false;
            operandSizeOverride = false;
            addressSizeOverride = false;
            useModrm32 = false;            
        }

        public Instruction getInstr(uint _codepos)
        {
            Instruction instr = null;
            instrBytes = new List<byte>();

            uint instrAddr = _codepos;
            codeaddr = _codepos;
            segprefix = Segment.SEG.DS;
            loopprefix = Instruction.LOOPPREFIX.None;
            lockprefix = false;
            operandSizeOverride = false;
            addressSizeOverride = false;
            useModrm32 = false;



            uint b = getNextByte();

            //prefix byte
            while ((b == 0x26) || (b == 0x2e) || (b == 0x36) || (b == 0x3e) ||
                   (b == 0x64) || (b == 0x65) || (b == 0x66) || (b == 0x67) ||
                   (b == 0xf0) || (b == 0xf2) || (b == 0xf3)
                )
            {
                setPrefix(b);
                b = getNextByte();
            }

            if (b == 0x0f)
            {
                uint nb = getNextByte();
                instr = op0f(nb);
            }
            else if ((b >= 0x00) && (b <= 0x3f))
            {
                instr = op03x(b);
            }
            else if ((b >= 0x40) && (b <= 0x5f))
            {
                instr = op45x(b);
            }
            else if ((b >= 0x60) && (b <= 0x6f))
            {
                instr = op6x(b);
            }
            else if ((b >= 0x70) && (b <= 0x7f))
            {
                instr = op7x(b);
            }
            else if ((b >= 0x80) && (b <= 0x8f))
            {
                instr = op8x(b);
            }
            else if ((b >= 0x90) && (b <= 0x9f))
            {
                instr = op9x(b);
            }
            else if ((b >= 0xa0) && (b <= 0xaf))
            {
                instr = opax(b);
            }
            else if ((b >= 0xb0) && (b <= 0xbf))
            {
                instr = opbx(b);
            }
            else if ((b >= 0xc0) && (b <= 0xcf))
            {
                instr = opcx(b);
            }
            else if ((b >= 0xd0) && (b <= 0xd7))
            {
                instr = opd7(b);
            }
            else if ((b >= 0xd8) && (b <= 0xdf))
            {
                instr = op8087(b);
            }
            else if ((b >= 0xe0) && (b <= 0xef))
            {
                instr = opex(b);
            }
            else if ((b >= 0xf0) && (b <= 0xff))
            {
                instr = opfx(b);
            }

            if (instr == null)
            {
                instr = new UnknownOp();
            }

            instr.lockprefix = lockprefix;
            instr.addr = instrAddr;
            instr.bytes = instrBytes;

            return instr;
        }

//- source bytes --------------------------------------------------------------

        public uint getNextByte()
        {
            uint b = (uint)srcBuf[srcpos++];
            instrBytes.Add((byte)b);
            codeaddr++;
            return b;
        }

//- prefixing -----------------------------------------------------------------

        //4 prefix groups, successive bytes in the same group will overwrite prev prefix byte
        public void setPrefix(uint b)
        {
            if (b == 0xf0) lockprefix = true;             //group1

            if (b == 0xf2) loopprefix = Instruction.LOOPPREFIX.REPNE;
            if (b == 0xf3) loopprefix = Instruction.LOOPPREFIX.REP;

            if (b == 0x26) segprefix = Segment.SEG.ES;    //group2
            if (b == 0x2e) segprefix = Segment.SEG.CS;
            if (b == 0x36) segprefix = Segment.SEG.SS;
            if (b == 0x3e) segprefix = Segment.SEG.DS;
            if (b == 0x64) segprefix = Segment.SEG.FS;
            if (b == 0x65) segprefix = Segment.SEG.GS;                
            
            if (b == 0x66) operandSizeOverride = true;      //group 3
            if (b == 0x67) addressSizeOverride = true;      //group 4
        }

//- addressing ----------------------------------------------------------------

        readonly int[] sibscale = { 1, 2, 4, 8 };

        public Memory getSib(uint mode, uint rm, OPSIZE size)
        {
            Memory result = null;
            Immediate imm = null;
            switch (rm)
            {
                case 0x00:
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x06:
                case 0x07:
                    //r1
                    result = new Memory(getReg(OPSIZE.DWord, rm), null, Memory.Mult.NONE, null, size, segprefix);
                    break;
                case 0x04:
                    uint sib = getNextByte();               
                    uint scale = (sib / 0x40) % 0x04;       //xx------
                    uint siba = (sib % 0x40) / 0x08;        //--xxx---
                    uint sibb = (sib % 0x08);               //-----xxx
                    if (siba != 0x04)                       //--100---
                    {
                        if ((sibb == 0x05) && (mode == 00))   //-----101
                        {
                            //r2 + imm32
                            imm = new Immediate(addr32(), OPSIZE.DWord);
                            result = new Memory(null, getReg(OPSIZE.DWord, siba), (Memory.Mult)scale, imm, size, segprefix);
                        }
                        else
                        {
                            //r1 + r2
                            result = new Memory(getReg(OPSIZE.DWord, sibb), getReg(OPSIZE.DWord, siba),
                                (Memory.Mult)scale, null, size, segprefix);
                        }
                    }
                    else
                    {
                        if ((sibb == 0x05) && (mode == 00))  //-----101
                        {
                            //imm32
                            imm = new Immediate(addr32(), OPSIZE.DWord);
                            result = new Memory(null, null, Memory.Mult.NONE, imm, size, segprefix);
                        }
                        else
                        {
                            //r1
                            result = new Memory(getReg(OPSIZE.DWord, sibb), null, Memory.Mult.NONE, null, size, segprefix);
                        }
                    }
                    break;
                case 0x05:
                    if (mode == 0x00)
                    {
                        //imm32
                        imm = new Immediate(addr32(), OPSIZE.DWord);
                        result = new Memory(null, null, Memory.Mult.NONE, imm, size, segprefix);
                    }
                    else
                    {
                        //r1 (+ imm 8/32)
                        result = new Memory(getReg(OPSIZE.DWord, rm), null, Memory.Mult.NONE, null, size, segprefix);
                    }
                    break;
            }

            if (mode == 0x01)
            {
                imm = new Immediate(getOfs(OPSIZE.Byte), OPSIZE.Byte);
                imm.isOffset = true;
                result.imm = imm;
            }
            if (mode == 0x02)
            {
                imm = new Immediate(getOfs(OPSIZE.DWord), OPSIZE.DWord);
                imm.isOffset = true;
                result.imm = imm;
            }

            return result;
        }

        public Operand getModrm(uint modrm, OPSIZE size)
        {
            Operand result = null;
            uint mode = (modrm / 0x40) % 0x04;
            uint rm = (modrm % 0x08);
            if (mode != 0x03)
            {
                result = getSib(mode, rm, size);
            }
            else
            {
                result = getReg(size, rm);
            }
            return result;
        }

//- opcodes -------------------------------------------------------------------

        public Instruction ArithmeticOps(uint op, Operand op1, Operand op2)
        {
            Instruction instr = null;
            switch (op)
            {
                case 0x00:
                    instr = new Add(op1, op2, false);
                    break;
                case 0x01:
                    instr = new Or(op1, op2);
                    break;
                case 0x02:
                    instr = new Add(op1, op2, true);        //ADC
                    break;
                case 0x03:
                    instr = new Subtract(op1, op2, true);   //SBB
                    break;
                case 0x04:
                    instr = new And(op1, op2);
                    break;
                case 0x05:
                    instr = new Subtract(op1, op2, false);
                    break;
                case 0x06:
                    instr = new Xor(op1, op2);
                    break;
                case 0x07:
                    instr = new Compare(op1, op2);
                    break;
            }
            return instr;
        }

        //0x0f starts two byte opcodes and should never get here
        //0X26, 0x2e, 0x36, 0x3e are prefix bytes and should never get here            
        public Instruction op03x(uint b)
        {
            Instruction instr = null;
            uint bhi = (b / 0x08) % 0x08;   //--bb b--- (top two bits should = 0)
            uint blo = b % 0x08;            //---- -bbb
            uint modrm = 0;

            if (blo < 6)
            {
                switch (blo)
                {
                    case 0x00:
                        modrm = getNextByte();
                        op1 = getModrm(modrm, OPSIZE.Byte);                     //Eb
                        op2 = getReg(OPSIZE.Byte, (modrm % 0x40) / 0x08);       //Gb
                        break;

                    case 0x01:
                        modrm = getNextByte();
                        op1 = getModrm(modrm, OPSIZE.DWord);                    //Ev
                        op2 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);      //Gv
                        break;

                    case 0x02:
                        modrm = getNextByte();
                        op1 = getReg(OPSIZE.Byte, (modrm % 0x40) / 0x08);       //Gb
                        op2 = getModrm(modrm, OPSIZE.Byte);                     //Eb
                        break;

                    case 0x03:
                        modrm = getNextByte();
                        op1 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);      //Gv
                        op2 = getModrm(modrm, OPSIZE.DWord);                    //Ev
                        break;

                    case 0x04:
                        op1 = getReg(OPSIZE.Byte, 0);           //AL
                        op2 = getImm(OPSIZE.Byte);              //Ib
                        break;

                    case 0x05:
                        op1 = getReg(OPSIZE.DWord, 0);          //EAX
                        op2 = getImm(OPSIZE.DWord);             //Iz
                        break;
                }

                instr = ArithmeticOps(bhi, op1, op2);
            }

            if (blo == 0x06)        //0x06, 0x0e, 0x16, 0x1e
            {
                op1 = Segment.getSeg((int)bhi);
                instr = new Push(op1);                
            }

            if ((blo == 0x07))
            {
                if (bhi < 0x03) {                                   //0x07, 0x0f, 0x17, 0x1f
                    op1 = Segment.getSeg((int)bhi);
                    instr  = new Pop(op1);
                }
                else if (bhi < 0x06)
                {
                    instr = new DecimalAdjust((DecimalAdjust.MODE)(bhi - 4));       //0x27, 0x2f
                }
                else
                {
                    instr = new AsciiAdjust((AsciiAdjust.MODE)(bhi - 6));           //0x37, 0x3f
                }
            }
            return instr;
        }

        public Instruction op45x(uint b)
        {
            Instruction instr = null;
            op1 = getReg(OPSIZE.DWord, (b % 0x08));
            uint bhi = (b / 0x08) % 0x04;
            switch (bhi)
            {
                case 0x00:
                    instr = new Increment(op1);
                    break;
                case 0x01:
                    instr = new Decrement(op1);
                    break;
                case 0x02:
                    instr = new Push(op1);
                    break;
                case 0x03:
                    instr = new Pop(op1);
                    break;
            }
            return instr;            
        }

        //0X64, 0x65, 0x66, 0x67 are prefix bytes and should never get here
        public Instruction op6x(uint b)
        {
            Instruction instr = null;
            uint bhi = (b / 0x08) % 0x08;   //--bb b--- (top two bits should = 0)
            uint blo = b % 0x08;            //---- -bbb
            uint modrm = 0;
            switch (b)
            {
                case 0x60:
                    instr = new PushRegisters();
                    break;
                case 0x61:
                    instr = new PopRegisters();
                    break;

                case 0x62:
                    modrm = getNextByte();
                    uint mode = (modrm / 0x40) % 0x04;      //62 0c - 62 ff undefined
                    if (mode < 0x03)
                    {
                        op1 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);      //Gv
                        op2 = getModrm(modrm, OPSIZE.QWord);                    //Mz
                        instr = new Bound(op1, op2);
                    }
                    break;

                case 0x63:
                    bhi = (b / 0x08) % 0x08;   //--bb b--- (top two bits should = 0)
                    blo = b % 0x08;            //---- -bbb
                    modrm = getNextByte();
                    op1 = getModrm(modrm, OPSIZE.Word);                         //Ew
                    op2 = getReg(OPSIZE.Word, (modrm % 0x40) / 0x08);           //Gw
                    instr = new AdjustRPL(op1, op2);
                    break;

                case 0x68:
                    op1 = getImm(OPSIZE.DWord);                 //Iz
                    instr = new Push(op1);
                    break;

                case 0x69:
                    bhi = (b / 0x08) % 0x08;   //--bb b--- (top two bits should = 0)
                    blo = b % 0x08;            //---- -bbb
                    modrm = getNextByte();
                    op1 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);          //Gv
                    op2 = getModrm(modrm, OPSIZE.DWord);                        //Ev
                    op3 = getImm(OPSIZE.DWord);                                 //Iz
                    instr = new IntMultiply(op1, op2, op3);
                    break;

                case 0x6a:
                    op1 = getImm(OPSIZE.SignedByte);            //Ib
                    instr = new Push(op1);
                    break;

                case 0x6b:
                    bhi = (b / 0x08) % 0x08;   //--bb b--- (top two bits should = 0)
                    blo = b % 0x08;            //---- -bbb
                    modrm = getNextByte();
                    op1 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);          //Gv
                    op2 = getModrm(modrm, OPSIZE.DWord);                        //Ev
                    op3 = getImm(OPSIZE.SignedByte);                            //Ib
                    instr = new IntMultiply(op1, op2, op3);
                    break;

                case 0x6c:
                case 0x6d:
                    op1 = new Memory(Register32.EDI, null, Memory.Mult.NONE, null,
                        (b == 0x6c) ? OPSIZE.Byte : OPSIZE.DWord, Segment.SEG.ES);      //Yb or Yz
                    op2 = Register16.DX;                                                //DX
                    instr = new InputString(op1, op2, loopprefix);
                    break;

                case 0x6e:
                case 0x6f:
                    op1 = Register16.DX;                                                //DX
                    op2 = new Memory(Register32.ESI, null, Memory.Mult.NONE, null,
                        (b == 0x6e) ? OPSIZE.Byte : OPSIZE.DWord, Segment.SEG.DS);      //Xb or Xz
                    instr = new OutputString(op1, op2, loopprefix);
                    break;

            }
            return instr;
        }

        public Instruction op7x(uint b)
        {
            JumpConditional.CONDIT condit = (JumpConditional.CONDIT)(b % 0x10);           
            op1 = rel8();                                                               //Jb
            return new JumpConditional(op1, condit);
        }

        public Instruction op8x(uint b)
        {
            Instruction instr = null;
            uint modrm = getNextByte();
            uint bhi = (modrm % 0x40) / 0x08;       //--bb b---
            switch (b)
            {
                case 0x80:
                case 0x82:                      //0x82 is the same as 0x80?
                    op1 = getModrm(modrm, OPSIZE.Byte);                 //Eb
                    op2 = getImm(OPSIZE.Byte);                          //Ib
                    instr = ArithmeticOps(bhi, op1, op2);
                    break;

                case 0x81:
                    op1 = getModrm(modrm, OPSIZE.DWord);                //Ev
                    op2 = getImm(OPSIZE.DWord);                         //Iz
                    instr = ArithmeticOps(bhi, op1, op2);
                    break;

                case 0x83:
                    op1 = getModrm(modrm, OPSIZE.DWord);                //Ev
                    op2 = getImm(OPSIZE.SignedByte);                    //Ib
                    instr = ArithmeticOps(bhi, op1, op2);
                    break;

                case 0x84:
                case 0x85:
                    op1 = getModrm(modrm, (b == 0x84) ? OPSIZE.Byte : OPSIZE.DWord);        //Eb or Ev
                    op2 = getReg((b == 0x84) ? OPSIZE.Byte : OPSIZE.DWord, bhi);            //Gb or Gv
                    instr = new Test(op1, op2);
                    break;

                case 0x86:
                case 0x87:
                    op1 = getReg((b == 0x86) ? OPSIZE.Byte : OPSIZE.DWord, bhi);
                    op2 = getModrm(modrm, (b == 0x86) ? OPSIZE.Byte : OPSIZE.DWord);
                    instr = new Exchange(op1, op2);
                    break;

                case 0x88:
                case 0x89:
                    op1 = getModrm(modrm, (b == 0x88) ? OPSIZE.Byte : OPSIZE.DWord);
                    op2 = getReg((b == 0x88) ? OPSIZE.Byte : OPSIZE.DWord, bhi);
                    instr = new Move(op1, op2);
                    break;

                case 0x8a:
                case 0x8b:
                    op1 = getReg((b == 0x8a) ? OPSIZE.Byte : OPSIZE.DWord, bhi);
                    op2 = getModrm(modrm, (b == 0x8a) ? OPSIZE.Byte : OPSIZE.DWord);
                    instr = new Move(op1, op2);
                    break;

                case 0x8c:
                    //useModrm32 = true;                        //kludge to fix dumpbin bug
                    //if ((modrm < 0xc0) && (bhi < 0x06))         //8c 30 - 8c 3f, 8c 70 - 8c 7f, 8c b0 - 8c ff undefined
                    if ((bhi < 0x06))
                    {                                                   
                        op1 = getModrm(modrm, OPSIZE.Word);
                        op2 = Segment.getSeg((int)bhi);
                        instr = new Move(op1, op2);
                    }
                    break;

                case 0x8d:
                    if (modrm < 0xc0)       //8d 0c - 8d ff undefined
                    {
                        op1 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);
                        op2 = getModrm(modrm, OPSIZE.None);          //no "byte/dword ptr " prefix
                        instr = new LoadEffAddress(op1, op2);
                    }
                    break;

                case 0x8e:
                    if (bhi < 0x06)                 //8e 30 - 8e 3f, 8e 70 - 8e 7f, 8e b0 - 8e bf, 8e f0 - 8e ff undefined
                    {
                        op1 = Segment.getSeg((int)bhi);
                        op2 = getModrm(modrm, OPSIZE.Word);
                        instr = new Move(op1, op2);
                    }
                    break;

                case 0x8f:
                    if (bhi == 0)                               //8f 08 - 8f 3f, 8f 48 - 8f 7f \ undefined 
                    {                                           //8f 88 - 88 bf, 88 c8 - 88 ff /
                        op1 = getModrm(modrm, OPSIZE.DWord);
                        instr = new Pop(op1);
                    }
                    break;

            }
            return instr;
        }

        public Instruction op9x(uint b)
        {
            Instruction instr = null;
            switch (b)
            {
                case 0x90:
                    instr = new NoOp();
                    break;

                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                case 0x97:
                    op1 = getReg(OPSIZE.DWord, 0);
                    op2 = getReg(OPSIZE.DWord, b % 0x8);
                    instr = new Exchange(op1, op2);
                    break;

                case 0x98:
                    instr = new ConvertSize(ConvertSize.MODE.CWDE);
                    break;

                case 0x99:
                    instr = new ConvertSize(ConvertSize.MODE.CDQ);
                    break;

                case 0x9a:
                    op1 = absolute();
                    instr = new Call(op1);
                    break;

                case 0x9b:
                    instr = new Wait();
                    break;

                case 0x9c:
                    instr = new PushFlags();
                    break;

                case 0x9d:
                    instr = new PopFlags();
                    break;

                case 0x9e:
                    instr = new StoreFlags();
                    break;

                case 0x9f:
                    instr = new LoadFlags();
                    break;
            }
            return instr;
        }

        public Instruction opax(uint b)
        {
            Instruction instr = null;
            Immediate imm = null;
            OPSIZE size = (b % 2 == 0) ? OPSIZE.Byte : OPSIZE.DWord;
            switch (b)
            {
                case 0xa0:
                case 0xa1:
                    op1 = getReg(size, 0);
                    imm = new Immediate(addr32(), OPSIZE.DWord);
                    op2 = new Memory(null, null, Memory.Mult.NONE, imm, size, Segment.SEG.DS);
                    instr = new Move(op1, op2);
                    break;

                case 0xa2:
                case 0xa3:
                    imm = new Immediate(addr32(), OPSIZE.DWord);
                    op1 = new Memory(null, null, Memory.Mult.NONE, imm, size, Segment.SEG.DS);
                    op2 = getReg(size, 0);
                    instr = new Move(op1, op2);
                    break;
                
                case 0xa4:                
                case 0xa5:
                    op1 = new Memory(Register32.EDI, null, Memory.Mult.NONE, null, size, Segment.SEG.ES);
                    op2 = new Memory(Register32.ESI, null, Memory.Mult.NONE, null, size, Segment.SEG.DS);
                    instr = new MoveString(op1, op2, loopprefix);
                    break;

                case 0xa6:
                case 0xa7:
                    op1 = new Memory(Register32.ESI, null, Memory.Mult.NONE, null, size, Segment.SEG.DS);
                    op2 = new Memory(Register32.EDI, null, Memory.Mult.NONE, null, size, Segment.SEG.ES);
                    instr = new CompareString(op1, op2, loopprefix);
                    break;
                
                case 0xa8:
                case 0xa9:
                    op1 = getReg(size, 0);
                    op2 = getImm(size);
                    instr = new Test(op1, op2);
                    break;

                case 0xaa:
                case 0xab:
                    op1 = new Memory(Register32.EDI, null, Memory.Mult.NONE, null, size, Segment.SEG.ES);
                    instr = new StoreString(op1, loopprefix);
                    break;

                case 0xac:
                case 0xad:
                    op1 = new Memory(Register32.ESI, null, Memory.Mult.NONE, null, size, Segment.SEG.DS);
                    instr = new LoadString(op1, loopprefix);
                    break;

                case 0xae:
                case 0xaf:
                    op1 = new Memory(Register32.EDI, null, Memory.Mult.NONE, null, size, Segment.SEG.ES);
                    instr = new ScanString(op1, loopprefix);
                    break;                
            }
            return instr;
        }

        public Instruction opbx(uint b)
        {
            if (b <= 0xb7)
            {
                op1 = getReg(OPSIZE.Byte, b % 0x8);
                op2 = getImm(OPSIZE.Byte);
            }
            else
            {
                op1 = getReg(OPSIZE.DWord, b % 0x8);
                op2 = getImm(OPSIZE.DWord);
            }
            return new Move(op1, op2);
        }

        public Instruction BitOps(uint op, Operand op1, Operand op2)
        {
            Instruction instr = null;
            switch (op)
            {
                case 0x00:
                    instr = new Rotate(op1, op2, Rotate.MODE.LEFT, false);
                    break;
                case 0x01:
                    instr = new Rotate(op1, op2, Rotate.MODE.RIGHT, false);
                    break;
                case 0x02:
                    instr = new Rotate(op1, op2, Rotate.MODE.LEFT, true);
                    break;
                case 0x03:
                    instr = new Rotate(op1, op2, Rotate.MODE.RIGHT, true);
                    break;
                case 0x04:
                    instr = new Shift(op1, op2, Shift.MODE.LEFT, false);
                    break;
                case 0x05:
                    instr = new Shift(op1, op2, Shift.MODE.RIGHT, false);
                    break;
                case 0x06:
                    instr = new Shift(op1, op2, Shift.MODE.LEFT, true);
                    break;
                case 0x07:
                    instr = new Shift(op1, op2, Shift.MODE.RIGHT, true);
                    break;
            }
            return instr;
        }

        public Instruction opcx(uint b)
        {
            Instruction instr = null;
            uint modrm = 0;
            OPSIZE size = (b % 2 == 0) ? OPSIZE.Byte : OPSIZE.DWord;
            switch (b)
            {
                case 0xc0:
                case 0xc1:
                    modrm = getNextByte();
                    op1 = getModrm(modrm, size);
                    op2 = getImm(OPSIZE.Byte);
                    instr = BitOps(((modrm % 0x40) / 0x08), op1, op2);
                    break;

                case 0xc2:
                    op1 = getImm(OPSIZE.Word);
                    instr = new Return(op1, false);
                    break;

                case 0xc3:
                    instr = new Return(false);        
                    break;

                case 0xc4:
                case 0xc5:
                    modrm = getNextByte();
                    op1 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);
                    op2 = getModrm(modrm, OPSIZE.FWord);
                    instr = new LoadPtr(op1, op2, (b == 0xc4) ? LoadPtr.MODE.LES : LoadPtr.MODE.LDS);
                    break;

                case 0xc6:
                case 0xc7:
                    modrm = getNextByte();
                    uint mode = (modrm % 0x40) / 0x08;          //08 - 3f, 48 - 7f \ undefined 
                    if (mode == 0)                              //88 - bf, c8 - ff /
                    {
                        op1 = getModrm(modrm, size);
                        op2 = getImm(size);
                        instr = new Move(op1, op2);        
                    }
                    break;

                case 0xc8:
                    op1 = getImm(OPSIZE.Word);
                    op2 = getImm(OPSIZE.Byte);
                    instr = new Enter(op1, op2);
                    break;

                case 0xc9:
                    instr = new Leave();
                    break;

                case 0xca:
                    op1 = getImm(OPSIZE.Word);
                    instr = new Return(op1, true);
                    break;

                case 0xcb:
                    instr = new Return(true);
                    break;

                case 0xcc:
                    instr = new InterruptDebug();
                    break;

                case 0xcd:
                    op1 = getImm(OPSIZE.Byte);
                    instr = new Interrupt(op1);
                    break;

                case 0xce:
                    instr = new InterruptOverflow();
                    break;

                case 0xcf:
                    instr = new IReturn();
                    break;

            }
            return instr;
        }

        public Instruction opd7(uint b)
        {
            Instruction instr = null;
            uint modrm = 0;
            OPSIZE size = (b % 2 == 0) ? OPSIZE.Byte : OPSIZE.DWord;
            switch (b)
            {
                case 0xd0:
                case 0xd1:
                    modrm = getNextByte();
                    op1 = getModrm(modrm, size);
                    op2 = new Immediate(1, OPSIZE.Byte);
                    instr = BitOps(((modrm % 0x40) / 0x08), op1, op2);
                    break;

                case 0xd2:
                case 0xd3:
                    modrm = getNextByte();
                    op1 = getModrm(modrm, size);
                    op2 = getReg(OPSIZE.Byte, 1);                    
                    instr = BitOps(((modrm % 0x40) / 0x08), op1, op2);
                    break;

                case 0xd4:
                case 0xd5:
                    op1 = getImm(OPSIZE.Byte);
                    AsciiAdjust.MODE mode = (b == 0xd4) ? AsciiAdjust.MODE.Mult : AsciiAdjust.MODE.Div;
                    if (((Immediate)op1).val == 10)
                    {
                        instr = new AsciiAdjust(mode);      //base 10 is implied & doesn't need an operand
                    }
                    else
                    {
                        instr = new AsciiAdjust(mode, op1);     //for bases other than 10
                    }
                    break;                

                case 0xd6:              //0xd6 is undefined
                    break;

                case 0xd7:
                    op1 = new Memory(Register32.EBX, null, Memory.Mult.NONE, null, OPSIZE.Byte, Segment.SEG.DS);
                    instr = new XlateString(op1, loopprefix);
                    break;
            }
            return instr;
        }

        readonly Loop.MODE[] exloop = { Loop.MODE.LOOPNE, Loop.MODE.LOOPE, Loop.MODE.LOOP, Loop.MODE.JECXZ };

        public Instruction opex(uint b)
        {
            Instruction instr = null;
            OPSIZE size = (b % 2 == 0) ? OPSIZE.Byte : OPSIZE.DWord;
            switch (b)
            {
                case 0xe0:
                case 0xe1:
                case 0xe2:
                case 0xe3:
                    op1 = rel8();
                    instr = new Loop(op1, exloop[b - 0xe0]);
                    break;

                case 0xe4:
                case 0xe5:
                case 0xec:
                case 0xed:
                    op1 = getReg(size, 0);
                    op2 = (b < 0xec) ? getImm(OPSIZE.Byte) : getReg(OPSIZE.Word, 2);
                    instr = new Input(op1, op2);
                    break;

                case 0xe6:
                case 0xe7:
                case 0xee:
                case 0xef:
                    op1 = (b < 0xee) ? getImm(OPSIZE.Byte) : getReg(OPSIZE.Word, 2);
                    op2 = getReg(size, 0);
                    instr = new Output(op1, op2);
                    break;

                case 0xe8:
                    op1 = rel32();
                    instr = new Call(op1);
                    break;

                case 0xe9:
                    op1 = rel32();
                    instr = new Jump(op1);
                    break;

                case 0xea:
                    op1 = absolute();
                    instr = new Jump(op1);
                    break;

                case 0xeb:
                    op1 = rel8();
                    instr = new Jump(op1);
                    break;
            }
            return instr;
        }

        readonly ClearFlag.FLAG[] fxflagclear = { ClearFlag.FLAG.Carry, ClearFlag.FLAG.Int, ClearFlag.FLAG.Dir };
        readonly SetFlag.FLAG[] fxflagset = { SetFlag.FLAG.Carry, SetFlag.FLAG.Int, SetFlag.FLAG.Dir };

        //0Xf0, 0xf2, 0xf3 are prefix bytes and should never get here
        public Instruction opfx(uint b)
        {
            Instruction instr = null;
            uint modrm = 0;
            uint mode = 0;
            OPSIZE size = (b % 2 == 0) ? OPSIZE.Byte : OPSIZE.DWord;
            switch (b)
            {
                case 0xf1:              //0xf1 is undefined
                    break;

                case 0xf4:
                    instr = new Halt();
                    break;

                case 0xf5:
                    instr = new ComplementCarry();
                    break;

                case 0xf6:
                case 0xf7:
                    modrm = getNextByte();
                    mode = (modrm % 0x40) / 0x08;
                    switch (mode)                       //mode == 1 is undefined
                    {
                        case 0 :
                            op1 = getModrm(modrm, size);
                            op2 = getImm(size);
                            instr = new Test(op1, op2);
                            break;

                        case 2:
                            op1 = getModrm(modrm, size);
                            instr = new Not(op1);                            
                            break;
                        
                        case 3:
                            op1 = getModrm(modrm, size);
                            instr = new Negate(op1);                            
                            break;

                        case 4:
                            op1 = getReg(size, 0);
                            op2 = getModrm(modrm, size);
                            instr = new Multiply(op1, op2);
                            break;

                        case 5:
                            op1 = getModrm(modrm, size);
                            instr = new IntMultiply(op1);
                            break;

                        case 6:
                            op1 = getReg(size, 0);
                            op2 = getModrm(modrm, size);
                            instr = new Divide(op1, op2);
                            break;

                        case 7:
                            if (b == 0xf6)
                            {
                                op1 = getModrm(modrm, OPSIZE.Byte);
                                instr = new IntDivide(op1);                                
                            }
                            else
                            {
                                op1 = getReg(OPSIZE.DWord, 0);
                                op2 = getModrm(modrm, OPSIZE.DWord);
                                instr = new IntDivide(op1, op2);
                            }
                            break;
                    }
                    break;

                case 0xf8:
                case 0xfa:
                case 0xfc:
                    instr = new ClearFlag(fxflagclear[(b - 0xf8)/2]);
                    break;

                case 0xf9:
                case 0xfb:
                case 0xfd:
                    instr = new SetFlag(fxflagset[(b - 0xf9) / 2]);
                    break;

                case 0xfe:
                    modrm = getNextByte();
                    op1 = getModrm(modrm, OPSIZE.Byte);
                    mode = (modrm % 0x40) / 0x08;                   //10 - 3f, 50 - 7f \ undefined 
                    if (mode == 0)                                  //90 - bf, d0 - ff /
                    {                        
                        instr = new Increment(op1);                            
                    }
                    else if (mode == 1)
                    {
                        instr = new Decrement(op1);
                    }
                    break;

                case 0xff:
                    modrm = getNextByte();
                    mode = (modrm / 0x40) % 0x04;               //38 - 3f, 78 - 7f \ undefined 
                    uint range = (modrm % 0x40) / 0x08;         //b8 - bf, f8 - ff /
                    switch (range)                              //range == 7 is undefined
                    {
                        case 0:
                            op1 = getModrm(modrm, OPSIZE.DWord);
                            instr = new Increment(op1);
                            break;

                        case 1:
                            op1 = getModrm(modrm, OPSIZE.DWord);
                            instr = new Decrement(op1);
                            break;

                        case 2:
                            op1 = getModrm(modrm, OPSIZE.DWord);
                            instr = new Call(op1);
                            break;

                        case 3:
                            if (mode < 3)
                            {
                                op1 = getModrm(modrm, OPSIZE.FWord);
                                instr = new Call(op1);
                            }
                            break;

                        case 4:
                            op1 = getModrm(modrm, OPSIZE.DWord);
                            instr = new Jump(op1);
                            break;

                        case 5:
                            if (mode < 3)
                            {
                                op1 = getModrm(modrm, OPSIZE.FWord);
                                instr = new Jump(op1);
                            }
                            break;

                        case 6:
                            op1 = getModrm(modrm, OPSIZE.DWord);
                            instr = new Push(op1);
                            break;
                    }
                    break;
            }
            return instr;
        }

//- 80x87 instructions --------------------------------------------------------

        public Instruction op8087(uint b)
        {
            Instruction instr = null;
            //all 80x87 opcodes are modr/m instrs
            uint modrm = getNextByte();
            uint mode = (modrm / 0x40) % 0x04;
            uint range = (modrm % 0x40) / 0x08;
            uint rm = (modrm % 0x08);

            if (mode < 3)        //modrm = 0x00 - 0xbf
            {
                switch (b)
                {
                    case 0xd8:
                        op1 = getModrm(modrm, OPSIZE.DWord);
                        instr = Arithmetic87Ops(range, op1, null, false, false, true);
                        break;

                    case 0xd9:
                        switch (range)
                        {
                            case 0x00:
                                op1 = getModrm(modrm, OPSIZE.DWord);
                                instr = new FLoad(op1);
                                break;
                            case 0x02:
                            case 0x03:
                                op1 = getModrm(modrm, OPSIZE.DWord);
                                instr = new FStore(op1, (range == 3));
                                break;
                            case 0x04:
                                op1 = getModrm(modrm, OPSIZE.None);
                                instr = new FLoadEnvironment(op1);
                                break;
                            case 0x05:
                                op1 = getModrm(modrm, OPSIZE.Word);
                                instr = new FLoadControlWord(op1);
                                break;
                            case 0x06:
                                op1 = getModrm(modrm, OPSIZE.None);
                                instr = new FStoreEnvironment(op1);
                                break;
                            case 0x07:
                                op1 = getModrm(modrm, OPSIZE.Word);
                                instr = new FStoreControlWord(op1);
                                break;
                        }
                        break;                    

                    case 0xda:
                        op1 = getModrm(modrm, OPSIZE.DWord);
                        instr = Arithmetic87Ops(range, op1, null, true, false, true);
                        break;

                    case 0xdb:
                        switch (range)
                        {
                            case 0x00:
                                op1 = getModrm(modrm, OPSIZE.DWord);
                                instr = new FLoadInteger(op1);
                                break;
                            case 0x01:
                                op1 = getModrm(modrm, OPSIZE.DWord);
                                instr = new FStoreInteger(op1, true, true);
                                break;
                            case 0x02:
                                op1 = getModrm(modrm, OPSIZE.DWord);
                                instr = new FStoreInteger(op1, false, false);
                                break;
                            case 0x03:
                                op1 = getModrm(modrm, OPSIZE.DWord);
                                instr = new FStoreInteger(op1, true, false);
                                break;
                            case 0x05:
                                op1 = getModrm(modrm, OPSIZE.TByte);
                                instr = new FLoad(op1);
                                break;
                            case 0x07:
                                op1 = getModrm(modrm, OPSIZE.TByte);
                                instr = new FStore(op1, true);
                                break;
                        }
                        break;

                    case 0xdc:
                        op1 = getModrm(modrm, OPSIZE.QWord);
                        instr = Arithmetic87Ops(range, op1, null, false, false, true);
                        break;

                    case 0xdd:
                        switch (range)
                        {
                            case 0x00:
                                op1 = getModrm(modrm, OPSIZE.QWord);
                                instr = new FLoad(op1);
                                break;
                            case 0x01:
                                op1 = getModrm(modrm, OPSIZE.QWord);
                                instr = new FStoreInteger(op1, true, true);
                                break;
                            case 0x02:
                                op1 = getModrm(modrm, OPSIZE.QWord);
                                instr = new FStore(op1, false);
                                break;
                            case 0x03:
                                op1 = getModrm(modrm, OPSIZE.QWord);
                                instr = new FStore(op1, true);
                                break;
                            case 0x04:
                                op1 = getModrm(modrm, OPSIZE.None);
                                instr = new FRestoreState(op1);
                                break;
                            case 0x06:
                                op1 = getModrm(modrm, OPSIZE.None);
                                instr = new FSaveState(op1);
                                break;
                            case 0x07:
                                op1 = getModrm(modrm, OPSIZE.Word);
                                instr = new FStoreStatusWord(op1);
                                break;
                        }
                        break;


                    case 0xde:
                        op1 = getModrm(modrm, OPSIZE.Word);
                        instr = Arithmetic87Ops(range, op1, null, true, false, true);
                        break;

                    case 0xdf:
                        switch (range)
                        {
                            case 0x00:
                                op1 = getModrm(modrm, OPSIZE.Word);
                                instr = new FLoadInteger(op1);
                                break;
                            case 0x01:
                                op1 = getModrm(modrm, OPSIZE.Word);
                                instr = new FStoreInteger(op1, true, true);
                                break;
                            case 0x02:
                                op1 = getModrm(modrm, OPSIZE.Word);
                                instr = new FStoreInteger(op1, false, false);
                                break;
                            case 0x03:
                                op1 = getModrm(modrm, OPSIZE.Word);
                                instr = new FStoreInteger(op1, true, false);
                                break;
                            case 0x04:
                                op1 = getModrm(modrm, OPSIZE.TByte);
                                instr = new FLoadBCD(op1);
                                break;
                            case 0x05:
                                op1 = getModrm(modrm, OPSIZE.QWord);
                                instr = new FLoadInteger(op1);
                                break;
                            case 0x06:
                                op1 = getModrm(modrm, OPSIZE.TByte);
                                instr = new FStoreBCD(op1);
                                break;
                            case 0x07:
                                op1 = getModrm(modrm, OPSIZE.QWord);
                                instr = new FStoreInteger(op1, true, false);
                                break;
                        }
                        break;
                }
            }
            else       // modrm  = 0xc0 - 0xff
            {
                switch (b)
                {
                    case 0xd8:
                        if ((range == 2) || (range == 3)) 
                        {
                            op1 = Register87.getReg((int)rm); 
                            op2 = null;
                        }
                        else 
                        {
                            op1 = Register87.getReg(0);
                            op2 = Register87.getReg((int)rm); 
                        }
                        instr = Arithmetic87Ops(range, op1, op2, false, false, true);
                        break;

                    case 0xd9:
                        if (modrm == 0xd0)
                        {
                            instr = new FNoOp();
                        }
                        else if (modrm < 0xe0)
                        {
                            op1 = Register87.getReg((int)rm);
                            switch (range)
                            {
                                case 0x00:
                                    instr = new FLoad(op1);
                                    break;
                                case 0x01:
                                    instr = new FExchange(op1);
                                    break;
                                case 0x03:
                                    instr = new FStore(op1, (range == 3));
                                    break;
                            }
                        }
                        else
                        {
                            instr = NoArg87Ops(modrm - 0xe0);
                        }
                        break;

                    case 0xda:
                        if (modrm < 0xe0)
                        {
                            op1 = Register87.getReg(0);
                            op2 = Register87.getReg((int)rm);
                            switch (range)
                            {
                                case 0x00:
                                    instr = new FConditionalMove(op1, op2, FConditionalMove.CONDIT.MOVB);
                                    break;
                                case 0x01:
                                    instr = new FConditionalMove(op1, op2, FConditionalMove.CONDIT.MOVE);
                                    break;
                                case 0x02:
                                    instr = new FConditionalMove(op1, op2, FConditionalMove.CONDIT.MOVBE);
                                    break;
                                case 0x03:
                                    instr = new FConditionalMove(op1, op2, FConditionalMove.CONDIT.MOVU);
                                    break;
                            }

                        }
                        else if (modrm == 0xe9)
                        {
                            instr = new FCompareUnordered(null, null, true, true, false);
                        }
                        break;

                    case 0xdb:
                        if ((modrm >= 0xe0) && (modrm <= 0xe4))
                        {
                            switch (modrm)
                            {
                                case 0xe0:
                                    instr = new FNoOp(FNoOp.NOPTYPE.FENI);
                                    break;
                                case 0xe1:
                                    instr = new FNoOp(FNoOp.NOPTYPE.FDISI);
                                    break;
                                case 0xe2:
                                    instr = new FClearExceptions();
                                    break;
                                case 0xe3:
                                    instr = new FInitialize();
                                    break;
                                case 0xe4:
                                    instr = new FNoOp(FNoOp.NOPTYPE.FSETPM);
                                    break;
                            }
                        }
                        else
                        {
                            op1 = Register87.getReg(0);
                            op2 = Register87.getReg((int)rm);
                            switch (range)
                            {
                                case 0x00:
                                    instr = new FConditionalMove(op1, op2, FConditionalMove.CONDIT.MOVNB);
                                    break;
                                case 0x01:
                                    instr = new FConditionalMove(op1, op2, FConditionalMove.CONDIT.MOVNE);
                                    break;
                                case 0x02:
                                    instr = new FConditionalMove(op1, op2, FConditionalMove.CONDIT.MOVNBE);
                                    break;
                                case 0x03:
                                    instr = new FConditionalMove(op1, op2, FConditionalMove.CONDIT.MOVNU);
                                    break;
                                case 0x05:
                                    instr = new FCompareUnordered(op1, op2, false, false, true);
                                    break;
                                case 0x06:
                                    instr = new FCompare(op1, op2, false, false, true);
                                    break;
                            }
                        }
                        break;

                    case 0xdc:
                        if ((range == 2) || (range == 3))
                        {
                            op1 = Register87.getReg((int)rm);
                            op2 = null;
                        }
                        else
                        {
                            op1 = Register87.getReg((int)rm);
                            op2 = Register87.getReg(0);
                        }
                        instr = Arithmetic87Ops(range, op1, op2, false, false, false);
                        break;

                    case 0xdd:
                        if (modrm < 0xf0)
                        {
                            op1 = Register87.getReg((int)rm);
                            switch (range)
                            {
                                case 0x00:
                                    instr = new FFreeRegister(op1, false);
                                    break;
                                case 0x01:
                                    instr = new FExchange(op1);
                                    break;
                                case 0x02:
                                    instr = new FStore(op1, false);
                                    break;
                                case 0x03:
                                    instr = new FStore(op1, true);
                                    break;
                                case 0x04:
                                    instr = new FCompareUnordered(op1, null, false, false, false);
                                    break;
                                case 0x05:
                                    instr = new FCompareUnordered(op1, null, true, false, false);
                                    break;
                            }
                        }
                        break;

                    case 0xde:
                        if (range != 3)
                        {
                            if (range == 2)
                            {
                                op1 = Register87.getReg((int)rm);
                                op2 = null;
                            }
                            else
                            {
                                op1 = Register87.getReg((int)rm);
                                op2 = Register87.getReg(0);
                            }
                            instr = Arithmetic87Ops(range, op1, op2, false, true, false);
                        }
                        else if (modrm == 0xd9)
                        {
                            instr = new FCompare(null, null, true, true, false);
                        }
                        break;

                    case 0xdf:
                        if ((range != 4) && (range != 7))
                        {
                            if (range < 5)
                            {
                                op1 = Register87.getReg((int)rm);
                                op2 = null;
                            }
                            else
                            {
                                op1 = Register87.getReg(0);
                                op2 = Register87.getReg((int)rm);
                            }

                            switch (range)
                            {
                                case 0x00:
                                    instr = new FFreeRegister(op1, true);
                                    break;
                                case 0x01:
                                    instr = new FExchange(op1);
                                    break;
                                case 0x02:
                                case 0x03:
                                    instr = new FStore(op1, true);
                                    break;
                                case 0x05:
                                    instr = new FCompareUnordered(op1, op2, true, false, true);
                                    break;
                                case 0x06:
                                    instr = new FCompare(op1, op2, true, false, true);
                                    break;
                            }
                        }
                        else
                        {
                            if (modrm == 0xe0)
                            {
                                op1 = getReg(OPSIZE.Word, 0);
                                instr = new FStoreStatusWord(op1);
                            }
                        }
                        break;
                }
            }
            return instr;
        }

        public Instruction Comparison87Op(Operand op1, Operand op2, bool intop, bool pop, bool doublepop, bool flags)
        {
            Instruction instr = null;
            if (intop)
            {
                instr = new FCompareInt(op1, op2, pop);
            }
            else
            {
                instr = new FCompare(op1, op2, pop, doublepop, flags);
            }
            return instr;
        }

        public Instruction Arithmetic87Ops(uint b, Operand op1, Operand op2, bool intop, bool pop, bool rev)
        {
            Instruction instr = null;
            switch (b)
            {
                case 0x00:
                    instr = new FAdd(op1, op2, intop, pop);
                    break;
                case 0x01:
                    instr = new FMulitply(op1, op2, intop, pop);
                    break;
                case 0x02:
                    instr = Comparison87Op(op1, op2, intop, pop, false, false);
                    break;
                case 0x03:
                    instr = Comparison87Op(op1, op2, intop, true, false, false);
                    break;
                case 0x04:
                    instr = new FSubtract(op1, op2, intop, pop, !rev);
                    break;
                case 0x05:
                    instr = new FSubtract(op1, op2, intop, pop, rev);
                    break;
                case 0x06:
                    instr = new FDivide(op1, op2, intop, pop, !rev);
                    break;
                case 0x07:
                    instr = new FDivide(op1, op2, intop, pop, rev);
                    break;
            }
            return instr;
        }

        private Instruction NoArg87Ops(uint range)
        {
            Instruction instr = null;
            switch (range)
            {
                case 0x00:
                    instr = new FChangeSign();
                    break;
                case 0x01:
                    instr = new FAbsolute();
                    break;
                case 0x04:
                    instr = new FTest();
                    break;
                case 0x05:
                    instr = new FExamine();
                    break;
                case 0x08:
                    instr = new FLoadConstant(FLoadConstant.CONSTOP.ONE);
                    break;
                case 0x09:
                    instr = new FLoadConstant(FLoadConstant.CONSTOP.LOG210);
                    break;
                case 0x0a:
                    instr = new FLoadConstant(FLoadConstant.CONSTOP.LOG2E);
                    break;
                case 0x0b:
                    instr = new FLoadConstant(FLoadConstant.CONSTOP.PI);
                    break;
                case 0x0c:
                    instr = new FLoadConstant(FLoadConstant.CONSTOP.LOG102);
                    break;
                case 0x0d:
                    instr = new FLoadConstant(FLoadConstant.CONSTOP.LOGE2);
                    break;
                case 0x0e:
                    instr = new FLoadConstant(FLoadConstant.CONSTOP.ZERO);
                    break;
                case 0x10:
                    instr = new F2XM1();
                    break;
                case 0x11:
                    instr = new FYL2X();
                    break;
                case 0x12:
                    instr = new FTangent();
                    break;
                case 0x13:
                    instr = new FArcTangent();
                    break;
                case 0x14:
                    instr = new FExtract();
                    break;
                case 0x15:
                    instr = new FRemainder(FRemainder.MODE.ROUND1);
                    break;
                case 0x16:
                    instr = new FDecrement();
                    break;
                case 0x17:
                    instr = new FIncrement();
                    break;
                case 0x18:
                    instr = new FRemainder(FRemainder.MODE.ROUND0);
                    break;
                case 0x19:
                    instr = new FYL2XP1();
                    break;
                case 0x1a:
                    instr = new FSquareRoot();
                    break;
                case 0x1b:
                    instr = new FSineCosine();
                    break;
                case 0x1c:
                    instr = new FRound();
                    break;
                case 0x1d:
                    instr = new FScale();
                    break;
                case 0x1e:
                    instr = new FSine();
                    break;
                case 0x1f:
                    instr = new FCosine();
                    break;
            }
            return instr;
        }

//- two byte instructions -----------------------------------------------------

        public Instruction op0f(uint b)
        {
            Instruction instr = null;
            if ((b >= 0x00) && (b <= 0x0f))
            {
               instr = op0f0x(b);
            }
            else if ((b >= 0x10) && (b <= 0x1f))
            {
                instr = op0f1x(b);
            }
            else if ((b >= 0x20) && (b <= 0x2f))
            {
                instr = op0f2x(b);
            }
            else if ((b >= 0x30) && (b <= 0x3f))
            {
                instr = op0f3x(b);
            }
            else if ((b >= 0x40) && (b <= 0x4f))
            {
                instr = op0f4x(b);
            }
            else if ((b >= 0x50) && (b <= 0x5f))
            {
                instr = op0f5x(b);
            }
            else if ((b >= 0x60) && (b <= 0x6f))
            {
                instr = op0f6x(b);
            }
            else if ((b >= 0x70) && (b <= 0x7f))
            {
                instr = op0f7x(b);
            }
            else if ((b >= 0x80) && (b <= 0x8f))
            {
                instr = op0f8x(b);
            }
            else if ((b >= 0x90) && (b <= 0x9f))
            {
                instr = op0f9x(b);
            }
            else if ((b >= 0xa0) && (b <= 0xaf))
            {
                instr = op0fax(b);
            }
            else if ((b >= 0xb0) && (b <= 0xbf))
            {
                instr = op0fbx(b);
            }
            else if ((b >= 0xc0) && (b <= 0xcf))
            {
                instr = op0fcx(b);
            }
            else if ((b >= 0xd0) && (b <= 0xdf))
            {
                instr = op0fdx(b);
            }
            else if ((b >= 0xe0) && (b <= 0xef))
            {
                instr = op0fex(b);
            }
            else if ((b >= 0xf0) && (b <= 0xff))
            {
                instr = op0ffx(b);
            }
            return instr;
        }

        public Instruction  op0f0x(uint b)
        {
            Instruction instr = null;
            uint modrm = 0;
            uint mode = 0;
            uint range = 0;
            OPSIZE size;
            switch (b)
            {
                //group 6 instructions
                case 0x00:
                    modrm = getNextByte();
                    mode = (modrm / 0x40) % 0x04;                        
                    range = (modrm % 0x40) / 0x08;
                    size = ((mode == 3) && (range < 2)) ? OPSIZE.DWord : OPSIZE.Word;
                    op1 = getModrm(modrm, size);
                    switch(range) {
                        case 0:
                            instr = new StoreDescriptor(op1, StoreDescriptor.MODE.SLDT);
                            break;
                        case 1:
                            instr = new StoreTaskRegister(op1);
                            break;
                        case 2:
                            instr = new LoadDescriptor(op1, LoadDescriptor.MODE.LLDT);
                            break;
                        case 3:
                            instr = new LoadTaskRegister(op1);
                            break;
                        case 4:
                            instr = new VerifySegment(op1, VerifySegment.MODE.VERR);
                            break;
                        case 5:
                            instr = new VerifySegment(op1, VerifySegment.MODE.VERW);
                            break;
                    }
                    break;

                //group 7 instructions
                case 0x01:
                    modrm = getNextByte();
                    mode = (modrm / 0x40) % 0x04;                        
                    range = (modrm % 0x40) / 0x08;
                    if (mode < 3) {
                        size = (range <= 3) ? OPSIZE.FWord : ((range % 2 == 0) ? OPSIZE.Word : OPSIZE.None);
                        op1 = getModrm(modrm, size);
                        switch (range)
                        {
                            case 0:
                                instr = new StoreDescriptor(op1, StoreDescriptor.MODE.SGDT);
                                break;
                            case 1:
                                instr = new StoreDescriptor(op1, StoreDescriptor.MODE.SIDT);
                                break;
                            case 2:
                                instr = new LoadDescriptor(op1, LoadDescriptor.MODE.LGDT);
                                break;
                            case 3:
                                instr = new LoadDescriptor(op1, LoadDescriptor.MODE.LIDT);
                                break;
                            case 4:
                                instr = new StoreMachineStatusWord(op1);
                                break;
                            case 6:
                                instr = new LoadSMachinetatusWord(op1);
                                break;
                            case 7:
                                instr = new InvalidateTLB(op1);
                                break;
                        }

                    } else {

                        //group 7 0xc1 - 0xF9 instructions not implemented yet
                    }
                    break;

                case 0x02:
                    modrm = getNextByte();
                    op1 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);
                    op2 = getModrm(modrm, OPSIZE.DWord);
                    instr = new LoadAccessRights(op1, op2);
                    break;

                case 0x03:
                    modrm = getNextByte();
                    op1 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);
                    op2 = getModrm(modrm, OPSIZE.DWord);
                    instr = new LoadSegementLimit(op1, op2);
                    break;

                case 0x05:
                    instr = new SystemCall(SystemCall.MODE.SYSCALL);
                    break;

                case 0x06:
                    instr = new ClearTaskFlag();
                    break;

                case 0x07:
                    instr = new SystemRet(SystemRet.MODE.SYSRET);
                    break;

                case 0x08:
                    instr = new InvalidateCache(InvalidateCache.MODE.INVD);
                    break;

                case 0x09:
                    instr = new InvalidateCache(InvalidateCache.MODE.WBINVD);
                    break;

                case 0x0b:
                    instr = new UndefinedOp(2);
                    break;
            }
                        return instr;
        }

        public Instruction op0f1x(uint b)
        {
            Instruction instr = null;
            uint modrm = 0;
            uint mode = 0;
            switch (b)
            {
                case 0x10:
                    modrm = getNextByte();
                    op1 = RegisterXMM.getReg((int)((modrm % 0x40) / 0x08));
                    op2 = getModrm(modrm, OPSIZE.XMM);
                    instr = new SSEMovePacked(op1, op2, false);
                    break;

                case 0x11:
                    modrm = getNextByte();
                    op1 = getModrm(modrm, OPSIZE.XMM);
                    op2 = RegisterXMM.getReg((int)((modrm % 0x40) / 0x08));
                    instr = new SSEMovePacked(op1, op2, false);
                    break;

                case 0x12:
                    modrm = getNextByte();
                    mode = (modrm / 0x40) % 0x04;
                    op1 = RegisterXMM.getReg((int)((modrm % 0x40) / 0x08));
                    op2 = getModrm(modrm, (mode < 3) ? OPSIZE.QWord : OPSIZE.XMM);
                    instr = new SSEMoveLow(op1, op2, (mode == 3));
                    break;

                case 0x13:
                    modrm = getNextByte();
                    mode = (modrm / 0x40) % 0x04;
                    if (mode < 3)
                    {
                        op1 = getModrm(modrm, OPSIZE.QWord);
                        op2 = RegisterXMM.getReg((int)((modrm % 0x40) / 0x08));
                        instr = new SSEMoveLow(op1, op2, false);
                    }
                    break;

                case 0x14:
                case 0x15:
                    modrm = getNextByte();
                    op1 = RegisterXMM.getReg((int)((modrm % 0x40) / 0x08));
                    op2 = getModrm(modrm, OPSIZE.XMM);
                    instr = new SSEUnpack(op1, op2, (b == 0x14) ? SSEUnpack.MODE.LOW : SSEUnpack.MODE.HIGH);
                    break;

                case 0x16:
                    modrm = getNextByte();
                    mode = (modrm / 0x40) % 0x04;
                    op1 = RegisterXMM.getReg((int)((modrm % 0x40) / 0x08));
                    op2 = getModrm(modrm, (mode < 3) ? OPSIZE.QWord : OPSIZE.XMM);
                    instr = new SSEMoveHigh(op1, op2, (mode == 3));
                    break;

                case 0x17:
                    modrm = getNextByte();
                    mode = (modrm / 0x40) % 0x04;
                    if (mode < 3)
                    {
                        op1 = getModrm(modrm, OPSIZE.QWord);
                        op2 = RegisterXMM.getReg((int)((modrm % 0x40) / 0x08));
                        instr = new SSEMoveHigh(op1, op2, false);
                    }
                    break;

                //group 16 instructions
                case 0x18:
                    modrm = getNextByte();
                    mode = (modrm / 0x40) % 0x04;
                    uint range = (modrm % 0x40) / 0x08;
                    if ((mode < 3) && (range <= 3))    
                    {
                        op1 = getModrm(modrm, OPSIZE.None);
                        instr = new SSEPrefetchData(op1, (SSEPrefetchData.MODE)range);
                    }
                    break;
            }
            return instr;
        }

        public Instruction op0f2x(uint b)
        {
            Instruction instr = null;
            uint modrm = 0;
            uint mode = 0;
            switch (b)
            {
                case 0x20:
                    modrm = getNextByte();
                    op1 = Register32.getReg((int)(modrm % 0x08));
                    op2 = RegisterCR.getReg((int)((modrm % 0x40) / 0x08));
                    instr = new Move(op1, op2);
                    break;

                case 0x21:
                    modrm = getNextByte();
                    op1 = Register32.getReg((int)(modrm % 0x08));
                    op2 = RegisterDR.getReg((int)((modrm % 0x40) / 0x08));
                    instr = new Move(op1, op2);
                    break;

                case 0x22:
                    modrm = getNextByte();
                    op1 = RegisterCR.getReg((int)((modrm % 0x40) / 0x08));
                    op2 = Register32.getReg((int)(modrm % 0x08));
                    instr = new Move(op1, op2);
                    break;

                case 0x23:
                    modrm = getNextByte();
                    op1 = RegisterDR.getReg((int)((modrm % 0x40) / 0x08));
                    op2 = Register32.getReg((int)(modrm % 0x08));
                    instr = new Move(op1, op2);
                    break;

                case 0x28:
                    modrm = getNextByte();
                    op1 = RegisterXMM.getReg((int)((modrm % 0x40) / 0x08));
                    op2 = getModrm(modrm, OPSIZE.XMM);
                    instr = new SSEMovePacked(op1, op2, true);
                    break;

                case 0x29:
                    modrm = getNextByte();
                    op1 = getModrm(modrm, OPSIZE.XMM);
                    op2 = RegisterXMM.getReg((int)((modrm % 0x40) / 0x08));
                    instr = new SSEMovePacked(op1, op2, true);
                    break;

                case 0x2a:
                    modrm = getNextByte();
                    op1 = RegisterXMM.getReg((int)((modrm % 0x40) / 0x08));
                    op2 = getModrm(modrm, OPSIZE.MM);
                    instr = new SSEConvertFromInt(op1, op2, true);
                    break;

                case 0x2b:
                    modrm = getNextByte();
                    mode = (modrm / 0x40) % 0x04;
                    if (mode < 3)
                    {
                        op1 = getModrm(modrm, OPSIZE.XMM);
                        op2 = RegisterXMM.getReg((int)((modrm % 0x40) / 0x08));
                        instr = new SSEStorePacked(op1, op2);
                    }
                    break;

                case 0x2c:
                case 0x2d:
                    modrm = getNextByte();
                    mode = (modrm / 0x40) % 0x04;
                    op1 = RegisterMM.getReg((int)((modrm % 0x40) / 0x08));
                    op2 = getModrm(modrm, (mode < 3) ? OPSIZE.MM : OPSIZE.XMM);
                    instr = new SSEConvertPackedToInt(op1, op2, (b == 0x02c) ? true : false);
                    break;

                case 0x2e:
                case 0x2f:
                    modrm = getNextByte();
                    mode = (modrm / 0x40) % 0x04;
                    op1 = RegisterXMM.getReg((int)((modrm % 0x40) / 0x08));
                    op2 = getModrm(modrm, (mode < 3) ? OPSIZE.DWord : OPSIZE.XMM);
                    instr = new SSECompareSetFlags(op1, op2, (b == 0x2e) ? true : false);
                    break;
            }
            return instr;
        }

        public Instruction  op0f3x(uint b)
        {
            Instruction instr = null;
            switch (b)
            {
                case 0x30:
                    instr = new WriteModelSpecReg();
                    break;

                case 0x31:
                    instr = new ReadCounters(ReadCounters.MODE.TIMESTAMP);
                    break;

                case 0x32:
                    instr = new ReadModelSpecReg();
                    break;

                case 0x33:
                    instr = new ReadCounters(ReadCounters.MODE.PERFORMANCE);
                    break;

                case 0x34:
                    instr = new SystemCall(SystemCall.MODE.SYSENTER);
                    break;

                case 0x35:
                    instr = new SystemRet(SystemRet.MODE.SYSEXIT);
                    break;

                    //0x0f87 instruction not implemented yet
            }
            return instr;
        }

        public Instruction  op0f4x(uint b)
        {
            uint modrm = getNextByte();
            ConditionalMove.CONDIT condit = (ConditionalMove.CONDIT)(b % 0x10);
            op1 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);
            op2 = getModrm(modrm, OPSIZE.DWord);
            return new ConditionalMove(op1, op2, condit);
        }

        public Instruction  op0f5x(uint b)
        {
            Instruction instr = null;
            uint modrm = 0;
            uint mode = 0;
            if (b == 0x50)
            {
                modrm = getNextByte();
                mode = (modrm / 0x40) % 0x04;
                if (mode == 3)
                {
                    op1 = Register32.getReg((int)((modrm % 0x40) / 0x08));
                    op2 = getModrm(modrm, OPSIZE.XMM);
                    instr = new SSEExtract(op1, op2);
                }
            }
            else if (b == 0x5a)
            {
                modrm = getNextByte();
                mode = (modrm / 0x40) % 0x04;
                op1 = RegisterXMM.getReg((int)((modrm % 0x40) / 0x08));
                op2 = getModrm(modrm, (mode != 3) ? OPSIZE.MM : OPSIZE.XMM);
                instr = new SSE2ConvertPrecision(op1, op2, SSE2ConvertPrecision.DIR.SINGLETODOUBLE, true);
            }
            else
            {
                modrm = getNextByte();
                op1 = RegisterXMM.getReg((int)((modrm % 0x40) / 0x08));
                op2 = getModrm(modrm, OPSIZE.XMM);
                switch (b)
                {
                    case 0x51:
                        instr = new SSESqrt(op1, op2, true);
                        break;

                    case 0x52:
                        instr = new SSEReciprocalSqrt(op1, op2, true);
                        break;

                    case 0x53:
                        instr = new SSEReciprocal(op1, op2, true);
                        break;

                    case 0x54:
                        instr = new SSEAnd(op1, op2);
                        break;

                    case 0x55:
                        instr = new SSENand(op1, op2);
                        break;

                    case 0x56:
                        instr = new SSEOr(op1, op2);
                        break;

                    case 0x57:
                        instr = new SSEXor(op1, op2);
                        break;

                    case 0x58:
                        instr = new SSEAdd(op1, op2, true);
                        break;

                    case 0x59:
                        instr = new SSEMult(op1, op2, true);
                        break;

                    case 0x5b:
                        instr = new SSE2ConvertSingle(op1, op2, SSE2ConvertSingle.DIR.DOUBLETOSINGLE, false);
                        break;

                    case 0x5c:
                        instr = new SSESubtract(op1, op2, true);
                        break;

                    case 0x5d:
                        instr = new SSEMin(op1, op2, true);
                        break;

                    case 0x5e:
                        instr = new SSEDivide(op1, op2, true);
                        break;

                    case 0x5f:
                        instr = new SSEMax(op1, op2, true);
                        break;
                }
            }
            return instr;
        }

        public Instruction  op0f6x(uint b)
        {
            Instruction instr = null;
            uint modrm = 0;
            uint mode = 0;
            if ((b >= 0x60 && b <= 0x62) || (b >= 0x68 && b <= 0x6a))
            {
                modrm = getNextByte();
                mode = (modrm / 0x40) % 0x04;
                op1 = RegisterMM.getReg((int)((modrm % 0x40) / 0x08));
                op2 = getModrm(modrm, ((b <= 0x62) && (mode != 3)) ? OPSIZE.DWord : OPSIZE.MM);
                instr = new MMXUnpack(op1, op2, (MMXUnpack.MODE)((b % 8) % 3), (b >= 0x68));
            }
            else if ((b == 0x63) || (b == 0x67) || (b == 0x6b))
            {
                modrm = getNextByte();
                mode = (modrm / 0x40) % 0x04;
                op1 = RegisterMM.getReg((int)((modrm % 0x40) / 0x08));
                op2 = getModrm(modrm, OPSIZE.MM);
                instr = new MMXPack(op1, op2, (b <= 0x67) ? MMXPack.MODE.WB : MMXPack.MODE.DW, (b != 0x67));
            }
            else if (b >= 0x64 && b <= 0x66)
            {
                modrm = getNextByte();
                op1 = RegisterMM.getReg((int)((modrm % 0x40) / 0x08));
                op2 = getModrm(modrm, OPSIZE.MM);
                instr = new MMXCompGtrThn(op1, op2, (MMXCompGtrThn.MODE)(b - 0x64));
            }
            else
            {
                modrm = getNextByte();
                op1 = RegisterMM.getReg((int)((modrm % 0x40) / 0x08));
                op2 = getModrm(modrm, (b == 0x6e) ? OPSIZE.DWord : OPSIZE.MM);
                instr = new MMXMoveWord(op1, op2, (b == 0x6e) ? MMXMoveWord.MODE.DOUBLE : MMXMoveWord.MODE.QUAD);
            }
            return instr;
        }

        public Instruction  op0f7x(uint b)
        {
            Instruction instr = null;
            uint modrm = 0;
            uint mode = 0;
            if (b >= 0x74 && b <= 0x76)
            {
                modrm = getNextByte();
                op1 = RegisterMM.getReg((int)((modrm % 0x40) / 0x08));
                op2 = getModrm(modrm, OPSIZE.MM);
                instr = new MMXCompEqual(op1, op2, (MMXCompEqual.MODE)(b - 0x74));
            }
            else if (b == 0x77)
            {
                instr = new MMXEmptyState();
            }
            else if (b == 0x7e || b == 0x7f)
            {
                modrm = getNextByte();
                op1 = getModrm(modrm, (b == 0x7e) ? OPSIZE.DWord : OPSIZE.MM);
                op2 = RegisterMM.getReg((int)((modrm % 0x40) / 0x08));
                instr = new MMXMoveWord(op1, op2, (b == 0x7e) ? MMXMoveWord.MODE.DOUBLE : MMXMoveWord.MODE.QUAD);
            }
            return instr;
        }

        public Instruction  op0f8x(uint b)
        {
            JumpConditional.CONDIT condit = (JumpConditional.CONDIT)(b % 0x10);
            op1 = rel32();                                                               //Jz
            return new JumpConditional(op1, condit);
        }

        public Instruction op0f9x(uint b)
        {
            uint modrm = getNextByte();
            op1 = getModrm(modrm, OPSIZE.Byte);
            SetByte.CONDIT condit = (SetByte.CONDIT)(b % 0x10);
            return new SetByte(op1, condit);
        }

        public Instruction op0fax(uint b)
        {
            Instruction instr = null;
            uint modrm = 0;
            switch (b)
            {
                case 0xa0:
                case 0xa8:
                    op1 = (b == 0xa0) ? Segment.FS : Segment.GS;
                    instr = new Push(op1);
                    break;

                case 0xa1:
                case 0xa9:
                    op1 = (b == 0xa1) ? Segment.FS : Segment.GS;
                    instr = new Pop(op1);
                    break;

                case 0xa2:
                    instr = new CpuId();
                    break;

                case 0xa3:
                case 0xab:
                    modrm = getNextByte();
                    op1 = getModrm(modrm, OPSIZE.DWord);
                    op2 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);
                    instr = new BitTest(op1, op2, ((b == 0xa3) ? BitTest.MODE.BT : BitTest.MODE.BTS));
                    break;

                case 0xa4:
                case 0xac:
                case 0xa5:
                case 0xad:
                    modrm = getNextByte();
                    op1 = getModrm(modrm, OPSIZE.DWord);
                    op2 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);
                    op3 = (b % 2 == 0) ? getImm(OPSIZE.Byte) : Register8.CL;
                    instr = new DoublePrecShift(op1, op2, op3, ((b <= 0xa5) ? DoublePrecShift.MODE.LEFT : DoublePrecShift.MODE.RIGHT));
                    break;


                case 0xaa:
                    instr = new ResumeFromSysMgt();
                    break;

                case 0xae:
                    modrm = getNextByte();
                    uint mode = (modrm / 0x40) % 0x04;
                    uint range = (modrm % 0x40) / 0x08;
                    if ((mode < 3) && ((range <= 3) || (range == 7)))
                    {
                        switch (range)
                        {
                            case 0:
                                op1 = getModrm(modrm, OPSIZE.None);
                                instr = new StoreMMXState(op1);
                                break;
                            case 1:
                                op1 = getModrm(modrm, OPSIZE.None);
                                instr = new RestoreMMXState(op1);
                                break;
                            case 2:
                                op1 = getModrm(modrm, OPSIZE.DWord);
                                instr = new SSELoadState(op1);
                                break;
                            case 3:
                                op1 = getModrm(modrm, OPSIZE.DWord);
                                instr = new SSEStoreState(op1);
                                break;
                            case 7:
                                op1 = getModrm(modrm, OPSIZE.None);
                                instr = new CacheFlush(op1, false);
                                break;

                        }
                    }
                    else if (modrm == 0xe8)
                    {
                        instr = new SSE2LoadFence();
                    }
                    else if (modrm == 0xf0)
                    {
                        instr = new SSE2MemoryFence();
                    }
                    else if (modrm == 0xf8)
                    {
                        instr = new SSEStoreFence();
                    }
                    break;

                case 0xaf:
                    modrm = getNextByte();
                    op1 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);
                    op2 = getModrm(modrm, OPSIZE.DWord);
                    instr = new IntMultiply(op1, op2);
                    break;
            }
            return instr;
        }

        public Instruction op0fbx(uint b)
        {
            Instruction instr = null;
            uint modrm = 0;
            uint mode = 0;
            switch (b)
            {
                case 0xb0:
                case 0xb1:
                    modrm = getNextByte();
                    OPSIZE size = (b == 0xb0) ? OPSIZE.Byte : OPSIZE.DWord;
                    op1 = getModrm(modrm, size);
                    op2 = getReg(size, (modrm % 0x40) / 0x08);
                    instr = new CompareExchange(op1, op2, false);
                    break;

                case 0xb2:
                case 0xb4:
                case 0xb5:
                    modrm = getNextByte();
                    mode = (modrm / 0x40) % 0x04;
                    if (mode < 3)
                    {
                        op1 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);
                        op2 = getModrm(modrm, OPSIZE.FWord);
                        instr = new LoadFarPointer(op1, op2,
                            ((b == 0xb2) ? LoadFarPointer.SEG.SS : (b == 0xb4) ? LoadFarPointer.SEG.FS : LoadFarPointer.SEG.GS));
                    }
                    break;

                case 0xb6:
                case 0xb7:
                case 0xbe:
                case 0xbf:
                    modrm = getNextByte();
                    op1 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);
                    op2 = getModrm(modrm, (((b % 2) == 0) ? OPSIZE.Byte : OPSIZE.Word));
                    instr = new MoveExtend(op1, op2, ((b < 0xbe) ? MoveExtend.MODE.ZERO : MoveExtend.MODE.SIGN));
                    break;

                //group 8 instructions
                case 0xba:
                    modrm = getNextByte();
                    mode = (modrm / 0x40) % 0x04;
                    uint range = (modrm % 0x40) / 0x08;
                    if (range >= 4  && range <= 8)
                    {
                        op1 = getModrm(modrm, OPSIZE.DWord);
                        op2 = getImm(OPSIZE.Byte);
                        instr = new BitTest(op1, op2, (BitTest.MODE)(range - 4));
                    }
                    break;

                case 0xb3:
                case 0xbb:
                    modrm = getNextByte();
                    op1 = getModrm(modrm, OPSIZE.DWord);
                    op2 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);
                    instr = new BitTest(op1, op2, (b == 0xb3) ? BitTest.MODE.BTR : BitTest.MODE.BTC);
                    break;

                case 0xbc:
                case 0xbd:
                    modrm = getNextByte();
                    op1 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);
                    op2 = getModrm(modrm, OPSIZE.DWord);
                    instr = new BitScan(op1, op2, (b == 0xbc) ? BitScan.MODE.BSF : BitScan.MODE.BSR);
                    break;
            }
            return instr;
        }

        public Instruction op0fcx(uint b)
        {
            Instruction instr = null;
            uint modrm = 0;
            uint mode = 0;
            if (b <= 0xc7)
            {
                switch (b)
                {
                    case 0xc0:
                        modrm = getNextByte();
                        op1 = getModrm(modrm, OPSIZE.Byte);                     //Eb
                        op2 = getReg(OPSIZE.Byte, (modrm % 0x40) / 0x08);       //Gb
                        instr = new ExchangeAdd(op1, op2);
                        break;

                    case 0xc1:
                        modrm = getNextByte();
                        op1 = getModrm(modrm, OPSIZE.DWord);                    //Ev
                        op2 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);      //Gv
                        instr = new ExchangeAdd(op1, op2);
                        break;

                    case 0xc2:
                        modrm = getNextByte();
                        op1 = RegisterXMM.getReg((int)((modrm % 0x40) / 0x08));
                        op2 = getModrm(modrm, OPSIZE.XMM);
                        op3 = getImm(OPSIZE.Byte);
                        instr = new SSECompare(op1, op2, op3, true);
                        break;

                    case 0xc3:
                        modrm = getNextByte();
                        mode = (modrm / 0x40) % 0x04;
                        if (mode < 3)
                        {
                            op1 = getModrm(modrm, OPSIZE.DWord);                    //Ev
                            op2 = getReg(OPSIZE.DWord, (modrm % 0x40) / 0x08);      //Gv
                            instr = new SSE2StoreInt(op1, op2);
                        }
                        break;

                    case 0xc6:
                        modrm = getNextByte();
                        op1 = RegisterXMM.getReg((int)((modrm % 0x40) / 0x08));
                        op2 = getModrm(modrm, OPSIZE.XMM);
                        op3 = getImm(OPSIZE.Byte);
                        instr = new SSEShuffle(op1, op2, op3);
                        break;

                    case 0xc7:
                        modrm = getNextByte();
                        mode = (modrm / 0x40) % 0x04;
                        uint range = (modrm % 0x40) / 0x08;
                        if ((range == 1) && (mode < 3))
                        {
                            op1 = getModrm(modrm, OPSIZE.QWord);
                            instr = new CompareExchange(op1, null, true);
                        }
                        break;
                }
            }
            else
            {
                op1 = Register32.getReg((int)(b - 0xc8));
                instr = new ByteSwap(op1);
            }
            return instr;
        }

        public Instruction op0fdx(uint b)
        {
            Instruction instr = null;
            uint modrm = 0;
            uint mode = 0;
            if ((b >= 0xd1 && b <= 0xd5) || (b == 0xd8) || (b == 0xd9) || (b >= 0xdb && b <= 0xdd) || (b == 0xdf))
            {
                modrm = getNextByte();
                mode = (modrm / 0x40) % 0x04;
                op1 = RegisterMM.getReg((int)((modrm % 0x40) / 0x08));
                op2 = getModrm(modrm, OPSIZE.MM);
                switch (b)
                {
                    case 0xd1:
                    case 0xd2:
                    case 0xd3:
                        instr = new MMXShift(op1, op2, (MMXShift.SIZE)(b - 0xd1), MMXShift.DIR.RIGHT, false);
                        break;

                    case 0xd4:
                        instr = new SSE2Add128(op1, op2);
                        break;

                    case 0xd5:
                        instr = new MMXMult(op1, op2, MMXMult.MODE.LOW);
                        break;

                    case 0xd8:
                    case 0xd9:
                        instr = new MMXSubtract(op1, op2, (MMXSubtract.SIZE)(b - 0xd8), true, false);
                        break;

                    case 0xdb:
                        instr = new MMXAnd(op1, op2);
                        break;

                    case 0xdc:
                    case 0xdd:
                        instr = new MMXAdd(op1, op2, (MMXAdd.SIZE)(b - 0xdc), true, false);
                        break;

                    case 0xdf:
                        instr = new MMXAddNot(op1, op2);
                        break;
                }
            }
            return instr;
        }

        public Instruction op0fex(uint b)
        {
            Instruction instr = null;
            uint modrm = 0;
            uint mode = 0;
            if (b == 0xe1 || b == 0xe2 || b == 0xe5 || b == 0xe8 || b == 0xe9 || b == 0xeb || b == 0xec || b == 0xed || b == 0xef)
            {
                modrm = getNextByte();
                op1 = RegisterMM.getReg((int)((modrm % 0x40) / 0x08));
                op2 = getModrm(modrm, OPSIZE.MM);
                switch (b)
                {
                    case 0xe1:
                    case 0xe2:
                        instr = new MMXShift(op1, op2, (MMXShift.SIZE)(b - 0xe1), MMXShift.DIR.RIGHT, true);
                        break;

                    case 0xe5:
                        instr = new MMXMult(op1, op2, MMXMult.MODE.HIGH);
                        break;

                    case 0xe8:
                    case 0xe9:
                        instr = new MMXSubtract(op1, op2, (MMXSubtract.SIZE)(b - 0xe8), true, true);
                        break;

                    case 0xeb:
                        instr = new MMXOr(op1, op2);
                        break;

                    case 0xec:
                    case 0xed:
                        instr = new MMXAdd(op1, op2, (MMXAdd.SIZE)(b - 0xec), true, true);
                        break;

                    case 0xef:
                        instr = new MMXXor(op1, op2);
                        break;
                }
            }
            else if (b == 0xe7)
            {
                modrm = getNextByte();
                mode = (modrm / 0x40) % 0x04;
                if (mode < 3)
                {
                    op1 = getModrm(modrm, OPSIZE.MM);
                    op2 = RegisterMM.getReg((int)((modrm % 0x40) / 0x08));
                    instr = new SSEStoreQuad(op1, op2);
                }
            }
            return instr;
        }

        public Instruction op0ffx(uint b)
        {
            Instruction instr = null;
            uint modrm = 0;
            uint mode = 0;
            if (b >= 0xf1 && b < 0xff)
            {
                modrm = getNextByte();
                mode = (modrm / 0x40) % 0x04;
                op1 = RegisterMM.getReg((int)((modrm % 0x40) / 0x08));
                op2 = getModrm(modrm, OPSIZE.MM);
                switch (b)
                {
                    case 0xf1:
                    case 0xf2:
                    case 0xf3:
                        instr = new MMXShift(op1, op2, (MMXShift.SIZE)(b - 0xf1), MMXShift.DIR.LEFT, false);
                        break;

                    case 0xf4:
                        instr = new SSE2Mult128(op1, op2);
                        break;

                    case 0xf5:
                        instr = new MMXMultAdd(op1, op2);
                        break;

                    case 0xf7:
                        instr = new SSEStoreQuadBytes(op1, op2);
                        break;
                        
                    case 0xf8:
                    case 0xf9:
                    case 0xfa:
                        instr = new MMXSubtract(op1, op2, (MMXSubtract.SIZE)(b - 0xf8), false, false);
                        break;

                        case 0xfb:
                        instr = new SSE2Subtract128(op1, op2);
                        break;
                        
                    case 0xfc:
                    case 0xfd:
                    case 0xfe:
                        instr = new MMXAdd(op1, op2, (MMXAdd.SIZE)(b - 0xfc), false, false);
                        break;
                }
            }
            else if (b == 0xff)
            {
                instr = instr = new UndefinedOp(0);
            }
            return instr;
        }

//- registers -----------------------------------------------------------------

        public Register getReg(OPSIZE rtype, uint reg)
        {
            if (operandSizeOverride && (rtype == OPSIZE.DWord)) rtype = OPSIZE.Word;
            if (useModrm32 && (rtype == OPSIZE.Word)) rtype = OPSIZE.DWord;

            Register result = null;
            switch (rtype)
            {
                case OPSIZE.Byte:
                    result = Register8.getReg((int)reg);
                    break;
                case OPSIZE.Word:
                    result = Register16.getReg((int)reg);
                    break;
                case OPSIZE.DWord:
                case OPSIZE.FWord:
                    result = Register32.getReg((int)reg);
                    break;
                case OPSIZE.MM:
                    result = RegisterMM.getReg((int)reg);
                    break;
                case OPSIZE.XMM:
                    result = RegisterXMM.getReg((int)reg);
                    break;
            }

            return result;
        }

//- immediates ----------------------------------------------------------------

        public uint getNextWord()
        {
            uint b = getNextByte();
            uint a = getNextByte();
            return (a * 256 + b);
        }

        public uint getNextDWord()
        {
            uint d = getNextByte();
            uint c = getNextByte();
            uint b = getNextByte();
            uint a = getNextByte();
            return (((a * 256 + b) * 256 + c) * 256 + d);            
        }

        public Operand getImm(OPSIZE size)
        {
            if (operandSizeOverride && (size == OPSIZE.DWord)) size = OPSIZE.Word;

            uint val = 0;
            switch (size)
            {
                case OPSIZE.Byte:
                    val = getNextByte();
                    break;
                case OPSIZE.SignedByte:
                    val = getNextByte();
                    break;
                case OPSIZE.Word:
                    val = getNextWord();
                    break;
                case OPSIZE.DWord:
                    val = getNextDWord();
                    break;
            }

            Immediate imm = new Immediate(val, size);

            return imm;
        }

        public uint getOfs(OPSIZE size)
        {
            uint result = 0;
            if (size == OPSIZE.Byte) result = ofs8();
            if (size == OPSIZE.DWord) result = ofs32();
            return result;
        }

        public uint ofs8()   
        {
            return getNextByte();            
        }

        public uint ofs32()
        {
            return getNextDWord();
        }

//- addresses ----------------------------------------------------------------

        public Relative rel8()
        {
            uint b = getNextByte();
            uint target = b + codeaddr;
            if (b >= 0x80)
            {
                target -= 0x100;
            }

            Relative addr = new Relative(target, b, OPSIZE.Byte);
            return addr;
        }

        public Relative rel32()
        {
            uint dw = getNextDWord();
            uint target = dw + codeaddr;
            Relative addr = new Relative(target, dw, OPSIZE.DWord);
            return addr;
        }

        public uint addr32()          //mem addrs aren't prefixed with '0'
        {
            uint sum = getNextDWord();
            return sum;
        }

        public Absolute absolute()   //6 bytes -> ssss:aaaaaaaa
        {
            uint addr = getNextDWord();
            uint seq = getNextWord();
            Absolute abs = new Absolute(seq, addr);
            return abs;
        }
    }
}
