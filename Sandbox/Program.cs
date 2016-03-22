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
            //RunRootQueryUsingRows();
            RunRootQueryUsingFields();
            Console.Write("Push a key to exit...");
            Console.ReadKey();
        }

        private static void RunRootQueryUsingRows() {
            var server = new SandboxQueryServer<RowBasedQuerySource>();
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

        private static void RunRootQueryUsingFields() {
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

        private static void RunRootQueryUsingExpressions() {
            IRootQuery2 root = null;
            var doc = new {
                Data = root.Page(1, page => new {
                    page.Title,
                    page.Modified,
                    Author = page.Author(user => new {
                        user.Id,
                        user.Name
                    }),
                    Subpages = page.Subpages(subpage => new {
                        subpage.Title,
                        Author = subpage.Author(user => new {
                            user.Id,
                            user.Name
                        })
                    })
                })
            };
            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(doc, Formatting.Indented));
        }
    }
}