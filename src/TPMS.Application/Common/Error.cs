namespace TPMS.Application.Common;

public sealed record Error(string Code, string Message, object? Metadata = null);
