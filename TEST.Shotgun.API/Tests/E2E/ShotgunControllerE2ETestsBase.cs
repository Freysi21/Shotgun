using System.Net;
using System.Net.Http.Json;
using System.Web;
using Shotgun.Entity;
using Xunit;

namespace TEST.Shotgun.API.Tests.E2E;

// Drives every HTTP endpoint a Shotgun<TEntity, TRepository, TId> controller exposes through
// its full lifecycle: create -> update -> find via search -> delete -> confirm it's gone.
// Concrete tests only need to supply a route, an HttpClient (via ShotgunWebApplicationFactory),
// and a ShotgunE2EOptions describing how to generate/mutate/search their entity.
public abstract class ShotgunControllerE2ETestsBase<TEntity, TId> : IAsyncLifetime
    where TEntity : IEntity<TId>
{
    protected abstract string RoutePrefix { get; }
    protected abstract ShotgunE2EOptions<TEntity, TId> Options { get; }
    protected abstract HttpClient CreateClient();

    private HttpClient _client = null!;
    private readonly List<TId> _createdIds = new();

    public Task InitializeAsync()
    {
        _client = CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var id in _createdIds)
        {
            try { await _client.DeleteAsync($"{RoutePrefix}/{id}"); }
            catch { /* best-effort cleanup */ }
        }
    }

    [Fact]
    public async Task FullLifecycle_CreateUpdateSearchDelete()
    {
        // CREATE
        var randomEntity = Options.CreateRandom();
        var postResponse = await _client.PostAsJsonAsync(RoutePrefix, randomEntity);
        var postBody = await postResponse.Content.ReadAsStringAsync();
        Assert.True(postResponse.StatusCode == HttpStatusCode.Created,
            $"POST {RoutePrefix} returned {postResponse.StatusCode}: {postBody}");

        var created = await postResponse.Content.ReadFromJsonAsync<TEntity>();
        Assert.NotNull(created);
        _createdIds.Add(created!.Id);

        // UPDATE
        Options.ApplyRandomUpdate(created);
        var putResponse = await _client.PutAsJsonAsync($"{RoutePrefix}/{created.Id}", created);
        var putBody = await putResponse.Content.ReadAsStringAsync();
        Assert.True(putResponse.StatusCode == HttpStatusCode.OK,
            $"PUT {RoutePrefix}/{created.Id} returned {putResponse.StatusCode}: {putBody}");

        var updated = await putResponse.Content.ReadFromJsonAsync<TEntity>();
        Assert.NotNull(updated);

        // SEARCH — must find the updated record via the controller's /search endpoint
        var filter = Options.BuildSearchFilter(updated!);
        var searchUrl = $"{RoutePrefix}/search?{BuildDictQuery("dict", filter)}";
        var searchResponse = await _client.GetAsync(searchUrl);
        var searchBody = await searchResponse.Content.ReadAsStringAsync();
        Assert.True(searchResponse.StatusCode == HttpStatusCode.OK,
            $"GET {searchUrl} returned {searchResponse.StatusCode}: {searchBody}");

        var results = await searchResponse.Content.ReadFromJsonAsync<List<TEntity>>();
        Assert.NotNull(results);
        Assert.Contains(results!, e => Equals(e.Id, updated!.Id));

        // DELETE
        var deleteResponse = await _client.DeleteAsync($"{RoutePrefix}/{updated!.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        _createdIds.Remove(created.Id);

        var getResponse = await _client.GetAsync($"{RoutePrefix}/{updated.Id}?detail=false");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private static string BuildDictQuery(string paramName, Dictionary<string, string[]> dict)
    {
        var parts = new List<string>();
        foreach (var (key, values) in dict)
        {
            foreach (var value in values)
            {
                parts.Add($"{paramName}[{HttpUtility.UrlEncode(key)}]={HttpUtility.UrlEncode(value)}");
            }
        }
        return string.Join("&", parts);
    }
}
