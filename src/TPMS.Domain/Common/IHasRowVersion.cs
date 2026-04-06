namespace TPMS.Domain.Common;

public interface IHasRowVersion
{
    byte[] RowVersion { get; }
}
