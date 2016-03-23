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

using MindTouch.GraphQL.Schema;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace MindTouch.GraphQL.Syntax {

    public enum GraphSyntaxOperationType {
        Query,
        Mutation
    }

    public sealed class GraphSyntaxOperationDefinition : AGraphSyntaxDefinition {

        //--- Fields ---
        public struct Variable {

            //--- Fields ---
            public readonly string Name;
            public readonly AGraphType Type;

            //--- Constructors ---
            public Variable(string name, AGraphType type) {
                if(name == null) {
                    throw new ArgumentNullException(nameof(name));
                }
                if(type == null) {
                    throw new ArgumentNullException(nameof(type));
                }
                Name = name;
                Type = type;
            }
        }

        //--- Fields ---
        public readonly GraphSyntaxOperationType Type;
        public readonly ImmutableArray<Variable> Variables;

        //--- Constructors ---
        public GraphSyntaxOperationDefinition(
                GraphSyntaxOperationType type,
                string name,
                IEnumerable<Variable> variables,
                IEnumerable<GraphSyntaxDirective> directives,
                GraphSyntaxSelectionSet selectionSet
        ) : base(name, directives, selectionSet) {
            if(variables == null) {
                throw new ArgumentNullException(nameof(variables));
            }
            Type = type;
            Variables = variables.ToImmutableArray();
        }
    }
}