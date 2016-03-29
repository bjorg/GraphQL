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

namespace MindTouch.GraphQL.Syntax {

    public abstract class GraphException : Exception {

        //--- Fields ---
        public Location Location;

        //--- Constructors ---
        protected GraphException(Location location) {
            Location = location;
        }

        protected GraphException(Location location, string message) : base(message) {
            Location = location;
        }

        protected GraphException(Location location, string message, Exception inner) : base(message, inner) {
            Location = location;
        }
    }

    public class GraphParserException : GraphException {

        //--- Constructors ---
        public GraphParserException(Location location, string message) : base(location, message) { }

        public GraphParserException(Location location, string message, Exception inner) : base(location, message, inner) { }
    }
}