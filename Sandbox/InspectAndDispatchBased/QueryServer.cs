/*
 * MindTouch
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
using System.Linq;

namespace Sandbox.InspectAndDispatchBased {

    internal sealed class QueryServer {

        //--- Types ---
        private sealed class QuerySource : IQuerySource {

            //--- Types ---
            private sealed class Node {

                //--- Fields ---
                public readonly Node Parent;
                public readonly List<string> Actions = new List<string>();
                public readonly List<Node> ChildNodes = new List<Node>();

                //--- Constructors ---
                public Node(Node parent) {
                    Parent = parent;
                }
            }

            //--- Class Methods ---
            private static void Print(Node node, int level) {
                Console.WriteLine(new string(' ', level) + " " + string.Join(", ", node.Actions));
                foreach(var child in node.ChildNodes) {
                    if(child.Actions.Any()) {
                        Print(child, level + 1);
                    }
                }
            }

            //--- Fields ---
            private readonly Node _root;
            private Node _current;

            //---- Constructors ---
            public QuerySource() {
                _root = new Node(null);
                _current = _root;
            }

            //--- Methosd ---
            public void Dispose() {
                _current = _current.Parent;
            }

            public IQuerySource OpenNested() {
                var parent = _current;
                _current = new Node(parent);
                parent.ChildNodes.Add(_current);
                return this;
            }

            public void FetchPageById() {
                _current.Actions.Add(nameof(FetchPageById));
            }

            public void FetchUserById() {
                _current.Actions.Add(nameof(FetchUserById));
            }

            public void FetchSubpagesById() {
                _current.Actions.Add(nameof(FetchSubpagesById));
            }

            public void Print() {
                Print(_root, 0);
            }
        }

        //--- Methods ---
        public TResult Query<TResult>(Func<IRootQuery, TResult> selection) {
            using(var source = new QuerySource()) {
                selection(new InspectRootQuery(source));
                source.Print();
            }
            return default(TResult);
        }
    }
}