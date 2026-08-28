using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NovaKart.Application.Dtos.Products;
using Xunit;
using Xunit.Sdk;

namespace NovaKart.WebApi.IntegrationTests;

[Collection(ProductsApiCollection.Name)]
public sealed class GetProductsEndpointTests
{
    private const string Url = "/api/v1/products";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ProductsApiFixture _fixture;
    private readonly HttpClient _client;

    public GetProductsEndpointTests(ProductsApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    private static class Seeded
    {
        public const int TotalProducts = 15;
        public const int ActiveProducts = 14;
        public const int InactiveProducts = 1;

        public static readonly Guid Electronics = new("a1000000-0000-0000-0000-000000000001");
        public static readonly Guid Clothing = new("a1000000-0000-0000-0000-000000000002");
        public static readonly Guid Books = new("a1000000-0000-0000-0000-000000000004");
        public static readonly Guid Sports = new("a1000000-0000-0000-0000-000000000005");
        public static readonly Guid Toys = new("a1000000-0000-0000-0000-000000000006");

        public static readonly Guid Headphones = new("c1000000-0000-0000-0000-000000000001");
        public static readonly Guid Hoodie = new("c1000000-0000-0000-0000-000000000007");

        public const decimal LowestPrice = 18.50m;
    }

    // ---------------------------------------------------------------------
    // Defaults and response shape
    // ---------------------------------------------------------------------

    [Fact]
    public async Task NoQueryParameters_ReturnsFirstPageWithDefaultPageSize()
    {
        var result = await GetProductsAsync(Url);

        Assert.Equal(Seeded.TotalProducts, result.TotalCount);
        Assert.Equal(Seeded.TotalProducts, result.Products.Count);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(20, result.PageSize);
    }

    [Fact]
    public async Task EveryProduct_CarriesItsCategory()
    {
        var result = await GetProductsAsync(Url);

        Assert.All(result.Products, product =>
        {
            Assert.NotNull(product.Category);
            Assert.NotEqual(Guid.Empty, product.Category.Id);
            Assert.False(string.IsNullOrWhiteSpace(product.Category.Name));
        });
    }

    [Fact]
    public async Task Response_IsCamelCaseJson()
    {
        using var response = await _client.GetAsync(Url);
        response.EnsureSuccessStatusCode();

        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("products", out var products));
        Assert.True(root.TryGetProperty("totalCount", out _));
        Assert.True(root.TryGetProperty("pageNumber", out _));
        Assert.True(root.TryGetProperty("pageSize", out _));

        var first = products.EnumerateArray().First();
        Assert.True(first.TryGetProperty("id", out _));
        Assert.True(first.TryGetProperty("name", out _));
        Assert.True(first.TryGetProperty("description", out _));
        Assert.True(first.TryGetProperty("price", out _));
        Assert.True(first.TryGetProperty("stock", out _));
        Assert.True(first.TryGetProperty("isActive", out _));
        Assert.True(first.TryGetProperty("category", out _));
    }

    [Fact]
    public async Task UnknownQueryParameter_IsIgnored()
    {
        var result = await GetProductsAsync($"{Url}?foo=bar");

        Assert.Equal(Seeded.TotalProducts, result.TotalCount);
        Assert.Equal(Seeded.TotalProducts, result.Products.Count);
    }

    // ---------------------------------------------------------------------
    // Pagination
    // ---------------------------------------------------------------------

    [Theory]
    [InlineData(1, 5, 5)]
    [InlineData(2, 5, 5)]
    [InlineData(3, 5, 5)]
    [InlineData(4, 5, 0)]
    [InlineData(1, 100, Seeded.TotalProducts)]
    [InlineData(2, 10, 5)]
    public async Task Pagination_ReturnsExpectedSliceAndEchoesPageFields(int pageNumber, int pageSize, int expectedItems)
    {
        var result = await GetProductsAsync($"{Url}?pageNumber={pageNumber}&pageSize={pageSize}");

        Assert.Equal(expectedItems, result.Products.Count);
        Assert.Equal(Seeded.TotalProducts, result.TotalCount);
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
    }

