using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using SemanticSearch.WebApi.Extensions;
using SemanticSearch.WebApi.Options;
using SemanticSearch.WebApi.Utils;

namespace SemanticSearch.WebApi.Controllers;

[ApiController]
public class SearchController : ControllerBase
{
    private readonly List<Property> _properties = new()
    {
        new Property
        {
            Id = 1,
            Type = "House",
            Price = 500_000,
            Bedrooms = 4,
            Bathrooms = 3,
            Garden = true,
            Parking = true,
            Location = "Edinburgh"
        },
        new Property
        {
            Id = 2,
            Type = "Flat",
            Price = 350_000,
            Bedrooms = 3,
            Bathrooms = 2,
            Garden = false,
            Parking = true,
            Location = "London"
        },
        new Property
        {
            Id = 3,
            Type = "Flat",
            Price = 300_000,
            Bedrooms = 2,
            Bathrooms = 1,
            Garden = false,
            Parking = false,
            Location = "Glasgow"
        },
        new Property
        {
            Id = 4,
            Type = "House",
            Price = 450_000,
            Bedrooms = 3,
            Bathrooms = 2,
            Garden = true,
            Parking = true,
            Location = "Manchester"
        },
    };

    private readonly ILogger<SearchController> _logger;
    private readonly IOptions<OpenAiOptions> _openAiOptions;

    public SearchController(ILogger<SearchController> logger, IOptions<OpenAiOptions> openAiOptions)
    {
        _logger = logger;
        _openAiOptions = openAiOptions;
    }

    [HttpGet("api/search")]
    public async Task<ActionResult<SearchResult>> Search(string input)
    {
        var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");

        var kernel = Kernel.Builder
            .WithLogger(ConsoleLogger.Log)
            .WithOpenAIChatCompletionService("gpt-3.5-turbo", _openAiOptions.Value.ApiKey, _openAiOptions.Value.OrganizationId)
            .Build();

        var searchPlugin = kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "SemanticSearchPlugin");
        var generateFilters = searchPlugin["GenerateFilters"];
        var generateQueryParams = searchPlugin["GenerateQueryParams"];

        var generateFiltersResult = await generateFilters.InvokeAsync(input);
        var generateQueryParamsResult = await generateQueryParams.InvokeAsync(generateFiltersResult.Result);

        // The LLM can sometimes be disobedient and say stuff before actually giving the result
        // This is a hack to remove that stuff. Will need to work on optimising the prompt and/or
        // researching other techniques to prevent this.
        // e.g. it sometimes gives "Here's the result: {the result}" instead of just "{the result}"
        var cleanedQueryParamsResult = RemoveBeforeColon(generateQueryParamsResult.Result);
        var queryParams = ExtractQueryParamsFromUrl(cleanedQueryParamsResult);

        var getPropertiesQuery = new GetPropertiesQuery(queryParams);

        var properties = _properties
            .Where(_ => string.IsNullOrWhiteSpace(getPropertiesQuery.Type) || _.Type.EqualsIgnoreCase(getPropertiesQuery.Type))
            .Where(_ => string.IsNullOrWhiteSpace(getPropertiesQuery.Location) || _.Location.EqualsIgnoreCase(getPropertiesQuery.Location))
            .Where(_ => getPropertiesQuery.Price is null || _.Price == getPropertiesQuery.Price)
            .Where(_ => getPropertiesQuery.Bedrooms is null || _.Bedrooms == getPropertiesQuery.Bedrooms)
            .Where(_ => getPropertiesQuery.Bathrooms is null || _.Bathrooms == getPropertiesQuery.Bathrooms)
            .Where(_ => getPropertiesQuery.Garden is null || _.Garden == getPropertiesQuery.Garden)
            .Where(_ => getPropertiesQuery.Parking is null || _.Parking == getPropertiesQuery.Parking)
            .ToList();

        return Ok(new SearchResult
        {
            Properties = properties,
            GenerateFiltersResult = generateFiltersResult.Result,
            GenerateQueryParamsResult = generateQueryParamsResult.Result
        });
    }

    private static string RemoveBeforeColon(string input)
    {
        var colonIndex = input.IndexOf(':');

        if (colonIndex >= 0 && colonIndex < input.Length - 1)
        {
            return input.Substring(colonIndex + 2);
        }
        
        return input;
    }
    
    private static string ExtractQueryParamsFromUrl(string input)
    {
        var colonIndex = input.IndexOf('?');

        if (colonIndex >= 0 && colonIndex < input.Length - 1)
        {
            return input.Substring(colonIndex + 1);
        }
        
        return input;
    }
}

public record SearchResult
{
    public List<Property> Properties { get; init; } = new();
    public string GenerateFiltersResult { get; init; } = "";
    public string GenerateQueryParamsResult { get; init; } = "";
}

public class Property
{
    public int Id { get; set; }
    public required string Type { get; set; }
    public int Price { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public bool Garden { get; set; }
    public bool Parking { get; set; }
    public required string Location { get; set; }
}

public record GetPropertiesQuery
{
    public string? Type { get; init; }
    public int? Price { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public bool? Garden { get; init; }
    public bool? Parking { get; init; }
    public string? Location { get; init; }

    public GetPropertiesQuery(string url)
    {
        var queryString = QueryHelpers.ParseQuery(url);

        Type = queryString.TryGetValue("type", out var outType) ? outType.ToString() : null;
        Price = queryString.TryGetValue("price", out var outPrice) ? int.Parse(outPrice.ToString().Replace("£", "").Replace(",", "")) : null;
        Bedrooms = queryString.TryGetValue("bedrooms", out var outBedrooms) ? int.Parse(outBedrooms.ToString()) : null;
        Bathrooms = queryString.TryGetValue("bathrooms", out var outBathrooms) ? int.Parse(outBathrooms.ToString()) : null;
        Garden = queryString.TryGetValue("garden", out var outGarden) ? bool.Parse(outGarden.ToString()) : null;
        Parking = queryString.TryGetValue("parking", out var outParking) ? bool.Parse(outParking.ToString()) : null;
        Location = queryString.TryGetValue("location", out var outLocation) ? outLocation.ToString() : null;
    }
}