using System.Linq.Expressions;
using System.Reflection;

namespace Shotgun.Expressions
{
	public static class ThenBy
	{
		private static readonly MethodInfo ThenByPropertyMethod = typeof(Queryable).GetMethods().Single((MethodInfo method) => method.Name == "ThenBy" && method.GetParameters().Length == 2);

		private static readonly MethodInfo ThenByPropertyDescendingMethod = typeof(Queryable).GetMethods().Single((MethodInfo method) => method.Name == "ThenByDescending" && method.GetParameters().Length == 2);

		public static IOrderedQueryable<T> ThenByProperty<T>(this IOrderedQueryable<T> source, string propertyName)
		{
			if (typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public) == null)
			{
				return null;
			}

			ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
			Expression expression = Expression.Property(parameterExpression, propertyName);
			LambdaExpression lambdaExpression = Expression.Lambda(expression, parameterExpression);
			MethodInfo methodInfo = ThenByPropertyMethod.MakeGenericMethod(typeof(T), expression.Type);
			object obj = methodInfo.Invoke(null, new object[2] { source, lambdaExpression });
			return (IOrderedQueryable<T>)obj;
		}

		public static IOrderedQueryable<T> ThenByPropertyDescending<T>(this IOrderedQueryable<T> source, string propertyName)
		{
			if (typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public) == null)
			{
				return null;
			}

			ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
			Expression expression = Expression.Property(parameterExpression, propertyName);
			LambdaExpression lambdaExpression = Expression.Lambda(expression, parameterExpression);
			MethodInfo methodInfo = ThenByPropertyDescendingMethod.MakeGenericMethod(typeof(T), expression.Type);
			object obj = methodInfo.Invoke(null, new object[2] { source, lambdaExpression });
			return (IOrderedQueryable<T>)obj;
		}
	}
}