    [Fact]
    public async Task PastLastPage_ReturnsEmptyArrayNotNull()
    {
        var result = await GetProductsAsync($"{Url}?pageNumber=4&pageSize=5");

        Assert.NotNull(result.Products);
        Assert.Empty(result.Products);
        Assert.Equal(Seeded.TotalProducts, result.TotalCount);
    }

    [Fact]
    public async Task WalkingEveryPage_YieldsEachProductExactlyOnce()
    {
        var seen = new List<Guid>();

        for (var page = 1; page <= 3; page++)
        {
            var result = await GetProductsAsync($"{Url}?pageNumber={page}&pageSize=5");
            seen.AddRange(result.Products.Select(p => p.Id));
        }

        Assert.Equal(Seeded.TotalProducts, seen.Count);
        Assert.Equal(Seeded.TotalProducts, seen.Distinct().Count());
    }

    [Fact]
    public async Task Paging_IsStable_AfterRowUpdate()
    {
        var before = await GetProductsAsync($"{Url}?pageNumber=1&pageSize=5");

        await _fixture.WithDbContextAsync(db =>
            db.Database.ExecuteSqlRawAsync(
                """UPDATE "Products" SET "Stock" = "Stock" + 1 WHERE "Id" = {0}""",
                Seeded.Headphones));

        var after = await GetProductsAsync($"{Url}?pageNumber=1&pageSize=5");

        Assert.Equal(before.Products.Select(p => p.Id), after.Products.Select(p => p.Id));
    }

