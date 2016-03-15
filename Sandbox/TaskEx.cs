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

    public static class TaskEx {

        //--- Methods ---
        public static Task<Tuple<T1, T2>> Tuple<T1, T2>(Task<T1> first, Task<T2> second) {
            return Task.WhenAll(first, second).ContinueWith(_ => new Tuple<T1, T2>(first.Result, second.Result));
        }

        public static Task<Tuple<T1, T2, T3>> Tuple<T1, T2, T3>(Task<T1> first, Task<T2> second, Task<T3> third) {
            return Task.WhenAll(first, second, third).ContinueWith(_ => new Tuple<T1, T2, T3>(first.Result, second.Result, third.Result));
        }

        public static Task<TResult> Then<TSource, TResult>(this Task<TSource> task, Func<TSource, TResult> convert) {
            return task.ContinueWith(t => convert(t.Result));
        }

        public static Task<TResult> Then<TSource, TResult>(this Task<TSource> task, Func<TSource, Task<TResult>> convert) {
            return task.ContinueWith(t => convert(t.Result)).Unwrap();
        }
    }
}