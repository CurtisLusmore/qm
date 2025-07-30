using Microsoft.Extensions.Options;

namespace qm;

/// <summary>
/// Options which can be updated manually
/// </summary>
/// <typeparam name="T">The type of options</typeparam>
public class ManualOptions<T> : IOptions<T> where T : class
{
    /// <inheritdoc/>
    public required T Value { get; set; }

    /// <summary>
    /// Set the value of the options
    /// </summary>
    /// <param name="value">The new value</param>
    public void SetValue(T value) => Value = value;
}
