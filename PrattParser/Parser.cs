using System;
using System.Collections.Generic;
using System.IO;

namespace PrattParser
{
    public enum Precedence : int
    {
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
        public TextReader reader;
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
                throw new Exception( );
            }

            public virtual T Led(T left)
            {
                throw new Exception( );
            }
        }

        /// <summary>
        /// Initializes the actual parsing using the given TextReader.
        /// </summary>
        /// <param name="input">The input source to be parsed.</param>
        /// <returns>The result of type T.</returns>
        public T Parse(TextReader input)
        {
            this.reader = input;

            this.token = this.Advance( );
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
            this.token = this.Advance();

            // Handle first token of expression.
            T left = t.Nud();

            // Parser loop. Handles precedence.
            while (rbp < token.Lbp)
            {
                t = this.token;
                this.token = this.Advance();
                left = t.Led(left);
            }

            return left;
        }

        /// <summary>Advances to the next token in the reader stream.</summary>
        /// <remarks>
        /// The current string being read is held in this.reader, and is designed to be used by this function.
        /// 
        /// For an example see the SimpleParser class.
        /// </remarks>
        /// <returns>Either a new token, or an existing one in the symbol table.</returns>
        public virtual Token Advance()
        {
            throw new NotImplementedException( );
        }
    }
}
