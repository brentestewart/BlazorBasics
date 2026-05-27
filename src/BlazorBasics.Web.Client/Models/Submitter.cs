using System.ComponentModel.DataAnnotations;

namespace BlazorBasics.Web.Client.Models;

public class Submitter
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = "";

    public string? FavoriteLlm { get; set; }
}
