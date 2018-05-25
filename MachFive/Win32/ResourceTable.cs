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
using System.Drawing;
using System.IO;

//https://en.wikibooks.org/wiki/X86_Disassembly/Windows_Executable_Files
//https://msdn.microsoft.com/en-us/library/windows/desktop/ms680547(v=vs.85).aspx#the_.rsrc_section
//https://msdn.microsoft.com/en-us/library/ms648009(v=vs.85).aspx

namespace Origami.Win32
{
    public class ResourceTable
    {
        String[] NODETYPES = {
            "?", "Cursor", "Bitmap", "Icon", "Menu", "Dialog", "String Table", "Font Directory", "Font",           //0-8
            "Accelerator", "User Data", "Message Table", "Cursor Group", "?", "Icon Group", "?", "Version",        //9-16
            "Dialog Include", "?", "Plug and Plug", "VXD", "Animated Cursor", "Animated Icon", "HTML", "Manifest"  //17-24
        };

        //I could make these subclass specific lists (ie List<ResBitmap> instead of List<ResourceData>)
        //but you can't cast them back to List<ResourceData>, which would then require 13 versions of
        //the same helper functions
        public List<ResourceData> accelerators;
        public List<ResourceData> bitmaps;
        public List<ResourceData> cursors;
        public List<ResourceData> dialogs;
        public List<ResourceData> fonts;
        public List<ResourceData> fontDirectories;
        public List<ResourceData> cursorGroups;
        public List<ResourceData> iconGroups;
        public List<ResourceData> icons;
        public List<ResourceData> menus;
        public List<ResourceData> stringtable;
        public List<ResourceData> versions;
        public List<ResourceData> userData;

        public byte[] data;                 //the raw data from/to a resource section in an exe file
        public uint imageBase;              //exe image base, this an resourceRVA determine where specific records would be
        public uint resourceRVA;            //located when the resource section is loaded into memory, for parsing purposes

        public ResourceTable()
        {
            accelerators = new List<ResourceData>();
            bitmaps = new List<ResourceData>();
            cursors = new List<ResourceData>();
            dialogs = new List<ResourceData>();
            fonts = new List<ResourceData>();
            fontDirectories = new List<ResourceData>();
            cursorGroups = new List<ResourceData>();
            iconGroups = new List<ResourceData>();
            icons = new List<ResourceData>();
            menus = new List<ResourceData>();
            stringtable = new List<ResourceData>();
            versions = new List<ResourceData>();
            userData = new List<ResourceData>();

            data = null;
            imageBase = 0;
            resourceRVA = 0;
        }

//- loading in ----------------------------------------------------------------

        uint[] resIdNameValues;         //for holding id/name values during the parse descent

        //recursively descend through resource directory structure
        //resource directories are 3 levels deep by Microsoft convention:
        //level 1 : resource type
        //level 2 : resource name str/id num
        //level 3 : language (aka code page)
        private void parseResourceDirectory(SourceFile source, int level)
        {
            //parse IMAGE_RESOURCE_DIRECTORY
            uint characteristics = source.getFour();    //unused
            uint timeDateStamp = source.getFour();
            uint majorVersion = source.getTwo();
            uint minorVersion = source.getTwo();
            uint numberOfNamedEntries = source.getTwo();
            uint numberOfIdEntries = source.getTwo();
            int entryCount = (int)(numberOfNamedEntries + numberOfIdEntries);

            for (int i = 0; i < entryCount; i++)
            {
                uint idName = source.getFour();         //either numeric val or a ptr to name str
                uint data = source.getFour();           //either ptr to subdir or a leaf node
                resIdNameValues[level] = idName;        //store id/name val at this level

                uint curPos = source.getPos();          //save cur pos in resource directory
                uint dataPos = (data & 0x7FFFFFFF);
                source.seek(dataPos);                   //goto leaf/subtree data

                if (data < 0x80000000)                                  //high bit not set -> data points to leaf node
                {
                    parseResourceData(source);                    
                }
                else
                {                                                       //high bit is set -> data points to subtree
                    parseResourceDirectory(source, level + 1);          //recurse next subtree
                }   

                source.seek(curPos);        //ret to pos in resource directory
            }
        }

