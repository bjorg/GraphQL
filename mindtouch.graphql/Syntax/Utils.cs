/*
 * MindTouch GraphQL
 * Copyright (C) 2006-2016 MindTouch, Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit mindtouch.com;
 * please review the licensing section.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Globalization;
using System.Text;

namespace MindTouch.GraphQL.Syntax {

    public static class Utils {

        //--- Class Methods ---
        public static GraphSyntaxLiteralString ToLiteralString(string text) {
            var result = new StringBuilder(text.Length);
            for(var i = 1; i < text.Length - 1; ++i) {
                var c = text[i];
                if((c == '\\') && (++i < text.Length)) {
                    switch(text[i]) {
                    case 'a':
                        result.Append('\a');
                        break;
                    case 'b':
                        result.Append("\b");
                        break;
                    case 'f':
                        result.Append('\f');
                        break;
                    case 'n':
                        result.Append("\n");
                        break;
                    case 'r':
                        result.Append("\r");
                        break;
                    case 't':
                        result.Append("\t");
                        break;
                    case 'u':
                        var code = text.Substring(i + 1, 4);
                        if(code.Length != 4) {
                            throw new FormatException("illegal \\u escape sequence");
                        }
                        result.Append((char)int.Parse(code, NumberStyles.AllowHexSpecifier));
                        i += 4;
                        break;
                    case 'v':
                        result.Append('\v');
                        break;
                    default:
                        result.Append(text[i]);
                        break;
                    }
                } else {
                    result.Append(c);
                }
            }
            return new GraphSyntaxLiteralString(result.ToString());
        }

        public static AGraphSyntaxValue ToLiteralNumber(string text) {
            int intValue;
            if(int.TryParse(text, out intValue)) {
                return new GraphSyntaxLiteralInt(intValue);
            }
            return new GraphSyntaxLiteralFloat(double.Parse(text));
        }

        public static GraphSyntaxDocument Parse(Location location, string source) {
            try {
                var scanner = new Scanner(source, location.Origin, location.Path, location.Line, location.Column, -1);
                var parser = new Parser(scanner);
                parser.Parse();
                return parser.Result;
            } catch(GraphParserException) {
                throw;
            } catch(Exception e) {
                throw new GraphParserException(location, "unexpected parse error: " + e.Message, e);
            }
        }
    }
}