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
        public List<ResourceData> cursors;
        public List<ResourceData> bitmaps;
        public List<ResourceData> icons;
        public List<ResourceData> menus;
        public List<ResourceData> dialogs;
        public List<ResourceData> stringtable;
        public List<ResourceData> fontDirectories;
        public List<ResourceData> fonts;
        public List<ResourceData> accelerators;
        public List<ResourceData> userData;
        public List<ResourceData> cursorGroups;
        public List<ResourceData> iconGroups;
        public List<ResourceData> versions;

        //the exe file side
        public byte[] data;                 //the raw data from/to a resource section in an exe file
        public uint imageBase;              //exe image base, this an resourceRVA determine where specific records would be
        public uint resourceRVA;            //located when the resource section is loaded into memory, for parsing purposes

        public ResourceTable()
        {
            cursors = new List<ResourceData>();
            bitmaps = new List<ResourceData>();
            icons = new List<ResourceData>();
            menus = new List<ResourceData>();
            dialogs = new List<ResourceData>();
            stringtable = new List<ResourceData>();
            fontDirectories = new List<ResourceData>();
            fonts = new List<ResourceData>();
            accelerators = new List<ResourceData>();
            userData = new List<ResourceData>();
            cursorGroups = new List<ResourceData>();
            iconGroups = new List<ResourceData>();
            versions = new List<ResourceData>();

            data = null;
            imageBase = 0;
            resourceRVA = 0;
        }

//- loading in from exe -------------------------------------------------------

        uint[] resIdNameValues;         //for holding id/name values during the parse descent
        List<ResData> cursorItems;
        List<ResData> iconItems;

        //loading an exe file gives the raw resource data, this parses it into resource objs & adds them to the matching list
        public void parseData()
        {
            resIdNameValues = new uint[3];
            cursorItems = new List<ResData>();
            iconItems = new List<ResData>();

            SourceFile source = new SourceFile(data);   //source file pts to resource table's raw data buf
            parseResourceDirectory(source, 0);          //start on level 0

            parseCursorGroups();        //having stored icon & cursor resource data during the parse
            parseIconGroups();          //now create icon & cursor list entries from this data and the icon/cursor group entries
        }

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
                    ResData curdata = new ResData(resid, resname, reslang, resdata);
                    cursorItems.Add(curdata);                    
                    break;

                case 2:
                    Bitmap bmp = ResBitmap.parseData(resdata);
                    addBitmap(resid, resname, reslang, bmp);
                    getDataItem(bitmaps, resid, resname).getItem(reslang).dataBuf = resdata;
                    break;

                case 3:
                    ResData icondata = new ResData(resid, resname, reslang, resdata);
                    iconItems.Add(icondata);                    
                    break;

                case 4:
                    addMenu(resid, resname, reslang, resdata);                    
                    //List<String> menu = ResMenu.parseData(resdata);
                    //addMenu(resid, resname, reslang, menu);                    
                    getDataItem(menus, resid, resname).getItem(reslang).dataBuf = resdata;
                    break;

                case 5:
                    addDialog(resid, resname, reslang, resdata);                    
                    //List<String> dlg = ResDialog.parseData(resdata);
                    //addDialog(resid, resname, reslang, dlg);                    
                    getDataItem(dialogs, resid, resname).getItem(reslang).dataBuf = resdata;
                    break;

                case 6: 
                    List<String> strings = ResStringTable.parseData(resdata);
                    addStringTable(resid, resname, reslang, strings);
                    getDataItem(stringtable, resid, resname).getItem(reslang).dataBuf = resdata;
                    break;

                case 7:
                    addFontDirectory(resid, resname, reslang, resdata);                    
                    getDataItem(fontDirectories, resid, resname).getItem(reslang).dataBuf = resdata;
                    break;

                case 8:
                    addFont(resid, resname, reslang, resdata);
                    getDataItem(fonts, resid, resname).getItem(reslang).dataBuf = resdata;
                    break;

                case 9:
                    List<String> accel = ResAccelerator.parseData(resdata);
                    addAccelerator(resid, resname, reslang, accel);
                    getDataItem(accelerators, resid, resname).getItem(reslang).dataBuf = resdata;
                    break;

                case 10:
                    addUserData(resid, resname, reslang, resdata);
                    getDataItem(userData, resid, resname).getItem(reslang).dataBuf = resdata;
                    break;

                case 12:
                    ResImageGroupData cg = ResImageGroupData.parseData(resdata);
                    addCursorGroup(resid, resname, reslang, cg);
                    getDataItem(cursorGroups, resid, resname).getItem(reslang).dataBuf = resdata;
                    break;

                case 14:
                    ResImageGroupData ig = ResImageGroupData.parseData(resdata);
                    addIconGroup(resid, resname, reslang, ig);
                    getDataItem(iconGroups, resid, resname).getItem(reslang).dataBuf = resdata;
                    break;

                case 16:
                    addVersion(resid, resname, reslang, resdata);

                    //List<String> version = ResVersion.parseData(resdata);
                    //addVersion(resid, resname, reslang, version);
                    getDataItem(versions, resid, resname).getItem(reslang).dataBuf = resdata;
                    break;

                default:
                    addUserData(resid, resname, reslang, resdata);
                    getDataItem(userData, resid, resname).getItem(reslang).dataBuf = resdata;
                    break;
            }
        }

