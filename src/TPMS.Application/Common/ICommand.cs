using MediatR;

namespace TPMS.Application.Common;

public interface ICommand<out TResponse> : IRequest<TResponse>;
