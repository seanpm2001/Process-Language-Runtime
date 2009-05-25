//
//  This CSharp output file generated by Managed Package LEX
//  Version:  0.6.0 (1-August-2007)
//  Machine:  EINAR-PC
//  DateTime: 11.5.2009 23:45:41
//  UserName: einar
//  MPLEX input file <lexer.lex>
//  MPLEX frame file <C:\Program Files\Microsoft Visual Studio 2008 SDK\VisualStudioIntegration\Tools\Bin\mplex.frame>
//
//  Option settings: unicode, parser, minimize, classes, compressmap, compressnext
//

#define BACKUP
//
// mplex.frame
// Version 0.6.1 of 1 August 2007
// Left and Right Anchored state support.
// Start condition stack. Two generic params.
// Using fixed length context handling for right anchors
//
using System;
using System.IO;
using System.Collections.Generic;
#if !STANDALONE
using Babel.ParserGenerator;
#endif // STANDALONE

using Babel.ParserGenerator;
using Babel;
using Babel.Parser;

namespace Babel.Lexer
{   
    /// <summary>
    /// Summary Canonical example of MPLEX automaton
    /// </summary>
    
#if STANDALONE
    //
    // These are the dummy declarations for stand-alone MPLEX applications
    // normally these declarations would come from the parser.
    // If you declare /noparser, or %option noparser then you get this.
    //

    public enum Tokens
    { 
      EOF = 0, maxParseToken = int.MaxValue 
      // must have at least these two, values are almost arbitrary
    }

    public abstract class ScanBase
    {
        public abstract int yylex();
        protected abstract int CurrentSc { get; set; }
        //
        // Override this virtual EolState property if the scanner state is more
        // complicated then a simple copy of the current start state ordinal
        //
        public virtual int EolState { get { return CurrentSc; } set { CurrentSc = value; } }
    }
    
    public interface IColorScan
    {
        void SetSource(string source, int offset);
        int GetNext(ref int state, out int start, out int end);
    }
    
    

#endif // STANDALONE

    public abstract class ScanBuff
    {
        public const int EOF = -1;
        public abstract int Pos { get; set; }
        public abstract int Read();
        public abstract int Peek();
        public abstract int ReadPos { get; }
        public abstract string GetString(int b, int e);
    }
    
    // If the compiler can't find ScanBase maybe you need
    // to run mppg with /mplex, or run mplex with /noparser
    public sealed class Scanner : ScanBase, IColorScan
    {
   
        public ScanBuff buffer;
        private IErrorHandler handler;
        int scState;
        
        private static int GetMaxParseToken() {
            System.Reflection.FieldInfo f = typeof(Tokens).GetField("maxParseToken");
            return (f == null ? int.MaxValue : (int)f.GetValue(null));
        }
        
        static int parserMax = GetMaxParseToken();        
        
        protected override int CurrentSc 
        {
             // The current start state is a property
             // to try to avoid the user error of setting
             // scState but forgetting to update the FSA
             // start state "currentStart"
             //
             get { return scState; }
             set { scState = value; currentStart = startState[value]; }
        }
        
        enum Result {accept, noMatch, contextFound};

        const int maxAccept = 50;
        const int initial = 51;
        const int eofNum = 0;
        const int goStart = -1;
        const int INITIAL = 0;
        const int COMMENT = 1;

internal void LoadYylval()
       {
           yylval.str = tokTxt;
           yylloc = new LexLocation(tokLin, tokCol, tokLin, tokCol + tokLen);
       }
       
       public override void yyerror(string s, params object[] a)
       {
           if (handler != null) handler.AddError(s, tokLin, tokCol, tokLin, tokCol + tokLen);
       }
        int state;
        int currentStart = initial;
        int chr;           // last character read
        int cNum;          // ordinal number of chr
        int lNum = 0;      // current line number
        int lineStartNum;  // cNum at start of line

