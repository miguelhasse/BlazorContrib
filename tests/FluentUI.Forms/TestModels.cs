using System.ComponentModel.DataAnnotations;

namespace Hasseware.FluentUI.AspNetCore.Components.Forms.Tests;

public enum Color
{
    Red,
    Green,

    [Display(Name = "Deep Blue")]
    Blue,
}

[Flags]
public enum Permissions
{
    None = 0,
    Read = 1,
    Write = 2,
}

public class InferenceModel
{
    public bool IsActive { get; set; }

    public string? PlainText { get; set; }

    [StringLength(50, MinimumLength = 5)]
    public string? Constrained { get; set; }

    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [DataType(DataType.PhoneNumber)]
    public string? Phone { get; set; }

    [DataType(DataType.Url)]
    public string? Website { get; set; }

    [DataType(DataType.Date)]
    public string? DateText { get; set; }

    [DataType(DataType.Time)]
    public string? TimeText { get; set; }

    [DataType(DataType.DateTime)]
    public string? DateTimeText { get; set; }

    public int Count { get; set; }

    [Range(1, 10)]
    public int RangedCount { get; set; }

    public DateTime When { get; set; }

    public Guid Identifier { get; set; }

    public Uri? Homepage { get; set; }

    public Color Favorite { get; set; }

    public Permissions Access { get; set; }

    [Required]
    public string? RequiredText { get; set; }

    [Key]
    public string? KeyText { get; set; }

    [Required]
    public string? DisabledRequiredText { get; set; }
}
