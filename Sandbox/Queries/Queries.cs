using Sandbox.Logic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sandbox.Queries {

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
        public Task<T> Page<T>(int id, Func<IPageQuery, Task<T>> selection) => selection(new PageQuery(_source, id));
        public Task<T> User<T>(int id, Func<IUserQuery, Task<T>> selection) => selection(new UserQuery(_source, id));
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
        public Task<T> Author<T>(Func<IUserQuery, Task<T>> selection) => _source.GetPageAuthorId(_pageId).Then(authorId => selection(new UserQuery(_source, authorId)));
        public Task<T[]> Subpages<T>(Func<IPageQuery, Task<T>> selection) => _source.GetPageSubpages(_pageId).Then(ids => Task.WhenAll(ids.Select(id => selection(new PageQuery(_source, id)))));
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