        //
        // The following instance variables are useful, among other
        // things, for constructing the yylloc location objects.
        //
        int tokPos;        // buffer position at start of token
        int tokNum;        // ordinal number of first character
        int tokLen;        // number of character in token
        int tokCol;        // zero-based column number at start of token
        int tokLin;        // line number at start of token
        int tokEPos;       // buffer position at end of token
        int tokECol;       // column number at end of token
        int tokELin;       // line number at end of token
        string tokTxt;     // lazily constructed text of token
#if STACK          
        private Stack<int> scStack = new Stack<int>();
#endif // STACK

#region ScannerTables
    struct Table {
        public int min; public int rng; public int dflt;
        public sbyte[] nxt;
        public Table(int m, int x, int d, sbyte[] n) {
            min = m; rng = x; dflt = d; nxt = n;
        }
    };

    static int[] startState = {51, 46, 0};

#region CharacterMap
    //
    // There are 33 equivalence classes
    // There are 2 character sequence regions
    // There are 1 tables, 126 entries
    // There are 1 runs, 0 singletons
    //
    static sbyte[] map0 = new sbyte[126] {
/* \0     */ 31, 31, 31, 31, 31, 31, 31, 31, 31, 32, 0, 32, 32, 32, 31, 31, 
/* \020   */ 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 
/* \040   */ 32, 26, 10, 30, 31, 31, 29, 31, 15, 16, 24, 11, 14, 23, 6, 25, 
/* 0      */ 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 9, 13, 28, 21, 27, 31, 
/* @      */ 31, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 
/* P      */ 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 17, 31, 18, 22, 8, 
/* `      */ 31, 7, 7, 7, 7, 3, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 
/* p      */ 7, 7, 7, 2, 7, 1, 7, 7, 7, 7, 7, 19, 12, 20 };

    sbyte Map(int chr)
    { // '\0' <= chr <= '\uFFFF'
      if (chr < 126) return map0[chr - 0];
      else return (sbyte)31;
    }
#endregion

    static Table[] NxS = new Table[55];

