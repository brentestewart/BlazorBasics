using System.ComponentModel.DataAnnotations;

namespace BlazorBasics.Web.Client.Models;

public class ComparisonResponse
{
    public string LlmName { get; set; } = "";

    [Required(ErrorMessage = "Response text is required.")]
    public string Text { get; set; } = "";

    [Range(0, 10, ErrorMessage = "Rating must be between 0 and 10.")]
    public decimal Rating { get; set; }
}
