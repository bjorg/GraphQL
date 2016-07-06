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
using Castle.DynamicProxy;

namespace Sandbox.DynamicProxyBased {

    internal sealed class QueryServer<TQuery> {

        //--- Types ---
        private sealed class CaptureMembers : IInterceptor {

            //--- Fields ---
            private readonly int _level;

            //--- Constructors ---
            public CaptureMembers(int level) {
                _level = level;
            }

            //--- Methods ---
            public void Intercept(IInvocation invocation) {
                var methodName = invocation.Method.Name;
                var returnType = invocation.Method.ReturnType;
                if(methodName.StartsWith("get_", StringComparison.Ordinal)) {
                    Console.WriteLine(new string(' ', _level * 4) + $"calling property {methodName.Substring(4)}");
                } else {
                    Console.WriteLine(new string(' ', _level * 4) + $"calling method {methodName}()");

                    var args = invocation.Arguments;
                    var last = args[args.Length - 1];
                    var lastType = last.GetType();

                    if(lastType.GetGenericTypeDefinition() == typeof(Func<,>)) {
                        var selectionType = lastType.GenericTypeArguments[0];
                        var selectionProxy = CreateCaptureProxy(selectionType, _level + 1);
                        ((Delegate)last).DynamicInvoke(selectionProxy);
                    }
                }
                invocation.ReturnValue = returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
            }
        }

        //--- Class Fields ---
        private static readonly ProxyGenerator _generator = new ProxyGenerator();

        //--- Class Methods ---
        private static object CreateCaptureProxy(Type type, int level) {
            return _generator.CreateInterfaceProxyWithoutTarget(type, new CaptureMembers(level));
        }

        //--- Methods ---
        public TResult Query<TResult>(Func<TQuery, TResult> selection) {
            var proxy = (TQuery)CreateCaptureProxy(typeof(TQuery), 0);
            return selection(proxy);
        }
    }
}