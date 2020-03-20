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
using System.IO;

namespace Origami.Asm32
{
    public class Assembly
    {
        List<Instruction> insns;
        Dictionary<String, Symbol> symbolTable;

        //this will work for now
        public List<String> outlines;

        public Assembly()
        {
            insns = new List<Instruction>();
            symbolTable = new Dictionary<string, Symbol>();

            outlines = new List<string>();
        }

        public void AddInsn(Instruction insn)
        {
            insns.Add(insn);
        }

        public void AddSymbol(Symbol sym)
        {
            symbolTable[sym.name] = sym;
        }

        public void addLine(String line)
        {
            outlines.Add(line);
        }

        //- reading in --------------------------------------------------------

        public static Assembly readIn(string inname)
        {
            return null;
        }

        //- writing out -------------------------------------------------------

        //tiny steps for now
        public void writeOut(string outname)
        {
            try
            {
                File.WriteAllLines(outname, outlines);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error writing assembly file " + outname + ": " + e.Message);
            }
        }
    }

    //-------------------------------------------------------------------------

    public class Symbol : Operand
    {
        public String name;
        public Instruction def;

        public Symbol(string _name)
        {
            name = _name;
            def = null;
        }
    }
}
