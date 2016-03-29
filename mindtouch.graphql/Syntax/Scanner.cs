
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

using System;
using System.Collections.Generic;

namespace MindTouch.GraphQL.Syntax {

internal class Token {
	public int kind;    // token kind
	public int pos;     // token position in bytes in the source text (starting at 0)
	public int charPos;  // token position in characters in the source text (starting at 0)
	public int col;     // token column (starting at 1)
	public int line;    // token line (starting at 1)
    public string origin; // origin of source code
    public string path; // xpath to location in XML document
	public string val;  // token value
	public Token next;  // ML 2005-03-11 Tokens are kept in linked list
	
//	public Location Location { get { return new Location(origin, path, line, col); } }
}

//-----------------------------------------------------------------------------------
// Buffer
//-----------------------------------------------------------------------------------
internal class Buffer {

	//--- Constants ---
	public const int EOF = char.MaxValue + 1;

	//--- Fields ---
	private readonly string _source;
	private readonly int _start;
	private readonly int _end;
	private int _current;
	
	//--- Constructors ---
	public Buffer(string source, int start, int end) {
		if(source == null) {
			throw new ArgumentNullException("source");
		}
		if(start > end) {
			throw new ArgumentException("start index is beyond the end position", "start");
		}
		if(end > source.Length) {
			throw new ArgumentException("end position is greater than source length", "start");
		}
		if((_start != _end) && (start >= source.Length)) {
			throw new ArgumentException("start index is greater than source length", "start");
		}
		_source = source;
		_start = start;
		_end = end;
		_current = start;
	}

	//--- Methods ---	
	public int Read() {
	next:
		if(_current >= _end) {
			return EOF;
		}
		var result = _source[_current++];
		switch(result) {
		case '\u00A0':
		case '\u1680':
		case '\u180E':
		case '\u2000':
		case '\u2001':
		case '\u2002':
		case '\u2003':
		case '\u2004':
		case '\u2005':
		case '\u2006':
		case '\u2007':
		case '\u2008':
		case '\u2009':
		case '\u200A':
		case '\u202F':
		case '\u205F':
		case '\u3000':

			// 00A0: No-break space -> space
			// 1680: Ogham separator -> space
			// 180E: Mongolian vowel separator -> space
			// 2000: En quad -> space
			// 2001: Em quad -> space
			// 2002: En space -> space
			// 2003: Em space -> space
			// 2004: Three-per-em space -> space
			// 2005: Four-per-em space -> space
			// 2006: Six-per-em space -> space
			// 2007: Figure space -> space
			// 2008: Punctuation space -> space
			// 2009: Thin space -> space
			// 200A: Hair space -> space
			// 202F: Narrow no-break space -> space
			// 205F: Medium math space -> space
			// 3000: Ideographic space -> space
			return ' ';
		case '\u00AD':
		case '\u1806':
		case '\u200B':
		case '\u200C':
		case '\u200D':
		case '\u2060':

			// 00AD: Soft-hypen -> skipped
			// 1806: Mongolian soft-hyphen -> skipped
			// 200B: Zero-width space -> skipped
			// 200C: Zero-width non-joiner -> skipped
			// 200D: Zero-width joiner -> skipped
			// 2060: Word joiner -> skipped
			goto next;
		}
		return result;
	}

	public int Peek() {
		var curPos = Pos;
		var ch = Read();
		Pos = curPos;
		return ch;
	}

	public int Pos {
		get { return _current; }
		set {
			if((value < _start) || (value >= _end)) {
				throw new ArgumentOutOfRangeException("value", "trying to set Position out of bounds");
			}
			_current = value;
		}
	}
}

//-----------------------------------------------------------------------------------
// Scanner
//-----------------------------------------------------------------------------------
internal class Scanner {
	const int EOL = '\n';
	const int eofSym = 0; /* pdt */
	const int maxT = 23;
	const int noSym = 23;


	public Buffer buffer; // scanner buffer
	
	Token t;			// current token
	int ch;				// current input character
	int pos;			// byte position of current character
	int charPos;		// position by unicode characters starting with 0
	int col;			// column number of current character
	int line;			// line number of current character
	string origin;		// code origin (e.g. filename, web address, ...)
	string path;		// xpath to location in XML documents
	int oldEols;		// EOLs that appeared in a comment;
	static readonly Dictionary<int, int> start; // maps first token character to start state

