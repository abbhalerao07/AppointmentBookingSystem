using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.CQRS
{
    // Command 
    public interface ICommand : IRequest { }
    public interface ICommand<out TResponse> : IRequest<TResponse> { }
}
