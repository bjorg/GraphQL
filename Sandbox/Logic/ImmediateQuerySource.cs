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
using System.Threading.Tasks;

namespace Sandbox.Logic {

    internal class UserBE {

        public readonly string Name;
        public readonly DateTime Created;

        public UserBE(string name, DateTime created) {
            Name = name;
            Created = created;
        }
    }

    internal class PageBE {

        public readonly int Id;
        public readonly string Title;
        public readonly int AuthorId;
        public readonly DateTime Created;
        public readonly DateTime Modified;

        public PageBE(int id, string title, DateTime created, int authorId, DateTime modified) {
            Id = id;
            Title = title;
            Created = created;
            AuthorId = authorId;
            Modified = modified;
        }
    }

    internal interface IQuerySource {
        Task<string> GetPageTitle(int id);
        Task<DateTime> GetPageModified(int id);
        Task<int> GetPageAuthorId(int id);
        Task<string> GetUserName(int id);
        Task<DateTime> GetUserCreated(int id);
    }

    internal sealed class ImmediateQuerySource : IQuerySource {

        public Task<string> GetPageTitle(int id) {
            Console.WriteLine($"{nameof(GetPageTitle)}({id})");
            switch(id) {
            case 1:
                return Task.FromResult("Page Title");
            default:
                return Task.FromException<string>(new ArgumentException($"page {id} not found"));
            }
        }

        public Task<DateTime> GetPageModified(int id) {
            Console.WriteLine($"{nameof(GetPageModified)}({id})");
            switch(id) {
            case 1:
                return Task.FromResult(new DateTime(2010, 3, 31, 17, 23, 00));
            default:
                return Task.FromException<DateTime>(new ArgumentException($"page {id} not found"));
            }
        }

        public Task<int> GetPageAuthorId(int id) {
            Console.WriteLine($"{nameof(GetPageAuthorId)}({id})");
            switch(id) {
            case 1:
                return Task.FromResult(42);
            default:
                return Task.FromException<int>(new ArgumentException($"page {id} not found"));
            }
        }

        public Task<string> GetUserName(int id) {
            Console.WriteLine($"{nameof(GetUserName)}({id})");
            switch(id) {
            case 42:
                return Task.FromResult("John Doe");
            default:
                return Task.FromException<string>(new ArgumentException($"user {id} not found"));
            }
        }

        public Task<DateTime> GetUserCreated(int id) {
            Console.WriteLine($"{nameof(GetUserCreated)}({id})");
            switch(id) {
            case 42:
                return Task.FromResult(new DateTime(1972, 9, 23, 16, 00, 00));
            default:
                return Task.FromException<DateTime>(new ArgumentException($"user {id} not found"));
            }
        }
    }
}