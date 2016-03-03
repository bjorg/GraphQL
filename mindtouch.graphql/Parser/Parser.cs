using System;



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

// ReSharper disable TooWideLocalVariableScope
// ReSharper disable JoinDeclarationAndInitializer

namespace MindTouch.GraphQL.Parser {



internal class GraphQLParser {
	public const int _EOF = 0;
	public const int _name = 1;
	public const int _directive = 2;
	public const int _variable = 3;
	public const int _string = 4;
	public const int _number = 5;
	public const int _true = 6;
	public const int _false = 7;
	public const int _null = 8;
	public const int maxT = 23;
	public const int _comment = 24;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;
	


	public GraphQLParser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.origin, la.path, la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(la.origin, la.path, t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }
				if (la.kind == 24) {
				}

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void GraphQL() {
		Document();
	}

	void Document() {
		Definition();
		while (StartOf(1)) {
			Definition();
		}
	}

	void Definition() {
		if (la.kind == 9 || la.kind == 10 || la.kind == 11) {
			OperationDefinition();
		} else if (la.kind == 18) {
			FragmentDefinition();
		} else SynErr(24);
	}

	void OperationDefinition() {
		if (la.kind == 11) {
			SelectionSet();
		} else if (la.kind == 9 || la.kind == 10) {
			OperationType();
			if (la.kind == 1) {
				OperationName();
			}
			if (la.kind == 15) {
				VariableDefinitions();
			}
			if (la.kind == 2) {
				Directives();
			}
			SelectionSet();
		} else SynErr(25);
	}

	void FragmentDefinition() {
		Expect(18);
		FragmentName();
		Expect(17);
		NamedType();
		if (la.kind == 2) {
			Directives();
		}
		SelectionSet();
	}

	void SelectionSet() {
		Expect(11);
		Selection();
		while (la.kind == 1 || la.kind == 13) {
			Selection();
		}
		Expect(12);
	}

	void OperationType() {
		if (la.kind == 9) {
			Get();
		} else if (la.kind == 10) {
			Get();
		} else SynErr(26);
	}

	void OperationName() {
		Expect(1);
	}

	void VariableDefinitions() {
		Expect(15);
		VariableDefinition();
		while (la.kind == 3) {
			VariableDefinition();
		}
		Expect(16);
	}

	void Directives() {
		Expect(2);
		while (la.kind == 2) {
			Get();
		}
	}

	void Selection() {
		if (la.kind == 1) {
			Field();
		} else if (la.kind == 13) {
			Get();
			FragmentSpread();
			InlineFragment();
		} else SynErr(27);
	}

	void Field() {
		FieldName();
		if (la.kind == 15) {
			Arguments();
		}
		if (la.kind == 2) {
			Directives();
		}
		if (la.kind == 11) {
			SelectionSet();
		}
	}

	void FragmentSpread() {
		FragmentName();
		if (la.kind == 2) {
			Directives();
		}
	}

	void InlineFragment() {
		Expect(17);
		NamedType();
		if (la.kind == 2) {
			Directives();
		}
		SelectionSet();
	}

	void FieldName() {
		Expect(1);
		if (la.kind == 14) {
			Get();
			Expect(1);
		}
	}

	void Arguments() {
		Expect(15);
		Argument();
		while (la.kind == 1) {
			Argument();
		}
		Expect(16);
	}

	void Argument() {
		Expect(1);
		Expect(14);
		Value();
	}

	void Value() {
		switch (la.kind) {
		case 4: {
			Get();
			break;
		}
		case 5: {
			Get();
			break;
		}
		case 6: case 7: {
			Boolean();
			break;
		}
		case 3: {
			Get();
			break;
		}
		case 1: {
			Enum();
			break;
		}
		case 19: {
			List();
			break;
		}
		case 11: {
			Object();
			break;
		}
		default: SynErr(28); break;
		}
	}

	void FragmentName() {
		Expect(1);
	}

	void NamedType() {
		Expect(1);
	}

	void Boolean() {
		if (la.kind == 6) {
			Get();
		} else if (la.kind == 7) {
			Get();
		} else SynErr(29);
	}

	void Enum() {
		Expect(1);
	}

	void List() {
		Expect(19);
		while (StartOf(2)) {
			Value();
		}
		Expect(20);
	}

	void Object() {
		Expect(11);
		ObjectField();
		while (la.kind == 1) {
			ObjectField();
		}
		Expect(12);
	}

	void ObjectField() {
		Expect(1);
		Expect(21);
		Value();
	}

	void VariableDefinition() {
		Expect(3);
		Expect(14);
		Type();
		if (la.kind == 21) {
			Get();
			Value();
		}
	}

	void Type() {
		if (la.kind == 1) {
			NamedType();
			if (la.kind == 22) {
				Get();
			}
		} else if (la.kind == 19) {
			ListType();
			if (la.kind == 22) {
				Get();
			}
		} else SynErr(30);
	}

	void ListType() {
		Expect(19);
		Type();
		Expect(20);
	}



	public void Parse() {
        la = new Token { val = "" };
		Get();
		GraphQL();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,T,T,T, x,x,x,x, x,x,T,x, x,x,x,x, x},
		{x,T,x,T, T,T,T,T, x,x,x,T, x,x,x,x, x,x,x,T, x,x,x,x, x}

	};
} // end Parser


internal class Errors {
	public void SynErr(string origin, string path, int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "name expected"; break;
			case 2: s = "directive expected"; break;
			case 3: s = "variable expected"; break;
			case 4: s = "string expected"; break;
			case 5: s = "number expected"; break;
			case 6: s = "true expected"; break;
			case 7: s = "false expected"; break;
			case 8: s = "null expected"; break;
			case 9: s = "\"query\" expected"; break;
			case 10: s = "\"mutation\" expected"; break;
			case 11: s = "\"{\" expected"; break;
			case 12: s = "\"}\" expected"; break;
			case 13: s = "\"...\" expected"; break;
			case 14: s = "\":\" expected"; break;
			case 15: s = "\"(\" expected"; break;
			case 16: s = "\")\" expected"; break;
			case 17: s = "\"on\" expected"; break;
			case 18: s = "\"fragment\" expected"; break;
			case 19: s = "\"[\" expected"; break;
			case 20: s = "\"]\" expected"; break;
			case 21: s = "\"=\" expected"; break;
			case 22: s = "\"!\" expected"; break;
			case 23: s = "??? expected"; break;
			case 24: s = "invalid Definition"; break;
			case 25: s = "invalid OperationDefinition"; break;
			case 26: s = "invalid OperationType"; break;
			case 27: s = "invalid Selection"; break;
			case 28: s = "invalid Value"; break;
			case 29: s = "invalid Boolean"; break;
			case 30: s = "invalid Type"; break;

			default: s = "error " + n; break;
		}
		throw new GraphQLParserException(new Location(origin, path, line, col), s);
	}

	public void SemErr(string origin, string path, int line, int col, string s) {
		throw new GraphQLParserException(new Location(origin, path, line, col), s);
	}
	
	public void Warning(int line, int col, string s) {
		// ignore warnings
	}
	
	public void Warning(string s) {
		// ignore warnings
	}
} // Errors
}