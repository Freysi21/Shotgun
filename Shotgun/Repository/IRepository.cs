
using Shotgun.Helpers;

namespace Shotgun.Repos
{
	//Generic repository interface. States common methods too implement to fit into
	//the generic repository pattern.
	public interface IRepository<T, IDType> where T : class
	{
		Task<List<T>> GetAll();
		Task<PagedList<T>> GetAll(PagingQuery page, string? orderBy = null, bool asc = false);
		Task<T> Get(IDType id);
		Task<T> GetWithDetails(IDType id);
		Task<T> Add(T entity);
		Task<T> Put(T entity);
		Task<T> Delete(IDType id);
		Task<List<T>> Search(Dictionary<string, string[]> dict, string? orderBy = null);
		Task<PagedList<T>> Search(PagingQuery page, Dictionary<string, string[]> dict, string? orderBy = null, bool asc = false);
		Task<PagedList<T>> Search(PagingQuery page, Dictionary<string, string[]> dict, string? orderBy = null, bool asc = false, Dictionary<string, string[]>? dateDict = null);
		Task<PagedList<T>> Search(PagingQuery page, Dictionary<string, string[]> dict, Dictionary<string, string[]> dateDict, Dictionary<string, bool> orderByDict);
		Task<byte[]> GetSearchAsCSV(Dictionary<string, string[]> dict, string? orderBy = null, bool asc = false, Dictionary<string, string[]>? dateDict = null);
		Task<byte[]> GetAllAsCSV();
	}
}