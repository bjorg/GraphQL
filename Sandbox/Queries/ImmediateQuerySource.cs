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

using Sandbox.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Sandbox.Queries {

    internal interface IQuerySource : IDisposable {

        //--- Methods ---
        IQuerySource New();
        Task<string> GetPageTitle(int id);
        Task<DateTime> GetPageCreated(int id);
        Task<DateTime> GetPageModified(int id);
        Task<int> GetPageAuthorId(int id);
        Task<IEnumerable<int>> GetPageSubpages(int id);
        Task<string> GetUserName(int id);
        Task<DateTime> GetUserCreated(int id);
    }

    internal sealed class ImmediateQuerySource : IQuerySource {

        //--- Types ---
        private sealed class Scheduler {

            //--- Fields ---
            private readonly Dictionary<int, int> _counters = new Dictionary<int, int>();
            private readonly Dictionary<int, List<Task>> _tasks = new Dictionary<int, List<Task>>();

            //--- Methods ---
            public void Begin(int generation) {
                lock (_counters) {
                    int counter;
                    if(!_counters.TryGetValue(generation, out counter)) {
                        _tasks[generation] = new List<Task>();
                    }
                    _counters[generation] = ++counter;
                }
            }

            public Task<T> Add<T>(int generation, Func<T> function) {
                var result = new Task<T>(function);
                lock (_counters) {
                    var tasks = _tasks[generation];
                    tasks.Add(result);
                }
                return result;
            }

            public void End(int generation) {
                List<Task> tasks = null;
                lock (_counters) {
                    int counter;
                    if(_counters.TryGetValue(generation, out counter)) {
                        switch(counter--) {
                        case 0:
                            throw new InvalidOperationException("counter is 0");
                        case 1:
                            _counters.Remove(generation);
                            tasks = _tasks[generation];
                            _tasks.Remove(generation);
                            break;
                        default:
                            _counters[generation] = counter;
                            break;
                        }
                    } else {
                        throw new InvalidOperationException("counter not found");
                    }
                }
                if(tasks != null) {
                    foreach(var task in tasks) {
                        task.Start();
                    }
                }
            }
        }

        //--- Class Fields ---
        private static readonly Dictionary<int, PageBE> _pages;
        private static readonly Dictionary<int, UserBE> _users;

        //--- Class Constructor ---
        static ImmediateQuerySource() {
            _pages = new Dictionary<int, PageBE> {
                { 1, new PageBE(1, "Homepage", new DateTime(2010, 3, 31, 17, 23, 00), 42, new DateTime(2016, 2, 27, 11, 35, 00), new[] { 2, 3, 4 }) },
                { 2, new PageBE(2, "Subpage 1", new DateTime(2010, 3, 31, 17, 23, 01), 13, new DateTime(2016, 2, 27, 11, 35, 01), new int[0]) },
                { 3, new PageBE(3, "Subpage 2", new DateTime(2010, 3, 31, 17, 23, 02), 42, new DateTime(2016, 2, 27, 11, 35, 02), new int[0]) },
                { 4, new PageBE(4, "Subpage 3", new DateTime(2010, 3, 31, 17, 23, 03), 13, new DateTime(2016, 2, 27, 11, 35, 03), new int[0]) }
            };
            _users = new Dictionary<int, UserBE> {
                { 13, new UserBE(13, "Jane Doe", new DateTime(1976, 04, 30, 11, 00, 00)) },
                { 42, new UserBE(42, "John Doe", new DateTime(1972, 09, 23, 16, 00, 00)) }
            };
        }

        //--- Fields ---
        private readonly int _generation;
        private readonly Scheduler _scheduler;
        private bool _disposed;

        //--- Constructors ---
        public ImmediateQuerySource() : this(0, new Scheduler()) { }

        private ImmediateQuerySource(int generation, Scheduler scheduler) {
            _scheduler = scheduler;
            _generation = generation;
            _scheduler.Begin(_generation);
        }

        //--- Methods ---
        public IQuerySource New() {
            if(_disposed) {
                throw new ObjectDisposedException("already disposed");
            }
            return new ImmediateQuerySource(_generation + 1, _scheduler);
        }

        public void Dispose() {
            if(_disposed) {
                throw new ObjectDisposedException("already disposed");
            }
            _disposed = true;
            _scheduler.End(_generation);
        }

        public Task<string> GetPageTitle(int id) {
            return Run(() => {
                Log(new { id });
                PageBE page;
                if(!_pages.TryGetValue(id, out page)) {
                    throw new ArgumentException($"page {id} not found");
                }
                return page.Title;
            });
        }

        public Task<DateTime> GetPageCreated(int id) {
            return Run(() => {
                Log(new { id });
                PageBE page;
                if(!_pages.TryGetValue(id, out page)) {
                    throw new ArgumentException($"page {id} not found");
                }
                return page.Created;
            });
        }

        public Task<DateTime> GetPageModified(int id) {
            return Run(() => {
                Log(new { id });
                PageBE page;
                if(!_pages.TryGetValue(id, out page)) {
                    throw new ArgumentException($"page {id} not found");
                }
                return page.Modified;
            });
        }

        public Task<int> GetPageAuthorId(int id) {
            return Run(() => {
                Log(new { id });
                PageBE page;
                if(!_pages.TryGetValue(id, out page)) {
                    throw new ArgumentException($"page {id} not found");
                }
                return page.AuthorId;
            });
        }

        public Task<IEnumerable<int>> GetPageSubpages(int id) {
            return Run(() => {
                Log(new { id });
                PageBE page;
                if(!_pages.TryGetValue(id, out page)) {
                    throw new ArgumentException($"page {id} not found");
                }
                return (IEnumerable<int>)page.Subpages;
            });
        }

        public Task<string> GetUserName(int id) {
            return Run(() => {
                Log(new { id });
                UserBE user;
                if(!_users.TryGetValue(id, out user)) {
                    throw new ArgumentException($"user {id} not found");
                }
                return user.Name;
            });
        }

        public Task<DateTime> GetUserCreated(int id) {
            return Run(() => {
                Log(new { id });
                UserBE user;
                if(!_users.TryGetValue(id, out user)) {
                    throw new ArgumentException($"user {id} not found");
                }
                return user.Created;
            });
        }

        private Task<T> Run<T>(Func<T> function, [CallerMemberName] string method = "<missing>") {
            if(_disposed) {
                throw new ObjectDisposedException("already disposed");
            }
            return _scheduler.Add(_generation, function);
        }

        private void Log(object arguments, [CallerMemberName] string method = "<missing>") {
            //var args = (arguments != null) ? JsonConvert.SerializeObject(arguments) : "";
            //Console.WriteLine($"[{_generation}] {method}({args})");
        }
    }
}