        private String getResourceName(SourceFile source, uint pos)
        {
            uint curPos = source.getPos();
            pos = (pos & 0x7FFFFFFF);
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

        //leaf node of resource directory tree, this rec points to actual data
        private void parseResourceData(SourceFile source)
        {
            uint datapos = source.getFour();
            uint datasize = source.getFour();
            uint codepage = source.getFour();
            uint reserved = source.getFour();
            datapos -= resourceRVA;
            byte[] resdata = source.getRange(datapos, datasize);        //get resource data

            //get the store type/id/lang vals we stored in our decent to this node
            uint restype = resIdNameValues[0];
            uint resid = resIdNameValues[1];
            String resname = (resid >= 0x80000000) ? getResourceName(source, resid) : null;            
            uint reslang = resIdNameValues[2];

            switch (restype)
            {
                case 1:
                    addCursor(resid, resname, reslang, resdata);
                    break;

                case 2:
                    addBitmap(resid, resname, reslang, resdata);
                    break;

                case 3:
                    addIcon(resid, resname, reslang, resdata);
                    break;

                case 4:
                    addMenu(resid, resname, reslang, resdata);
                    break;

                case 5:
                    addDialog(resid, resname, reslang, resdata);
                    break;

                case 6:
                    addStringTable(resid, resname, reslang, resdata);
                    break;

                case 7:
                    addFontDirectory(resid, resname, reslang, resdata);
                    break;

                case 8:
                    addFont(resid, resname, reslang, resdata);
                    break;

                case 9:
                    addAccelerator(resid, resname, reslang, resdata);
                    break;

                case 10:
                    addUserData(resid, resname, reslang, resdata);
                    break;

                case 12:
                    addCursorGroup(resid, resname, reslang, resdata);
                    break;

                case 14:
                    addIconGroup(resid, resname, reslang, resdata);
                    break;

                case 16:
                    addVersion(resid, resname, reslang, resdata);
                    break;

                default:
                    addUserData(resid, resname, reslang, resdata);
                    break;
            }
        }

        public void parseData()
        {
            resIdNameValues = new uint[3];

            SourceFile source = new SourceFile(data);   //source file pts to resource table's raw data buf
            parseResourceDirectory(source, 0);          //start on level 0
        }

//- storing out ---------------------------------------------------------------

        public byte[] getData()
        {
            byte[] result = null;
            return result;
        }

//- helpers -----------------------------------------------------------------------

        //search list to find existing item with either matching name or id
        public ResourceData getDataItem(List<ResourceData> resList, uint resId, String resName)
        {
            ResourceData result = null;
            if (resName != null)
            {
                foreach (ResourceData resData in resList)
                {
                    if (resData.name.Equals(resName))
                    {
                        result = resData;
                        break;
                    }
                }
            }
            else
            {
                foreach (ResourceData resData in resList)
                {
                    if (resData.id == resId)
                    {
                        result = resData;
                        break;
                    }
                }
            }
            return result;
        }

        public void addResourceItem(ResourceData resData, uint language, byte[] data) 
        {
            ResourceItem item = resData.getLanguageItem(language);      //get the specific resource for this language
            if (item == null)                                           //don't have a resource for this lang yet
            {
                item = new ResourceItem(resData, language);             //so make a new one
                resData.items.Add(item);                                //and add it to the resource data obj's list
            }
            item.parseData(data);            
        }

//- resource CRUD -------------------------------------------------------------

        public void addAccelerator(uint id, String name, uint language, byte[] data)
        {
            ResAccelerator acc = (ResAccelerator)getDataItem(accelerators, id, name);
            if (acc == null)
            {
                acc = new ResAccelerator(id, name);
                accelerators.Add(acc);
            }
            addResourceItem(acc, language, data);
        }

        public void deleteAccelerator(String name)
        {
        }

        public byte[] getAccelerator(String name)
        {
            byte[] result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addBitmap(uint id, String name, uint language, byte[] data)
        {
            ResBitmap bmp = (ResBitmap)getDataItem(bitmaps, id, name);
            if (bmp == null)
            {
                bmp = new ResBitmap(id, name);
                bitmaps.Add(bmp);
            }
            addResourceItem(bmp, language, data);

        }

        public void addBitmap(String name, Bitmap bmp)
        {
        }

        public void deleteBitmap(String name)
        {
        }

        public Bitmap getBitmap(String name)
        {
            Bitmap result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addCursor(uint id, String name, uint language, byte[] data)
        {
            ResCursor cur = (ResCursor)getDataItem(cursors, id, name);
            if (cur == null)
            {
                cur = new ResCursor(id, name);
                cursors.Add(cur);            
            }
            addResourceItem(cur, language, data);
        }

        public void deleteCursor(String name)
        {
        }

        public byte[] getCursor(String name)
        {
            byte[] result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addDialog(uint id, String name, uint language, byte[] data)
        {
            ResDialog dlg = (ResDialog)getDataItem(dialogs, id, name);
            if (dlg == null)
            {
                dlg = new ResDialog(id, name);
                dialogs.Add(dlg);
            }
            addResourceItem(dlg, language, data);

        }

        public void deleteDialog(String name)
        {
        }

        public byte[] getDialog(String name)
        {
            byte[] result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addFont(uint id, String name, uint language, byte[] data)
        {
            ResFont font = (ResFont)getDataItem(fonts, id, name);
            if (font == null)
            {
                font = new ResFont(id, name);
                fonts.Add(font);
            }
            addResourceItem(font, language, data);
        }

        public void deleteFont(String name)
        {
        }

        public byte[] getFont(String name)
        {
            byte[] result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addFontDirectory(uint id, String name, uint language, byte[] data)
        {
            ResFontDir fontd = (ResFontDir)getDataItem(fontDirectories, id, name);
            if (fontd == null)
            {
                fontd = new ResFontDir(id, name);
                fontDirectories.Add(fontd);
            }
            addResourceItem(fontd, language, data);
        }

        public void deleteFontDirectory(String name)
        {
        }

        public byte[] getFontDirectory(String name)
        {
            byte[] result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addCursorGroup(uint id, String name, uint language, byte[] data)
        {
            ResCursorGroup rcg = (ResCursorGroup)getDataItem(cursorGroups, id, name);
            if (rcg == null)
            {
                rcg = new ResCursorGroup(id, name);
                cursorGroups.Add(rcg);
            }
            addResourceItem(rcg, language, data);
        }

        public void deleteCursorGroup(String name)
        {
        }

        public byte[] getCursorGroup(String name)
        {
            byte[] result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addIconGroup(uint id, String name, uint language, byte[] data)
        {
            ResIconGroup rig = (ResIconGroup)getDataItem(iconGroups, id, name);
            if (rig == null)
            {
                rig = new ResIconGroup(id, name);
                iconGroups.Add(rig);
            }
            addResourceItem(rig, language, data);
        }

        public void deleteIconGroup(String name)
        {
        }

        public byte[] getIconGroup(String name)
        {
            byte[] result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addIcon(uint id, String name, uint language, byte[] data)
        {
            ResIcon ricon = (ResIcon)getDataItem(icons, id, name);
            if (ricon == null)
            {
                ricon = new ResIcon(id, name);
                icons.Add(ricon);
            }
            addResourceItem(ricon, language, data);
        }

        public void addIcon(String name, Icon icon)
        {
        }

        public void deleteIcon(String name)
        {
        }

        public Icon getIcon(String name)
        {
            Icon result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addMenu(uint id, String name, uint language, byte[] data)
        {
            ResMenu rmenu = (ResMenu)getDataItem(menus, id, name);
            if (rmenu == null)
            {
                rmenu = new ResMenu(id, name);
                menus.Add(rmenu);
            }
            addResourceItem(rmenu, language, data);
        }

        public void deleteMenu(String name)
        {
        }

        public byte[] getMenu(String name)
        {
            byte[] result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addStringTable(uint id, String name, uint language, byte[] data)
        {
            ResStringTable st = (ResStringTable)getDataItem(stringtable, id, name);
            if (st == null)
            {
                st = new ResStringTable(id, name);
                stringtable.Add(st);
            }
            addResourceItem(st, language, data);
        }

        public void addStringResource(String name, String str)
        {
        }

        public void deleteStringResource(String name)
        {
        }

        public String getStringResource(String name)
        {
            String result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addUserData(uint id, String name, uint language, byte[] data)
        {
            ResUserData udata = (ResUserData)getDataItem(userData, id, name);
            if (udata == null)
            {
                udata = new ResUserData(id, name);
                userData.Add(udata);
            }
            addResourceItem(udata, language, data);
        }

        public void deleteUserData(String name)
        {
        }

        public byte[] getUserData(String name)
        {
            byte[] result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addVersion(uint id, String name, uint language, byte[] data)
        {
            ResVersion ver = (ResVersion)getDataItem(versions, id, name);
            if (ver == null)
            {
                ver = new ResVersion(id, name);
                versions.Add(ver);
            }
            addResourceItem(ver, language, data);
        }

        public void deleteVersion(String name)
        {
        }

        public byte[] getVersion(String name)
        {
            byte[] result = null;
            return result;
        }
    }

//-----------------------------------------------------------------------------
//   RESOURCE DATA CLASSES
//-----------------------------------------------------------------------------

    //base class 
    public class ResourceData 
    {
        public uint id;
        public String name;
        public List<ResourceItem> items;        //there is one of these for each language this resource supports

        public ResourceData(uint _id, String _name)
        {
            id = _id;
            name = _name;
            items = new List<ResourceItem>();
        }

        //gets a list of lang ids that this resource suports
        public List<uint> getLanguageList()
        {
            List<uint> langs = new List<uint>();
            foreach (ResourceItem item in items)
            {
                langs.Add(item.lang);
            }
            return langs;
        }

        public ResourceItem getLanguageItem(uint lang)
        {
            ResourceItem result = null;
            foreach (ResourceItem item in items)
            {
                if (item.lang == lang)
                {
                    result = item;
                    break;
                }
            }
            return result;
        }

        public virtual object parseRawData(byte[] dataBuf)
        {
            return null;
        }
    }

    public class ResourceItem
    {
        public ResourceData parent; 
        public uint lang;
        public byte[] dataBuf;
        public Object item;

        public ResourceItem(ResourceData _parent, uint _lang)
        {
            parent = _parent;
            lang = _lang;
            dataBuf = null;
            item = null;
        }

        public void parseData(byte[] data)
        {
            dataBuf = data;
            item = parent.parseRawData(data);
        }
    }

//-----------------------------------------------------------------------------

    public class ResAccelerator : ResourceData
    {
        public ResAccelerator(uint id, String name)
            : base(id, name)
        {
        }
    }

//- bitmap --------------------------------------------------------------

    //https://en.wikipedia.org/wiki/BMP_file_format

    public class ResBitmap : ResourceData
    {
        public Bitmap bitmap;

        public ResBitmap(uint id, String name)
            : base(id, name)
        {
        }

        public override object parseRawData(byte[] dataBuf)
        {
            //the bitmap resource data is the same as in a bitmap file, except the header has been removed
            //so we build a header, prepend it to the front of our resource data
            //and create a bitmap from the total data, as if we read it from a file

            //BITMAPFILEHEADER struct
            byte[] hdr = { 0x42, 0x4D,                  //sig = BM
                           0x00, 0x00, 0x00, 0x00,      //file size
                           0x00, 0x00, 0x00, 0x00,      //reserved
                           0x0E, 0x00, 0x00, 0x00 };    //offset to image bits = this header size + resource hdr size

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
            return bitmap;
        }
    }

//-----------------------------------------------------------------------------

    public class ResCursor : ResourceData
    {
        public ResCursor(uint id, String name)
            : base(id, name)
        {
        }
    }
    
//-----------------------------------------------------------------------------

    public class ResDialog : ResourceData
    {
        public ResDialog(uint id, String name)
            : base(id, name)
        {
        }
    }
    
//-----------------------------------------------------------------------------

    public class ResFont : ResourceData
    {
                public ResFont(uint id, String name)
            : base(id, name)
        {
        }

    }

//-----------------------------------------------------------------------------

    public class ResFontDir : ResourceData
    {
                public ResFontDir(uint id, String name)
            : base(id, name)
        {
        }

    }

//-----------------------------------------------------------------------------

    public class ResCursorGroup : ResourceData
    {
        public ResCursorGroup(uint id, String name)
            : base(id, name)
        {
        }

    }

//-----------------------------------------------------------------------------

    public class ResIconGroup : ResourceData
    {
        public ResIconGroup(uint id, String name)
            : base(id, name)
        {
        }

    }

//-----------------------------------------------------------------------------

    public class ResIcon : ResourceData
    {
                public ResIcon(uint id, String name)
            : base(id, name)
        {
        }

    }

//-----------------------------------------------------------------------------

    public class ResMenu : ResourceData
    {
                public ResMenu(uint id, String name)
            : base(id, name)
        {
        }

    }
    
//-----------------------------------------------------------------------------

    public class ResUserData : ResourceData
    {
                public ResUserData(uint id, String name)
            : base(id, name)
        {
        }

    }

//- string table --------------------------------------------------------------

    public class ResStringTable : ResourceData
    {
        public int bundleNum;        

        public ResStringTable(uint id, String name)
            : base(id, name)
        {
            bundleNum = (int)(id - 1) * 16;
        }

        public int getString(int strpos, byte[] dataBuf, List<string> strings)
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

        public override object parseRawData(byte[] dataBuf)
        {
            List<string> strings = new List<string>(16);
            int strpos = 0;
            while (strpos < dataBuf.Length)
            {
                strpos = getString(strpos, dataBuf, strings);
            }
            return strings;
        }
    }

//-----------------------------------------------------------------------------

    public class ResVersion : ResourceData
    {
        public ResVersion(uint id, String name)
            : base(id, name)
        {
        }
    }
}

//Console.WriteLine("there's no sun in the shadow of the wizard");