    static Scanner() {
    NxS[0] = new Table(0, 0, 0, null);
    NxS[1] = new Table(0, 0, -1, null);
    NxS[2] = new Table(1, 7, -1, new sbyte[] {3, 44, 3, -1, -1, -1, 
        3});
    NxS[3] = new Table(1, 7, -1, new sbyte[] {3, 3, 3, -1, -1, -1, 
        3});
    NxS[4] = new Table(1, 7, -1, new sbyte[] {4, 4, 4, 4, 4, 54, 
        4});
    NxS[5] = new Table(5, 1, -1, new sbyte[] {5});
    NxS[6] = new Table(0, 0, -1, null);
    NxS[7] = new Table(1, 7, -1, new sbyte[] {53, 53, 53, -1, -1, -1, 
        53});
    NxS[8] = new Table(1, 8, -1, new sbyte[] {41, 41, 41, 41, 41, -1, 
        41, 41});
    NxS[9] = new Table(10, 1, 52, new sbyte[] {40});
    NxS[10] = new Table(0, 0, -1, null);
    NxS[11] = new Table(12, 1, -1, new sbyte[] {39});
    NxS[12] = new Table(0, 0, -1, null);
    NxS[13] = new Table(0, 0, -1, null);
    NxS[14] = new Table(0, 0, -1, null);
    NxS[15] = new Table(0, 0, -1, null);
    NxS[16] = new Table(0, 0, -1, null);
    NxS[17] = new Table(0, 0, -1, null);
    NxS[18] = new Table(0, 0, -1, null);
    NxS[19] = new Table(0, 0, -1, null);
    NxS[20] = new Table(21, 1, -1, new sbyte[] {38});
    NxS[21] = new Table(0, 0, -1, null);
    NxS[22] = new Table(0, 0, -1, null);
    NxS[23] = new Table(0, 0, -1, null);
    NxS[24] = new Table(0, 0, -1, null);
    NxS[25] = new Table(21, 1, -1, new sbyte[] {37});
    NxS[26] = new Table(21, 1, -1, new sbyte[] {36});
    NxS[27] = new Table(21, 1, -1, new sbyte[] {35});
    NxS[28] = new Table(29, 1, -1, new sbyte[] {34});
    NxS[29] = new Table(24, 10, 29, new sbyte[] {33, 29, 29, 29, 29, 29, 
        29, 29, 29, 32});
    NxS[30] = new Table(0, 0, -1, null);
    NxS[31] = new Table(32, 1, -1, new sbyte[] {31});
    NxS[32] = new Table(0, 0, -1, null);
    NxS[33] = new Table(24, 10, -1, new sbyte[] {33, -1, -1, -1, -1, -1, 
        -1, -1, -1, 32});
    NxS[34] = new Table(0, 0, -1, null);
    NxS[35] = new Table(0, 0, -1, null);
    NxS[36] = new Table(0, 0, -1, null);
    NxS[37] = new Table(0, 0, -1, null);
    NxS[38] = new Table(0, 0, -1, null);
    NxS[39] = new Table(0, 0, -1, null);
    NxS[40] = new Table(0, 0, -1, null);
    NxS[41] = new Table(1, 8, -1, new sbyte[] {41, 41, 41, 41, 41, -1, 
        41, 41});
    NxS[42] = new Table(0, 0, -1, null);
    NxS[43] = new Table(1, 7, -1, new sbyte[] {43, 43, 43, 43, 43, 54, 
        43});
    NxS[44] = new Table(1, 7, -1, new sbyte[] {3, 3, 45, -1, -1, -1, 
        3});
    NxS[45] = new Table(1, 7, -1, new sbyte[] {3, 3, 3, -1, -1, -1, 
        3});
    NxS[46] = new Table(24, 10, 48, new sbyte[] {49, 48, 48, 48, 48, 48, 
        48, 48, 48, 47});
    NxS[47] = new Table(0, 0, -1, null);
    NxS[48] = new Table(24, 10, 48, new sbyte[] {49, 48, 48, 48, 48, 48, 
        48, 48, 48, 50});
    NxS[49] = new Table(24, 10, -1, new sbyte[] {49, -1, -1, -1, -1, -1, 
        -1, -1, -1, 50});
    NxS[50] = new Table(0, 0, -1, null);
    NxS[51] = new Table(4, 31, 3, new sbyte[] {4, 5, 6, 3, 7, 8, 
        9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 
        25, 26, 27, 28, 29, 30, 31, 1, 2});
    NxS[52] = new Table(10, 1, 52, new sbyte[] {40});
    NxS[53] = new Table(1, 8, -1, new sbyte[] {53, 53, 53, -1, -1, -1, 
        53, 42});
    NxS[54] = new Table(4, 1, -1, new sbyte[] {43});
    }

int NextState(int qStat) {
    if (chr == ScanBuff.EOF)
        return (qStat <= maxAccept && qStat != currentStart ? currentStart : eofNum);
    else {
        int rslt;
        int idx = Map(chr) - NxS[qStat].min;
        if (idx < 0) idx += 33;
        if ((uint)idx >= (uint)NxS[qStat].rng) rslt = NxS[qStat].dflt;
        else rslt = NxS[qStat].nxt[idx];
        return (rslt == goStart ? currentStart : rslt);
    }
}

int NextState() {
    if (chr == ScanBuff.EOF)
        return (state <= maxAccept && state != currentStart ? currentStart : eofNum);
    else {
        int rslt;
        int idx = Map(chr) - NxS[state].min;
        if (idx < 0) idx += 33;
        if ((uint)idx >= (uint)NxS[state].rng) rslt = NxS[state].dflt;
        else rslt = NxS[state].nxt[idx];
        return (rslt == goStart ? currentStart : rslt);
    }
}
#endregion


#if BACKUP
        // ====================== Nested class ==========================

        internal class Context // class used for automaton backup.
        {
            public int bPos;
            public int cNum;
            public int state;
            public int cChr;
        }
#endif // BACKUP


        // ====================== Nested class ==========================

        public sealed class StringBuff : ScanBuff
        {
            string str;        // input buffer
            int bPos;          // current position in buffer
            int sLen;

            public StringBuff(string str)
            {
                this.str = str;
                this.sLen = str.Length;
            }

            public override int Read()
            {
                if (bPos < sLen) return str[bPos++];
                else if (bPos == sLen) { bPos++; return '\n'; }   // one strike, see newline
                else return EOF;                                  // two strikes and you're out!
            }
            
            public override int ReadPos { get { return bPos - 1; } }

