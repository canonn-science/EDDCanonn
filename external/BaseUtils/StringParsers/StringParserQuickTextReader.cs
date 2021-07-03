/*
 * Copyright © 2018-2020 EDDiscovery development team
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

using BaseUtils.JSON;
using System;
using System.IO;
using System.Linq;

namespace BaseUtils
{
    // uses a text reader to feed in data so string parser is unlimited in length

    [System.Diagnostics.DebuggerDisplay("Action {new string(line,pos,line.Length-pos)} : ({new string(line,0,line.Length)})")]
    public class StringParserQuickTextReader : IStringParserQuick
    {
        // ensure chunksize is big enough to read the longest number

        public StringParserQuickTextReader(TextReader t, int chunksize)
        {
            tr = t;
            line = new char[chunksize];
        }

        public int Position { get { return pos; } }
        public string Line { get { return new string(line, 0, length); } }

        private TextReader tr;
        private char[] line;
        private int length = 0;
        private int pos = 0;

        public bool IsEOL()
        {
            if (pos < length)       // if we have data, its not eol
                return false;
            else
                return !Reload();   // else reload. reload returns true if okay, so its inverted for EOL
        }

        public char GetChar()
        {
            if (pos < length)
                return line[pos++];
            else
            {
                Reload();
                return pos < length ? line[pos++] : char.MinValue;
            }
        }

        public char PeekChar()
        {
            if (pos < length)
                return line[pos];
            else
            {
                Reload();
                return pos < length ? line[pos] : char.MinValue;
            }
        }

        public void SkipSpace()
        {
            while (pos < length && char.IsWhiteSpace(line[pos]))        // first skip what we have
                pos++;

            while (pos == length)                   // if we reached end, then reload, and skip, and repeat if required
            {
                if (!Reload())
                    return;
                while (pos < length && char.IsWhiteSpace(line[pos]))
                    pos++;
            }
        }

        public bool IsStringMoveOn(string s)
        {
            if (pos > length - s.Length)            // if not enough to cover s, reload
                Reload();

            if (pos + s.Length > length)            // if not enough for string, not true
                return false;

            for (int i = 0; i < s.Length; i++)
            {
                if (line[pos + i] != s[i])
                    return false;
            }

            pos += s.Length;
            SkipSpace();

            return true;
        }

        public bool IsCharMoveOn(char t, bool skipspace = true)
        {
            if (pos == length)                          // if at end, reload
                Reload();

            if (pos < length && line[pos] == t)
            {
                pos++;
                if (skipspace)
                    SkipSpace();
                return true;
            }
            else
                return false;
        }

        public void BackUp()
        {
            pos--;
        }

        public int NextQuotedString(char quote, char[] buffer, bool replaceescape = false)
        {
            int bpos = 0;

            while (true)
            {
                if (pos == line.Length)                 // if out of chars, reload
                    Reload();

                if (pos == line.Length || bpos == buffer.Length)  // if reached end of line, or out of buffer, error
                {
                    return -1;
                }
                else if (line[pos] == quote)        // if reached quote, end of string
                {
                    pos++; //skip end quote

                    SkipSpace();

                    return bpos;
                }
                else if (line[pos] == '\\' ) // 2 chars min
                {
                    if (pos >= length - 6)      // this number left for a good go
                    {
                        Reload();
                        if (length < 1)
                            return -1;
                    }

                    pos++;
                    char esc = line[pos++];     // grab escape and move on

                    if (esc == quote)
                    {
                        buffer[bpos++] = esc;      // place in the character
                    }
                    else if (replaceescape)
                    {
                        switch (esc)
                        {
                            case '\\':
                                buffer[bpos++] = '\\';
                                break;
                            case '/':
                                buffer[bpos++] = '/';
                                break;
                            case 'b':
                                buffer[bpos++] = '\b';
                                break;
                            case 'f':
                                buffer[bpos++] = '\f';
                                break;
                            case 'n':
                                buffer[bpos++] = '\n';
                                break;
                            case 'r':
                                buffer[bpos++] = '\r';
                                break;
                            case 't':
                                buffer[bpos++] = '\t';
                                break;
                            case 'u':
                                if (pos < line.Length - 4)
                                {
                                    int? v1 = line[pos++].ToHex();
                                    int? v2 = line[pos++].ToHex();
                                    int? v3 = line[pos++].ToHex();
                                    int? v4 = line[pos++].ToHex();
                                    if (v1 != null && v2 != null && v3 != null && v4 != null)
                                    {
                                        char c = (char)((v1 << 12) | (v2 << 8) | (v3 << 4) | (v4 << 0));
                                        buffer[bpos++] = c;
                                    }
                                }
                                else
                                    return -1;
                                break;
                        }
                    }
                }
                else
                    buffer[bpos++] = line[pos++];
            }
        }

        static char[] decchars = new char[] { '.', 'e', 'E', '+', '-' };

        public JToken JNextNumber(bool sign)     // must be on a digit
        {
            ulong ulv = 0;
            bool bigint = false;
            int start = pos;
            bool slid = false;

            while (true)
            {
                if (pos == line.Length)
                {
                    System.Diagnostics.Debug.Assert(slid == false);         // must not slide more than once
                    Reload(start);              // get more data, keeping text back to start
                    start = 0;                  // start of number now at 0 in buffer
                    slid = true;
                }

                if (pos == line.Length)         // if at end, return number got
                {
                    if (bigint)
                    {
#if JSONBIGINT
                        string part = new string(line, start, pos - start);    // get double string

                        SkipSpace();

                        if (System.Numerics.BigInteger.TryParse(part, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out System.Numerics.BigInteger bv))
                            return new JToken(JToken.TType.BigInt, sign ? -bv : bv);
                        else
#endif
                            return null;
                    }
                    else if (pos == start)      // no chars read
                        return null;
                    else if (ulv <= long.MaxValue)
                        return new JToken(JToken.TType.Long, sign ? -(long)ulv : (long)ulv);
                    else if (sign)
                        return null;
                    else
                        return new JToken(JToken.TType.ULong, ulv);
                }
                else if (line[pos] < '0' || line[pos] > '9')        // if at end of integer..
                {
                    if (line[pos] == '.' || line[pos] == 'E' || line[pos] == 'e')  // if we have gone into a decimal, collect the string and return
                    {
                        while(true)
                        {
                            if (pos == line.Length)
                            {
                                System.Diagnostics.Debug.Assert(slid == false);         // must not slide more than once
                                Reload(start);              // get more data, keeping text back to start
                                start = 0;                  // start of number now at 0 in buffer
                                slid = true;
                            }

                            if (pos < line.Length && ((line[pos] >= '0' && line[pos] <= '9') || decchars.Contains(line[pos])))
                                pos++;
                            else
                                break;
                        }

                        string part = new string(line, start, pos - start);    // get double string

                        SkipSpace();

                        if (double.TryParse(part, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double dv))
                            return new JToken(JToken.TType.Double, sign ? -dv : dv);
                        else
                            return null;
                    }
                    else if (bigint)
                    {
#if JSONBIGINT
                        string part = new string(line, start, pos - start);    // get double string

                        SkipSpace();

                        if (System.Numerics.BigInteger.TryParse(part, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out System.Numerics.BigInteger bv))
                            return new JToken(JToken.TType.BigInt, sign ? -bv : bv);
                        else
#endif
                            return null;
                    }
                    else
                    {
                        if (pos == start)   // this means no chars, caused by a - nothing
                            return null;

                        SkipSpace();

                        if (ulv <= long.MaxValue)
                            return new JToken(JToken.TType.Long, sign ? -(long)ulv : (long)ulv);
                        else if (sign)
                            return null;
                        else
                            return new JToken(JToken.TType.ULong, ulv);
                    }
                }
                else
                {
                    if (ulv > ulong.MaxValue / 10)  // if going to overflow, bit int. collect all ints
                        bigint = true;

                    ulv = (ulv * 10) + (ulong)(line[pos++] - '0');
                }
            }
        }

        private bool Reload(int from = -1)          // from means keep at this position onwards, default is pos.
        {
            if (from == -1)
                from = pos;

            if (from < length)                      // if any left, slide
            {
                Array.Copy(line, from, line, 0, length - from);
                length -= from;
                pos -= from;
            }
            else
            {
                pos = length = 0;
            }

            if (length < line.Length)               // if space left, fill
            {
                length += tr.ReadBlock(line, length, line.Length - length);
            }

            return length > 0;
        }

    }
}
