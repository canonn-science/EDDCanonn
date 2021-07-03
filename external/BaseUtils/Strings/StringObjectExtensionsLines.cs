/*
 * Copyright © 2016 - 2019 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class ObjectExtensionsLineStrings
{
    static public int Lines(this string s, string lineterm = null)      // Newline standard  
    {
        if (lineterm == null)
            lineterm = Environment.NewLine;

        int count = 1;         // supposedly the fastest https://www.codeproject.com/Tips/312312/Counting-Lines-in-a-String
        int position = 0;
        while ((position = s.IndexOf(lineterm, position)) != -1)
        {
            count++;
            position++;         // Skip this occurrence!
        }
        return count;
    }

    static public string LineLimit(this string s, int limit, string cutindicator)   // love extension methods
    {
        int count = 1;
        int position = 0;
        while ((position = s.IndexOf(Environment.NewLine, position)) != -1)
        {
            if (count == limit)
            {
                return s.Substring(0, position) + cutindicator;
            }

            count++;
            position++;         // Skip this occurrence!
        }

        return s;
    }

    static public string LineNumbering(this string s, int start, string fmt = "N", string newline = null)   
    {
        if (newline == null)
            newline = Environment.NewLine;

        StringBuilder sb = new StringBuilder();
        int position = 0, positions = 0;
        while ((positions = s.IndexOf(newline, position)) != -1)
        {
            sb.Append(start.ToStringInvariant(fmt));
            sb.Append(':');
            sb.Append(s.Substring(position, positions - position));
            sb.Append(newline);
            position = positions + newline.Length;
            start++;
        }

        if (position < s.Length)
            sb.Append(s.Substring(position));

        return sb.ToNullSafeString();
    }

    static public string LineTextInsersion(this string s, string insertatlinestart, string insertafternewline = "", string newline = null)
    {
        if (newline == null)
            newline = Environment.NewLine;

        StringBuilder sb = new StringBuilder();
        int position = 0, positions = 0;
        while ((positions = s.IndexOf(newline, position)) != -1)
        {
            sb.Append(insertatlinestart);
            sb.Append(s.Substring(position, positions - position));
            sb.Append(newline);
            sb.Append(insertafternewline);
            position = positions + newline.Length;
        }

        if (position < s.Length)
        {
            sb.Append(insertatlinestart);
            sb.Append(s.Substring(position));
            sb.Append(newline);
            sb.Append(insertafternewline);
        }

        return sb.ToNullSafeString();
    }

    // outputstart = 1 to N, if less than 1, removes it off count. (so -1 count 3 only produces line 1 (-1,0 removed))
    // count = number of lines to show
    // markline = mark line number N (1..)
    // startlineno = logical number to add onto line number in case its come from a split string
    static public string LineMarking(this string s, int outputstart, int count, string lineformat = null, int markline = -1, int startlinenooffset = 0, string newline = null )
    {
        if (newline == null)
            newline = Environment.NewLine;

        string ret = string.Empty;
        if (outputstart < 1)
            count += outputstart - 1;
        if (count < 1)
            return ret;

        int position = 0, newposition;
        int lineno = 0;

        while ((newposition = s.IndexOf(newline, position)) != -1)
        {
            lineno++;
            if (lineno >= outputstart)
            {
                if (markline >= 1)
                    ret += (lineno == markline) ? ">>> " : "";
                if (lineformat != null)
                    ret += (startlinenooffset + lineno).ToStringInvariant(lineformat) + ": ";
                ret += s.Substring(position, newposition - position) + newline;
                if (--count <= 0)
                    return ret;
            }

            position = newposition + newline.Length;
        }

        return ret;
    }


    // returns text between starttoken (excluded) and endtoken (excluded), returning line start number
    static public string CutBetween(this string s, string starttoken, string endtoken, ref int linestart, string newline = null)
    {
        if (newline == null)
            newline = Environment.NewLine;

        int position = 0, newposition;
        int lineno = 0;

        int startpos = -1;

        while ((newposition = s.IndexOf(newline, position)) != -1)      // find next LF
        {
            lineno++;

            string curline = s.Substring(position, newposition - position);
            System.Diagnostics.Debug.WriteLine("Line {0} {1}", lineno, curline);

            if (startpos == -1 && curline.Trim().StartsWith(starttoken))
            {
                startpos = newposition + newline.Length;
                linestart = lineno + 1;
            }
            else if (curline.Trim().StartsWith(endtoken))
            {
                return s.Substring(startpos, position - startpos);
            }
            position = newposition + newline.Length; 
        }

        if (startpos >= 0)
            return s.Substring(startpos);
        else
            return null;
    }

    // Format into lines, breaking at linelimit.
    public static string FormatIntoLines(this IEnumerable<string> list, int linelimit = 80, string newline = null)
    {
        if (newline == null)
            newline = Environment.NewLine;

        StringBuilder res = new StringBuilder();
        int lastlf = 0;

        foreach (var s in list)
        {
            if (res.Length != lastlf)
                res.Append(", ");
            res.Append(s);
            if (res.Length - lastlf >= linelimit)
            {
                res.Append(newline);
                lastlf = res.Length;
            }
        }

        return res.ToNullSafeString();
    }


    static public string StackTrace(this string trace, string enclosingfunc, int lines)
    {
        int offset = trace.IndexOf(enclosingfunc);

        string ret = "";

        if (offset != -1)
        {
            CutLine(ref trace, offset);

            while (lines-- > 0)
            {
                string l = CutLine(ref trace, 0);
                if (l != "")
                {
                    if (ret != "")
                        ret = ret + Environment.NewLine + l;
                    else
                        ret = l;
                }
                else
                    break;
            }
        }
        else
            ret = trace;

        return ret;
    }

    static public string CutLine(ref string trace, int offset)
    {
        int nloffset = trace.IndexOf(Environment.NewLine, offset);
        string ret;
        if (nloffset != -1)
        {
            ret = trace.Substring(offset, nloffset - offset);
            trace = trace.Substring(nloffset);
            if (trace.Length >= Environment.NewLine.Length)
                trace = trace.Substring(Environment.NewLine.Length);
        }
        else
        {
            ret = trace;
            trace = "";
        }

        return ret;
    }


}

