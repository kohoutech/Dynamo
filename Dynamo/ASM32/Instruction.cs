/* ----------------------------------------------------------------------------
Origami Asm32 Library
Copyright (C) 1998-2020  George E Greaney

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

//base instruction class

namespace Origami.Asm32
{
    public class Instruction
    {
        public enum LOOPPREFIX { REP, REPNE, None };

        public bool lockprefix;

        public Operand op1;
        public Operand op2;
        public Operand op3;
        public int opcount;

        public uint addr;
        public List<byte> bytes;

        public Instruction()
        {
            opcount = 0;
            op1 = null;
            op2 = null;
            op3 = null;
            lockprefix = false;
            addr = 0;
            bytes = null;

        }

        //- convert instr to its opcode byte representation -------------------

        public enum OpMode { REGREG, REGMEM, MEMREG }

        public List<byte> getModrm(Operand op1, Operand op2, out OpMode opmode, out OPSIZE size)
        {
            int mode;
            int reg;
            int rm;
            List<byte> result = new List<byte>();
            List<byte> membytes = null;

            if (op1 is Register)
            {
                size = ((Register)op1).size;
                if (op2 is Register)
                {
                    opmode = OpMode.REGREG;         //register, register
                    mode = 3;
                    rm = ((Register)op1).code;
                    reg = ((Register)op2).code;
                }
                else
                {
                    opmode = OpMode.REGMEM;                                 //register, memory
                    reg = ((Register)op1).code;
                    membytes = ((Memory)op2).getBytes(out mode, out rm);
                }
            }
            else            //Op1 is memory
            {
                opmode = OpMode.MEMREG;             //memory, register
                size = ((Memory)op1).size;
                membytes = ((Memory)op1).getBytes(out mode, out rm);
                reg = ((Register)op2).code;
            }

            byte b = (byte)(mode * 0x40 + reg * 8 + rm);        //the modrm byte
            result.Add(b);
            if (membytes != null)
            {
                result.AddRange(membytes);           //add any additional memory op bytes
            }
            return result;
        }

        //base generate bytes method, every instr will override this
        public virtual void generateBytes()
        {
            bytes = new List<byte>() { 0xd6 };      //d6 is not a defined opcode
        }

        //returns the bytes if we already have them, else it generates them first
        //if this intruction is from a disassmbly, we keep the original bytes is was disassembled from
        //instead of genertaing new ones, which may be different (but equivalent) to the original ones
        public List<byte> getBytes()
        {
            if (bytes == null)
            {
                generateBytes();
            }
            return bytes;
        }

        //- display -----------------------------------------------------------

        const int BYTESFIELDWIDTH = 6;              //in bytes = each byte takes up 3 spaces
        const int OPCODEFIELDWIDTH = 12;            //in actual spaces

        //returns opcode and operands formated in Intel syntax
        public string displayIntruction()
        {
            //opcode field
            String result = ToString();
            if (lockprefix)
            {
                result = "LOCK " + result;
            }

            //operands field
            if (opcount > 0)
            {
                String spacer = " ";
                if (result.Length < 12)
                {
                    spacer = "            ".Substring(result.Length);
                }
                result = result + spacer + op1.ToString();
            }
            if (opcount > 1)
            {
                result = result + "," + op2.ToString();
            }
            if (opcount > 2)
            {
                result = result + "," + op3.ToString();
            }
            return result;
        }

        //returns instruction address, bytes, opcode and operands formated in Intel syntax
        public String displayIntructionLine()
        {
            StringBuilder asmLine = new StringBuilder();
            uint instrlen = (uint)bytes.Count;

            //address field
            asmLine.Append("  " + addr.ToString("X8") + ": ");

            //bytes field
            for (int i = 0; i < BYTESFIELDWIDTH; i++)
            {
                if (i < instrlen)
                {
                    asmLine.Append(bytes[i].ToString("X2") + " ");
                }
                else
                {
                    asmLine.Append("   ");          //extra space in field
                }
            }
            asmLine.Append(" ");                    //space over to opcode field

            asmLine.Append(displayIntruction());    //add opcode & operands

            //if all of instructions bytes were too long for one line, put the extra bytes on the next line
            if (instrlen > 6)
            {
                asmLine.AppendLine();
                asmLine.Append("            ");                 //blank addr field
                for (int i = 6; i < instrlen; i++)
                {
                    asmLine.Append(bytes[i].ToString("X2"));    //extra bytes
                    if (i < (instrlen - 1))
                    {
                        asmLine.Append(" ");
                    }
                }
            }

            return asmLine.ToString();
        }
    }

    //- catch all -------------------------------------------------------------

    //for byte sequences that don't match any known instruction
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
