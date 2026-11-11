namespace Games;

/// <summary>
/// Picks the rbxsig signature wrapper header for a client build year.
/// Wrapper forms (https://docs.robloxreverseengineering.net/Client Security/Signatures/):
///   %DATA%           (2010-2012)
///   --rbxsig%DATA%   (2013-2017)
///   --rbxsig2%DATA%  (2018-2019)
///   --rbxsig4%DATA%  (2020+)
/// </summary>
public static class RbxSig
{
    public static string GetHeader(string? universeYear)
    {
        var year = universeYear?.Trim() ?? "";

        if (year.Contains("2018") || year.Contains("2019"))
            return "--rbxsig2%";

        if (year.Contains("2020") || year.Contains("2021") || year.Contains("2022") ||
            year.Contains("2023") || year.Contains("2024") || year.Contains("2025") ||
            year.Contains("2026"))
            return "--rbxsig4%";

        if (year.Contains("2010") || year.Contains("2011") || year.Contains("2012"))
            return "%";

        return "--rbxsig%";
    }

    public static string Wrap(string header, string signatureBase64, string data)
    {
        return $"{header}{signatureBase64}%\r\n{data}";
    }
}
