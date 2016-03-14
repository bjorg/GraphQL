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

    public abstract class AGraphSyntaxSelection {

        //--- Fields ---
        public readonly ImmutableArray<GraphSyntaxDirective> Directives;

        //--- Constructors ---
        protected AGraphSyntaxSelection(IEnumerable<GraphSyntaxDirective> directives) {
            if(directives == null) {
                throw new ArgumentNullException(nameof(directives));
            }
            Directives = directives.ToImmutableArray();
        }
    }

    public sealed class GraphSyntaxFieldSelection : AGraphSyntaxSelection {

        //--- Fields ---
        public readonly string Alias;
        public readonly string Name;
        public readonly ImmutableArray<GraphSyntaxArgument> Arguments;
        public readonly GraphSyntaxSelectionSet SelectionSet;

        //--- Constructors ---
        public GraphSyntaxFieldSelection(string alias, string name, IEnumerable<GraphSyntaxArgument> arguments, IEnumerable<GraphSyntaxDirective> directives, GraphSyntaxSelectionSet selectionSet) : base(directives) {
            if(name == null) {
                throw new ArgumentNullException(nameof(name));
            }
            if(arguments == null) {
                throw new ArgumentNullException(nameof(arguments));
            }
            Alias = alias;
            Name = name;
            Arguments = arguments.ToImmutableArray();
            SelectionSet = selectionSet;
        }
    }

    public sealed class GraphSyntaxFragmentSpread : AGraphSyntaxSelection {

        //--- Fields ---
        public readonly string Name;

        //--- Constructors ---
        public GraphSyntaxFragmentSpread(string name, IEnumerable<GraphSyntaxDirective> directives) : base(directives) {
            if(name == null) {
                throw new ArgumentNullException(nameof(name));
            }
            Name = name;
        }
    }

    public sealed class GraphSyntaxInlineFragment : AGraphSyntaxSelection {

        //--- Fields ---
        public readonly GraphSyntaxTypeName OnTypeCondition;
        public readonly GraphSyntaxSelectionSet SelectionSet;

        //--- Constructors ---
        public GraphSyntaxInlineFragment(GraphSyntaxTypeName onTypeCondition, IEnumerable<GraphSyntaxDirective> directives, GraphSyntaxSelectionSet selectionSet) : base(directives) {
            if(selectionSet == null) {
                throw new ArgumentNullException(nameof(selectionSet));
            }
            OnTypeCondition = onTypeCondition;
            SelectionSet = selectionSet;
        }
    }
}