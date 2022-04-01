using CSharpFunctionalExtensions;
using MediatR;

namespace Logistic.DaysOfStayTracker.Core.Common;

public interface IValidationRequestHandler<TRequest> : IRequestHandler<TRequest, Result<Unit, ICollection<string>>> 
    where TRequest : IValidationRequest
{
    
}

public interface IValidationRequestHandler<TRequest, T> : IRequestHandler<TRequest, Result<T, ICollection<string>>> 
    where TRequest : IValidationRequest<T>
{
}

public interface IValidationRequest : IValidationRequest<Unit>
{
}

public interface IValidationRequest<T> : IRequest<Result<T, ICollection<string>>>
{
    
}