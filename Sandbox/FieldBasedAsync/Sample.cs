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
using Newtonsoft.Json;

namespace Sandbox.FieldBasedAsync {

    internal class Sample {

        //--- Class Methods ---
        internal static void Run() {
            var server = new SandboxQueryServer<FieldBasedQuerySource>();
            var doc = server.Query(root => root.Page(1, page => TaskEx.Record(
                (Title, Modified, Author, Subpages) => new { Title, Modified, Author, Subpages },
                page.Title(),
                page.Modified(),
                page.Author(user => TaskEx.Record(
                    (Id, Name) => new { Id, Name },
                    user.Id(),
                    user.Name()
                )), page.Subpages(subpage => TaskEx.Record(
                    (Title, Author) => new { Title, Author },
                    subpage.Title(),
                    subpage.Author(user => TaskEx.Record(
                        (Id, Name) => new { Id, Name },
                        user.Id(),
                        user.Name()
                    ))
                ))
            )).Then(Data => new { Data })).Result;
            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(doc, Formatting.Indented));
        }
    }
}
