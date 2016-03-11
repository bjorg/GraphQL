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

namespace MindTouch.GraphQL.Schema {

    public sealed class GraphTypeScalar : AGraphType {

        //--- Class Fields ---
        public static readonly GraphTypeScalar Boolean = new GraphTypeScalar("Boolean", /* TODO: missing description */ null, value => Convert.ChangeType(value, typeof(bool)));
        public static readonly GraphTypeScalar Float = new GraphTypeScalar("Float", /* TODO: missing description */ null, value => Convert.ChangeType(value, typeof(double)));
        public static readonly GraphTypeScalar Int = new GraphTypeScalar("Int", /* TODO: missing description */ null, value => Convert.ChangeType(value, typeof(int)));
        public static readonly GraphTypeScalar String = new GraphTypeScalar("String", /* TODO: missing description */ null, value => Convert.ChangeType(value, typeof(string)));

        //--- Fields ---
        private readonly string _name;
        private readonly Func<object, object> _coerce;

        //--- Constructors ---
        private GraphTypeScalar(string name, string description, Func<object, object> coerce) : base(BuildJsonType(GraphTypeKind.SCALAR, name: name, description: description)) {
            GraphUtils.Validate(name, nameof(name));
            if(coerce == null) {
                throw new ArgumentNullException(nameof(coerce));
            }
            _name = name;
            _coerce = coerce;
        }

        //--- Methods ---
        public override object Coerce(object value) => _coerce(value);
        public override string ToString() => _name;
    }
}