using System.ComponentModel.DataAnnotations;

namespace BlazorBasics.Web.Client.Models;

[Microsoft.Extensions.Validation.ValidatableType]
public class Comparison
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Prompt is required.")]
    [MinLength(5, ErrorMessage = "Prompt must be at least 5 characters.")]
    public string Prompt { get; set; } = "";

    [Required(ErrorMessage = "Pick a category.")]
    public string Category { get; set; } = "";

    public Submitter Submitter { get; set; } = new();

    public List<ComparisonResponse> Responses { get; set; } = new();
}