            public override int Peek()
            {
                if (bPos < sLen) return str[bPos];
                else return '\n';
            }

            public override string GetString(int beg, int end)
            {
                //  "end" can be greater than sLen with the BABEL
                //  option set.  Read returns a "virtual" EOL if
                //  an attempt is made to read past the end of the
                //  string buffer.  Without the guard any attempt 
                //  to fetch yytext for a token that includes the 
                //  EOL will throw an index exception.
                if (end > sLen) end = sLen;
                if (end <= beg) return ""; 
                else return str.Substring(beg, end - beg);
            }

            public override int Pos
            {
                get { return bPos; }
                set { bPos = value; }
            }
        }

        // ====================== Nested class ==========================

        public sealed class StreamBuff : ScanBuff
        {
            BufferedStream bStrm;   // input buffer
            int delta = 1;

            public StreamBuff(Stream str) { this.bStrm = new BufferedStream(str); }

            public override int Read() {
                return bStrm.ReadByte(); 
            }
            
            public override int ReadPos {
                get { return (int)bStrm.Position - delta; }
            }

            public override int Peek()
            {
                int rslt = bStrm.ReadByte();
                bStrm.Seek(-delta, SeekOrigin.Current);
                return rslt;
            }

            public override string GetString(int beg, int end)
            {
                if (end - beg <= 0) return "";
                long savePos = bStrm.Position;
                char[] arr = new char[end - beg];
                bStrm.Position = (long)beg;
                for (int i = 0; i < (end - beg); i++)
                    arr[i] = (char)bStrm.ReadByte();
                bStrm.Position = savePos;
                return new String(arr);
            }

            // Pos is the position *after* reading chr!
            public override int Pos
            {
                get { return (int)bStrm.Position; }
                set { bStrm.Position = value; }
            }
        }

        // ====================== Nested class ==========================

        /// <summary>
        /// This is the Buffer for UTF8 files.
        /// It attempts to read the encoding preamble, which for 
        /// this encoding should be unicode point \uFEFF which is 
        /// encoded as EF BB BF
        /// </summary>
        public class TextBuff : ScanBuff
        {
            protected BufferedStream bStrm;   // input buffer
            protected int delta = 1;
            
            private Exception BadUTF8()
            { return new Exception(String.Format("BadUTF8 Character")); }

            /// <summary>
            /// TextBuff factory.  Reads the file preamble
            /// and returns a TextBuff, LittleEndTextBuff or
            /// BigEndTextBuff according to the result.
            /// </summary>
            /// <param name="strm">The underlying stream</param>
            /// <returns></returns>
            public static TextBuff NewTextBuff(Stream strm)
            {
                // First check if this is a UTF16 file
                //
                int b0 = strm.ReadByte();
                int b1 = strm.ReadByte();

                if (b0 == 0xfe && b1 == 0xff)
                    return new BigEndTextBuff(strm);
                if (b0 == 0xff && b1 == 0xfe)
                    return new LittleEndTextBuff(strm);
                
                int b2 = strm.ReadByte();
                if (b0 == 0xef && b1 == 0xbb && b2 == 0xbf)
                    return new TextBuff(strm);
                //
                // There is no unicode preamble, so we
                // must go back to the UTF8 default.
                //
                strm.Seek(0, SeekOrigin.Begin);
                return new TextBuff(strm);
            }

            protected TextBuff(Stream str) { 
                this.bStrm = new BufferedStream(str);
            }

            public override int Read()
            {
                int ch0 = bStrm.ReadByte();
                int ch1;
                int ch2;
                if (ch0 < 0x7f)
                {
                    delta = (ch0 == EOF ? 0 : 1);
                    return ch0;
                }
                else if ((ch0 & 0xe0) == 0xc0)
                {
                    delta = 2;
                    ch1 = bStrm.ReadByte();
                    if ((ch1 & 0xc0) == 0x80)
                        return ((ch0 & 0x1f) << 6) + (ch1 & 0x3f);
                    else
                        throw BadUTF8();
                }
                else if ((ch0 & 0xf0) == 0xe0)
                {
                    delta = 3;
                    ch1 = bStrm.ReadByte();
                    ch2 = bStrm.ReadByte();
                    if ((ch1 & ch2 & 0xc0) == 0x80)
                        return ((ch0 & 0xf) << 12) + ((ch1 & 0x3f) << 6) + (ch2 & 0x3f);
                    else
                        throw BadUTF8();
                }
                else
                    throw BadUTF8();
            }

