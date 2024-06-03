using System.Diagnostics;
using System.Reflection;

namespace Unflow.WebApi.Example;

public static class Properties
{
    private static FileVersionInfo _fileVersionInfo;

    public static FileVersionInfo FileVersionInfo
    {
        get
        {
            if (_fileVersionInfo != null)
                return _fileVersionInfo;

            var assemblyLocation = typeof(Program).GetTypeInfo().Assembly.Location;
            if (string.IsNullOrWhiteSpace(assemblyLocation))
                throw new InvalidOperationException("Failed to get the assembly location");

            _fileVersionInfo = FileVersionInfo.GetVersionInfo(assemblyLocation);

            return _fileVersionInfo;
        }
    }
}