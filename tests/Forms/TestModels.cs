using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Hasseware.AspNetCore.Components.Forms.Tests;

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

public class ChildModel
{
    [Required]
    public string? Name { get; set; }
}

public class ParentModel
{
    [Required]
    public string? Title { get; set; }

    public ChildModel? Child { get; set; }

    public List<ChildModel> Items { get; set; } = new();
}

public class InferenceModel
{
    public bool IsActive { get; set; }

    public string? PlainText { get; set; }

    [DataType(DataType.Date)]
    public string? DateText { get; set; }

    [DataType(DataType.DateTime)]
    public string? DateTimeText { get; set; }

    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [DataType(DataType.PhoneNumber)]
    public string? Phone { get; set; }

    [DataType(DataType.Time)]
    public string? TimeText { get; set; }

    [DataType(DataType.Url)]
    public string? Website { get; set; }

    public int Count { get; set; }

    public int? NullableCount { get; set; }

    public double Ratio { get; set; }

    public decimal Amount { get; set; }

    public DateTime When { get; set; }

    [DataType(DataType.Date)]
    public DateTime WhenDateOnly { get; set; }

    public DateTimeOffset WhenOffset { get; set; }

    public Guid Identifier { get; set; }

    public Uri? Homepage { get; set; }

    public Color Favorite { get; set; }

    public Permissions Access { get; set; }

    [Editor(typeof(CustomEditorStub), typeof(Microsoft.AspNetCore.Components.Forms.InputBase<>))]
    public string? CustomEdited { get; set; }
}

public class CustomEditorStub : Microsoft.AspNetCore.Components.Forms.InputBase<string?>
{
    protected override bool TryParseValueFromString(string? value, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out string? result, [System.Diagnostics.CodeAnalysis.NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value;
        validationErrorMessage = null;
        return true;
    }
}

public class DisplayModel
{
    [Display(Name = "Full Name", Description = "Enter your name", Prompt = "e.g. Jane Doe", GroupName = "Basics", Order = 5)]
    public string? Name { get; set; }

    [DisplayName("Legacy Label")]
    public string? Legacy { get; set; }

    public string? Raw { get; set; }

    public string ReadOnlyComputed => "computed";

    [Editable(false)]
    public string? NotEditable { get; set; } = "value";

    [Display]
    public string? OnlyWithDisplay { get; set; }
}
