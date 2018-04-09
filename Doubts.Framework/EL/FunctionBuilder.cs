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
        private static readonly Expression _emptyLambda = Expression.Lambda<Action<TextWriter, object>>(Expression.Empty(), Expression.Parameter(typeof(TextWriter)), Expression.Parameter(typeof(object)));

        public Expression Compile(IEnumerable<Expression> expressions, Expression parentContext, string templatePath = null)
        {
            try
            {
                if (expressions.Any() == false)
                {
                    return _emptyLambda;
                }

                if (expressions.IsOneOf<Expression, DefaultExpression>() == true)
                {
                    return _emptyLambda;
                }

                //var compilationContext = new CompilationContext(_configuration);

                var expression = CreateExpressionBlock(expressions);

                //expression = CommentVisitor.Visit(expression, compilationContext);
                //expression = UnencodedStatementVisitor.Visit(expression, compilationContext);
                //expression = PartialBinder.Bind(expression, compilationContext);
                //expression = StaticReplacer.Replace(expression, compilationContext);
                //expression = IteratorBinder.Bind(expression, compilationContext);
                //expression = BlockHelperFunctionBinder.Bind(expression, compilationContext);
                //expression = DeferredSectionVisitor.Bind(expression, compilationContext);
                //expression = HelperFunctionBinder.Bind(expression, compilationContext);
                //expression = BoolishConverter.Convert(expression, compilationContext);
                //expression = PathBinder.Bind(expression, compilationContext);
                //expression = SubExpressionVisitor.Visit(expression, compilationContext);
                //expression = ContextBinder.Bind(expression, compilationContext, parentContext, templatePath);

                return expression;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Action<TextWriter, object> Compile(IEnumerable<Expression> expressions, string templatePath = null)
        {
            try
            {
                var expression = Compile(expressions, null, templatePath);
                return ((Expression<Action<TextWriter, object>>)expression).Compile();
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
