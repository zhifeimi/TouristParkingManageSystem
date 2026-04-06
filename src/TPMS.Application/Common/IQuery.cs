using MediatR;

namespace TPMS.Application.Common;

public interface IQuery<out TResponse> : IRequest<TResponse>;
