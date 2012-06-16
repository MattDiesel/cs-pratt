using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrattParser
{
    public class AST
    {
        public string Name;
        public AST Left;
        public AST Right;

        public AST(string name, AST left, AST right)
        {
            this.Name = name;
            this.Left = left;
            this.Right = right;
        }

        /// <summary>
        /// Basic function that prints an AST as LISP like expressions
        /// </summary>
        /// <returns></returns>
        public override string ToString( )
        {
            StringBuilder sb = new StringBuilder( );

            sb.Append( "(" + this.Name );

            if ( this.Left != null )
            {
                sb.Append( " " + this.Left.ToString( ) );
            }

            if ( this.Right != null )
            {
                sb.Append( " " + this.Right.ToString( ) );
            }

            sb.Append( ")" );

            return sb.ToString( );
        }
    }
}