//- icon loading --------------------------------------------------------------

        //we create an .ico file in memory for icon resources
        private byte[] buildIconFile(ResImageGroupData idata)
        {
            //build .ico file header
            byte[] hdrbytes = { 0, 0, 1, 0, 0, 0 };
            hdrbytes[4] = (byte)(idata.entries.Count % 0x100);
            hdrbytes[5] = (byte)(idata.entries.Count / 0x100);

            //build .ico data directory
            int datapos = 6 + (0x10 * idata.entries.Count);
            byte[] dirbytes = new byte[0x10 * idata.entries.Count];
            int dirpos = 0;
            for (int i = 0; i < idata.entries.Count; i++)
            {
                ResImageGroupDataEntry ientry = idata.entries[i];
                dirbytes[dirpos++] = (byte)ientry.bWidth;
                dirbytes[dirpos++] = (byte)ientry.bHeight;
                dirbytes[dirpos++] = (byte)ientry.bColorCount;
                dirbytes[dirpos++] = 0;
                dirbytes[dirpos++] = (byte)(ientry.wPlanes % 0x100);
                dirbytes[dirpos++] = (byte)(ientry.wPlanes / 0x100);
                dirbytes[dirpos++] = (byte)(ientry.wBitCount % 0x100);
                dirbytes[dirpos++] = (byte)(ientry.wBitCount / 0x100);
                byte[] sizebytes = BitConverter.GetBytes(ientry.dwBytesInRes);
                Array.Copy(sizebytes, 0, dirbytes, dirpos, 4);
                byte[] posbytes = BitConverter.GetBytes(datapos);
                Array.Copy(posbytes, 0, dirbytes, dirpos + 4, 4);
                dirpos += 8;
                datapos += (int)ientry.dwBytesInRes;
            }

            byte[] iconbytes = new byte[datapos];                       //total .ico data buf
            Array.Copy(hdrbytes, 0, iconbytes, 0, 6);
            Array.Copy(dirbytes, 0, iconbytes, 6, dirbytes.Length);     //copy the .ico header to it

            //append icon data for each icon in directory
            datapos = 6 + (0x10 * idata.entries.Count);
            ResData image = null;
            for (int i = 0; i < idata.entries.Count; i++)
            {
                ResImageGroupDataEntry ientry = idata.entries[i];
                foreach (ResData imagedata in iconItems)                 //find the matching icon data
                {
                    if (imagedata.id == ientry.nID)
                    {
                        image = imagedata;
                        break;
                    }
                }
                if (image != null)
                {
                    Array.Copy(image.data, 0, iconbytes, datapos, image.data.Length);     //and add it to the data buf
                    datapos += image.data.Length;
                }
            }
            return iconbytes;
        }

        private void createIconResources(ResImageGroupData idata, byte[] iconbytes)
        {
            //now we've re-created the .ico file as a memory stream
            //extract each icon from it by the icon dimensions from the data directory
            MemoryStream ms = new MemoryStream(iconbytes);
            ResData image = null;
            for (int i = 0; i < idata.entries.Count; i++)
            {
                ResImageGroupDataEntry ientry = idata.entries[i];
                ms.Position = 0;
                Icon iconRes = new Icon(ms, (int)ientry.bWidth, (int)ientry.bHeight);       //get the icon
                ientry.image = iconRes;                                                      //link it to this res obj
                foreach (ResData imagedata in iconItems)
                {
                    if (imagedata.id == ientry.nID)
                    {
                        image = imagedata;
                        break;
                    }
                }
                //and add it to the ICON resource list as well
                if (image != null)
                {
                    addIcon(image.id, image.name, image.lang, iconRes);
                    getDataItem(icons, image.id, image.name).getItem(image.lang).dataBuf = image.data;
                }
            }
        }

        //like bmp files (see below) we build an .ico file in memory, and then create an Icon obj from it
        //the complication here is (unlike bitmap resources) the data is split between ICON and ICONGROUP resources
        //so we've stored all the ICON resource data while parsing the resource data & now join head to body
        private void parseIconGroups()
        {
            foreach (ResIconGroup ig in iconGroups)
            {
                foreach (ResourceItem item in ig.items)
                {
                    ResImageGroupData idata = (ResImageGroupData)item.item;     //this contains the .ico file header data
                    byte[] iconbytes = buildIconFile(idata);                    //build .ico file from header & saved icon data
                    createIconResources(idata, iconbytes);                      //extract images from .ico file & add to icon list
                }
            }
            icons.Sort();
        }

