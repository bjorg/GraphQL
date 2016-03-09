/*
 * MindTouch GraphQL
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

using Newtonsoft.Json.Linq;
using System;

namespace MindTouch.GraphQL.Schema {

    public enum GraphTypeKind {
        SCALAR,
        OBJECT,
        INTERFACE,
        UNION,
        ENUM,
        INPUT_OBJECT,
        LIST,
        NON_NULL
    }

    public sealed class GraphObjectField {

        //--- Fields ---
        public readonly string Name;
        public readonly string Description;
        public readonly AGraphType Type;
        public readonly JObject JsonType;

        //--- Constructors ---
        public GraphObjectField(string name, string description, AGraphType type) {
            if(name == null) {
                throw new ArgumentNullException(nameof(name));
            }
            if(type == null) {
                throw new ArgumentNullException(nameof(type));
            }
            Name = name;
            Description = description;
            Type = type;
            JsonType = new JObject {
                {
                    "name", Name
                }, {
                    "description", description
                }, {
                    "type", Type.JsonType
                }
            };
        }
    }

    public abstract class AGraphType {

        //--- Class Methods ---
        internal static void ValidateArray(Array value, string parameterName) {
            if(value == null) {
                throw new ArgumentNullException(parameterName);
            }
            if(value.Length == 0) {
                throw new ArgumentException("array cannot be empty", parameterName);
            }
        }

        protected static JObject BuildJsonType(
            GraphTypeKind kind,
            string name = null,
            string description = null,
            GraphObjectField[] fields = null,
            // interfaces
            GraphTypeObject[] possibleTypes = null,
            GraphTypeEnum.Value[] enumValues = null,
            // input fields
            AGraphType ofType = null
        ) {
            var result = new JObject();
            result.Add("kind", kind.ToString());
            if(name != null) {
                result.Add("name", name);
            }
            if(description != null) {
                result.Add("description", description);
            }

            // add fields specific to grapht-type kind
            JArray jsonPossibleTypes = null;
            JArray jsonFields = null;
            switch(kind) {
            case GraphTypeKind.SCALAR:

                // nothing more to do
                break;
            case GraphTypeKind.OBJECT:

                // add fields
                ValidateArray(fields, nameof(fields));
                foreach(var field in fields) {
                    jsonFields.Add(field.JsonType);
                }

                // TODO: add interfaces
                break;
            case GraphTypeKind.INTERFACE:

                // add fields
                ValidateArray(fields, nameof(fields));
                foreach(var field in fields) {
                    jsonFields.Add(field.JsonType);
                }

                // add possible types
                if(possibleTypes != null) {
                    jsonPossibleTypes = new JArray();
                    foreach(var possibleType in possibleTypes) {
                        jsonPossibleTypes.Add(possibleType.JsonType);
                    }
                }
                result.Add("possibleTypes", jsonPossibleTypes);
                break;
            case GraphTypeKind.UNION:

                // add possible types
                if(possibleTypes != null) {
                    jsonPossibleTypes = new JArray();
                    foreach(var possibleType in possibleTypes) {
                        jsonPossibleTypes.Add(possibleType.JsonType);
                    }
                }
                result.Add("possibleTypes", jsonPossibleTypes);
                break;
            case GraphTypeKind.ENUM:
                ValidateArray(enumValues, nameof(enumValues));

                // add enum values
                var jsonEnumValues = new JArray();
                foreach(var enumValue in enumValues) {
                    jsonEnumValues.Add(enumValue.JsonType);
                }
                result.Add("enumValues", jsonEnumValues);
                break;
            case GraphTypeKind.INPUT_OBJECT:
                throw new NotImplementedException();

                // TODO: add inputFields
                break;
            case GraphTypeKind.LIST:
                if(ofType == null) {
                    throw new ArgumentNullException(nameof(ofType));
                }
                result.Add("ofType", ofType.JsonType);
                break;
            case GraphTypeKind.NON_NULL:
                if(ofType == null) {
                    throw new ArgumentNullException(nameof(ofType));
                }
                result.Add("ofType", ofType.JsonType);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
            return result;
        }

        //--- Fields ---
        public readonly JObject JsonType;

        //--- Constructors ---
        protected AGraphType(JObject jsonType) {
            if(jsonType == null) {
                throw new ArgumentNullException(nameof(jsonType));
            }
            JsonType = jsonType;
        }

        //--- Abstract Methods ----
        public abstract object Coerce(object value);
    }
}