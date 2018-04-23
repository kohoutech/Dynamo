/* ----------------------------------------------------------------------------
Origami Win32 Library
Copyright (C) 1998-2017  George E Greaney

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
using System.Drawing;
using System.IO;

//https://en.wikibooks.org/wiki/X86_Disassembly/Windows_Executable_Files

namespace Origami.Win32
{
    public class ResourceParser
    {
        public Section res;
        public SourceFile source;
        public uint resfileloc;
        public uint resmemloc;
        public ResourceDirectory rootNode;
        public int curNodeType;


        public ResourceParser(SourceFile _source, Section _rsrc)
        {
            source = _source;
            res = _rsrc;
            resfileloc = res.fileloc;
            resmemloc = res.memloc - res.imageBase;
            rootNode = null;
            curNodeType = 0;
        }

        //set source loc to start of resource section and parse resource tree starting with root node
        public void parse()
        {
            source.seek(resfileloc);
            rootNode = new ResourceDirectory(null, this, 0, 0);
            //rootNode.dump();
        }

        //https://msdn.microsoft.com/en-us/library/ms648009(v=vs.85).aspx
        String[] NODETYPES = {
            "?", "Cursor", "Bitmap", "Icon", "Menu", "Dialog", "String Table", "Font Directory", "Font",           //0-8
            "Accelerator", "User Data", "Message Table", "Cursor Group", "?", "Icon Group", "?", "Version",        //9-16
            "Dialog Include", "?", "Plug and Plug", "VXD", "Animated Cursor", "Animated Icon", "HTML", "Manifest"  //17-24
        };

        public String getNodeType(int nodetype)
        {
            return (nodetype < NODETYPES.Length) ? NODETYPES[nodetype] : "User Data";
        }
    }

//-----------------------------------------------------------------------------

    //base class
    public class ResourceNode
    {
        public ResourceParser parser;
        public ResourceNode parent;
        public int level;
        public uint addr;
        public int id;
        public String name;

        public ResourceNode(ResourceNode _parent, ResourceParser _parser, int _level, uint _idName)
        {
            parser = _parser;
            parent = _parent;
            level = _level;
            SourceFile source = parser.source;
            addr = source.getPos();

            if (_idName < 0x80000000)           //high bit not set -> id is a number
            {
                id = (int)_idName;
                name = null;
                if (level == 1)
                {
                    parser.curNodeType = id;
                }
            }
            else
            {                               //high bit is set -> id points to a name string
                id = -1;
                name = getNameString(_idName - 0x80000000);
            }
        }

        public String getNameString(uint pos)
        {
            SourceFile source = parser.source;
            uint curPos = source.getPos();
            pos = (pos & 0x0000FFFF) + parser.resfileloc;
            source.seek(pos);

            int strLen = (int)source.getTwo();
            pos += 2;
            StringBuilder str = new StringBuilder(strLen);
            for (int i = 0; i < strLen; i++)
            {
                uint ch = source.getTwo();
                str.Append(Convert.ToChar(ch));
                pos += 2;
            }
            source.seek(curPos);
            return str.ToString();
        }

        public virtual void dump()
        {
        }
    }

//-----------------------------------------------------------------------------

    public class ResourceDirectory : ResourceNode
    {
        uint characteristics;       //unused
        uint timeDateStamp;
        uint majorVersion;
        uint minorVersion;
        uint numberOfNamedEntries;
        uint numberOfIdEntries;
        public List<ResourceNode> entries;

        public ResourceDirectory(ResourceNode _parent, ResourceParser _parser, int _level, uint _idName)
            : base(_parent, _parser, _level, _idName)
        {
            SourceFile source = parser.source;
            characteristics = source.getFour();
            timeDateStamp = source.getFour();
            majorVersion = source.getTwo();
            minorVersion = source.getTwo();
            numberOfNamedEntries = source.getTwo();
            numberOfIdEntries = source.getTwo();
            int entryCount = (int)(numberOfNamedEntries + numberOfIdEntries);

            entries = new List<ResourceNode>(entryCount);
            ResourceNode node;
            for (int i = 0; i < entryCount; i++)
            {
                uint idName = source.getFour();
                uint data = source.getFour();

                uint curPos = source.getPos();
                uint dataPos = (data & 0x0000FFFF) + parser.resfileloc;
                source.seek(dataPos);
                if (data < 0x80000000)                                          //high bit not set -> data points to leaf node
                {
                    node = ResourceData.getDataEntry(this, parser, level + 1, idName);
                }
                else
                {                                                               //high bit is set -> data points to subtree
                    node = new ResourceDirectory(this, parser, level + 1, idName);
                }

                entries.Add(node);
                source.seek(curPos);
            }
        }

        public override void dump()
        {
            Console.Out.WriteLine("Resource Directory " + id + " at " + addr.ToString("X8") + " with " + entries.Count + " entries ");
            foreach (ResourceNode node in entries)
            {
                node.dump();
            }
        }
    }

//-----------------------------------------------------------------------------

    public class ResourceData : ResourceNode
    {
        const int RT_BITMAP = 2;
        const int RT_STRING = 6;

        public uint dataPos;
        public uint size;
        uint codePage;
        uint reserved;
        public byte[] dataBuf;
        public int nodeType;

        //static method because the type of node we return is dependant on the parser's current node type
        public static ResourceData getDataEntry(ResourceNode parent, ResourceParser parser, int level, uint idName)
        {
            ResourceData result = null;
            switch (parser.curNodeType)
            {
                case RT_BITMAP:
                    result = new BitmapEntry(parent, parser, level, idName);
                    break;

                case RT_STRING:
                    result = new StringTableEntry(parent, parser, level, idName);
                    break;

                default:
                    result = new ResourceData(parent, parser, level, idName);
                    break;
            }            
            return result;
        }

        public ResourceData(ResourceNode _parent, ResourceParser _parser, int _level, uint _idName)
            : base(_parent, _parser, _level, _idName)
        {
            parser = _parser;
            SourceFile source = parser.source;
            nodeType = parser.curNodeType;

            dataPos = source.getFour();
            size = source.getFour();
            codePage = source.getFour();
            reserved = source.getFour();

            dataPos = dataPos - parser.resmemloc + parser.resfileloc;
            dataBuf = source.getRange(dataPos, size);
        }

        public override void dump()
        {
            Console.Out.WriteLine("Resource Entry " + id + " at " + addr.ToString("X8") + 
                " node type: " + nodeType + " data: " +
                dataPos.ToString("X8") + " size: " + size.ToString("X8"));
        }

    }

//- bitmap --------------------------------------------------------------

    //https://en.wikipedia.org/wiki/BMP_file_format

    public class BitmapEntry : ResourceData
    {
        public Bitmap bitmap;
        
        public BitmapEntry(ResourceNode _parent, ResourceParser _parser, int _level, uint _idName)
            : base(_parent, _parser, _level, _idName)
        {
            //BITMAPFILEHEADER struct
            byte[] hdr = { 0x42, 0x4D,                  //sig = BM
                           0x00, 0x00, 0x00, 0x00,      //file size
                           0x00, 0x00, 0x00, 0x00,      //reserved
                           0x0E, 0x00, 0x00, 0x00 };    //ofsset to image bits = this header size + resource hdr size
            
            //update file size and bits offset fields
            int filesize = 0x0E + dataBuf.Length;
            byte[] sizebytes = BitConverter.GetBytes(filesize);
            Array.Copy(sizebytes, 0, hdr, 2, 4);

            byte[] hdrsizebytes = new byte[4];
            Array.Copy(dataBuf, hdrsizebytes, 4);
            int hdrsize = BitConverter.ToInt32(hdrsizebytes, 0) + 0x0E;
            byte[] bitofsbytes = BitConverter.GetBytes(hdrsize);
            Array.Copy(bitofsbytes, 0, hdr, 10, 4);

            //join the file header to the resource data
            byte[] filebytes = new byte[filesize];
            Array.Copy(hdr, filebytes, 0x0E);
            Array.Copy(dataBuf, 0, filebytes, 0x0E, dataBuf.Length);

            //create a bitmap from the resource data
            MemoryStream ms = new MemoryStream(filebytes);
            bitmap = new Bitmap(ms);
        }
    }

//- string table --------------------------------------------------------------

    public class StringTableEntry : ResourceData
    {
        public int bundleNum;
        public List<String> strings;

        public int getString(int strpos)
        {
            int strLen = (int)((dataBuf[strpos + 1] * 256) + dataBuf[strpos]);
            strpos += 2;
            StringBuilder str = new StringBuilder(strLen);
            for (int i = 0; i < strLen; i++) 
            {
                int ch = (int)((dataBuf[strpos + 1] * 256) + dataBuf[strpos]);
                str.Append(Convert.ToChar(ch));
                strpos += 2;            
            }
            strings.Add(str.ToString());
            return strpos;
        }

        public StringTableEntry(ResourceNode _parent, ResourceParser _parser, int _level, uint _idName)
            : base(_parent, _parser, _level, _idName)
        {
            bundleNum = (parent.id - 1) * 16;
            strings = new List<string>(16);
            int strpos = 0;
            while (strpos < dataBuf.Length)
            {
                strpos = getString(strpos);
            }
        }

        public override void dump()
        {
            Console.Out.WriteLine("String Table " + id + " at " + addr.ToString("X8") +
                " node type: " + nodeType + " data: " +
                dataPos.ToString("X8") + " size: " + size.ToString("X8"));
            int i = bundleNum;
            foreach (String str in strings) {
                Console.Out.WriteLine(i.ToString("X4") + " : " + str);
                i++;
            }
        }

    }
}