using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Utilities;
using Microsoft.FluentUI.AspNetCore.Components.Extensions;

namespace Hasseware.FluentUI.AspNetCore.Components.Forms;

[CascadingTypeParameter(nameof(TValue))]
public partial class FluentDateTime<TValue> : FluentInputBase<TValue>
{
    public FluentDateTime()
    {
        var type = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

        if (type != typeof(DateTime) && type != typeof(DateTimeOffset) && type != typeof(DateOnly) && type != typeof(TimeOnly))
        {
            throw new InvalidOperationException($"Unsupported {GetType()} type param '{type}'.");
        }
    }

    private TValue? _selectedDate = default!;

	protected override string? StyleValue => new StyleBuilder(Style).Build();

	protected override string? ClassValue => new CssBuilder(base.ClassValue).AddClass("fluent-datepicker").Build();

	public bool Opened { get; set; } = false;

	protected DateTime? CurrentDateTime => ValueAsDateTime(CurrentValue);

    protected string? CurrentTimeAsString => ValueAsDateTime(CurrentValue)?.ToString("HH:mm");

    [Parameter]
	public virtual FluentInputAppearance Appearance { get; set; } = FluentInputAppearance.Outline;

	[Parameter]
	public virtual bool DisabledSelectable { get; set; } = true;

	/// <summary>
	/// Gets or sets the Type style for the day (numeric or 2-digits).
	/// </summary>
	[Parameter]
	public DayFormat? DayFormat { get; set; } = Microsoft.FluentUI.AspNetCore.Components.DayFormat.Numeric;

	/// <summary>
	/// Gets or sets the verification to do when the selected value has changed.
	/// By default, ValueChanged is called only if the selected value has changed.
	/// </summary>
	[Parameter]
	public bool CheckIfSelectedValueHasChanged { get; set; } = true;

	/// <summary>
	/// Defines the appearance of the <see cref="FluentCalendar"/> component.
	/// </summary>
	[Parameter]
	public virtual CalendarViews View { get; set; } = CalendarViews.Days;


	[Parameter]
	public virtual CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

    [Parameter]
    public Orientation Orientation { get; set; } = Orientation.Horizontal;

    [Parameter]
	public InputDateType Type { get; set; } = InputDateType.DateTimeLocal;

    [Parameter]
	public string ParsingErrorMessage { get; set; } = default!;

	public override TValue? Value
	{
		get
		{
			return _selectedDate;
		}
		set
		{
			if (CheckIfSelectedValueHasChanged && object.Equals(_selectedDate, value))
			{
				return;
			}
  
            _selectedDate = value;

			if (ValueChanged.HasDelegate)
			{
				ValueChanged.InvokeAsync(value);
			}
			if (ValueExpression != null || ValueChanged.HasDelegate)
			{
				EditContext?.NotifyFieldChanged(FieldIdentifier);
			}
		}
	}

    [Parameter]
    public virtual Func<DateTime, bool>? DisabledDateFunc { get; set; }

    [Parameter]
	public EventCallback<bool> OnCalendarOpen { get; set; }

	protected override string FormatValueAsString(TValue? value) => value switch
	{
		DateTime dateTimeValue => BindConverter.FormatValue(dateTimeValue, Culture.DateTimeFormat.ShortDatePattern, CultureInfo.InvariantCulture),
		DateTimeOffset dateTimeOffsetValue => BindConverter.FormatValue(dateTimeOffsetValue, Culture.DateTimeFormat.ShortDatePattern, CultureInfo.InvariantCulture),
		DateOnly dateOnlyValue => BindConverter.FormatValue(dateOnlyValue, Culture.DateTimeFormat.ShortDatePattern, CultureInfo.InvariantCulture),
		TimeOnly timeOnlyValue => BindConverter.FormatValue(timeOnlyValue, Culture.DateTimeFormat.ShortDatePattern, CultureInfo.InvariantCulture),
		_ => string.Empty,// Handles null for Nullable<DateTime>, etc.
	};

	/// <inheritdoc />
	protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out TValue result, [NotNullWhen(false)] out string? validationErrorMessage)
	{
        if (View == CalendarViews.Years && int.TryParse(value, out var year))
        {
            value = new DateTime(year, 1, 1).ToString(Culture.DateTimeFormat.ShortDatePattern);
        }
        
		if (value != null && TimeOnly.TryParse(value, out var valueConverted))
        {
            result = DateTimeAsValue((ValueAsDateTime(Value) ?? DateTime.MinValue).Date.Add(valueConverted.ToTimeSpan()));
			validationErrorMessage = null;
			return true;
        }

        if (BindConverter.TryConvertTo(value, CultureInfo.InvariantCulture, out result))
        {
            Debug.Assert(result != null);
            validationErrorMessage = null;
            return true;
        }

		validationErrorMessage = string.Format(CultureInfo.CurrentCulture, ParsingErrorMessage, FieldIdentifier.FieldName);
		return false;
	}

    protected Task OnCalendarOpenHandlerAsync(MouseEventArgs _)
    {
        if (!ReadOnly)
        {
            Opened = !Opened;

            if (OnCalendarOpen.HasDelegate)
            {
                return OnCalendarOpen.InvokeAsync(Opened);
            }
        }

        return Task.CompletedTask;
    }

    protected Task OnSelectedDateAsync(DateTime? value)
    {
        Opened = false;

        if (Value != null && CurrentDateTime?.TimeOfDay != TimeSpan.Zero)
        {
            DateTime currentValue = value ?? DateTime.MinValue;
            value = currentValue + CurrentDateTime?.TimeOfDay;
        }

        Value = DateTimeAsValue(value);
        return Task.CompletedTask;
    }

    private static TValue DateTimeAsValue(DateTime? value)
    {
        var type = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

		if (value != null)
		{
			if (type == typeof(DateTimeOffset))
				return (TValue)(object)new DateTimeOffset(value.Value);

            if (type == typeof(DateOnly))
                return (TValue)(object)value.ToDateOnly();

            if (type == typeof(TimeOnly))
                return (TValue)(object)value.ToTimeOnly();

            return (TValue)(object)value.Value;
        }

		return default!;
    }

    private string PlaceholderAccordingToView() => View switch
	{
		CalendarViews.Years => "yyyy",
		CalendarViews.Months => Culture.DateTimeFormat.YearMonthPattern,
		_ => Culture.DateTimeFormat.ShortDatePattern
	};

    private static DateTime? ValueAsDateTime(TValue? value) => value switch
    {
        DateTime dateTimeValue => dateTimeValue,
        DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue.DateTime,
        DateOnly dateOnlyValue => dateOnlyValue.ToDateTime(),
        TimeOnly timeOnlyValue => timeOnlyValue.ToDateTime(),
        _ => null
    };
}