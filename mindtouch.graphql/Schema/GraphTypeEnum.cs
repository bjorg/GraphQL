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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace MindTouch.GraphQL.Schema {

    public sealed class GraphTypeEnum : ANamedGraphType {

        //--- Types ---
        public sealed class Value {

            //--- Fields ---
            public readonly string Name;
            public readonly string Description;
            public readonly bool IsDeprecated;
            public readonly string DeprecationReason;
            public readonly JObject JsonType;

            //--- Constructors ---
            public Value(string name, string description, bool isDeprecated = false, string deprecationReason = null) {
                GraphUtils.Validate(name, nameof(name));
                Name = name;
                Description = description;
                IsDeprecated = isDeprecated;
                DeprecationReason = deprecationReason;
                JsonType = new JObject {{ "name", Name }};
                if(description != null) {
                    JsonType.Add("description", description);
                }
                if(isDeprecated) {
                    JsonType.Add("isDeprecated", IsDeprecated);
                    if(deprecationReason != null) {
                        JsonType.Add("deprecationReason", deprecationReason);
                    }
                }
            }
        }

        //--- Fields ---
        public readonly ImmutableArray<Value> Values;

        //--- Methods ---
        public GraphTypeEnum(string name, string description, IEnumerable<Value> values) : base(name, BuildJsonType(GraphTypeKind.ENUM, name, description, enumValues: values)) {
            GraphUtils.Validate(values, nameof(values));
            Values = values.ToImmutableArray();
        }

        //--- Methods ---
        public override object Coerce(object value) {
            throw new NotImplementedException();
        }
    }
}