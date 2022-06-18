using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace BlackMarket_API.EnhancedAutomapperNS
{
	public static class EnhancedAutomapper
	{
		public static EnhancedAutomapperFrom<TSource> MapFrom<TSource>(IQueryable<TSource> source)
		{
			return new EnhancedAutomapperFrom<TSource>(source);
		}

		public static IQueryable<TDest> Map<TSource, TDest>(IQueryable<TSource> source)
		{
			return (new EnhancedAutomapperFrom<TSource>(source)).To<TDest>();
		}

		//Extension methods for LINQ to DB
		public static IQueryable<TDest> EnhancedMap<TSource, TDest>(this IQueryable<TSource> source)
		{
			return (new EnhancedAutomapperFrom<TSource>(source)).To<TDest>();
		}
		public static EnhancedAutomapperFrom<TSource> EnhancedMap<TSource>(this IQueryable<TSource> source)
		{
			return new EnhancedAutomapperFrom<TSource>(source);
		}
	}

	public class EnhancedAutomapperFrom<TSource>
	{
		private static readonly Dictionary<string, Expression> ExpressionCache = new Dictionary<string, Expression>();
		private readonly IQueryable<TSource> source;

		public EnhancedAutomapperFrom(IQueryable<TSource> source)
		{
			this.source = source;
		}


		//Client invokes only this method
		public IQueryable<TDest> To<TDest>()
		{
			var queryExpression = GetCachedExpression<TDest>() ?? BuildExpression<TDest>();

			return source.Select(queryExpression);
		}


		//Cache
		private static Expression<Func<TSource, TDest>> GetCachedExpression<TDest>()
		{
			var key = GetCacheKey<TDest>();

			return ExpressionCache.ContainsKey(key) ? ExpressionCache[key] as Expression<Func<TSource, TDest>> : null;
		}

		//Building expression
		private static Expression<Func<TSource, TDest>> BuildExpression<TDest>()
		{
			var sourceProperties = typeof(TSource).GetProperties();
			var destinationProperties = typeof(TDest).GetProperties().Where(dest => dest.CanWrite);
			var parameterExpression = Expression.Parameter(typeof(TSource), "src");

			var bindings = destinationProperties
								.Select(destinationProperty => BuildBinding(parameterExpression, destinationProperty, sourceProperties))
								.Where(binding => binding != null);

			var expression = Expression.Lambda<Func<TSource, TDest>>(Expression.MemberInit(Expression.New(typeof(TDest)), bindings), parameterExpression);

			var key = GetCacheKey<TDest>();

			ExpressionCache.Add(key, expression);

			return expression;
		}

		private static MemberAssignment BuildBinding(Expression parameterExpression, MemberInfo destinationProperty, IEnumerable<PropertyInfo> sourceProperties)
		{
			var sourceProperty = sourceProperties.FirstOrDefault(src => src.Name == destinationProperty.Name);

			if (sourceProperty != null)
			{
				return Expression.Bind(destinationProperty, Expression.Property(parameterExpression, sourceProperty));
			}

			var propertyNames = SplitCamelCase(destinationProperty.Name);

			if (propertyNames.Length == 2)
			{
				sourceProperty = sourceProperties.FirstOrDefault(src => src.Name == propertyNames[0]);

				if (sourceProperty != null)
				{
					var sourceChildProperty = sourceProperty.PropertyType.GetProperties().FirstOrDefault(src => src.Name == propertyNames[1]);

					if (sourceChildProperty != null)
					{
						return Expression.Bind(destinationProperty, Expression.Property(Expression.Property(parameterExpression, sourceProperty), sourceChildProperty));
					}
				}
			}

			return null;
		}

		private static string GetCacheKey<TDest>()
		{
			return string.Concat(typeof(TSource).FullName, typeof(TDest).FullName);
		}

		private static string[] SplitCamelCase(string input)
		{
			return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim().Split(' ');
		}
	}
}