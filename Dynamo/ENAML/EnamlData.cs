/* ----------------------------------------------------------------------------
Origami ENAML Library
Copyright (C) 2019-2020  George E Greaney

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

//ENAML - (just what) Everybody Needs - Another Markup Language 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Origami.ENAML
{
    public class EnamlData
    {
        ENAMLStem root;

        public EnamlData()
        {
            root = null;
        }

        //---------------------------------------------------------------------
        // READING IN
        //---------------------------------------------------------------------
        
        public static EnamlData loadFromFile(String filename)
        {
            EnamlData enaml = null;
            string[] lines = null;
            try
            {
                lines = File.ReadAllLines(filename);
            }
            catch (Exception e)
            {
                throw new ENAMLException("Error reading ENAML file from disk");
            }
            enaml = new EnamlData();
            enaml.parseRoot(lines);
            return enaml;
        }

        char[] wspace = new char[] { ' ' };

        //no error checking yet!
        private void parseRoot(string[] lines)
        {
            int lineNum = 0;
            root = parseSubtree(lines, ref lineNum);
        }

        private ENAMLStem parseSubtree(string[] lines, ref int lineNum)
        {
            ENAMLStem curStem = new ENAMLStem();
            int indentLevel = -1;
            while (lineNum < lines.Length)
            {
                String line = lines[lineNum++].TrimEnd(wspace);
                if (line.Length == 0 || line[0] == '#')                 //skip blank lines & comments
                {
                    continue;
                }

                int indent = 0;
                while ((indent < line.Length) && (line[indent] == ' ')) indent++;   //get line indent
                if (indentLevel == -1) indentLevel = indent;                        //if first line of subtree, get indent level

                if (indent < indentLevel)
                {
                    lineNum--;              //this line is not in subgroup so back up one line
                    return curStem;
                }
                else
                {
                    line = line.TrimStart(wspace);                              //we have the indent count, remove the leading spaces
                    int colonpos = line.IndexOf(':');
                    String name = line.Substring(0, colonpos).Trim();
                    if (colonpos + 1 != line.Length)                                //nnn : xxx
                    {
                        String val = line.Substring(colonpos + 1).Trim();
                        curStem.children.Add(name, new ENAMLLeaf(val));
                    }
                    else
                    {
                        ENAMLStem substem = parseSubtree(lines, ref lineNum);
                        curStem.children.Add(name, substem);
                    }
                }
            }
            return curStem;
        }

        //- getting values ----------------------------------------------------

        public String getStringValue(String path, String defval)
        {
            if (root != null)
            {
                String strval = findLeafValue(path);
                if (strval != null)
                {
                    return strval;
                }
            }
            return defval;
        }

        public int getIntValue(String path, int defval)
        {
            if (root != null)
            {
                String intstr = findLeafValue(path);
                if (intstr != null)
                {
                    try
                    {
                        int intval = Int32.Parse(intstr);
                        return intval;
                    }
                    catch (Exception e)
                    {
                        throw new ENAMLException("Error reading integer value from ENAML file");
                    }
                }
            }
            return defval;
        }

        public double getFloatValue(String path, double defval)
        {
            if (root != null)
            {
                String floatstr = findLeafValue(path);
                if (floatstr != null)
                {
                    try
                    {
                        double floatval = Double.Parse(floatstr);
                        return floatval;
                    }
                    catch (Exception e)
                    {
                        throw new ENAMLException("Error reading float value from ENAML file");
                    }
                }
            }
            return defval;
        }

        //returns an empty list if the path is invalid
        public List<String> getPathKeys(String path)
        {
            //List<string> keyList = getSubpathKeys(root, path);
            //return keyList;
            return null;
        }

        public List<string> getPathKeysStartingWith(String path, String str)
        {
            throw new NotImplementedException();
        }

        char[] sep = new char[] { '.' };

        private ENAMLStem getSubPath(String path)
        {
            string[] parts = path.Split(sep);
            ENAMLNode subtree = root;
            for (int i = 0; i < parts.Length; i++)
            {
                string name = parts[i];
                if ((subtree is ENAMLStem) && (((ENAMLStem)subtree).children.ContainsKey(name)))
                {
                    subtree = ((ENAMLStem)subtree).children[name];
                }
                else
                {
                    return null;
                }
            }
            if (subtree is ENAMLStem)
            {
                return (ENAMLStem)subtree;
            }
            return null;
        }

        private String findLeafValue(String path)
        {
            String result = null;

            String leafname = "";
            ENAMLStem subpath = null;

            int leafpos = path.LastIndexOf('.');
            if (leafpos != -1)
            {
                path = path.Substring(0, leafpos);      
                leafname = path.Substring(leafpos + 1);     //split the leaf node name from the end of the path
                subpath = getSubPath(path);
            }
            else
            {
                leafname = path;
                subpath = root;
            }

            if ((subpath != null) && (subpath.children.ContainsKey(leafname)))
            {
                ENAMLNode leaf = subpath.children[leafname];
                if (leaf != null && leaf is ENAMLLeaf)
                {
                    result = ((ENAMLLeaf)leaf).value;
                }
            }

            return result;
        }

        private void getSubpathKeys(ENAMLStem subtree, String path)
        {
            String result = null;

            String leafname = "";
            ENAMLStem subpath = null;

            int leafpos = path.LastIndexOf('.');
            if (leafpos != -1)
            {
                path = path.Substring(0, leafpos);
                leafname = path.Substring(leafpos + 1);     //split the leaf node name from the end of the path
                subpath = getSubPath(path);
            }
            else
            {
                leafname = path;
                subpath = root;
            }


                //if (subtree.children.ContainsKey(path))      //at end of path
                //{
                //    ENAMLNode val = subtree.children[path];
                //    if (val != null && val is ENAMLStem)
                //    {
                //        foreach (string key in ((ENAMLStem)val).children.Keys)
                //        {
                //            keyList.Add(key);
                //        }
                //    }
                //}
                        
        }

        //---------------------------------------------------------------------
        // WRITING OUT
        //---------------------------------------------------------------------

        public void saveToFile(String filename)
        {
            List<String> lines = new List<string>();
            storeSubTree(lines, root, "");
            try
            {
                File.WriteAllLines(filename, lines);
            }
            catch (Exception e)
            {
                throw new ENAMLException("Error writing ENAML file to disk");
            }            
        }

        private void storeSubTree(List<string> lines, ENAMLStem stem, String indent)
        {
            List<string> childNameList = new List<string>(stem.children.Keys);
            foreach (String childname in childNameList)
            {
                storeNode(lines, stem.children[childname], indent + ((stem != root) ? "  " : ""), childname);
            }
        }

        private void storeNode(List<string> lines, ENAMLNode node, String indent, String name)
        {
            String line = indent + name + ":";
            if (node is ENAMLLeaf)
            {
                lines.Add(line + " " + ((ENAMLLeaf)node).value);
            }
            else
            {
                lines.Add(line);
                storeSubTree(lines, (ENAMLStem)node, indent);
            }
        }

        //- setting values ----------------------------------------------------

        private void setLeafValue(String path, ENAMLStem subtree, String val)
        {
            int dotpos = path.IndexOf('.');
            if (dotpos != -1)                                                           //path is name.subpath
            {
                String name = path.Substring(0, dotpos);
                String subpath = path.Substring(dotpos + 1);
                if (!subtree.children.ContainsKey(name))
                {
                    subtree.children[name] = new ENAMLStem();
                }
                setLeafValue(subpath, (ENAMLStem)subtree.children[name], val);
            }
            else
            {
                if (!subtree.children.ContainsKey(path))
                {
                    subtree.children[path] = new ENAMLLeaf(val);
                }
                else
                {
                    ((ENAMLLeaf)subtree.children[path]).value = val;
                }
            }
        }

        public void setStringValue(String path, String str)
        {
            if (root == null)
            {
                root = new ENAMLStem();
            }
            setLeafValue(path, root, str);
        }
        
        public void setIntValue(String path, int val)
        {
            String intstr = val.ToString();
            if (root == null)
            {
                root = new ENAMLStem();
            }
            setLeafValue(path, root, intstr);
        }

        public void setFloatValue(String path, double val)
        {
            String floatstr = val.ToString("G17");
            if (root == null)
            {
                root = new ENAMLStem();
            }
            setLeafValue(path, root, floatstr);
        }

        //- internal tree node classes -----------------------------------------------------

        //base class
        private class ENAMLNode
        {
        }

        private class ENAMLStem : ENAMLNode
        {
            public Dictionary<string, ENAMLNode> children;

            public ENAMLStem()
            {
                children = new Dictionary<string, ENAMLNode>();
            }
        }

        private class ENAMLLeaf : ENAMLNode
        {
            public String value;

            public ENAMLLeaf(String val)
            {
                value = val;
            }
        }
    }

    //- exception class -----------------------------------------------------

    public class ENAMLException : Exception
    {
        public ENAMLException(String msg)
            : base(msg)
        {
        }
    }
}
