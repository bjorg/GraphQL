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
using System;
using System.Threading.Tasks;

namespace Sandbox {

    internal interface IRootQuery {

        //--- Methods ---
        Task<T> Page<T>(int id, Func<IPageQuery, Task<T>> selection);
    }

    internal sealed class RootQuery : IRootQuery {

        //--- Fields ---
        public readonly IQuerySource _source;

        //--- Constructors ---
        public RootQuery(IQuerySource source) {
            _source = source;
        }

        //--- Methods ---
        public Task<T> Page<T>(int id, Func<IPageQuery, Task<T>> selection) => selection(new PageQuery(_source, _source.GetPageById(id)));
    }

    internal interface IPageQuery {

        //--- Methods ---
        Task<string> Title();
        Task<DateTime> Modified();
        Task<T> Author<T>(Func<IUserQuery, Task<T>> selection);
    }

    internal sealed class PageQuery : IPageQuery {

        //--- Fields ---
        private readonly IQuerySource _source;
        private readonly Task<PageBE> _page;

        //--- Constructors ---
        public PageQuery(IQuerySource source, Task<PageBE> page) {
            _source = source;
            _page = page;
        }

        //--- Methods ---
        public Task<string> Title() => _page.Then(page => page.Title);
        public Task<DateTime> Modified() => _page.Then(page => page.Modified);
        public Task<T> Author<T>(Func<IUserQuery, Task<T>> selection) => _page.Then(page => selection(new UserQuery(_source, _source.GetUserById(page.AuthorId))));
    }

    internal interface IUserQuery {

        //--- Methods ---
        Task<string> Name();
        Task<DateTime> Created();
    }

    internal sealed class UserQuery : IUserQuery {

        //--- Fields ---
        private readonly IQuerySource _source;
        private readonly Task<UserBE> _user;

        //--- Constructors ---
        public UserQuery(IQuerySource source, Task<UserBE> user) {
            _source = source;
            _user = user;
        }

        //--- Methods ---
        public Task<string> Name() => _user.Then(user => user.Name);
        public Task<DateTime> Created() => _user.Then(user => user.Created);
    }

    internal class Program {

        //--- Class Methods ---
        private static void Main(string[] args) {
            RunPageQuery(new RootQuery(new ImmediateQuerySource()));
            Console.Write("Push a key to exit...");
            Console.ReadKey();
        }

        private static void RunPageQuery(IRootQuery root) {
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

            var doc = root.Page(1, page => TaskEx.Tuple(page.Title(), page.Modified(), page.Author(user => user.Name().Then(name => new {
                Name = name
            }))).Then(tuple => new {
                Title = tuple.Item1,
                Created = tuple.Item2,
                Author = tuple.Item3
            }));
            Console.WriteLine(JsonConvert.SerializeObject(doc.Result, Formatting.Indented));
        }
    }
}