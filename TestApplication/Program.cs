using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PrattParser;
using System.IO;

namespace TestApplication
{
    class Program
    {
        static void Main( string[ ] args )
        {
            Console.Write( "> " );
            string s = Console.ReadLine( );

            SimpleParser p = new SimpleParser( );

            StringReader r = new StringReader( s );

            Console.Write( "{0} = ", s );
            Console.WriteLine(p.Parse( r ));

            Console.ReadKey( );
        }
    }

    public class SimpleParser : OperatorParser<int>
    {
        /// <summary>
        /// Contructs the simple parser, adding the operators.
        /// </summary>
        public SimpleParser( )
            : base( )
        {
            this.Add( new Token( "(end)", 0 ) );
            this.Add( new AddOperator( ) );
            this.Add( new SubOperator( ) );
            this.Add( new InfixOperator( "/", Precedence.Multiplication, ( a, b ) => a / b ) );
            this.Add( new InfixOperator( "*", Precedence.Multiplication, ( a, b ) => a * b ) );
            this.Add( new Group( "(", ")" ) );

            this.Add( new PostfixOperator( "!", Precedence.UnaryOp, this.factorial ) );
        }

        public int factorial(int a)
        {
            if ( a <= 1 )
                return 1;
            else
                return a * this.factorial( a - 1 );
        }

        internal class AddOperator : Token
        {
            public AddOperator( )
                : base( "+", Precedence.Addition )
            {
            }

            public override int Nud()
            {
                return this.parser.Parse( this.Lbp );
            }

            public override int Led(int left)
            {
                return left + this.parser.Parse( this.Lbp );
            }
        }

        internal class SubOperator : Token
        {
            public SubOperator( )
                : base( "-", Precedence.Addition )
            {
            }

            public override int Nud()
            {
                return -this.parser.Parse( this.Lbp );
            }

            public override int Led(int left)
            {
                return left - this.parser.Parse( this.Lbp );
            }
        }

        public override Token Advance( )
        {
            int ch;

            ch = this.reader.Read( );

            if ( Char.IsDigit( ( char )ch ) )
            {
                StringBuilder b = new StringBuilder( );

                for ( ; ; )
                {
                    b.Append( ( char )ch );

                    if ( !Char.IsDigit( ( char )this.reader.Peek( ) ) )
                        break;

                    ch = this.reader.Read( );
                }

                return new Literal( Int32.Parse( b.ToString( ) ) );
            }
            else if ( ch == '+' )
            {
                return this.Symbols[ "+" ];
            }
            else if ( ch == '-' )
            {
                return this.Symbols[ "-" ];
            }
            else if ( ch == '*' )
            {
                return this.Symbols[ "*" ];
            }
            else if ( ch == '/' )
            {
                return this.Symbols[ "/" ];
            }
            else if ( ch == '!' )
            {
                return this.Symbols[ "!" ];
            }
            else if ( ch == '(' )
            {
                return this.Symbols[ "(" ];
            }
            else if ( ch == ')' )
            {
                return ((Group)this.Symbols[ "(" ]).End;
            }
            else if ( ch == -1 )
            {
                return this.Symbols[ "(end)" ];
            }
            else
            {
                throw new Exception( );
            }
        }
    }
}
