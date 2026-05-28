using System.ComponentModel.DataAnnotations;

namespace BlazorBasics.Web.Client.Models;

[Microsoft.Extensions.Validation.ValidatableType]
public class PersonalInfo
{
    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Date of birth is required.")]
    [DataType(DataType.Date)]
    public DateOnly? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Pick a country.")]
    public string Country { get; set; } = "";

    public string? AboutYou { get; set; }

    public bool SubscribeToNewsletter { get; set; }
}