    [Theory]
    [InlineData("pageNumber=0")]
    [InlineData("pageNumber=-1")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=-5")]
    [InlineData("pageSize=10000")]
    public async Task InvalidPagingArguments_AreRejected(string query)
    {
        var status = await GetStatusAsync($"{Url}?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }

    [Theory]
    [InlineData("pageNumber=abc")]
    [InlineData("pageSize=1.5")]
    public async Task UnparseablePagingArguments_AreRejected(string query)
    {
        var status = await GetStatusAsync($"{Url}?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }

    // ---------------------------------------------------------------------
    // searchTerm
    // ---------------------------------------------------------------------

    [Fact]
    public async Task SearchTerm_MatchesName()
    {
        var result = await GetProductsAsync($"{Url}?searchTerm=headphones");

        var product = Assert.Single(result.Products);
        Assert.Equal(Seeded.Headphones, product.Id);
        Assert.Equal(1, result.TotalCount);
    }

    [Theory]
    [InlineData("headphones")]
    [InlineData("HEADPHONES")]
    [InlineData("HeAdPhOnEs")]
    public async Task SearchTerm_IsCaseInsensitive(string searchTerm)
    {
        var result = await GetProductsAsync($"{Url}?searchTerm={searchTerm}");

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(Seeded.Headphones, Assert.Single(result.Products).Id);
    }

    [Fact]
    public async Task SearchTerm_MatchesDescriptionOnly()
    {

        var result = await GetProductsAsync($"{Url}?searchTerm=bluetooth");

        Assert.Equal(Seeded.Headphones, Assert.Single(result.Products).Id);
    }

    [Fact]
    public async Task SearchTerm_TreatsLikeWildcardAsLiteral()
    {
        var result = await GetProductsAsync($"{Url}?searchTerm=%25");

        Assert.Empty(result.Products);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task SearchTerm_HandlesApostropheWithoutInjection()
    {

        var result = await GetProductsAsync($"{Url}?searchTerm={Uri.EscapeDataString("Men's")}");

        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task SearchTerm_SurvivesSqlInjectionAttempt()
    {
        var payload = Uri.EscapeDataString("'; DROP TABLE \"Products\"; --");

        var result = await GetProductsAsync($"{Url}?searchTerm={payload}");
        Assert.Empty(result.Products);

        var unfiltered = await GetProductsAsync(Url);
        Assert.Equal(Seeded.TotalProducts, unfiltered.TotalCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("%20")]
    [InlineData("%20%20%20")]
    public async Task BlankSearchTerm_IsNotAFilter(string searchTerm)
    {
        var result = await GetProductsAsync($"{Url}?searchTerm={searchTerm}");

        Assert.Equal(Seeded.TotalProducts, result.TotalCount);
    }

    [Fact]
    public async Task SearchTerm_WithNoMatches_ReturnsEmptyPage()
    {
        var result = await GetProductsAsync($"{Url}?searchTerm=zzzznotfound");

        Assert.NotNull(result.Products);
        Assert.Empty(result.Products);
        Assert.Equal(0, result.TotalCount);
    }

    // ---------------------------------------------------------------------
    // categoryId
    // ---------------------------------------------------------------------

    [Fact]
    public async Task CategoryId_ReturnsOnlyThatCategory()
    {
        var result = await GetProductsAsync($"{Url}?categoryId={Seeded.Electronics}");

        Assert.Equal(4, result.TotalCount);
        Assert.All(result.Products, p => Assert.Equal(Seeded.Electronics, p.Category.Id));
    }

    [Fact]
    public async Task CategoryId_WithSingleProduct_ReturnsThatProduct()
    {
        var result = await GetProductsAsync($"{Url}?categoryId={Seeded.Toys}");

        var product = Assert.Single(result.Products);
        Assert.Equal("Toys & Games", product.Category.Name);
    }

    [Fact]
    public async Task UnknownCategoryId_ReturnsEmptyPage()
    {
        var result = await GetProductsAsync($"{Url}?categoryId={Guid.NewGuid()}");

        Assert.Empty(result.Products);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task EmptyCategoryId_ReturnsEmptyPage()
    {
        var result = await GetProductsAsync($"{Url}?categoryId={Guid.Empty}");

        Assert.Empty(result.Products);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task MalformedCategoryId_IsRejected()
    {
        var status = await GetStatusAsync($"{Url}?categoryId=not-a-guid");

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }

    // ---------------------------------------------------------------------
    // minPrice / maxPrice
    // ---------------------------------------------------------------------

    [Theory]
    [InlineData("minPrice=100", 5)]
    [InlineData("maxPrice=25", 3)]
    [InlineData("minPrice=30&maxPrice=60", 4)]
    [InlineData("minPrice=18.50", Seeded.TotalProducts)] // lower bound is inclusive
    [InlineData("maxPrice=18.50", 1)]                    // upper bound is inclusive
    [InlineData("minPrice=200&maxPrice=100", 0)]         // inverted range
    [InlineData("minPrice=1000", 0)]
    public async Task PriceFilters_ReturnExpectedCount(string query, int expectedCount)
    {
        var result = await GetProductsAsync($"{Url}?{query}&pageSize=100");

        Assert.Equal(expectedCount, result.TotalCount);
        Assert.Equal(expectedCount, result.Products.Count);
    }

    [Fact]
    public async Task PriceRange_ReturnsOnlyProductsInsideIt()
    {
        var result = await GetProductsAsync($"{Url}?minPrice=30&maxPrice=60&pageSize=100");

        Assert.All(result.Products, p =>
        {
            Assert.True(p.Price >= 30m, $"{p.Name} priced {p.Price} is below minPrice");
            Assert.True(p.Price <= 60m, $"{p.Name} priced {p.Price} is above maxPrice");
        });
    }

    [Fact]
    public async Task MaxPriceAtLowestSeededPrice_ReturnsTheCheapestProduct()
    {
        var result = await GetProductsAsync($"{Url}?maxPrice=18.50");

        Assert.Equal(Seeded.LowestPrice, Assert.Single(result.Products).Price);
    }

    [Theory]
    [InlineData("minPrice=abc")]
    [InlineData("maxPrice=abc")]
    public async Task MalformedPrice_IsRejected(string query)
    {
        var status = await GetStatusAsync($"{Url}?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }

    [Theory]
    [InlineData("minPrice=-5")]
    [InlineData("maxPrice=-5")]
    public async Task NegativePrice_IsRejected(string query)
    {
        var status = await GetStatusAsync($"{Url}?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }

    // ---------------------------------------------------------------------
    // isActive
    // ---------------------------------------------------------------------

    [Fact]
    public async Task IsActiveTrue_ReturnsOnlyActiveProducts()
    {
        var result = await GetProductsAsync($"{Url}?isActive=true&pageSize=100");

        Assert.Equal(Seeded.ActiveProducts, result.TotalCount);
        Assert.All(result.Products, p => Assert.True(p.IsActive));
    }

    [Fact]
    public async Task IsActiveFalse_ReturnsOnlyInactiveProducts()
    {
        var result = await GetProductsAsync($"{Url}?isActive=false&pageSize=100");

        Assert.Equal(Seeded.InactiveProducts, result.TotalCount);
        var product = Assert.Single(result.Products);
        Assert.Equal(Seeded.Hoodie, product.Id);
        Assert.False(product.IsActive);
    }

    [Fact]
    public async Task IsActiveOmitted_IncludesInactiveProducts()
    {
        var result = await GetProductsAsync($"{Url}?pageSize=100");

        Assert.Equal(Seeded.TotalProducts, result.TotalCount);
        Assert.Contains(result.Products, p => !p.IsActive);
    }

    [Fact]
    public async Task MalformedIsActive_IsRejected()
    {
        var status = await GetStatusAsync($"{Url}?isActive=maybe");

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }

    // ---------------------------------------------------------------------
    // Combined filters
    // ---------------------------------------------------------------------

    [Fact]
    public async Task CategoryAndIsActive_CombineAsAnd()
    {
        var result = await GetProductsAsync($"{Url}?categoryId={Seeded.Clothing}&isActive=true&pageSize=100");

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Products, p =>
        {
            Assert.Equal(Seeded.Clothing, p.Category.Id);
            Assert.True(p.IsActive);
        });
    }

    [Fact]
    public async Task CategoryAndPriceRange_CombineAsAnd()
    {
        var result = await GetProductsAsync($"{Url}?categoryId={Seeded.Electronics}&minPrice=50&maxPrice=200&pageSize=100");

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Products, p =>
        {
            Assert.Equal(Seeded.Electronics, p.Category.Id);
            Assert.InRange(p.Price, 50m, 200m);
        });
    }

    [Fact]
    public async Task SearchTermAndCategory_CombineAsAnd()
    {
        var result = await GetProductsAsync($"{Url}?searchTerm=set&categoryId={Seeded.Sports}&pageSize=100");

        var product = Assert.Single(result.Products);
        Assert.Equal("Adjustable Dumbbell Set 20kg", product.Name);
    }

    [Fact]
    public async Task AllFiltersTogether_NarrowToASingleProduct()
    {
        var query = $"{Url}?searchTerm=headphones&categoryId={Seeded.Electronics}" +
                    "&minPrice=100&maxPrice=200&isActive=true";

        var result = await GetProductsAsync(query);

        var product = Assert.Single(result.Products);
        Assert.Equal(Seeded.Headphones, product.Id);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task FiltersWithNoOverlap_ReturnEmptyPage()
    {

        var result = await GetProductsAsync($"{Url}?categoryId={Seeded.Books}&minPrice=100");

        Assert.Empty(result.Products);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task Pagination_CountsFilteredRowsNotAllRows()
    {
        var result = await GetProductsAsync($"{Url}?isActive=true&pageSize=5&pageNumber=3");

        Assert.Equal(Seeded.ActiveProducts, result.TotalCount);
        Assert.Equal(4, result.Products.Count);
        Assert.All(result.Products, p => Assert.True(p.IsActive));
    }


    private async Task<GetProductsResponse> GetProductsAsync(string url)
    {
        using var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GetProductsResponse>(body, JsonOptions)
            ?? throw new XunitException($"GET {url} returned a null body.");
    }

    private async Task<HttpStatusCode> GetStatusAsync(string url)
    {
        try
        {
            using var response = await _client.GetAsync(url);
            return response.StatusCode;
        }
        catch (Exception ex)
        {
            throw new XunitException(
                $"GET {url} faulted instead of returning a status code: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
