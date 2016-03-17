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

using Newtonsoft.Json;
using Sandbox.Queries;
using System;

namespace Sandbox {

    internal class Program {

        //--- Class Methods ---
        private static void Main(string[] args) {
            RunRootQuery();
            Console.Write("Push a key to exit...");
            Console.ReadKey();
        }

        private static void RunRootQuery() {
            /*
            {
                page(1) {
                    title
                    lastModified
                    author {
                        id
                        name
                    }
                    subpages {
                        title
                        author {
                            id
                            name
                        }
                    }
                }
            }
            */
            var server = new SandboxQueryServer();
            var doc = server.Query(root => root.Page(1, page => TaskEx.ToRecord(
                page.Title(),
                page.Modified(),
                page.Author(user => TaskEx.ToRecord(
                    user.Id(),
                    user.Name(),
                    (Id, Name) => new { Id, Name }
                )),
                page.Subpages(subpage => TaskEx.ToRecord(
                    subpage.Title(),
                    subpage.Author(user => TaskEx.ToRecord(
                        user.Id(),
                        user.Name(),
                        (Id, Name) => new { Id, Name }
                    )),
                    (Title, Author) => new { Title, Author }
                )),
                (Title, Modified, Author, Subpages) => new { Title, Modified, Author, Subpages }
            )).Then(Data => new { Data })).Result;
            Console.WriteLine(JsonConvert.SerializeObject(doc, Formatting.Indented));
        }
    }
}