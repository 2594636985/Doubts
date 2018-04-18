using Doubts.Framework.EL.Compiler;
using Doubts.Framework.EL.Compiler.Translation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Doubts.Framework.EL
{
    internal class FunctionBuilder
    {
        private static readonly Expression EmptyLambda = Expression.Lambda<Func<object, object>>(Expression.Constant(null), Expression.Parameter(typeof(object)));

        public Expression Compile(IEnumerable<Expression> expressions, Expression parentContext)
        {
            try
            {
                if (expressions.Any() == false)
                {
                    return EmptyLambda;
                }

                if (expressions.IsOneOf<Expression, DefaultExpression>() == true)
                {
                    return EmptyLambda;
                }

                var objectParameter = Expression.Parameter(typeof(object), "data");

                var enumerator = expressions.GetEnumerator();

                Expression LambdaExprBody = null;

                while (enumerator.MoveNext())
                {
                    Expression item = enumerator.Current;

                    if (item is ElPropertyExpression)
                    {
                        ElPropertyExpression elPropertyExpr = item as ElPropertyExpression;

                        LambdaExprBody = Expression.(elPropertyExpr.PropertyName);

                        if (string.IsNullOrWhiteSpace(elPropertyExpr.Indexes))
                        {

                        }
                    }
                }


                return Expression.Lambda<Func<object, object>>(LambdaExprBody, new[] { objectParameter });


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Func<object, object> Compile(IEnumerable<Expression> expressions)
        {
            try
            {
                var expression = Compile(expressions, null);

                return ((Expression<Func<object, object>>)expression).Compile();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Expression CreateExpressionBlock(IEnumerable<Expression> expressions)
        {
            return Expression.Block(expressions);
        }
    }
}
