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

    public sealed class GraphTypeSchema {

        //--- Fields ---
        public readonly AGraphType[] Types;

        public readonly GraphTypeObject QueryType;
        public readonly GraphTypeObject MutationType;
        public readonly GraphDirective[] Directives;

        //--- Constructors ---
        public GraphTypeSchema(AGraphType[] types, GraphTypeObject queryType, GraphTypeObject mutationType, GraphDirective[] directives) {
            AGraphType.ValidateArray(types, nameof(types));
            if(queryType == null) {
                throw new ArgumentNullException(nameof(queryType));
            }
            if(directives == null) {
                throw new ArgumentNullException(nameof(directives));
            }
            Types = types;
            QueryType = queryType;
            MutationType = mutationType;
            Directives = directives;
        }

        //--- Methods ---
        public void AddObjectType(string name) {
            throw new NotImplementedException();
        }

        public void AddEnumType(string name) {
            throw new NotImplementedException();
        }

        public void AddInterfaceType(string name) {
            throw new NotImplementedException();
        }

        public void AddUnionType(string name) {
            throw new NotImplementedException();
        }

        public void AddInputObjectType(string name) {
            throw new NotImplementedException();
        }
    }
}