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
        public static Task<TResult> Record<T1, T2, TResult>(Func<T1, T2, TResult> combine, Task<T1> task1, Task<T2> task2) {
            return Task.WhenAll(task1, task2).ContinueWith(_ => combine(task1.Result, task2.Result));
        }

        public static Task<TResult> Record<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> combine, Task<T1> task1, Task<T2> task2, Task<T3> task3) {
            return Task.WhenAll(task1, task2, task3).ContinueWith(_ => combine(task1.Result, task2.Result, task3.Result));
        }

        public static Task<TResult> Record<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> combine, Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4) {
            return Task.WhenAll(task1, task2, task3, task4).ContinueWith(_ => combine(task1.Result, task2.Result, task3.Result, task4.Result));
        }

        public static Task<TResult> Then<TSource, TResult>(this Task<TSource> task, Func<TSource, TResult> convert, IDisposable disposable = null) {
            return task.ContinueWith(t => {
                try {
                    return convert(t.Result);
                } finally {
                    disposable?.Dispose();
                }
            });
        }

        public static Task<TResult> Then<TSource, TResult>(this Task<TSource> task, Func<TSource, Task<TResult>> convert, IDisposable disposable = null) {
            return task.ContinueWith(t => {
                try {
                    return convert(t.Result);
                } finally {
                    disposable?.Dispose();
                }
            }).Unwrap();
        }
    }
}