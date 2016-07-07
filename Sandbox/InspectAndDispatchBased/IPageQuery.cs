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

namespace Sandbox.InspectAndDispatchBased {

    public interface IPageQuery {

        //--- Methods ---
        string Title { get; }
        DateTime Modified { get; }
        T Author<T>(Func<IUserQuery, T> selection);
        IEnumerable<T> Subpages<T>(Func<IPageQuery, T> selection);
    }

    internal class InspectPageQuery : IPageQuery {

        //--- Fields ---
        private readonly Stack<object> _stack;

        //--- Constructors ---
        public InspectPageQuery(Stack<object> stack) {
            if(stack == null) {
                throw new ArgumentNullException(nameof(stack));
            }
            _stack = stack;
        }

        //--- Properties ---
        public string Title => default(string);
        public DateTime Modified => default(DateTime);

        //--- Methods ---
        public T Author<T>(Func<IUserQuery, T> selection) {
            _stack.Push("IPageQuery.Author()");
            var nested = new Stack<object>();
            _stack.Push(nested);
            selection(new InspectUserQuery(nested));
            return default(T);
        }

        public IEnumerable<T> Subpages<T>(Func<IPageQuery, T> selection) {
            _stack.Push("IPageQuery.Subpages()");
            var stack = new Stack<object>();
            _stack.Push(stack);
            selection(new InspectPageQuery(stack));
            return default(IEnumerable<T>);
        }
    }
}