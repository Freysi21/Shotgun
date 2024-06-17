using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Shotgun.Expressions
{
    public static class Range
    {
        public static Expression<Func<T, bool>> RangeExpression<T>(Dictionary<string, string[]> dateDict)
        {
            Expression and = null;
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "o");
            foreach (var entry in dateDict)
            {
                if (entry.Value.Count() == 2)
                {
                    var from2 = Convert.ToDateTime(entry.Value[0]);
                    var to2 = Convert.ToDateTime(entry.Value[1]);

                    Expression expression = Expression.PropertyOrField(parameterExpression, entry.Key);
                    ConstantExpression valueExpression1 = null;
                    ConstantExpression valueExpression2 = null;

                    Type typeIfNullable = Nullable.GetUnderlyingType(expression.Type);
                    if (typeIfNullable != null)
                    {
                        valueExpression1 = Expression.Constant(to2, typeof(Nullable<DateTime>));
                        valueExpression2 = Expression.Constant(from2, typeof(Nullable<DateTime>));
                    }
                    else
                    {
                        valueExpression1 = Expression.Constant(to2, typeof(DateTime));
                        valueExpression2 = Expression.Constant(from2, typeof(DateTime));
                    }

                    BinaryExpression binaryExpression1 = Expression.GreaterThanOrEqual(expression, valueExpression1);
                    BinaryExpression binaryExpression2 = Expression.LessThanOrEqual(expression, valueExpression2);

                    and = Expression.AndAlso(binaryExpression1, binaryExpression2);
                }
            }
            if (and == null)
            {
                return null;
            }
            return Expression.Lambda<Func<T, bool>>(and, parameterExpression);
        }
    }
}
