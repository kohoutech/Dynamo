/* ----------------------------------------------------------------------------
Origami Win32 Library
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
using System.Text.RegularExpressions;

//using Origami.Asm32;

namespace Origami.Win32
{
    
    class CodeSection : Section
    { 
        const int BYTESFIELDWIDTH = 6;              //in bytes = each byte takes up 3 spaces
        const int OPCODEFIELDWIDTH = 12;            //in actual spaces

        List<String> codeList;

        public CodeSection()
            : base()
        {
            codeList = null;
        }

        //public CodeSection(int _secnum, String _sectionName, uint _memsize, uint _memloc, uint _filesize, uint _fileloc, 
        //    uint _pRelocations, uint _pLinenums, int _relocCount, int _linenumCount, uint _flags)
        //    : base(_secnum, _sectionName, _memsize, _memloc, _filesize, _fileloc, 
        //    _pRelocations, _pLinenums, _relocCount, _linenumCount, _flags)
        //{            
        //}

//-----------------------------------------------------------------------------

        //format addr, instruction bytes & asm ops into a list of strings for all bytes in code section
        //code format based on MS dumpbin utility
        public List<String> disasmCode()
        {
            uint srcpos = 0;
            codeList = new List<String>();
            //StringBuilder asmLine = new StringBuilder();
            //i32Disasm disasm = new i32Disasm(data, srcpos);
            //Instruction instr;
            //uint instrlen = 0;
            //List<int> instrBytes;

            //uint codeaddr = memloc;         //starting pos of code in mem, used for instr addrs

            //while (srcpos < (data.Length - disasm.MAXINSTRLEN))
            //{
            //    instr = disasm.getInstr(codeaddr);          //disasm bytes at cur source pos into next instruction
            //    instrBytes = instr.getBytes();              //the instruction's bytes
            //    instrlen = (uint)instrBytes.Count;          //determines how many bytes to format in line

            //    asmLine.Clear();

            //    //address field
            //    asmLine.Append("  " + codeaddr.ToString("X8") + ": ");

            //    //bytes field
            //    for (int i = 0; i < BYTESFIELDWIDTH; i++)
            //    {
            //        if (i < instrlen)
            //        {
            //            asmLine.Append(instrBytes[i].ToString("X2") + " ");
            //        }
            //        else
            //        {
            //            asmLine.Append("   ");          //extra space in field
            //        }
            //    }
            //    asmLine.Append(" ");                    //space over to opcode field

            //    //opcode field
            //    String opcode = instr.ToString();
            //    if (instr.lockprefix)
            //    {
            //        opcode = "LOCK " + opcode;
            //    }
            //    asmLine.Append(opcode);

            //    //operands field
            //    String spacer = (opcode.Length < OPCODEFIELDWIDTH) ? 
            //        "            ".Substring(0, OPCODEFIELDWIDTH - opcode.Length) : "";

            //    if (instr.opcount > 0)
            //    {
            //        asmLine.Append(spacer + instr.op1.ToString());
            //    }
            //    if (instr.opcount > 1)
            //    {
            //        asmLine.Append("," + instr.op2.ToString());
            //    }
            //    if (instr.opcount > 2)
            //    {
            //        asmLine.Append("," + instr.op3.ToString());
            //    }

            //    //if all of instructions bytes were too long for one line, put the extra bytes on the next line
            //    if (instrlen > 6)
            //    {
            //        asmLine.AppendLine();
            //        asmLine.Append("            ");                 //blank addr field
            //        for (int i = 6; i < instrlen; i++)
            //        {
            //            asmLine.Append(instrBytes[i].ToString("X2"));    //extra bytes
            //            if (i < (instrlen - 1))
            //            {
            //                asmLine.Append(" ");
            //            }
            //        }                    
            //    }

            //    codeList.Add(asmLine.ToString());

            //    srcpos += instrlen;
            //    codeaddr += instrlen;
            //}

            return codeList;
        }

        public void getAddrList()
        {
            Regex regex = new Regex("[0-9A-F]{8}");
            uint codestart = memloc;
            uint codeend = codestart + memsize;
            
            foreach (String line in codeList)
            {
                if (line.Length < 44) continue;
                Match match = regex.Match(line, 32);
                if (match.Success)
                {
                    uint val = Convert.ToUInt32(match.Value, 16);
                    if ((val >= codestart) && (val <= codeend)) {
                    Console.Out.WriteLine(val.ToString("X8"));
                    }
                }
            }
        }        
    }
}
