﻿/*
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
using Sandbox.Logic;
using Sandbox.Queries;
using System;
using System.Threading.Tasks;

namespace Sandbox {

    internal sealed class QueryRunner<TQuery> {

        //--- Fields ---
        private readonly TQuery _query;

        //--- Constructors ---
        public QueryRunner(TQuery query) {
            _query = query;
        }

        //--- Methods ---
        public Task<T> Query<T>(Func<TQuery, Task<T>> selection) => selection(_query);
    }

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
                        name
                    }
                }
            }
            */
            var runner = new QueryRunner<IRootQuery>(new RootQuery(new ImmediateQuerySource()));
            var doc = runner.Query(root => root.Page(1, page => TaskEx.ContinueWithTuple(page.Title(), page.Modified(), page.Author(user => user.Name().Then(name => new {
                Name = name
            }))).Then(tuple => new {
                Title = tuple.Item1,
                Modified = tuple.Item2,
                Author = tuple.Item3
            })).Then(data => new {
                Data = data
            }));
            Console.WriteLine(JsonConvert.SerializeObject(doc.Result, Formatting.Indented));
        }
    }
}