	Token tokens;     // list of tokens already peeked (first token is a dummy)
	Token pt;         // current peek token
	
	char[] tval = new char[128]; // text of current token
	int tlen;         // length of current token
	
	static Scanner() {
		start = new Dictionary<int, int>(128);
// ReSharper disable SuggestUseVarKeywordEverywhere
		for (int i = 95; i <= 95; ++i) start[i] = 1;
		for (int i = 97; i <= 122; ++i) start[i] = 1;
		for (int i = 48; i <= 57; ++i) start[i] = 24;
		start[34] = 2; 
		start[46] = 40; 
		for (int i = 43; i <= 43; ++i) start[i] = 8;
		for (int i = 45; i <= 45; ++i) start[i] = 8;
		start[35] = 22; 
		start[123] = 27; 
		start[125] = 28; 
		start[58] = 31; 
		start[40] = 32; 
		start[41] = 33; 
		start[61] = 34; 
		start[33] = 35; 
		start[91] = 36; 
		start[93] = 37; 
		start[64] = 38; 
		start[36] = 39; 
		start[Buffer.EOF] = -1;

// ReSharper restore SuggestUseVarKeywordEverywhere
	}
	
	public Scanner(string source, string origin, string path, int line, int column, int charPos) {
		buffer = new Buffer(source, 0, source.Length);
		Init(origin, path, line, column, charPos);
	}
	
	void Init(string o, string p, int l, int c, int cp) {
		pos = -1;
		line = l;
		col = c; 
		charPos = cp;
		origin = o;
		path = p;
		oldEols = 0;
		NextCh();
		pt = tokens = new Token();  // first token is a dummy
	}
	
	void NextCh() {
		if(oldEols > 0) { ch = EOL; oldEols--; } 
		else {
			pos = buffer.Pos;
			ch = buffer.Read();
			col++;
			charPos++;

			// replace isolated '\r' by '\n' in order to make
			// eol handling uniform across Windows, Unix and Mac
			if(ch == '\r' && buffer.Peek() != '\n') ch = EOL;
			if(ch == EOL) { line++; col = 0; }
		}

	}

	void AddCh() {
		if(tlen >= tval.Length) {
			var newBuf = new char[2 * tval.Length];
			Array.Copy(tval, 0, newBuf, 0, tval.Length);
			tval = newBuf;
		}
		if(ch != Buffer.EOF) {
			tval[tlen++] = (char) ch;
			NextCh();
		}
	}

// ReSharper disable RedundantIfElseBlock

// ReSharper restore RedundantIfElseBlock

	void CheckLiteral() {
// ReSharper disable RedundantEmptyDefaultSwitchBranch
		switch (t.val) {
			case "true": t.kind = 4; break;
			case "false": t.kind = 5; break;
			case "null": t.kind = 6; break;
			case "query": t.kind = 7; break;
			case "mutation": t.kind = 8; break;
			case "on": t.kind = 15; break;
			case "fragment": t.kind = 16; break;
			default: break;
		}
// ReSharper restore RedundantEmptyDefaultSwitchBranch
	}

