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
using System.Collections.Generic;
using System.Collections.Immutable;

namespace MindTouch.GraphQL.Syntax {

    public abstract class AGraphSyntaxValue { }

    public sealed class GraphSyntaxLiteralBool : AGraphSyntaxValue {

        //--- Fields ---
        public readonly bool Value;

        //--- Constructors ---
        public GraphSyntaxLiteralBool(bool value) {
            Value = value;
        }
    }

    public sealed class GraphSyntaxLiteralInt : AGraphSyntaxValue {

        //--- Fields ---
        public readonly int Value;

        //--- Constructors ---
        public GraphSyntaxLiteralInt(int value) {
            Value = value;
        }
    }

    public sealed class GraphSyntaxLiteralFloat : AGraphSyntaxValue {

        //--- Fields ---
        public readonly double Value;

        //--- Constructors ---
        public GraphSyntaxLiteralFloat(double value) {
            Value = value;
        }
    }

    public sealed class GraphSyntaxLiteralString : AGraphSyntaxValue {

        //--- Fields ---
        public readonly string Value;

        //--- Constructors ---
        public GraphSyntaxLiteralString(string value) {
            if(value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            Value = value;
        }
    }

    public sealed class GraphSyntaxLiteralEnum : AGraphSyntaxValue {

        //--- Fields ---
        public readonly string Value;

        //--- Constructors ---
        public GraphSyntaxLiteralEnum(string value) {
            if(value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            Value = value;
        }
    }

    public sealed class GraphSyntaxVariable : AGraphSyntaxValue {

        //--- Fields ---
        public readonly string Name;

        //--- Constructors ---
        public GraphSyntaxVariable(string name) {
            if(name == null) {
                throw new ArgumentNullException(nameof(name));
            }
            Name = name;
        }
    }

    public sealed class GraphSyntaxList : AGraphSyntaxValue {

        //--- Fields ---
        public readonly ImmutableArray<AGraphSyntaxValue> Values;

        //--- Constructors ---
        public GraphSyntaxList(IEnumerable<AGraphSyntaxValue> values) {
            if(values == null) {
                throw new ArgumentNullException(nameof(values));
            }
            Values = values.ToImmutableArray();
        }
    }

    public sealed class GraphSyntaxInputObject : AGraphSyntaxValue {

        //--- Types ---
        public sealed class Field {

            //--- Fields ---
            public readonly string Name;
            public readonly AGraphSyntaxValue Value;

            //--- Constructors ---
            public Field(string name, AGraphSyntaxValue value) {
                if(name == null) {
                    throw new ArgumentNullException(nameof(name));
                }
                if(value == null) {
                    throw new ArgumentNullException(nameof(value));
                }
                Name = name;
                Value = value;
            }
        }

        //--- Fields ---
        public readonly ImmutableArray<Field> Fields;

        //--- Constructors ---
        public GraphSyntaxInputObject(IEnumerable<Field> fields) {
            if(fields == null) {
                throw new ArgumentNullException(nameof(fields));
            }
            Fields = fields.ToImmutableArray();
        }
    }
}