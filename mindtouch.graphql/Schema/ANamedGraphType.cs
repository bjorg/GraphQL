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
using Newtonsoft.Json.Linq;

namespace MindTouch.GraphQL.Schema {
    public abstract class ANamedGraphType : AGraphType {

        //--- Fields ---
        public readonly string Name;

        //--- Constructors ---
        protected ANamedGraphType(string name, JObject jsonType) : base(jsonType) {
            if(name == null) {
                throw new ArgumentNullException(nameof(name));
            }
            if(name.Length == 0) {
                throw new ArgumentException("name cannot be empty", nameof(name));
            }
            if(name.StartsWith("__", StringComparison.Ordinal)) {
                throw new ArgumentException("name cannot start with double underscore", nameof(name));
            }
            Name = name;
        }

        //--- Methods ---
        public override string ToString() => Name;
    }
}