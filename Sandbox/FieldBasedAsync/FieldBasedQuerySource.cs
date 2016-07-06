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
using Sandbox.Entities;

namespace Sandbox.FieldBasedAsync {

    internal sealed class FieldBasedQuerySource : IQuerySource {

        //--- Types ---
        private enum Source {
            PAGES,
            SUBPAGES,
            USERS
        }

        private class Request {

            //--- Fields ---
            public readonly Source Source;
            public readonly int Key;
            public readonly string Field;

            //--- Constructors ---
            public Request(Source source, int key, string field) {
                Source = source;
                Key = key;
                Field = field;
            }
        }

        private struct Row {

            //--- Methods ---
            public readonly Dictionary<string, object> Fields;

            //--- Constructors ---
            public Row(Dictionary<string, object> fields) {
                Fields = fields;
            }
        }

        private sealed class Scheduler {

            //--- Fields ---
            private readonly Dictionary<int, int> _counters = new Dictionary<int, int>();
            private readonly Dictionary<int, List<Tuple<Request, TaskCompletionSource<object>>>> _requests = new Dictionary<int, List<Tuple<Request, TaskCompletionSource<object>>>>();

            //--- Methods ---
            public void Begin(int generation) {
                lock (_counters) {
                    int counter;
                    if(!_counters.TryGetValue(generation, out counter)) {
                        _requests[generation] = new List<Tuple<Request, TaskCompletionSource<object>>>();
                    }
                    _counters[generation] = ++counter;
                }
            }

            public Task<object> Add(int generation, Request request) {
                var completion = new TaskCompletionSource<object>();
                lock (_counters) {
                    var tasks = _requests[generation];
                    tasks.Add(new Tuple<Request, TaskCompletionSource<object>>(request, completion));
                }
                return completion.Task;
            }