//- cursor loading ------------------------------------------------------------

        //the C# Cursor class is limited for displaying as an image, so we use an Icon obj instead
        //we create an .ico file in memory for cursor resources
        private byte[] buildCursorFile(ResImageGroupData idata, int entrynum)
        {
            //build .ico file header
            byte[] hdrbytes = { 0, 0, 1, 0, 1, 0 };

            //build .ico data directory for a single cursor resource
            int datapos = 0x16;
            byte[] dirbytes = new byte[0x10];
            int dirpos = 0;
            ResImageGroupDataEntry ientry = idata.entries[entrynum];
            dirbytes[dirpos++] = (byte)ientry.bWidth;
            dirbytes[dirpos++] = (byte)ientry.bWidth;           //square icon
            dirbytes[dirpos++] = 2;                             //monochrome
            dirbytes[dirpos++] = 0;
            dirbytes[dirpos++] = 1;
            dirbytes[dirpos++] = 0;
            dirbytes[dirpos++] = 1;
            dirbytes[dirpos++] = 0;
            uint bsize = ientry.dwBytesInRes - 4;
            byte[] sizebytes = BitConverter.GetBytes(bsize);
            Array.Copy(sizebytes, 0, dirbytes, dirpos, 4);
            byte[] posbytes = BitConverter.GetBytes(datapos);
            Array.Copy(posbytes, 0, dirbytes, dirpos + 4, 4);
            dirpos += 8;
            datapos += (int)ientry.dwBytesInRes;            

            byte[] cursorbytes = new byte[datapos];                       //total .cur data buf
            Array.Copy(hdrbytes, 0, cursorbytes, 0, 6);
            Array.Copy(dirbytes, 0, cursorbytes, 6, 0x10);                //copy the .cur header to it

            //append cursor data for each icon in directory
            ResData image = null;
            foreach (ResData imagedata in cursorItems)                 //find the matching cursor data
            {
                if (imagedata.id == ientry.nID)
                {
                    image = imagedata;
                    break;
                }
            }
            if (image != null)
            {
                Array.Copy(image.data, 4, cursorbytes, 0x16, image.data.Length - 4);     //and add it to the data buf, skip the hotspot bytes
            }
            return cursorbytes;
        }

        private void createCursorResources(ResImageGroupData idata, byte[] cursorbytes, int entrynum)
        {
            //get the cursor image from the .ico file we've created in memory
            MemoryStream ms = new MemoryStream(cursorbytes);
            ms.Position = 0;
            Icon curRes = new Icon(ms);                                     //get the cursor
            ResImageGroupDataEntry ientry = idata.entries[entrynum];
            ientry.image = curRes;                                          //link it to this res obj
            ResData image = null;
            foreach (ResData curdata in cursorItems)
            {
                if (curdata.id == ientry.nID)
                {
                    image = curdata;
                    break;
                }
            }
            //and add it to the CUSROR resource list as well
            if (image != null)
            {
                addCursor(image.id, image.name, image.lang, curRes);
                getDataItem(cursors, image.id, image.name).getItem(image.lang).dataBuf = image.data;
            }
        }

        private void parseCursorGroups()
        {
            foreach (ResCursorGroup cg in cursorGroups)
            {
                foreach (ResourceItem item in cg.items)
                {
                    ResImageGroupData idata = (ResImageGroupData)item.item;     //this contains the .cur file header data
                    for (int i = 0; i < idata.entries.Count; i++)
                    {
                        byte[] cursorbytes = buildCursorFile(idata, i);         //build .ico file from header & saved cursor data
                        createCursorResources(idata, cursorbytes, i);           //extract images from .ico file & add to icon list
                    }
                }
            }
            cursors.Sort();
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

//- resource CRUD -------------------------------------------------------------

        public void addCursor(uint id, String name, uint language, Icon cursor)
        {
            ResCursor cur = (ResCursor)getDataItem(cursors, id, name);
            if (cur == null)
            {
                cur = new ResCursor(id, name);
                cursors.Add(cur);
            }
            cur.addItem(language, cursor);
        }

        public void deleteCursor(uint id, String name, uint language)
        {
            ResCursor cur = (ResCursor)getDataItem(cursors, id, name);
            if (cur != null)
            {
                cur.deleteItem(language);
            }
        }

        public Icon getCursor(uint id, String name, uint language)
        {
            Icon result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addCursorGroup(uint id, String name, uint language, ResImageGroupData item)
        {
            ResCursorGroup cg = (ResCursorGroup)getDataItem(cursorGroups, id, name);
            if (cg == null)
            {
                cg = new ResCursorGroup(id, name);
                cursorGroups.Add(cg);
            }
            cg.addItem(language, item);
        }

        public void deleteCursorGroup(uint id, String name, uint language)
        {
            ResCursorGroup cg = (ResCursorGroup)getDataItem(cursorGroups, id, name);
            if (cg != null)
            {
                cg.deleteItem(language);
            }
        }

        public ResCursorGroup getCursorGroup(uint id, String name, uint language)
        {
            ResCursorGroup result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addBitmap(uint id, String name, uint language, Bitmap bitmap)
        {
            ResBitmap bmp = (ResBitmap)getDataItem(bitmaps, id, name);
            if (bmp == null)
            {
                bmp = new ResBitmap(id, name);
                bitmaps.Add(bmp);
            }
            bmp.addItem(language, bitmap);
        }

        public void deleteBitmap(uint id, String name, uint language)
        {
            ResBitmap bmp = (ResBitmap)getDataItem(bitmaps, id, name);
            if (bmp != null)
            {
                bmp.deleteItem(language);
            }            
        }

        public Bitmap getBitmap(uint id, String name, uint language)
        {
            Bitmap result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addIcon(uint id, String name, uint language, Icon icon)
        {
            ResIcon resicon = (ResIcon)getDataItem(icons, id, name);
            if (resicon == null)
            {
                resicon = new ResIcon(id, name);
                icons.Add(resicon);
            }
            resicon.addItem(language, icon);
        }

        public void deleteIcon(uint id, String name, uint language)
        {
            ResIcon resicon = (ResIcon)getDataItem(icons, id, name);
            if (resicon != null)
            {
                resicon.deleteItem(language);
            }
        }

        public Icon getIcon(uint id, String name, uint language)
        {
            Icon result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addIconGroup(uint id, String name, uint language, ResImageGroupData item)
        {
            ResIconGroup ig = (ResIconGroup)getDataItem(iconGroups, id, name);
            if (ig == null)
            {
                ig = new ResIconGroup(id, name);
                iconGroups.Add(ig);
            }
            ig.addItem(language, item);
        }

        public void deleteIconGroup(uint id, String name, uint language)
        {
            ResIconGroup ig = (ResIconGroup)getDataItem(iconGroups, id, name);
            if (ig != null)
            {
                ig.deleteItem(language);
            }
        }

        public ResIconGroup getIconGroup(uint id, String name, uint language)
        {
            ResIconGroup result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addMenu(uint id, String name, uint language, byte[] data)
        {
            ResMenu resmenu = (ResMenu)getDataItem(menus, id, name);
            if (resmenu == null)
            {
                resmenu = new ResMenu(id, name);
                menus.Add(resmenu);
            }
            resmenu.addItem(language, data);            
        }

        public void deleteMenu(uint id, String name, uint language)
        {
            ResMenu resmenu = (ResMenu)getDataItem(menus, id, name);
            if (resmenu != null)
            {
                resmenu.deleteItem(language);
            }
        }

        public List<String> getMenu(uint id, String name, uint language)
        {
            List<String> result = null;
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
            dlg.addItem(language, data);            
        }

        public void deleteDialog(uint id, String name, uint language)
        {
            ResDialog dlg = (ResDialog)getDataItem(dialogs, id, name);
            if (dlg != null)
            {
                dlg.deleteItem(language);
            }
        }

        public List<String> getDialog(uint id, String name, uint language)
        {
            List<String> result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addStringTable(uint id, String name, uint language, List<String> str)
        {
            ResStringTable resStr = (ResStringTable)getDataItem(stringtable, id, name);
            if (resStr == null)
            {
                resStr = new ResStringTable(id, name);
                stringtable.Add(resStr);
            }
            resStr.addItem(language, str);
        }

        public void deleteStringResource(uint id, String name, uint language)
        {
            ResStringTable resStr = (ResStringTable)getDataItem(stringtable, id, name);
            if (resStr != null)
            {
                resStr.deleteItem(language);
            }
        }

        public List<String> getStringResource(uint id, String name, uint language)
        {
            List<String> result = null;
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
            fontd.addItem(language, data);
        }

        public void deleteFontDirectory(uint id, String name, uint language)
        {
            ResFontDir fontd = (ResFontDir)getDataItem(fontDirectories, id, name);
            if (fontd != null)
            {
                fontd.deleteItem(language);
            }
        }

        public byte[] getFontDirectory(uint id, String name, uint language)
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
            font.addItem(language, data);
        }

        public void deleteFont(uint id, String name, uint language)
        {
            ResFont font = (ResFont)getDataItem(fonts, id, name);
            if (font != null)
            {
                font.deleteItem(language);
            }
        }

        public byte[] getFont(uint id, String name, uint language)
        {
            byte[] result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addAccelerator(uint id, String name, uint language, List<String> accel)
        {
            ResAccelerator resAccel = (ResAccelerator)getDataItem(accelerators, id, name);
            if (resAccel == null)
            {
                resAccel = new ResAccelerator(id, name);
                accelerators.Add(resAccel);
            }
            resAccel.addItem(language, accel);            
        }

        public void deleteAccelerator(uint id, String name, uint language)
        {
            ResAccelerator resAccel = (ResAccelerator)getDataItem(accelerators, id, name);
            if (resAccel != null)
            {
                resAccel.deleteItem(language);
            }
        }

        public List<String> getAccelerator(uint id, String name, uint language)
        {
            List<String> result = null;
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
            udata.addItem(language, data);
        }

        public void deleteUserData(uint id, String name, uint language)
        {
            ResUserData udata = (ResUserData)getDataItem(userData, id, name);
            if (udata != null)
            {
                udata.deleteItem(language);
            }
        }

        public byte[] getUserData(uint id, String name, uint language)
        {
            byte[] result = null;
            return result;
        }

//-----------------------------------------------------------------------------

        public void addVersion(uint id, String name, uint language, byte[] data)
        {
            ResVersion resVer = (ResVersion)getDataItem(versions, id, name);
            if (resVer == null)
            {
                resVer = new ResVersion(id, name);
                versions.Add(resVer);
            }
            resVer.addItem(language, data);
        }

        public void deleteVersion(uint id, String name, uint language)
        {
            ResVersion resVer = (ResVersion)getDataItem(versions, id, name);
            if (resVer != null)
            {
                resVer.deleteItem(language);
            }
        }

        public List<String> getVersion(uint id, String name, uint language)
        {
            List<String> result = null;
            return result;
        }
    }

//-----------------------------------------------------------------------------
//   RESOURCE DATA CLASSES
//-----------------------------------------------------------------------------

    //base class - this is at the name/id level of the res table tree & has a list of resource items
    public class ResourceData : IComparable<ResourceData>
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

        public void addItem(uint lang, Object obj)
        {
            deleteItem(lang);
            ResourceItem item = new ResourceItem(this, lang, obj);
            items.Add(item);
        }

        public void deleteItem(uint lang)
        {
            foreach (ResourceItem item in items)
            {
                if (item.lang == lang)
                {
                    items.Remove(item);
                    break;
                }
            }
        }

        public ResourceItem getItem(uint lang)
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

        public int CompareTo(ResourceData that)
        {
            return this.id.CompareTo(that.id);
        }
    }

    //this is at the lang level of the res table tree - each resource data has a list of these
    public class ResourceItem
    {
        public ResourceData parent; 
        public uint lang;
        public byte[] dataBuf;      //the raw data - either loaded from exe file, or generated from resource obj
        public Object item;         //the resource obj

        public ResourceItem(ResourceData _parent, uint _lang, Object _item)
        {
            parent = _parent;
            lang = _lang;
            dataBuf = null;
            item = _item;
        }
    }

//-----------------------------------------------------------------------------

    public class ResAccelerator : ResourceData
    {
        public ResAccelerator(uint id, String name)
            : base(id, name)
        {
        }

        public static List<string> parseData(byte[] dataBuf)
        {
            List<string> acceltbl = new List<string>();
            int pos = 0;
            while (pos < dataBuf.Length)
            {
                uint flags = ((uint)dataBuf[pos + 1] * 256) + dataBuf[pos];
                uint ansi = ((uint)dataBuf[pos + 3] * 256) + dataBuf[pos + 2];
                uint id = ((uint)dataBuf[pos + 5] * 256) + dataBuf[pos + 4];
                String accstr = "flags = " + flags.ToString("X4") + " ansi = " + ansi.ToString("X4") + " id = " + id.ToString("X4");
                acceltbl.Add(accstr);
                pos += 8;
            }
            return acceltbl;
        }
    }

//- bitmap --------------------------------------------------------------

    //https://en.wikipedia.org/wiki/BMP_file_format

    public class ResBitmap : ResourceData
    {        

        public ResBitmap(uint id, String name)
            : base(id, name)
        {            
        }

        public static Bitmap parseData(byte[] dataBuf)
        {
            //the bitmap resource data is the same as in a bitmap file, except the header has been removed
            //so we build a header, prepend it to the front of our resource data
            //and create a bitmap from the total data, as if we read it from a file

            Bitmap bitmap;

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

    public class ResCursorGroup : ResourceData
    {
        public ResCursorGroup(uint id, String name)
            : base(id, name)
        {
        }

        public static ResCursorGroup parseData(byte[] resdata)
        {
            throw new NotImplementedException();
        }
    }

    public class ResCursorGroupData
    {
        public static ResCursorGroupData parseData(byte[] resdata)
        {
            ResCursorGroupData cgdata = new ResCursorGroupData();
            SourceFile src = new SourceFile(resdata);
            return cgdata;
        }
    }

//-----------------------------------------------------------------------------

    public class ResDialog : ResourceData
    {
        public ResDialog(uint id, String name)
            : base(id, name)
        {
        }

        public static List<string> parseData(byte[] resdata)
        {
            throw new NotImplementedException();
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

    //https://msdn.microsoft.com/en-us/library/ms997538.aspx

    public class ResIcon : ResourceData
    {
        public Icon icon;

        public ResIcon(uint id, String name)
            : base(id, name)
        {
            icon = null;
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

    public class ResImageGroupData
    {
        public List<ResImageGroupDataEntry> entries;

        public static ResImageGroupData parseData(byte[] resdata)
        {
            ResImageGroupData igdata = new ResImageGroupData();
            SourceFile src = new SourceFile(resdata);
            uint res = src.getTwo();                
            uint type = src.getTwo();
            int count = (int)src.getTwo();
            igdata.entries = new List<ResImageGroupDataEntry>(count);
            for (int i = 0; i < count; i++)
            {
                ResImageGroupDataEntry igentry = ResImageGroupDataEntry.parseData(src);
                igdata.entries.Add(igentry);
            }
            return igdata;
        }
    }

    public class ResImageGroupDataEntry
    {
        public uint bWidth;               // Width, in pixels, of the image
        public uint bHeight;              // Height, in pixels, of the image
        public uint bColorCount;          // Number of colors in image (0 if >=8bpp)
        public uint wPlanes;              // Color Planes
        public uint wBitCount;            // Bits per pixel
        public uint dwBytesInRes;         // how many bytes in this resource?
        public uint nID;                  // the ID
        public Icon image;

        public static ResImageGroupDataEntry parseData(SourceFile src)
        {
            ResImageGroupDataEntry cgdata = new ResImageGroupDataEntry();
            cgdata.bWidth = src.getOne();
            cgdata.bHeight = src.getOne();
            cgdata.bColorCount = src.getOne();
            uint res = src.getOne();
            cgdata.wPlanes = src.getTwo();
            cgdata.wBitCount = src.getTwo();
            cgdata.dwBytesInRes = src.getFour();
            cgdata.nID = src.getTwo();
            cgdata.image = null;
            return cgdata;
        }
    }
    
//-----------------------------------------------------------------------------

    public class ResMenu : ResourceData
    {
        public ResMenu(uint id, String name)
            : base(id, name)
        {
        }

        public static List<string> parseData(byte[] resdata)
        {
            throw new NotImplementedException();
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

        public static void getString(ref int strpos, byte[] dataBuf, List<string> strings)
        {
            int strLen = (int)((dataBuf[strpos + 1] * 256) + dataBuf[strpos]) * 2;
            strpos += 2;
            String str = "";
            if (strLen > 0)
            {
                str = System.Text.Encoding.Unicode.GetString(dataBuf, strpos, strLen);
                strpos += strLen;
            }
            strings.Add(str);            
        }

        public static List<string> parseData(byte[] dataBuf)
        {
            List<string> strings = new List<string>(16);
            int strpos = 0;
            while (strpos < dataBuf.Length)
            {
                getString(ref strpos, dataBuf, strings);
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

        public static List<string> parseData(byte[] resdata)
        {
            throw new NotImplementedException();
        }
    }

//- helper class for loading/saving cursor / icon data ------------------------

    public class ResData
    {
        public uint id;
        public String name;
        public uint lang;
        public byte[] data;

        public ResData(uint _id, String _name, uint _lang, byte[] _data)
        {
            id = _id;
            name = _name;
            lang = _lang;
            data = _data;
        }
    }
}

//Console.WriteLine("there's no sun in the shadow of the wizard");