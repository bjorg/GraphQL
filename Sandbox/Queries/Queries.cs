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
using System.Threading.Tasks;

namespace Sandbox.Queries {

    internal sealed class QueryRunner {

        //--- Fields ---
        private readonly IQuerySource _source;
        private readonly Task<IEnumerable<KeyValuePair<string, Task>>> _done;

        //--- Constructors ---
        public QueryRunner() {
            var completion = new TaskCompletionSource<IEnumerable<KeyValuePair<string, Task>>>();
            _source = new ImmediateQuerySource(0, completion);
            _done = completion.Task;
        }

        //--- Methods ---
        public Task<T> Query<T>(Func<IRootQuery, Task<T>> selection) {
            Task<T> result;
            using(_source) {
                result = selection(new RootQuery(_source));
            }
            _done.ContinueWith(done => {
                var queries = done.Result.ToArray();
                Console.WriteLine($"execute generation RUNNER: START ({queries.Length})");
                foreach(var query in queries) {
                    Console.WriteLine($"[RUNNER] RUN: {query.Key}");
                    query.Value.Start();
                }
                Console.WriteLine($"execute generation RUNNER: DONE");
            });
            return result;
        }
    }

    internal interface IRootQuery {

        //--- Methods ---
        Task<T> Page<T>(int id, Func<IPageQuery, Task<T>> selection);
    }

    internal sealed class RootQuery : IRootQuery {

        //--- Fields ---
        private readonly IQuerySource _source;

        //--- Constructors ---
        public RootQuery(IQuerySource source) {
            _source = source;
        }

        //--- Methods ---
        public Task<T> Page<T>(int id, Func<IPageQuery, Task<T>> selection) {
            using(var source = _source.New()) {
                return selection(new PageQuery(source, id));
            }
        }

        public Task<T> User<T>(int id, Func<IUserQuery, Task<T>> selection) {
            using(var source = _source.New()) {
                return selection(new UserQuery(source, id));
            }
        }
    }

    internal interface IPageQuery {

        //--- Methods ---
        Task<string> Title();
        Task<DateTime> Modified();
        Task<T> Author<T>(Func<IUserQuery, Task<T>> selection);
        Task<T[]> Subpages<T>(Func<IPageQuery, Task<T>> selection);
    }

    internal sealed class PageQuery : IPageQuery {

        //--- Fields ---
        private readonly IQuerySource _source;
        private readonly int _pageId;

        //--- Constructors ---
        public PageQuery(IQuerySource source, int pageId) {
            _source = source;
            _pageId = pageId;
        }

        //--- Methods ---
        public Task<string> Title() => _source.GetPageTitle(_pageId);
        public Task<DateTime> Modified() => _source.GetPageModified(_pageId);

        public Task<T> Author<T>(Func<IUserQuery, Task<T>> selection) {
            var source = _source.New();
            return _source.GetPageAuthorId(_pageId).Then(authorId => selection(new UserQuery(source, authorId)), source);
        }

        public Task<T[]> Subpages<T>(Func<IPageQuery, Task<T>> selection) {
            var source = _source.New();
            return _source.GetPageSubpages(_pageId).Then(ids => Task.WhenAll(ids.Select(id => selection(new PageQuery(source, id)))), source);
        }
    }

    internal interface IUserQuery {

        //--- Methods ---
        Task<string> Name();
        Task<DateTime> Created();
    }

    internal sealed class UserQuery : IUserQuery {

        //--- Fields ---
        private readonly IQuerySource _source;
        private readonly int _userId;

        //--- Constructors ---
        public UserQuery(IQuerySource source, int userId) {
            _source = source;
            _userId = userId;
        }

        //--- Methods ---
        public Task<string> Name() => _source.GetUserName(_userId);
        public Task<DateTime> Created() => _source.GetUserCreated(_userId);
    }
}