            public void End(int generation) {
                List<Tuple<Request, TaskCompletionSource<object>>> requestTuples = null;
                lock (_counters) {
                    int counter;
                    if(_counters.TryGetValue(generation, out counter)) {
                        switch(counter--) {
                        case 0:
                            throw new InvalidOperationException("counter is 0");
                        case 1:
                            _counters.Remove(generation);
                            requestTuples = _requests[generation];
                            _requests.Remove(generation);
                            break;
                        default:
                            _counters[generation] = counter;
                            break;
                        }
                    } else {
                        throw new InvalidOperationException("counter not found");
                    }
                }

                //1) Aggregate Field names for same Source and Key
                //    [(Source,[Key],[Field1])]
                //    +
                //    [(Source,[Key],[Field2])]
                //    =>
                //    [(Source,[Key],[Field1, Field2])]

                //2) Aggregate Key values for same Source and Field
                //    [(Source,[Key1],[Field])]
                //    +
                //    [(Source,[Key2],[Field])]
                //    =>
                //    [(Source,[Key1, Key2],[Field])]

                if(requestTuples != null) {
                    Console.WriteLine($"/* Batch {generation} */");
                    foreach(var bySource in requestTuples.ToLookup(r => r.Item1.Source)) {

                        // colummnSet -> key -> field -> List<TaskCompletionSource<object>>

                        var batchByColumns = new Dictionary<string, Dictionary<int, Dictionary<string, List<TaskCompletionSource<object>>>>>();
                        foreach(var byKey in bySource.ToLookup(r => r.Item1.Key)) {

                            // group by column-set
                            var columns = string.Join("-", byKey.Select(r => r.Item1.Field).Distinct().OrderBy(k => k).ToArray());
                            Dictionary<int, Dictionary<string, List<TaskCompletionSource<object>>>> batchByColumnsAndKey;
                            if(!batchByColumns.TryGetValue(columns, out batchByColumnsAndKey)) {
                                batchByColumnsAndKey = new Dictionary<int, Dictionary<string, List<TaskCompletionSource<object>>>>();
                                batchByColumns.Add(columns, batchByColumnsAndKey);
                            }

                            // capture keys that what the captured column-set
                            Dictionary<string, List<TaskCompletionSource<object>>> batchByColumnsAndKeyAndField;
                            if(!batchByColumnsAndKey.TryGetValue(byKey.Key, out batchByColumnsAndKeyAndField)) {
                                batchByColumnsAndKeyAndField = new Dictionary<string, List<TaskCompletionSource<object>>>();
                                batchByColumnsAndKey.Add(byKey.Key, batchByColumnsAndKeyAndField);
                            }

                            // store completions for requested fields in column-set
                            foreach(var requestTuple in byKey) {
                                List<TaskCompletionSource<object>> tasks;
                                if(!batchByColumnsAndKeyAndField.TryGetValue(requestTuple.Item1.Field, out tasks)) {
                                    tasks = new List<TaskCompletionSource<object>>();
                                    batchByColumnsAndKeyAndField[requestTuple.Item1.Field] = tasks;
                                }
                                tasks.Add(requestTuple.Item2);
                            }
                        }
                        string table;
                        switch(bySource.Key) {
                        case Source.PAGES:
                            table = "pages";
                            break;
                        case Source.SUBPAGES:
                            table = "subpages";
                            break;
                        case Source.USERS:
                            table = "users";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                        }
                        foreach(var b1 in batchByColumns) {
                            var keys = b1.Value.Keys.OrderBy(key => key).ToArray();
                            var columns = b1.Value.First().Value.Keys.OrderBy(column => column).ToArray();
                            Console.WriteLine($"SELECT id, {string.Join(", ", columns)} FROM {table} WHERE id IN ({string.Join(", ", keys.Select(key => key.ToString()))});");
                            foreach(var key in keys) {
                                Row row;
                                switch(bySource.Key) {
                                case Source.PAGES:
                                    row = ToRow(_pages[key]);
                                    break;
                                case Source.SUBPAGES:
                                    row = ToRow(key, _subpages[key]);
                                    break;
                                case Source.USERS:
                                    row = ToRow(_users[key]);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                                }
                                foreach(var column in columns) {
                                    var value = row.Fields[column];
                                    foreach(var completion in b1.Value[key][column]) {
                                        completion.SetResult(value);
                                    }
                                }
                            }
                        }
                    }

                    // execute queries the old-fashioned way
                    //foreach(var requestTuple in requestTuples) {
                    //    var request = requestTuple.Item1;
                    //    var completion = requestTuple.Item2;
                    //    switch(request.Source) {
                    //    case Source.PAGES:
                    //        completion.SetResult(ToRow(_pages[request.Key]).Fields[request.Field]);
                    //        break;
                    //    case Source.USERS:
                    //        completion.SetResult(ToRow(_users[request.Key]).Fields[request.Field]);
                    //        break;
                    //    default:
                    //        completion.SetException(new ArgumentOutOfRangeException());
                    //        break;
                    //    }
                    //}
                }
            }
        }

        //--- Class Fields ---
        private static readonly Dictionary<int, PageBE> _pages;
        private static readonly Dictionary<int, IEnumerable<int>> _subpages;
        private static readonly Dictionary<int, UserBE> _users;

