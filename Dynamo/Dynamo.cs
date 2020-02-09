/* ----------------------------------------------------------------------------
Dynamo - a backend code generator
Copyright (C) 1997-2020  George E Greaney

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

using Origami.OIL;
using Origami.Asm32;
using Origami.Win32;

using Dynamo.SymbolTable;

namespace Dynamo
{
    class Dynamo
    {
        public Options options;
        public CodeGen codeGen;
        public SymTable symTable;
        public Assembler assembler;

        public Module module;
        public List<Instruction> insns;
        public Win32Coff objfile;

        //---------------------------------------------------------------------

        public Dynamo()
        {
            options = new Options();
            symTable = new SymTable(this);
            codeGen = new CodeGen(this);
            assembler = new Assembler(this);
        }

        //set options from the cmd line when used as a stand alone program
        public void setOptions(String[] args)
        {
            options.parseOptions(args);
        }

        //read an OIL module tree from a file
        public void load()
        {
            module = OILCan.loadOILCan(options.OILname);
        }

        //get a OIL module tree from the front end when used as a DLL
        public void setModule(Module _module)
        {
            module = _module;
        }

        //generate a list of instructions & psuedo ops from the module tree
        public void generate()
        {
            insns = codeGen.generate(module);

            //linker.BuildExecutable();

            //Win32Obj objfile = new Win32Obj();
            //objfile.timeStamp = 0;
            //objfile.characteristics = 0x104;

            //Section textSec = new Section(1, ".text", 0, 0, 0x24, 0x8c, 0xb0, 0, 3, 0, 0x60300020);
            //textSec.addReloc(new CoffReloc(0x07, 0x0c, CoffReloc.Reloctype.REL32));
            //textSec.addReloc(new CoffReloc(0x0d, 0x0b, CoffReloc.Reloctype.DIR32));
            //textSec.addReloc(new CoffReloc(0x17, 0x0b, CoffReloc.Reloctype.DIR32));
            //byte[] textdata = { 0x55, 0x89, 0xe5, 0x83, 0xec, 0x08, 0xe8, 0x00, 0x00, 0x00, 0x00, 0xc7, 0x05, 0x00, 0x00, 0x00, 
            //                    0x00, 0x45, 0x00, 0x00, 0x00, 0x8b, 0x15, 0x00, 0x00, 0x00, 0x00, 0x89, 0xd0, 0xeb, 0x01, 0x90, 
            //                    0x89, 0xec, 0x5d, 0xc3};
            //textSec.setData(textdata);

            //Section dataSec = new Section(2, ".data", 0, 0, 0, 0, 0, 0, 0, 0, 0xC0300040);
            //Section bssSec = new Section(3, ".bss", 0, 0, 0, 0, 0, 0, 3, 0, 0xC0300080);

            //objfile.addSection(textSec);
            //objfile.addSection(dataSec);
            //objfile.addSection(bssSec);

            //objfile.addSymbol(new CoffSymbol(".file",0,0xfffe,0,0x67,1));
            //objfile.addSymbol(new CoffSymbol("test.c", 0, 0, 0, 0, 0));
            //objfile.addSymbol(new CoffSymbol("alongstring", 0, 1, 0, 6, 0));
            //objfile.addSymbol(new CoffSymbol("alongerstring", 0, 1, 0, 6, 0));
            //objfile.addSymbol(new CoffSymbol("_main", 0, 1, 0x20, 2, 0));
            //objfile.addSymbol(new CoffSymbol(".text", 0, 1, 0, 3, 1));
            //objfile.addSymbol(new CoffSymbol("$\x00\x00\x00\x03", 0, 0, 0, 0, 0));
            //objfile.addSymbol(new CoffSymbol(".data", 0, 0x2, 0, 3, 1));
            //objfile.addSymbol(new CoffSymbol("", 0, 0, 0, 0, 0));
            //objfile.addSymbol(new CoffSymbol(".bss", 0, 3, 0, 3, 1));
            //objfile.addSymbol(new CoffSymbol("", 0, 0, 0, 0, 0));
            //objfile.addSymbol(new CoffSymbol("_i", 0x10, 0, 0, 2, 0));
            //objfile.addSymbol(new CoffSymbol("___main", 0, 0, 0x20, 2, 1));
            //objfile.addSymbol(new CoffSymbol("", 0, 0, 0, 0, 0));

            //objfile.addString("alongstring");
            //objfile.addString("alongerstring");

            //objfile.writeToFile("test.obj");
        }

        //assemble the instruction list into an object file
        public void assemble()
        {
            objfile = assembler.assemble(insns);
            //List<Instruction> instrs = new List<Instruction>();
            //instrs.Add(new Push(Register32.EBP));
            //instrs.Add(new Move(Register32.EBP, Register32.ESP));
            //instrs.Add(new Subtract(Register32.EBP, new Immediate(8, OPSIZE.DWord), false));
            //instrs.Add(new Move(new Symbol("i"), new Immediate(69, OPSIZE.DWord)));
            //instrs.Add(new Move(Register32.EDX, new Symbol("i")));
            //instrs.Add(new Move(Register32.EAX, Register32.EDX));
            //instrs.Add(new Move(Register32.ESP, Register32.EBP));
            //instrs.Add(new Pop(Register32.EBP));
            //instrs.Add(new Return(false));
        }

        //write the assembled object file out
        public void write()
        {
            objfile.write(options.ObjName);
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the Wizard");