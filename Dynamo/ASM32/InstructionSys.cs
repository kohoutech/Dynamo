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

//system instructions - section 5.1 Intel Architecture Manual

namespace Origami.Asm32
{

    //LoadDescriptor - LLDT/LGDT/LIDT
    public class LoadDescriptor : Instruction
    {
        public enum MODE { LLDT, LGDT, LIDT }

        MODE mode;

        public LoadDescriptor(Operand _op1, MODE _mode)
            : base()
        {
            opcount = 1;
            op1 = _op1;
            mode = _mode;
        }

        public override string ToString()
        {
            return ((mode == MODE.LLDT) ? "LLDT" : (mode == MODE.LGDT) ? "LGDT" : "LIDT");
        }
    }

    //StoreDescriptor - SLDT/SGDT/SIDT
    public class StoreDescriptor : Instruction
    {
        public enum MODE { SLDT, SGDT, SIDT }

        MODE mode;

        public StoreDescriptor(Operand _op1, MODE _mode)
            : base()
        {
            opcount = 1;
            op1 = _op1;
            mode = _mode;
        }

        public override string ToString()
        {
            return ((mode == MODE.SLDT) ? "SLDT" : (mode == MODE.SGDT) ? "SGDT" : "SIDT");
        }
    }

    //LoadTaskRegister - LTR
    public class LoadTaskRegister : Instruction
    {
        public LoadTaskRegister(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "LTR";
        }
    }

    //StoreaskRegister - STR
    public class StoreTaskRegister : Instruction
    {
        public StoreTaskRegister(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;

        }

        public override string ToString()
        {
            return "STR";
        }
    }

    //LoadSMachinetatusWord - LMSW
    public class LoadSMachinetatusWord : Instruction
    {
        public LoadSMachinetatusWord(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "LMSW";
        }
    }

    //StoreMachineStatusWord - SMSW
    public class StoreMachineStatusWord : Instruction
    {
        public StoreMachineStatusWord(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "SMSW";
        }
    }

    //ClearTaskFlag - CLTS
    public class ClearTaskFlag : Instruction
    {
        public ClearTaskFlag()
            : base()
        {
        }

        public override string ToString()
        {
            return "CLTS";
        }
    }

    //AdjustRPL - ARPL
    public class AdjustRPL : Instruction
    {
        public AdjustRPL(Operand _op1, Operand _op2)
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
            bytes.Add(0x63);
            bytes.AddRange(modrm);
        }

        public override string ToString()
        {
            return "ARPL";
        }
    }

    //LoadAccessRights - LAR
    public class LoadAccessRights : Instruction
    {
        public LoadAccessRights(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "LAR";
        }
    }

    //LoadSegementLimit - LSL
    public class LoadSegementLimit : Instruction
    {
        public LoadSegementLimit(Operand _op1, Operand _op2)
            : base()
        {
            opcount = 2;
            op1 = _op1;
            op2 = _op2;
        }

        public override string ToString()
        {
            return "LSL";
        }
    }

    //VerifySegment - VERR/VERW
    public class VerifySegment : Instruction
    {
        public enum MODE { VERR, VERW }

        MODE mode;

        public VerifySegment(Operand _op1, MODE _mode)
            : base()
        {
            opcount = 1;
            op1 = _op1;
            mode = _mode;
        }

        public override string ToString()
        {
            return ((mode == MODE.VERR ? "VERR" : "VERW"));
        }
    }

    //InvalidateCache - INVD/WBINVD
    public class InvalidateCache : Instruction
    {
        public enum MODE { INVD, WBINVD }

        MODE mode;

        public InvalidateCache(MODE _mode)
            : base()
        {
            mode = _mode;
        }

        public override string ToString()
        {
            return ((mode == MODE.INVD ? "INVD" : "WBINVD"));
        }
    }

    //InvalidateTLB - INVLPG
    public class InvalidateTLB : Instruction
    {
        bool intop;
        bool pop;

        public InvalidateTLB(Operand _op1)
            : base()
        {
            opcount = 1;
            op1 = _op1;
        }

        public override string ToString()
        {
            return "INVLPG";
        }
    }

    //Halt - HLT
    public class Halt : Instruction
    {
        public Halt()
            : base()
        {
        }

        public override void generateBytes()
        {
            bytes = new List<byte>() { 0xf4 };
        }


        public override string ToString()
        {
            return "HALT";
        }
    }

    //ResumeFromSysMgt - RSM
    public class ResumeFromSysMgt : Instruction
    {
        public ResumeFromSysMgt()
            : base()
        {
        }

        public override string ToString()
        {
            return "RSM";
        }
    }

    //ReadModelSpecReg - RDMSR
    public class ReadModelSpecReg : Instruction
    {

        public ReadModelSpecReg()
            : base()
        {
            opcount = 0;
        }

        public override string ToString()
        {
            return "RDMSR";
        }
    }

    //WriteModelSpecReg - WRMSR
    public class WriteModelSpecReg : Instruction
    {

        public WriteModelSpecReg()
            : base()
        {
            opcount = 0;
        }

        public override string ToString()
        {
            return "WRMSR";
        }
    }

    //ReadCounters - RDPMC/RDTSC
    public class ReadCounters : Instruction
    {
        public enum MODE { PERFORMANCE, TIMESTAMP };

        MODE mode;

        public ReadCounters(MODE _mode)
            : base()
        {
            opcount = 0;
            mode = _mode;
        }

        public override string ToString()
        {
            return (mode == MODE.PERFORMANCE) ? "RDPMC" : "RDTSC";
        }
    }

    //SystemCall - SYSCALL/SYSENTER
    public class SystemCall : Instruction
    {
        public enum MODE { SYSCALL, SYSENTER }

        MODE mode;

        public SystemCall(MODE _mode)
            : base()
        {
            mode = _mode;
        }

        public override string ToString()
        {
            return ((mode == MODE.SYSCALL ? "SYSCALL" : "SYSENTER"));
        }
    }

    //SystemRet - SYSRET/SYSEXIT
    public class SystemRet : Instruction
    {
        public enum MODE { SYSRET, SYSEXIT }

        MODE mode;

        public SystemRet(MODE _mode)
            : base()
        {
            mode = _mode;
        }

        public override string ToString()
        {
            return ((mode == MODE.SYSRET ? "SYSRET" : "SYSEXIT"));
        }
    }
}
