using System.ComponentModel.DataAnnotations;

namespace Client.Components.Pages.Account.Shared;

public sealed class UserProfileFormModel
{
    [Required(ErrorMessage = "Username is required.")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
    [MaxLength(50, ErrorMessage = "Username must not exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_\-]+$", ErrorMessage = "Username may only contain letters, digits, hyphens, and underscores.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    [MaxLength(256, ErrorMessage = "Email must not exceed 256 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Display name is required.")]
    [MaxLength(200, ErrorMessage = "Display name must not exceed 200 characters.")]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Bio must not exceed 500 characters.")]
    public string? Bio { get; set; }
}
