using TPMS.Application.Common;

namespace TPMS.Application.Lots;

public sealed record GetLotsQuery : IQuery<Result<IReadOnlyCollection<LotListItemDto>>>;
