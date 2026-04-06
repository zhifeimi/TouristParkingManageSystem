namespace TPMS.Application.Common;

public class ConcurrencyConflictException(string message) : Exception(message);
