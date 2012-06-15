using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrattParser
{
    /// <summary>
    /// Common layer over the basic parser, making adding operators and literals much easier.
    /// </summary>
    /// <remarks>
    /// Using this layer allows parsers using basic maths operators really trivial to implement, using just
    /// one statement to add them to the symbol table.
    /// </remarks>
    /// <typeparam name="T">
    /// The type that the parser works on. This will be the expected return value from the parser.
    /// </typeparam>
    public class OperatorParser<T> : Parser<T>
    {
        #region token types

        /// <summary>
        /// Class for literal tokens. These are automatically named "(literal)", and Literal.Nud returns itself.
        /// </summary>
        public class Literal : Token
        {
            public T Value;

            public Literal( T value )
                : base( "(literal)" )
            {
                this.Value = value;
            }

            public override T Nud()
            {
                return this.Value;
            }
        }

        /// <summary>
        /// Operator base class. Class for any token that stores a function.
        /// </summary>
        /// <typeparam name="F">The delegate for the function type.</typeparam>
        public class Operator<F> : Token
        {
            public F Function;

            public Operator( string name, Precedence lbp, F function )
                : base( name, lbp )
            {
                this.Function = function;
            }
        }

        #region operator classes

        public delegate T InfixOperation( T left, T right );
        public delegate T PrefixOperation( T right );
        public delegate T PostfixOperation( T left );

        /// <summary>
        /// Class for InfixOperators
        /// </summary>
        /// <example>MyParser.Add( new InfixOperator( "*", Precedence.Multiplication, ( a, b ) => ( a * b ) ) );</example>
        public class InfixOperator : Operator<InfixOperation>
        {
            public InfixOperator( string name, Precedence lbp, InfixOperation function )
                : base( name, lbp, function )
            {
            }

            public override T Led(T left)
            {
                return this.Function( left, this.parser.Parse( this.Lbp ) );
            }
        }

        public class PrefixOperator : Operator<PrefixOperation>
        {
            public PrefixOperator( string name, Precedence lbp, PrefixOperation function )
                : base( name, lbp, function )
            {
            }

            public override T Nud()
            {
                return this.Function( this.parser.Parse( this.Lbp ) );
            }
        }

        public class PostfixOperator : Operator<PostfixOperation>
        {
            public PostfixOperator( string name, Precedence lbp, PostfixOperation function )
                : base( name, lbp, function )
            {
            }

            public override T Led(T left)
            {
                return this.Function( left );
            }
        }

        #endregion operator classes

        #endregion token types
    }
}
