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

        //--- Class Methods ---
        private static void Print(IEnumerable<object> stack, int level) {
            foreach(var item in stack.Reverse()) {
                var subStack = item as Stack<object>;
                if(subStack != null) {
                    Print(subStack, level + 1);
                } else {
                    Console.WriteLine(new string(' ', level) + item);
                }
            }
        }

        //--- Methods ---
        public TResult Query<TResult>(Func<IRootQuery, TResult> selection) {
            var stack = new Stack<object>();
            var root = new InspectRootQuery(stack);
            var result =  selection(root);
            Print(stack, 0);
            return result;
        }
    }
}