        //--- Class Constructor ---
        static FieldBasedQuerySource() {
            _pages = new Dictionary<int, PageBE> {
                { 1, new PageBE(1, "Homepage", new DateTime(2010, 3, 31, 17, 23, 00), 42, new DateTime(2016, 2, 27, 11, 35, 00), new[] { 2, 3, 4 }) },
                { 2, new PageBE(2, "Subpage 1", new DateTime(2010, 3, 31, 17, 23, 01), 13, new DateTime(2016, 2, 27, 11, 35, 01), new int[0]) },
                { 3, new PageBE(3, "Subpage 2", new DateTime(2010, 3, 31, 17, 23, 02), 42, new DateTime(2016, 2, 27, 11, 35, 02), new int[0]) },
                { 4, new PageBE(4, "Subpage 3", new DateTime(2010, 3, 31, 17, 23, 03), 13, new DateTime(2016, 2, 27, 11, 35, 03), new int[0]) }
            };
            _subpages = new Dictionary<int, IEnumerable<int>>();
            foreach(var page in _pages.Values) {
                _subpages[page.Id] = page.Subpages;
            }
            _users = new Dictionary<int, UserBE> {
                { 13, new UserBE(13, "Jane Doe", new DateTime(1976, 04, 30, 11, 00, 00)) },
                { 42, new UserBE(42, "John Doe", new DateTime(1972, 09, 23, 16, 00, 00)) }
            };
        }

        //--- Class Methods ---
        private static Row ToRow(PageBE page) {
            return new Row(new Dictionary<string, object> {
                { "id", page.Id },
                { "title", page.Title },
                { "created", page.Created },
                { "modified", page.Modified },
                { "authorId", page.AuthorId },
                { "subpageids", page.Subpages }
            });
        }

        private static Row ToRow(UserBE user) {
            return new Row(new Dictionary<string, object> {
                { "id", user.Id },
                { "name", user.Name },
                { "created", user.Created }
            });
        }

        private static Row ToRow(int key, IEnumerable<int> values) {
            return new Row(new Dictionary<string, object> {
                { "id", key },
                { "subpageId", values }
            });
        }

        //--- Fields ---
        private readonly int _generation;
        private readonly Scheduler _scheduler;
        private bool _disposed;

        //--- Constructors ---
        public FieldBasedQuerySource() : this(0, new Scheduler()) { }

        private FieldBasedQuerySource(int generation, Scheduler scheduler) {
            _scheduler = scheduler;
            _generation = generation;
            _scheduler.Begin(_generation);
        }

        //--- Methods ---
        public IQuerySource New() {
            if(_disposed) {
                throw new ObjectDisposedException("already disposed");
            }
            return new FieldBasedQuerySource(_generation + 1, _scheduler);
        }

        public void Dispose() {
            if(_disposed) {
                throw new ObjectDisposedException("already disposed");
            }
            _disposed = true;
            _scheduler.End(_generation);
        }

        public Task<string> GetPageTitle(int id) {
            return Run(new Request(Source.PAGES, id, "title")).Then(value => (string)value);
        }

        public Task<DateTime> GetPageCreated(int id) {
            return Run(new Request(Source.PAGES, id, "created")).Then(value => (DateTime)value);
        }

        public Task<DateTime> GetPageModified(int id) {
            return Run(new Request(Source.PAGES, id, "modified")).Then(value => (DateTime)value);
        }

        public Task<int> GetPageAuthorId(int id) {
            return Run(new Request(Source.PAGES, id, "authorId")).Then(value => (int)value);
        }

        public Task<IEnumerable<int>> GetPageSubpages(int id) {
            return Run(new Request(Source.SUBPAGES, id, "subpageId")).Then(value => (IEnumerable<int>)value);
        }

        public Task<string> GetUserName(int id) {
            return Run(new Request(Source.USERS, id, "name")).Then(value => (string)value);
        }

        public Task<DateTime> GetUserCreated(int id) {
            return Run(new Request(Source.USERS, id, "created")).Then(value => (DateTime)value);
        }

        private Task<object> Run(Request request) {
            if(_disposed) {
                throw new ObjectDisposedException("already disposed");
            }
#if true
            return _scheduler.Add(_generation, request);
#else
            switch(request.Source) {
            case Source.PAGES:
                return Task.FromResult(ToRow(_pages[request.Key]).Fields[request.Field]);
            case Source.USERS:
                return Task.FromResult(ToRow(_users[request.Key]).Fields[request.Field]);
            default:
                throw new ArgumentOutOfRangeException();
            }
#endif
        }
    }
}