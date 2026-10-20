using System.Globalization;

namespace Timesheets.Tests;

internal sealed class CultureScope : IDisposable
{
    private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;

    public CultureScope(string cultureName) => CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

    public void Dispose() => CultureInfo.CurrentCulture = _originalCulture;
}
