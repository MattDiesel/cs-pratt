using System;
using System.Collections.Generic;
using System.IO;

namespace PrattParser
{
    public class ParserException : Exception
    {
        string s;
        int line;
        int col;
        string file;

        public ParserException(string msg)
            : this(msg, "", -1, -1)
        {
        }

        public ParserException(string msg, PositionalTextReader reader)
            : this(msg, reader.Source, reader.Line, reader.Col)
        {
        }

        public ParserException(string msg, string file, int line, int col)
        {
            this.s = msg;
            this.file = file;
            this.line = line;
            this.col = col;
        }

        public override string Message
        {
            get
            {
                return String.Format( "{0} ({1}:{2}): {3}", this.file, this.line, this.col, this.s );
            }
        }
    }

    public enum Precedence : int
    {
        End = -1,
        None = 0,
        Assignment = 100,
        LogicalOR = 200,
        LogicalAND = 300,
        BitwiseOR = 400,
        BitwiseXOR = 500,
        BitwiseAND = 600,
        Equality = 700,
        Comparison = 800,
        BitwiseSHIFT = 900,
        Addition = 1000,
        Multiplication = 1100,
        Exponential = 1200,
        Call = 1300,
        UnaryOp = 1400,
        Access = 1500
    }

    /// <summary>
    /// Base parser class.
    /// </summary>
    /// <typeparam name="T">
    /// The type that the parser works on. This will be the expected return value from the parser.
    /// </typeparam>
    public class Parser<T>
    {
        public PositionalTextReader reader;
        private Token token;

        /// <summary>
        /// The symbol table.
        /// </summary>
        public Dictionary<string, Token> Symbols;

        public Parser()
        {
            this.Symbols = new Dictionary<string, Token>( );
        }

        /// <summary>
        /// Adds a token to the list of symbols.
        /// </summary>
        /// <param name="t"></param>
        public void Add(Token t)
        {
            t.parser = this;
            this.Symbols.Add(t.Name, t);
        }

        /// <summary>
        /// Base Token class. All tokens must have a name, and a binding power.
        /// </summary>
        public class Token
        {
            public Parser<T> parser;
            public string Name;
            public Precedence Lbp;

            public Token(string name)
            {
                this.Name = name;
                this.Lbp = Precedence.None;
            }

            public Token(string name, Precedence lbp)
            {
                this.Name = name;
                this.Lbp = lbp;
            }
            
            public delegate T NudHandler();
            public delegate T LedHandler(T left);

            public virtual T Nud()
            {
                throw new ParserException(
                    String.Format("Token type {0} cannot appear at the beginning of an expression.", this.Name ),
                    this.parser.reader );
            }

            public virtual T Led(T left)
            {
                throw new ParserException(
                    String.Format( "Token type {0} cannot appear in the middle of an expression.", this.Name ),
                    this.parser.reader );
            }
        }

        /// <summary>
        /// Initializes the actual parsing using the given TextReader.
        /// </summary>
        /// <param name="input">The input source to be parsed.</param>
        /// <returns>The result of type T.</returns>
        public T Parse(TextReader input)
        {
            this.reader = new PositionalTextReader(input, input.GetType().Name);

            this.Step( );
            return this.Parse( Precedence.None );
        }

        /// <summary>
        /// Basic parsing function.
        /// </summary>
        /// <param name="rbp">The right hand side binding power.</param>
        /// <returns>The result of type T.</returns>
        public T Parse(Precedence rbp)
        {
            Token t = this.token;
            this.Step( );

            // Handle first token of expression.
            T left = t.Nud();

            // Parser loop. Handles precedence.
            while (rbp <= token.Lbp)
            {
                t = this.token;
                this.Step( );
                left = t.Led(left);
            }

            return left;
        }

        /// <summary>Advances to the next token in the reader stream.</summary>
        /// <remarks>
        /// Do not call this function directly unless you know you have to. Call the Parser.Step()
        /// method instead.
        /// 
        /// The current string being read is held in this.reader, and is designed to be used by this function.
        /// </remarks>
        /// <returns>Either a new token, or an existing one in the symbol table.</returns>
        public virtual Token Advance()
        {
            throw new NotImplementedException( );
        }

        /// <summary>
        /// Advance to the next token in the stream, and sets internal variables.
        /// </summary>
        public void Step()
        {
            this.Step( "" );
        }

        /// <summary>
        /// Advance to the next token in the stream, checking to make sure it is the right
        /// type, and sets internal variables.
        /// </summary>
        /// <param name="t">The token type that must appear next.</param>
        public void Step(Token t)
        {
            this.Step( t.Name );
        }

        /// <summary>
        /// Advance to the next token in the stream, checking to make sure it is the right
        /// type, and sets internal variables.
        /// </summary>
        /// <param name="t">The token type name that must appear next.</param>
        public void Step(string s)
        {
            if ( ( s != "" ) && ( this.token.Name != s ) )
                throw new Exception( );

            this.token = this.Advance( );
            this.token.parser = this;
        }
    }

    /// <summary>
    /// TextReader wrapper that keeps track of line and column number.
    /// </summary>
    public class PositionalTextReader
    {
        private TextReader reader;
        private string source;

        public string Source
        {
            get
            {
                return this.source;
            }
        }

        private int line;
        public int Line
        {
            get
            {
                return this.line;
            }
        }

        private int col;
        public int Col
        {
            get
            {
                return this.col;
            }
        }

        public PositionalTextReader(TextReader r, string source)
        {
            this.reader = r;
            this.source = source;
            this.line = 1;
            this.col = -1;
        }

        public PositionalTextReader(string s)
            : this(new StringReader(s), "<string>")
        {
        }

        public int Peek()
        {
            return this.reader.Peek( );
        }

        public int Read( )
        {
            int ch = this.reader.Read( );

            if ( ch == '\n' )
            {
                this.line++;
                this.col = 1;
            }
            else
            {
                this.col++;
            }

            return ch;
        }
    }
}
