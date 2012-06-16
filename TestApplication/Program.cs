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

            try
            {
                Console.WriteLine( p.Parse( r ) );
            }
            catch ( ParserException e )
            {
                Console.WriteLine( e.Message );
            }

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
            this.Add( new Token( "(end)", Precedence.End ) );
            this.Add( new AddOperator( ) );
            this.Add( new SubOperator( ) );
            this.Add( new InfixOperator( "/", Precedence.Multiplication, ( a, b ) => a / b ) );
            this.Add( new InfixOperator( "*", Precedence.Multiplication, ( a, b ) => a * b ) );
            this.Add( new Group( "(", ")" ) );
            this.Add( new Group( "{", "}" ) );
            this.Add( new Group( "[", "]" ) );
            this.Add( new Group( "<", ">" ) );

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

            do
            {
                ch = this.reader.Read( );
            } while ( Char.IsWhiteSpace( ( char )ch ) );

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
            else if ( ch == -1 )
            {
                return this.Symbols[ "(end)" ];
            }
            else if (this.Symbols.ContainsKey(((char)ch).ToString()))
            {
                return this.Symbols[ ( ( char )ch ).ToString( ) ];
            }
            else
            {
                switch (ch)
                {
                    case ')':
                        return ( ( Group )this.Symbols[ "(" ] ).End;
                    case ']':
                        return ( ( Group )this.Symbols[ "[" ] ).End;
                    case '}':
                        return ( ( Group )this.Symbols[ "{" ] ).End;
                    case '>':
                        return ( ( Group )this.Symbols[ "<" ] ).End;
                    default:
                        throw new ParserException( String.Format("Illegal character '{0}'.", (char)ch), this.reader );
                }
            }
        }
    }
}
