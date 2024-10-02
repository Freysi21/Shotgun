
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Shotgun.Expressions;
using Shotgun.Helpers;
using Shotgun.Models;
using IncludeService = Shotgun.Expressions.Include;
using OrderbyService = Shotgun.Expressions.OrderBy;
using RangeService = Shotgun.Expressions.Range;
using SearchService = Shotgun.Expressions.Search;

//Abstract implementation of a data repository relying on the Entity framework.
//Implements common procedures by utilizing linq query language.
namespace Shotgun.Repos
{
	public abstract class EFCoreRepository<TEntity, TContext, IDType> : IRepository<TEntity, IDType>
		where TEntity : IEntity<IDType>
		where TContext : DbContext
		where IDType : IEquatable<IDType>
	{
		protected readonly TContext context;
		private readonly string[] searchIncludes;

		public EFCoreRepository(TContext context)
		{
			this.context = context;
		}

		public EFCoreRepository(TContext context, string[] searchIncludes)
		{
			this.context = context;
			this.searchIncludes = searchIncludes;
		}

		public virtual async Task<TEntity> Add(TEntity entity)
		{
			await context.Set<TEntity>().AddAsync(entity);
			await context.SaveChangesAsync();
			return entity;
		}

		public virtual async Task<TEntity> Delete(IDType id)
		{
			var entity = await context.Set<TEntity>().FindAsync(id);
			if (entity == null)
			{
				return entity;
			}

			context.Set<TEntity>().Remove(entity);
			await context.SaveChangesAsync();

			return entity;
		}

		public virtual async Task<TEntity> Get(IDType id)
		{
			return await context.Set<TEntity>().FindAsync(id);
		}

		public virtual async Task<TEntity> GetWithDetails(IDType id)
		{
			/*var ent = await context.Set<TEntity>().FindAsync(id);
			if (ent != null)
			{
				await ent.GetSingleNavigations(context);
				await ent.GetCollectionNavigations(context);
			}
			return ent;*/
			var ent = await context.Set<TEntity>().FindAsync(id);
			var singles = IncludeService.GetSingleNavigationsAsStrings<TEntity>();
			var collections = IncludeService.GetCollectionNavigationsAsStrings<TEntity>();
			foreach (var prop in singles)
			{
				await context.Entry(ent).Reference(prop).LoadAsync();
			}
			foreach (var prop in collections)
			{
				await context.Entry(ent).Collection(prop).LoadAsync();
			}
			return ent;
		}

		public virtual async Task<List<TEntity>> GetAll()
		{
			return await context.Set<TEntity>().ToListAsync();
		}

		public virtual async Task<PagedList<TEntity>> GetAll(PagingQuery page, string orderBy = null, bool asc = false)
		{
			var query = GetAllQuery(orderBy, asc);
			return await PagedList<TEntity>.ToPagedListAsync(query, page.PageNumber, page.PageSize);
		}

		public virtual IQueryable<TEntity> GetAllQuery(string orderBy = null, bool asc = false)
		{
			var query = context.Set<TEntity>().Select(val => val);
			var type = typeof(TEntity);
			var properties = type.GetProperties();
			var sortByDefaultPropertyInfoList = properties.Where(
				prop =>
					prop.IsDefined(typeof(DefaultSortPropertyAttribute))
			).ToList();
			if (orderBy != null && OrderbyService.PropertyExists<TEntity>(query, orderBy))
			{
				return asc ? OrderbyService.OrderByProperty(query, orderBy)
				: OrderbyService.OrderByPropertyDescending(query, orderBy);
			}
			else if (sortByDefaultPropertyInfoList.Count > 0 && OrderbyService.PropertyExists<TEntity>(query, sortByDefaultPropertyInfoList[0].Name))
			{
				return OrderbyService.OrderByPropertyDescending(query, sortByDefaultPropertyInfoList[0].Name);
			}
			return context.Set<TEntity>().AsQueryable();
		}
		/*
				public virtual async Task<TEntity> Put(IDType id, string columnName, string value)
				{
					var entity = await context.Set<TEntity>().FindAsync(id);
					if (entity == null)
					{
						return entity;
					}
					var prop = typeof(TEntity).GetProperty(columnName);
					if (prop == null)
					{
						return entity;
					}
					prop.SetValue(entity, Convert.ChangeType(value, prop.PropertyType));
					context.Set<TEntity>().Update(entity);
					await context.SaveChangesAsync();
					return entity;
				}
		*/
		public virtual async Task<TEntity> Put(TEntity entity)
		{
			context.Update(entity);
			await context.SaveChangesAsync();
			return entity;
		}

		public virtual async Task<List<TEntity>> Search(Dictionary<string, string[]> dict, string orderBy = null)
		{
			var expr = SearchService.ContainsValues<TEntity>(dict);

			var query = GetQueryWithInclude(expr).Select(entity => entity);
			if (orderBy != null && OrderbyService.PropertyExists<TEntity>(query, orderBy))
			{
				return await OrderbyService.OrderByPropertyDescending(query, orderBy).ToListAsync();
			}
			return await query.ToListAsync();
		}

		public virtual async Task<PagedList<TEntity>> Search(PagingQuery page, Dictionary<string, string[]> dict, string orderBy = null, bool asc = false)
		{
			var expr = SearchService.ContainsValues<TEntity>(dict);

			var query = GetQueryWithInclude(expr).Select(entity => entity);
			if (orderBy != null && OrderbyService.PropertyExists<TEntity>(query, orderBy))
			{
				return asc ? await PagedList<TEntity>.ToPagedListAsync(OrderbyService.OrderByProperty(query, orderBy), page.PageNumber, page.PageSize)
				: await PagedList<TEntity>.ToPagedListAsync(OrderbyService.OrderByPropertyDescending(query, orderBy), page.PageNumber, page.PageSize);
			}
			return await PagedList<TEntity>.ToPagedListAsync(query, page.PageNumber, page.PageSize);
		}

		public virtual async Task<PagedList<TEntity>> Search(PagingQuery page, Dictionary<string, string[]> dict, string orderBy = null, bool asc = false, Dictionary<string, string[]> dateDict = null)
		{
			var query = SearchQuery(dict, orderBy, asc, dateDict);
			return await PagedList<TEntity>.ToPagedListAsync(query, page.PageNumber, page.PageSize);
		}

		public virtual async Task<PagedList<TEntity>> Search
		(
			PagingQuery page,
			Dictionary<string, string[]> dict,
			Dictionary<string, string[]> dateDict,
			Dictionary<string, bool> orderByDict
		)
		{
			if (orderByDict != null && orderByDict.Count > 0)
			{
				var orderBy = orderByDict.FirstOrDefault();
				var query = SearchQuery(dict, orderBy.Key, orderBy.Value, dateDict);
				orderByDict.Remove(orderBy.Key);
				var orderedQuery = (IOrderedQueryable<TEntity>)query;

				foreach (var keyValuePair in orderByDict)
				{
					if (keyValuePair.Value)
					{
						orderedQuery = orderedQuery.ThenByProperty(keyValuePair.Key);
					}
					else
					{
						orderedQuery = orderedQuery.ThenByPropertyDescending(keyValuePair.Key);
					}
				}

				return await PagedList<TEntity>.ToPagedListAsync(orderedQuery, page.PageNumber, page.PageSize);
			}
			else
			{
				var query = SearchQuery(dict, null, false, dateDict);
				return await PagedList<TEntity>.ToPagedListAsync(query, page.PageNumber, page.PageSize);
			}
		}

		protected IQueryable<TEntity> SearchQuery(Dictionary<string, string[]> dict, string orderBy = null, bool asc = false, Dictionary<string, string[]> dateDict = null)
		{
			var expr = SearchService.ContainsValues<TEntity>(dict);
			var dateExpr = RangeService.RangeExpression<TEntity>(dateDict);

			var query = dateExpr != null ? GetQueryWithInclude(expr).Where(dateExpr).Select(entity => entity) : GetQueryWithInclude(expr).Select(entity => entity);
			if (orderBy != null && OrderbyService.PropertyExists<TEntity>(query, orderBy))
			{
				return asc ? OrderbyService.OrderByProperty(query, orderBy)
				: OrderbyService.OrderByPropertyDescending(query, orderBy);
			}
			return query;
		}

		public IQueryable<TEntity> GetQueryWithInclude(Expression<Func<TEntity, bool>> expr = null)
		{
			var query = expr != null ? context.Set<TEntity>().Where(expr) : context.Set<TEntity>();

			if (this.searchIncludes != null)
			{
				foreach (var include in this.searchIncludes)
				{
					query = query.Include(include);
				}
			}

			return query;
		}

		protected byte[] CreateCSVFileFromList(List<TEntity> list)
		{
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream, Encoding.UTF8))
			using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.GetCultureInfo("is-IS"))
			{
				Delimiter = ";",
			}))
			{
				csv.WriteRecords(list);
				writer.Flush();
				stream.Seek(0, SeekOrigin.Begin);
				return stream.ToArray();
			}
		}

		public virtual async Task<byte[]> GetSearchAsCSV(Dictionary<string, string[]> dict, string orderBy = null, bool asc = false, Dictionary<string, string[]> dateDict = null)
		{
			var query = SearchQuery(dict, orderBy, asc, dateDict);
			return CreateCSVFileFromList(await query.ToListAsync());
		}

		public virtual async Task<byte[]> GetAllAsCSV()
		{
			return CreateCSVFileFromList(await context.Set<TEntity>().ToListAsync());
		}
	}
}