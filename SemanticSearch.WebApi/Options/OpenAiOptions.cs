using System.ComponentModel.DataAnnotations;

namespace SemanticSearch.WebApi.Options;

public record OpenAiOptions
{
    [Required]
    public string ApiKey { get; init; } = "";

    [Required]
    public string OrganizationId { get; init; } = "";
}