            public sealed override int ReadPos
            {
                get { return (int)bStrm.Position - delta; }
            }

            public sealed override int Peek()
            {
                int rslt = Read();
                bStrm.Seek(-delta, SeekOrigin.Current);
                return rslt;
            }

            /// <summary>
            /// Returns the string from the buffer between
            /// the given file positions.  This needs to be
            /// done carefully, as the number of characters
            /// is, in general, not equal to (end - beg).
            /// </summary>
            /// <param name="beg">Begin filepos</param>
            /// <param name="end">End filepos</param>
            /// <returns></returns>
            public sealed override string GetString(int beg, int end)
            {
                int i;
                if (end - beg <= 0) return "";
                long savePos = bStrm.Position;
                char[] arr = new char[end - beg];
                bStrm.Position = (long)beg;
                for (i = 0; bStrm.Position < end; i++)
                    arr[i] = (char)Read();
                bStrm.Position = savePos;
                return new String(arr, 0, i);
            }

            // Pos is the position *after* reading chr!
            public sealed override int Pos
            {
                get { return (int)bStrm.Position; }
                set { bStrm.Position = value; }
            }
        }

        // ====================== Nested class ==========================
        /// <summary>
        /// This is the Buffer for Big-endian UTF16 files.
        /// </summary>
        public sealed class BigEndTextBuff : TextBuff
        {
            internal BigEndTextBuff(Stream str) : base(str) { } // 

            public override int Read()
            {
                int ch0 = bStrm.ReadByte();
                int ch1 = bStrm.ReadByte();
                return (ch0 << 8) + ch1;
            }
        }
        
        // ====================== Nested class ==========================
        /// <summary>
        /// This is the Buffer for Little-endian UTF16 files.
        /// </summary>
        public sealed class LittleEndTextBuff : TextBuff
        {
            internal LittleEndTextBuff(Stream str) : base(str) { } // { this.bStrm = new BufferedStream(str); }

            public override int Read()
            {
                int ch0 = bStrm.ReadByte();
                int ch1 = bStrm.ReadByte();
                return (ch1 << 8) + ch0;
            }
        }
        
        // =================== End Nested classes =======================

        public Scanner(Stream file) {
            buffer = TextBuff.NewTextBuff(file); // selected by /unicode option
            this.cNum = -1;
            this.chr = '\n'; // to initialize yyline, yycol and lineStart
            GetChr();
        }

        public Scanner() { }

        void GetChr()
        {
            if (chr == '\n') 
            { 
                lineStartNum = cNum + 1; 
                lNum++; 
            }
            chr = buffer.Read();
            cNum++;
        }

        void MarkToken()
        {
            tokPos = buffer.ReadPos;
            tokNum = cNum;
            tokLin = lNum;
            tokCol = cNum - lineStartNum;
        }
        
        void MarkEnd()
        {
            tokTxt = null;
            tokLen = cNum - tokNum;
            tokEPos = buffer.ReadPos;
            tokELin = lNum;
            tokECol = cNum - lineStartNum;
        }
 
        // ================ StringBuffer Initialization ===================

        public void SetSource(string source, int offset)
        {
            this.buffer = new StringBuff(source);
            this.buffer.Pos = offset;
            this.cNum = offset - 1;
            this.chr = '\n'; // to initialize yyline, yycol and lineStart
            GetChr();
        }
        
        public int GetNext(ref int state, out int start, out int end)
        {
            Tokens next;
            EolState = state;
            next = (Tokens)Scan();
            state = EolState;
            start = tokPos;
            end = tokEPos - 1; // end is the index of last char.
            return (int)next;
        }

        // ======== IScanner<> Implementation =========

        public override int yylex()
        {
            // parserMax is set by reflecting on the Tokens
            // enumeration.  If maxParseTokeen is defined
            // that is used, otherwise int.MaxValue is used.
            //
            int next;
            do { next = Scan(); } while (next >= parserMax);
            return next;
        }
        
