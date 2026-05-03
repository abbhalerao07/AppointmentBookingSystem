using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.CQRS
{
    // Query 
    public interface IQuery<out TResponse> : IRequest<TResponse> { }
}
