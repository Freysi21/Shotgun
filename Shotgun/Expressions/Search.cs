using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using System.Reflection;

namespace Shotgun.Expressions
{
    public static class Search
    {
        public static Expression ContainsValueExpression<T>(string fieldName, string val, ParameterExpression member)
        {
            var memberExpression = Expression.PropertyOrField(member, fieldName);
            var type = memberExpression.Type;
            var methods = memberExpression.Type.GetMethods();

            var targetMethod = GetMethod(memberExpression);//memberExpression.Type.GetMethod("IndexOf", new Type[] { typeof(string), typeof(StringComparison) });
            var methodCallExpression = GetCallExpression(memberExpression, targetMethod, val);//Expression.Call(memberExpression, targetMethod, Expression.Constant(val), Expression.Constant(StringComparison.CurrentCultureIgnoreCase));

            return GetContainsValueReturn(memberExpression, methodCallExpression, val);
        }

        private static MethodInfo GetMethod(MemberExpression member)
        {
            if (member.Type == typeof(string))
            {
                var methods = member.Type.GetMethods();
                return member.Type.GetMethod("Contains", new Type[] { typeof(string) });
            }
            else if (member.Type == typeof(long) || member.Type == typeof(Nullable<Int64>) || member.Type == typeof(int) || member.Type == typeof(Nullable<Int32>) || member.Type == typeof(short) || member.Type == typeof(Nullable<Int16>))
            {
                return member.Type.GetMethod("Equals", new Type[] { member.Type });
            }
            else if (member.Type == typeof(bool) || member.Type == typeof(Nullable<Boolean>))
            {
                return member.Type.GetMethod("Equals", new Type[] { member.Type });
            }
            else if (member.Type == typeof(Guid) || member.Type == typeof(Nullable<Guid>))
            {
                return member.Type.GetMethod("Equals", new Type[] { member.Type });
            }
            return null;
        }