        int yyleng { get { return tokLen; } }
        int yypos { get { return tokPos; } }
        int yyline { get { return tokLin; } }
        int yycol { get { return tokCol; } }

        public string yytext
        {
            get 
            {
                if (tokTxt == null) 
                    tokTxt = buffer.GetString(tokPos, tokEPos);
                return tokTxt;
            }
        }

        void yyless(int n) { 
            buffer.Pos = tokPos;
            cNum = tokNum;
            for (int i = 0; i <= n; i++) GetChr();
            MarkEnd();
        }

        public IErrorHandler Handler { get { return this.handler; }
                                       set { this.handler = value; }}

        // ============ methods available in actions ==============

        internal int YY_START {
            get { return CurrentSc; }
            set { CurrentSc = value; } 
        }

        // ============== The main tokenizer code =================

        int Scan()
        {
            try {
                for (; ; )
                {
                    int next;              // next state to enter                   
#if BACKUP
                    bool inAccept = false; // inAccept ==> current state is an accept state
                    Result rslt = Result.noMatch;
                    // skip "idle" transitions
#if LEFTANCHORS
                    if (lineStartNum == cNum && NextState(anchorState[CurrentSc]) != currentStart)
                        state = anchorState[CurrentSc];
                    else {
                        state = currentStart;
                        while (NextState() == state) {
                            GetChr();
                            if (lineStartNum == cNum) {
                                int anchor = anchorState[CurrentSc];
                                if (NextState(anchor) != state) {
                                    state = anchor; 
                                    break;
                                }
                            }
                        }
                    }
#else // !LEFTANCHORS
                    state = currentStart;
                    while (NextState() == state) 
                        GetChr(); // skip "idle" transitions
#endif // LEFTANCHORS
                    MarkToken();
                    
                    while ((next = NextState()) != currentStart)
                        if (inAccept && next > maxAccept) // need to prepare backup data
                        {
                            Context ctx = new Context();
                            rslt = Recurse2(ctx, next);
                            if (rslt == Result.noMatch) RestoreStateAndPos(ctx);
                            // else if (rslt == Result.contextFound) RestorePos(ctx);
                            break;
                        }
                        else
                        {
                            state = next;
                            GetChr();
                            if (state <= maxAccept) inAccept = true;
                        }
#else // !BACKUP
#if LEFTANCHORS
                    if (lineStartNum == cNum) {
                        int anchor = anchorState[CurrentSc];
                        if (NextState(anchor) != currentStart)
                            state = anchor;
                    }
                    else {
                        state = currentStart;
                        while (NextState() == state) {
                            GetChr();
                            if (lineStartNum == cNum) {
                                anchor = anchorState[CurrentSc];
                                if (NextState(anchor) != state) {
                                    state = anchor;
                                    break;
                                }
                            }
                        }
                    }
#else // !LEFTANCHORS
                    state = currentStart;
                    while (NextState() == state) 
                        GetChr(); // skip "idle" transitions
#endif // LEFTANCHORS
                    MarkToken();
                    // common code
                    while ((next = NextState()) != currentStart)
                    {
                        state = next;
                        GetChr();
                    }
#endif // BACKUP
                    if (state > maxAccept) 
                        state = currentStart;
                    else
                    {
                        MarkEnd();
#region ActionSwitch
#pragma warning disable 162
    switch (state)
    {
        case eofNum:
            return (int)Tokens.EOF;
        case 1:
return (int)Tokens.LEX_WHITE;
            break;
        case 2:
        case 3:
        case 44:
return (int)Tokens.LCASEIDENT;
            break;
        case 4:
return (int)Tokens.PROC;
            break;
        case 5:
return (int)Tokens.NUMBER;
            break;
        case 6:
return (int)'.';
            break;
        case 7:
        case 8:
        case 9:
        case 30:
yyerror("illegal char");
                             return (int)Tokens.LEX_ERROR;
            break;
        case 10:
return (int)'+';
            break;
        case 11:
return (int)'|';
            break;
        case 12:
return (int)';';
            break;
        case 13:
return (int)',';
            break;
        case 14:
return (int)'(';
            break;
        case 15:
return (int)')';
            break;
        case 16:
return (int)'[';
            break;
        case 17:
return (int)']';
            break;
        case 18:
return (int)'{';
            break;
        case 19:
return (int)'}';
            break;
        case 20:
return (int)'=';
            break;
        case 21:
return (int)'^';
            break;
        case 22:
return (int)'-';
            break;
        case 23:
return (int)'*';
            break;
        case 24:
return (int)'/';
            break;
        case 25:
return (int)'!';
            break;
        case 26:
return (int)Tokens.GT;
            break;
        case 27:
return (int)Tokens.LT;
            break;
        case 28:
return (int)'&';
            break;
        case 29:
        case 33:
BEGIN(COMMENT); return (int)Tokens.LEX_COMMENT;
            break;
        case 31:
return (int)Tokens.LEX_WHITE;
            break;
        case 32:
return (int)Tokens.LEX_COMMENT;
            break;
        case 34:
return (int)Tokens.AMPAMP;
            break;
        case 35:
return (int)Tokens.LTE;
            break;
        case 36:
return (int)Tokens.GTE;
            break;
        case 37:
return (int)Tokens.NEQ;
            break;
        case 38:
return (int)Tokens.EQ;
            break;
        case 39:
return (int)Tokens.BARBAR;
            break;
        case 40:
return (int)Tokens.STRING;
            break;
        case 41:
return (int)Tokens.METHOD;
            break;
        case 42:
return (int)Tokens.INACTION;
            break;
        case 43:
return (int)Tokens.FULLCLASS;
            break;
        case 45:
return (int)Tokens.KWUSE;
            break;
        case 46:
        case 47:
        case 48:
        case 49:
return (int)Tokens.LEX_COMMENT;
            break;
        case 50:
BEGIN(INITIAL); return (int)Tokens.LEX_COMMENT;
            break;
        default:
            break;
    }
#pragma warning restore 162
#endregion
                    }
                }
            } // end try
            finally {
LoadYylval();
            } // end finally
        }

#if BACKUP
        Result Recurse2(Context ctx, int next)
        {
            // Assert: at entry "state" is an accept state AND
            //         NextState(state, chr) != currentStart AND
            //         NextState(state, chr) is not an accept state.
            //
            bool inAccept;
            SaveStateAndPos(ctx);
            state = next;
            if (state == eofNum) return Result.accept;
            GetChr();
            inAccept = false;

            while ((next = NextState()) != currentStart)
            {
                if (inAccept && next > maxAccept) // need to prepare backup data
                    SaveStateAndPos(ctx);
                state = next;
                if (state == eofNum) return Result.accept;
                GetChr(); 
                inAccept = (state <= maxAccept);
            }
            if (inAccept) return Result.accept; else return Result.noMatch;
        }

