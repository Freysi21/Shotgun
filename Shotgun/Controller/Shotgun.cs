using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shotgun.Models;
using Shotgun.Repos;
using System.Text.Json;

//File contains base implementation for CRUD(CREATE READ UPDATE DELETE) operations for a table.
namespace Shotgun.Controllers
{
    /// <summary>
    /// Abstract class for common rest resource functionality. 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TRepository"></typeparam>
    /// <typeparam name="IDType"></typeparam>
    [Route("api/[controller]")]
    [ApiController]
    public abstract class Shotgun<TEntity, TRepository, IDType> : ControllerBase
        where TEntity : IEntity<IDType>
        where TRepository : IRepository<TEntity, IDType>
    {
        protected readonly TRepository repository;
        public Shotgun(TRepository repository)
        {
            this.repository = repository;
        }

        /// <summary>
        /// Gets multple records from database, sa many as the page parameter asks for. MAX 50 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="Orderby"></param>
        /// <param name="asc"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual async Task<ActionResult<IEnumerable<TEntity>>> Get([FromQuery] PagingQuery page, [FromQuery] string Orderby, [FromQuery] bool asc)
        {
            var items = await repository.GetAll(page, Orderby, asc);
            var metadata = new
            {
                items.TotalCount,
                items.PageSize,
                items.CurrentPage,
                items.TotalPages,
                items.HasNext,
                items.HasPrevious
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
            return items;
        }
        /// <summary>
        /// Gets specific record with id. Detail parameter set to true retrieves children records as nested properties.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="detail"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TEntity>> Get(IDType id, [FromQuery] bool detail)
        {
            var item = detail ? await repository.GetWithDetails(id) : await repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        /// <summary>
        /// Updates a specific record with the TEntity item received.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public virtual async Task<ActionResult<TEntity>> Put(IDType id, TEntity item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                await repository.Put(item);
            }
            catch (Exception ex)
            {
                //TODO Inject ex Log.
                return BadRequest("Ekki hægt að uppfæra eigindi með breyttum gildum.");
            }
            return Ok(item);
        }


        /// <summary>
        /// Inserts the received item as a new record. 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<ActionResult<TEntity>> Post(TEntity item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newItem = await repository.Add(item);
                return CreatedAtAction("Get", new { id = newItem.Id }, newItem);
            }
            catch (DbUpdateException ex)
            {
                Exception e = ex;
                var innermessage = "git it";
                while (e.InnerException != null) e = e.InnerException;
                innermessage = e.Message;
                return Conflict(innermessage);
            }
        }

        /// <summary>
        /// Removes a record with the identifier id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public virtual async Task<ActionResult<TEntity>> Delete(IDType id)
        {
            var item = await repository.Delete(id);
            if (item == null)
            {
                return NotFound();
            }
            return item;
        }

        //GET: api/[controller]/
        //TODO 4 DOC: Don't remember how the uri parameter looks like as url
        /// <summary>
        /// Search route where parameters are translated to sql query with ef. 
        /// </summary>
        /// <param name="dict">
        /// Dictionary where key maps to entity prop and the value is the search term.
        /// </param>
        /// <param name="Orderby">
        /// String value that decides which entity to order results by
        /// </param>
        /// <param name="page">
        /// Page info
        /// </param>
        /// <param name="asc">
        /// asc = true = search result in ascending order in corresponce of Orderby
        /// </param>
        /// <param name="dateDict">
        /// range to order by. first value of key is used as "from date" while second value is used as "to date" in query builder. Only one key is supported.
        /// </param>
        /// <see cref="search"/>
        /// <returns></returns>
        [HttpGet("search")]
        public virtual async Task<ActionResult<List<TEntity>>> SearchGet([FromQuery] Dictionary<string, string[]> dict, [FromQuery] string Orderby, [FromQuery] PagingQuery page, [FromQuery] bool asc, [FromQuery] Dictionary<string, string[]> dateDict)
        {
            var results = await repository.Search(page, dict, Orderby, asc, dateDict);
            var metadata = new
            {
                results.TotalCount,
                results.PageSize,
                results.CurrentPage,
                results.TotalPages,
                results.HasNext,
                results.HasPrevious
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
            return Ok(results);
        }
        /// <summary>
        /// Get all records as csv file.
        /// </summary>
        /// <returns></returns>

        [HttpGet("GetAsCSV")]
        public async Task<FileContentResult> GetAsCSV()
        {
            return File(await repository.GetAllAsCSV(), "application/csv", "list.csv");
        }
        /// <summary>
        /// Get all records from search query as csv file. Same parameters as /search
        /// </summary>
        [HttpGet("GetSearchAsCSV")]
        public async Task<FileContentResult> GetSearchAsCSV([FromQuery] Dictionary<string, string[]> dict, [FromQuery] string Orderby, [FromQuery] bool asc, [FromQuery] Dictionary<string, string[]> dateDict)
        {
            return File(await repository.GetSearchAsCSV(dict, Orderby, asc, dateDict), "application/csv", "list.csv");
        }
    }
}
/*
 CreateCSVFileFromList(List
>  GetSearchAsCSV
>  GetAllAsCSV
*/