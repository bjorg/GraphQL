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
using System.Collections.Immutable;

namespace Sandbox.Entities {

    internal sealed class PageBE {

        //--- Fields ---
        public readonly int Id;
        public readonly string Title;
        public readonly int AuthorId;
        public readonly DateTime Created;
        public readonly DateTime Modified;
        public readonly ImmutableArray<int> Subpages;

        //--- Constructors ---
        public PageBE(int id, string title, DateTime created, int authorId, DateTime modified, IEnumerable<int> subpages) {
            Id = id;
            Title = title;
            Created = created;
            AuthorId = authorId;
            Modified = modified;
            Subpages = subpages.ToImmutableArray();
        }
    }
}