        void SaveStateAndPos(Context ctx)
        {
            ctx.bPos  = buffer.Pos;
            ctx.cNum  = cNum;
            ctx.state = state;
            ctx.cChr = chr;
        }

        void RestoreStateAndPos(Context ctx)
        {
            buffer.Pos = ctx.bPos;
            cNum = ctx.cNum;
            state = ctx.state;
            chr = ctx.cChr;
        }

        void RestorePos(Context ctx) { buffer.Pos = ctx.bPos; cNum = ctx.cNum; }
#endif // BACKUP

        // ============= End of the tokenizer code ================

        internal void BEGIN(int next)
        { CurrentSc = next; }

#if STACK        
        internal void yy_clear_stack() { scStack.Clear(); }
        internal int yy_top_state() { return scStack.Peek(); }
        
        internal void yy_push_state(int state)
        {
            scStack.Push(CurrentSc);
            CurrentSc = state;
        }
        
        internal void yy_pop_state()
        {
            // Protect against input errors that pop too far ...
            if (scStack.Count > 0) {
				int newSc = scStack.Pop();
				CurrentSc = newSc;
            } // Otherwise leave stack unchanged.
        }
 #endif // STACK

        internal void ECHO() { Console.Out.Write(yytext); }
        
#region UserCodeSection

/* .... */

#endregion
    } // end class Scanner
} // end namespace