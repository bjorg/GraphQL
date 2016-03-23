using System.Collections.Generic;
using System.Linq;
using MindTouch.GraphQL.Syntax;



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
	public const int _string = 2;
	public const int _number = 3;
	public const int _true = 4;
	public const int _false = 5;
	public const int _null = 6;
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
	
internal GraphSyntaxDocument Result;



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
		Document(out Result);
	}

	void Document(out GraphSyntaxDocument doc) {
		var definitions = new List<AGraphSyntaxDefinition>();
		AGraphSyntaxDefinition definition = null;
		
		Definition(out definition);
		definitions.Add(definition); 
		while (StartOf(1)) {
			Definition(out definition);
			definitions.Add(definition); 
		}
		doc = new GraphSyntaxDocument(definitions); 
	}

	void Definition(out AGraphSyntaxDefinition definition) {
		definition = null; 
		if (la.kind == 7 || la.kind == 8 || la.kind == 9) {
			OperationDefinition(out definition);
		} else if (la.kind == 16) {
			FragmentDefinition();
		} else SynErr(24);
	}

	void OperationDefinition(out AGraphSyntaxDefinition definition) {
		definition = null;
		GraphSyntaxSelectionSet selectionSet = null; 
		var type = GraphSyntaxOperationType.Query;
		string name = null;
		var variables = Enumerable.Empty<GraphSyntaxOperationDefinition.Variable>();
		var directives = Enumerable.Empty<GraphSyntaxDirective>();
		
		if (la.kind == 9) {
			SelectionSet(out selectionSet);
		} else if (la.kind == 7 || la.kind == 8) {
			OperationType(out type);
			if (la.kind == 1) {
				Name(out name);
			}
			if (la.kind == 13) {
				VariableDefinitions();
			}
			if (la.kind == 21) {
				Directives();
			}
			SelectionSet(out selectionSet);
			definition = new GraphSyntaxOperationDefinition(
			type,
			name,
			variables,
			directives,
			selectionSet
			); 
			
		} else SynErr(25);
	}

	void FragmentDefinition() {
		GraphSyntaxSelectionSet set = null; 
		Expect(16);
		FragmentName();
		Expect(15);
		NamedType();
		if (la.kind == 21) {
			Directives();
		}
		SelectionSet(out set);
	}

	void SelectionSet(out GraphSyntaxSelectionSet set) {
		var selections = new List<AGraphSyntaxSelection>();
		AGraphSyntaxSelection selection = null;
		
		Expect(9);
		Selection(out selection);
		selections.Add(selection); 
		while (la.kind == 1 || la.kind == 11) {
			Selection(out selection);
			selections.Add(selection); 
		}
		Expect(10);
		set = new GraphSyntaxSelectionSet(selections); 
	}

	void OperationType(out GraphSyntaxOperationType type) {
		type = GraphSyntaxOperationType.Query; 
		if (la.kind == 7) {
			Get();
			type = GraphSyntaxOperationType.Query; 
		} else if (la.kind == 8) {
			Get();
			type = GraphSyntaxOperationType.Mutation; 
		} else SynErr(26);
	}

	void Name(out string name) {
		Expect(1);
		name = t.val; 
	}

	void VariableDefinitions() {
		Expect(13);
		VariableDefinition();
		while (la.kind == 22) {
			VariableDefinition();
		}
		Expect(14);
	}

	void Directives() {
		Directive();
		while (la.kind == 21) {
			Directive();
		}
	}

	void Selection(out AGraphSyntaxSelection selection) {
		selection = null; 
		if (la.kind == 1) {
			Field(out selection);
		} else if (la.kind == 11) {
			Get();
			FragmentSpread();
			InlineFragment();
		} else SynErr(27);
	}

	void Field(out AGraphSyntaxSelection field) {
		string prefix = null;
		string name = null;
		var arguments = Enumerable.Empty<GraphSyntaxArgument>();
		var directives = Enumerable.Empty<GraphSyntaxDirective>();
		GraphSyntaxSelectionSet set = null;
		
		Name(out prefix);
		if (la.kind == 12) {
			Get();
			Name(out name);
		}
		if (la.kind == 13) {
			Arguments();
		}
		if (la.kind == 21) {
			Directives();
		}
		if (la.kind == 9) {
			SelectionSet(out set);
		}
		field = new GraphSyntaxSelectionField(
		prefix, 
		name ?? prefix, 
		arguments, 
		directives, 
		set
		);
		
	}

	void FragmentSpread() {
		FragmentName();
		if (la.kind == 21) {
			Directives();
		}
	}

	void InlineFragment() {
		GraphSyntaxSelectionSet set = null; 
		Expect(15);
		NamedType();
		if (la.kind == 21) {
			Directives();
		}
		SelectionSet(out set);
	}

	void Arguments() {
		Expect(13);
		Argument();
		while (la.kind == 1) {
			Argument();
		}
		Expect(14);
	}

	void Argument() {
		AGraphSyntaxValue value = null; 
		Expect(1);
		Expect(12);
		Value(out value);
	}

	void Value(out AGraphSyntaxValue value) {
		value = null; 
		switch (la.kind) {
		case 4: case 5: {
			Boolean(out value);
			break;
		}
		case 3: {
			Number(out value);
			break;
		}
		case 2: {
			String(out value);
			break;
		}
		case 1: {
			Enum(out value);
			break;
		}
		case 22: {
			Variable(out value);
			break;
		}
		case 19: {
			List(out value);
			break;
		}
		case 9: {
			InputObject(out value);
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

	void VariableDefinition() {
		AGraphSyntaxValue value = null; 
		Variable(out value);
		Expect(12);
		Type();
		if (la.kind == 17) {
			Get();
			Value(out value);
		}
	}

	void Variable(out AGraphSyntaxValue value) {
		string name = null; 
		Expect(22);
		Name(out name);
		value = new GraphSyntaxVariable(name); 
	}

	void Type() {
		if (la.kind == 1) {
			NamedType();
			if (la.kind == 18) {
				Get();
			}
		} else if (la.kind == 19) {
			ListType();
			if (la.kind == 18) {
				Get();
			}
		} else SynErr(29);
	}

	void ListType() {
		Expect(19);
		Type();
		Expect(20);
	}

	void Directive() {
		Expect(21);
		Expect(1);
		if (la.kind == 13) {
			Arguments();
		}
	}

	void Boolean(out AGraphSyntaxValue value) {
		value = null; 
		if (la.kind == 4) {
			Get();
			value = new GraphSyntaxLiteralBool(true); 
		} else if (la.kind == 5) {
			Get();
			value = new GraphSyntaxLiteralBool(false); 
		} else SynErr(30);
	}

	void Number(out AGraphSyntaxValue value) {
		Expect(3);
		value = Utils.ToLiteralNumber(t.val); 
	}

	void String(out AGraphSyntaxValue value) {
		Expect(2);
		value = Utils.ToLiteralString(t.val); 
	}

	void Enum(out AGraphSyntaxValue value) {
		string name = null; 
		Name(out name);
		value = new GraphSyntaxLiteralEnum(name); 
	}

	void List(out AGraphSyntaxValue value) {
		value = null; var values = new List<AGraphSyntaxValue>(); 
		Expect(19);
		while (StartOf(2)) {
			Value(out value);
			values.Add(value); 
		}
		Expect(20);
		value = new GraphSyntaxList(values); 
	}

	void InputObject(out AGraphSyntaxValue value) {
		value = null; 
		var fields = new List<GraphSyntaxInputObject.Field>(); 
		GraphSyntaxInputObject.Field field = null;
		
		Expect(9);
		InputObjectField(out field);
		fields.Add(field); 
		while (la.kind == 1) {
			InputObjectField(out field);
			fields.Add(field); 
		}
		Expect(10);
		value = new GraphSyntaxInputObject(fields); 
	}

	void InputObjectField(out GraphSyntaxInputObject.Field field) {
		AGraphSyntaxValue value = null; string name = null; 
		Name(out name);
		Expect(17);
		Value(out value);
		field = new GraphSyntaxInputObject.Field(name, value); 
	}



	public void Parse() {
        la = new Token { val = "" };
		Get();
		GraphQL();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,T, T,T,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,x,x, x,T,x,x, x,x,x,x, x,x,x,T, x,x,T,x, x}

	};
} // end Parser


internal class Errors {
	public void SynErr(string origin, string path, int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "name expected"; break;
			case 2: s = "string expected"; break;
			case 3: s = "number expected"; break;
			case 4: s = "true expected"; break;
			case 5: s = "false expected"; break;
			case 6: s = "null expected"; break;
			case 7: s = "\"query\" expected"; break;
			case 8: s = "\"mutation\" expected"; break;
			case 9: s = "\"{\" expected"; break;
			case 10: s = "\"}\" expected"; break;
			case 11: s = "\"...\" expected"; break;
			case 12: s = "\":\" expected"; break;
			case 13: s = "\"(\" expected"; break;
			case 14: s = "\")\" expected"; break;
			case 15: s = "\"on\" expected"; break;
			case 16: s = "\"fragment\" expected"; break;
			case 17: s = "\"=\" expected"; break;
			case 18: s = "\"!\" expected"; break;
			case 19: s = "\"[\" expected"; break;
			case 20: s = "\"]\" expected"; break;
			case 21: s = "\"@\" expected"; break;
			case 22: s = "\"$\" expected"; break;
			case 23: s = "??? expected"; break;
			case 24: s = "invalid Definition"; break;
			case 25: s = "invalid OperationDefinition"; break;
			case 26: s = "invalid OperationType"; break;
			case 27: s = "invalid Selection"; break;
			case 28: s = "invalid Value"; break;
			case 29: s = "invalid Type"; break;
			case 30: s = "invalid Boolean"; break;

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