        private static MethodCallExpression GetCallExpression(MemberExpression member, MethodInfo info, string val)
        {
            try
            {
                if (member.Type == typeof(string))
                {
                    return Expression.Call(member, info, Expression.Constant(val, typeof(string)));
                }
                else if (member.Type == typeof(long) || member.Type == typeof(Nullable<Int64>))
                {
                    if (member.Type == typeof(Nullable<Int64>))
                    {
                        return Expression.Call(member, info, Expression.Constant(Expression.Convert(Expression.Constant(Convert.ToInt64(val)), member.Type)));
                    }
                    return Expression.Call(member, info, Expression.Constant(Convert.ToInt64(val)));
                }
                else if (member.Type == typeof(int) || member.Type == typeof(Nullable<Int32>))
                {
                    if (member.Type == typeof(Nullable<Int32>))
                    {
                        return Expression.Call(member, info, Expression.Constant(Expression.Convert(Expression.Constant(Convert.ToInt32(val)), member.Type)));
                    }
                    return Expression.Call(member, info, Expression.Constant(Convert.ToInt32(val)));
                }
                else if (member.Type == typeof(short) || member.Type == typeof(Nullable<Int16>))
                {
                    if (member.Type == typeof(Nullable<Int16>))
                    {
                        return Expression.Call(member, info, Expression.Constant(Expression.Convert(Expression.Constant(Convert.ToInt16(val)), member.Type)));
                    }
                    return Expression.Call(member, info, Expression.Constant(Convert.ToInt16(val)));
                }
                else if (member.Type == typeof(bool) || member.Type == typeof(Nullable<Boolean>))
                {
                    var trueOrFalse = val == "true" ? true : false;
                    if (member.Type == typeof(Nullable<Boolean>))
                    {
                        return Expression.Call(member, info, Expression.Constant(Expression.Convert(Expression.Constant(trueOrFalse), member.Type)));
                    }
                    return Expression.Call(member, info, Expression.Constant(trueOrFalse));
                }
                else if (member.Type == typeof(Guid) || member.Type == typeof(Nullable<Guid>))
                {
                    if (member.Type == typeof(Nullable<Guid>))
                    {
                        return Expression.Call(member, info, Expression.Constant(Expression.Convert(Expression.Constant(Guid.Parse(val)), member.Type)));
                    }
                    return Expression.Call(member, info, Expression.Constant(Guid.Parse(val)));

                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
            return null;
        }

        private static Expression GetContainsValueReturn(MemberExpression member, MethodCallExpression callExpression, string val)
        {
            var methods = member.Type.GetMethods();
            try
            {
                if (member.Type == typeof(string))
                {
                    return Expression.AndAlso(
                                        Expression.NotEqual(member, Expression.Constant(null)),
                                        Expression.Equal(callExpression, Expression.Constant(true))
                                    );
                }
                else if (member.Type == typeof(long) || member.Type == typeof(Nullable<Int64>))
                {
                    if (member.Type == typeof(Nullable<Int64>))
                    {
                        var filter1 =
                            Expression.Constant(
                                Convert.ChangeType(val, member.Type.GetGenericArguments()[0]));
                        Expression typeFilter = Expression.Convert(filter1, member.Type);
                        var body = Expression.Equal(member, typeFilter);
                        return body;//Expression.Equal(member, Expression.Constant(Expression.Convert(Expression.Constant(Convert.ToInt64(val)), member.Type)));
                    }
                    return Expression.Equal(member, Expression.Constant(Convert.ToInt64(val)));
                }
                else if (member.Type == typeof(int) || member.Type == typeof(Nullable<Int32>))
                {
                    if (member.Type == typeof(Nullable<Int32>))
                    {
                        var filter1 =
                            Expression.Constant(
                                Convert.ChangeType(val, member.Type.GetGenericArguments()[0]));
                        Expression typeFilter = Expression.Convert(filter1, member.Type);
                        var body = Expression.Equal(member, typeFilter);
                        return body;
                    }
                    return Expression.Equal(member, Expression.Constant(Convert.ToInt32(val)));
                }
                else if (member.Type == typeof(short) || member.Type == typeof(Nullable<Int16>))
                {
                    if (member.Type == typeof(Nullable<Int16>))
                    {
                        var filter1 =
                            Expression.Constant(
                                Convert.ChangeType(val, member.Type.GetGenericArguments()[0]));
                        Expression typeFilter = Expression.Convert(filter1, member.Type);
                        var body = Expression.Equal(member, typeFilter);
                        return body;
                    }
                    return Expression.Equal(member, Expression.Constant(Convert.ToInt16(val)));
                }
                else if (member.Type == typeof(bool) || member.Type == typeof(Nullable<Boolean>))
                {
                    var trueOrFalse = val == "true" ? true : false;
                    if (member.Type == typeof(Nullable<Boolean>))
                    {
                        var filter1 =
                            Expression.Constant(
                                Convert.ChangeType(trueOrFalse, member.Type.GetGenericArguments()[0]));
                        Expression typeFilter = Expression.Convert(filter1, member.Type);
                        var body = Expression.Equal(member, typeFilter);
                        return body;
                    }
                    return Expression.Equal(member, Expression.Constant(trueOrFalse));
                }
                else if (member.Type == typeof(Guid) || member.Type == typeof(Nullable<Guid>))
                {
                    if (member.Type == typeof(Nullable<Guid>))
                    {
                        var filter1 =
                            Expression.Constant(
                                Convert.ChangeType(Guid.Parse(val), member.Type.GetGenericArguments()[0]));
                        Expression typeFilter = Expression.Convert(filter1, member.Type);
                        var body = Expression.Equal(member, typeFilter);
                        return body;
                    }
                    return Expression.Equal(member, Expression.Constant(Guid.Parse(val)));
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
            return null;
        }

        public static Expression<Func<T, bool>> ContainsValue<T>(string fieldName, string val)
        {
            var type = typeof(T);
            var member = Expression.Parameter(type, "param");

            return Expression.Lambda<Func<T, bool>>(
                ContainsValueExpression<T>(fieldName, val, member),
                member
            );
        }
        private static Expression GetTrueExpression(Type type)
        {
            return Expression.Lambda(Expression.Constant(true), Expression.Parameter(type, "_"));
        }

        public static Expression<Func<T, bool>> ContainsValues<T>(Dictionary<string, string> fieldsAndValues)
        {
            var type = typeof(T);
            var member = Expression.Parameter(type, "param");
            Expression expr = null;


            foreach (var entry in fieldsAndValues)
            {
                var key = entry.Key.FirstCharToUpper();
                if (type.GetProperty(key) != null)
                {
                    if (expr == null)
                    {
                        expr = Expression.And(Expression.Constant(true), ContainsValueExpression<T>(key, entry.Value, member));
                    }
                    else
                    {
                        expr = Expression.And(expr, ContainsValueExpression<T>(key, entry.Value, member));
                    }
                }
            }
            if (expr == null)
            {
                return null;
            }

            return Expression.Lambda<Func<T, bool>>(
                expr,
                member
            );
        }

        public static Expression<Func<T, bool>> ContainsValues<T>(Dictionary<string, string[]> fieldsAndValues)
        {
            var type = typeof(T);
            var member = Expression.Parameter(type, "param");
            Expression expr = null;
            var props = type.GetProperties();
            foreach (var entry in fieldsAndValues)
            {
                var key = entry.Key.FirstCharToUpper();
                if (type.GetProperty(key) != null)
                {
                    if (expr == null)
                    {
                        expr = Expression.AndAlso(Expression.Constant(true), CreateOrExpressions<T>(key, entry.Value, member));
                    }
                    else
                    {
                        expr = Expression.AndAlso(expr, CreateOrExpressions<T>(key, entry.Value, member));
                    }
                }
            }
            if (expr == null)
            {
                return null;
            }
            return Expression.Lambda<Func<T, bool>>(
                expr != null ? expr : Expression.Constant(true),
                member
            );
        }

        public static Expression CreateOrExpressions<T>(string key, string[] values, ParameterExpression member)
        {
            Expression expr = null;
            foreach (var value in values)
            {
                if (expr == null)
                {
                    expr = Expression.Or(Expression.Constant(false), ContainsValueExpression<T>(key, value, member));
                }
                else
                {
                    expr = Expression.Or(expr, ContainsValueExpression<T>(key, value, member));
                }
            }
            return expr;
        }
        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }
    }
}