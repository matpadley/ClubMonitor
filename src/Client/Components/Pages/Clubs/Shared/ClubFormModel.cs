using System.ComponentModel.DataAnnotations;

namespace Client.Components.Pages.Clubs.Shared;

public sealed class ClubFormModel
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200, ErrorMessage = "Name must not exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;
}
