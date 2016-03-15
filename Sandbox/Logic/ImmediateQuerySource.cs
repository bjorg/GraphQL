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

namespace Sandbox {

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
        Task<PageBE> GetPageById(int id);
        Task<UserBE> GetUserById(int id);
    }

    internal sealed class ImmediateQuerySource : IQuerySource {

        public Task<PageBE> GetPageById(int id) {
            switch(id) {
            case 1:
                return Task.FromResult(new PageBE(1, "Page Title", DateTime.Today.ToUniversalTime(), 42, DateTime.Now.ToUniversalTime()));
            default:
                return Task.FromException<PageBE>(new ArgumentException("page not found"));
            }
        }

        public Task<UserBE> GetUserById(int id) {
            switch(id) {
            case 42:
                return Task.FromResult(new UserBE("John Doe", DateTime.Today.AddHours(12).ToUniversalTime()));
            default:
                return Task.FromException<UserBE>(new ArgumentException("user not found"));
            }
        }
    }
}