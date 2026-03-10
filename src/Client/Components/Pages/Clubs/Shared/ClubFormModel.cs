using System.ComponentModel.DataAnnotations;

namespace Client.Components.Pages.Clubs.Shared;

public sealed class ClubFormModel
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}