	Token NextToken() {
		while(ch == ' ' ||
			ch >= 9 && ch <= 10 || ch == 13 || ch == ','
		) NextCh();

		var recKind = noSym;
		var recEnd = pos;
        t = new Token { pos = pos, col = col, line = line, origin = origin, path = path, charPos = charPos };
		int state;
		start.TryGetValue(ch, out state);
		tlen = 0; AddCh();
		
		switch(state) {
			case -1: { t.kind = eofSym; break; } // NextCh already done
			case 0: {
				if(recKind != noSym) {
					tlen = recEnd - t.pos;
					SetScannerBehindT();
				}
				t.kind = recKind; break;
			} // NextCh already done
// ReSharper disable RedundantIfElseBlock
// ReSharper disable RedundantAssignment
			case 1:
				recEnd = pos; recKind = 1;
				if (ch >= '0' && ch <= '9' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 2:
				if (ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 2;}
				else if (ch == '"') {AddCh(); goto case 7;}
				else if (ch == 92) {AddCh(); goto case 25;}
				else {goto case 0;}
			case 3:
				if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 4;}
				else {goto case 0;}
			case 4:
				if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 5;}
				else {goto case 0;}
			case 5:
				if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 6;}
				else {goto case 0;}
			case 6:
				if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 2;}
				else {goto case 0;}
			case 7:
				{t.kind = 2; break;}
			case 8:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 24;}
				else if (ch == '.') {AddCh(); goto case 9;}
				else {goto case 0;}
			case 9:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 10;}
				else {goto case 0;}
			case 10:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 10;}
				else if (ch == 'e') {AddCh(); goto case 11;}
				else {t.kind = 3; break;}
			case 11:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 13;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 12;}
				else {goto case 0;}
			case 12:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 13;}
				else {goto case 0;}
			case 13:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 13;}
				else {t.kind = 3; break;}
			case 14:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 15;}
				else {goto case 0;}
			case 15:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 15;}
				else if (ch == 'e') {AddCh(); goto case 16;}
				else {t.kind = 3; break;}
			case 16:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 18;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 17;}
				else {goto case 0;}
			case 17:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 18;}
				else {goto case 0;}
			case 18:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 18;}
				else {t.kind = 3; break;}
			case 19:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 21;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 20;}
				else {goto case 0;}
			case 20:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 21;}
				else {goto case 0;}
			case 21:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 21;}
				else {t.kind = 3; break;}
			case 22:
				recEnd = pos; recKind = 24;
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= 65535) {AddCh(); goto case 22;}
				else if (ch == 13) {AddCh(); goto case 26;}
				else if (ch == 10) {AddCh(); goto case 23;}
				else {t.kind = 24; break;}
			case 23:
				{t.kind = 24; break;}
			case 24:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 24;}
				else if (ch == '.') {AddCh(); goto case 14;}
				else if (ch == 'e') {AddCh(); goto case 19;}
				else {t.kind = 3; break;}
			case 25:
				if (ch == '"' || ch == 39 || ch == '/' || ch == 92 || ch >= 'a' && ch <= 'b' || ch == 'f' || ch == 'n' || ch == 'r' || ch == 't' || ch == 'v') {AddCh(); goto case 2;}
				else if (ch == 'u') {AddCh(); goto case 3;}
				else {goto case 0;}
			case 26:
				recEnd = pos; recKind = 24;
				if (ch == 10) {AddCh(); goto case 23;}
				else {t.kind = 24; break;}
			case 27:
				{t.kind = 9; break;}
			case 28:
				{t.kind = 10; break;}
			case 29:
				if (ch == '.') {AddCh(); goto case 30;}
				else {goto case 0;}
			case 30:
				{t.kind = 11; break;}
			case 31:
				{t.kind = 12; break;}
			case 32:
				{t.kind = 13; break;}
			case 33:
				{t.kind = 14; break;}
			case 34:
				{t.kind = 17; break;}
			case 35:
				{t.kind = 18; break;}
			case 36:
				{t.kind = 19; break;}
			case 37:
				{t.kind = 20; break;}
			case 38:
				{t.kind = 21; break;}
			case 39:
				{t.kind = 22; break;}
			case 40:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 10;}
				else if (ch == '.') {AddCh(); goto case 29;}
				else {goto case 0;}

// ReSharper restore RedundantAssignment
// ReSharper restore RedundantIfElseBlock
		}
		t.val = new String(tval, 0, tlen);
		return t;
	}
	
	private void SetScannerBehindT() {
		buffer.Pos = t.pos;
		NextCh();
		line = t.line;
		col = t.col;
		charPos = t.charPos;
		for(var i = 0; i < tlen; i++) NextCh();
	}
	
	// get the next token (possibly a token already seen during peeking)
	public Token Scan() {
		if(tokens.next == null) {
			return NextToken();
		}
		pt = tokens = tokens.next;
		return tokens;
	}

	// peek for the next token, ignore pragmas
	public Token Peek() {
		do {
			if(pt.next == null) {
				pt.next = NextToken();
			}
			pt = pt.next;
		} while(pt.kind > maxT); // skip pragmas
		return pt;
	}
	
	// make sure that peeking starts at the current scan position
	public void ResetPeek() { pt = tokens; }

} // end Scanner

}