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

using System.Text;

namespace MindTouch.GraphQL.Syntax {

    public struct Location {

        //--- Class Fields ---
        public static readonly Location None = new Location(null, null, 0, 0);

        //--- Fields ---
        public readonly string Origin;
        public readonly string Path;
        public readonly int Line;
        public readonly int Column;

        //--- Constructors ---
        public Location(string origin, string path = null) {
            this.Origin = origin;
            this.Path = path;
            this.Line = 1;
            this.Column = 0;
        }

        public Location(string origin, string path, int line, int column) {
            this.Origin = origin;
            this.Path = path;
            this.Line = line;
            this.Column = column;
        }

        //--- Properties ---
        public bool HasValue { get { return (Origin != null) || (Path != null) || ((Line != 0) && (Column != 0)); } }

        //--- Methods ---
        public Location AppendPath(string path) {
            return new Location(Origin, Path + path);
        }

        public override string ToString() {
            if(HasValue) {
                var result = new StringBuilder();
                if(Origin != null) {
                    result.Append(Origin);
                }
                if(Path != null) {
                    if(result.Length > 0) {
                        result.Append(", ");
                    }
                    result.Append(Path);
                }
                if((Line != 0) && (Column != 0)) {
                    if(result.Length > 0) {
                        result.Append(", ");
                    }
                    result.AppendFormat("line {0}, column {1}", Line, Column);
                }
                return result.ToString();
            }
            return string.Empty;
        }
    }
}