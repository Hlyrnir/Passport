using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Query.HealthCheck
{
    public sealed class PassportHealthCheckQueryHandler : IQueryHandler<PassportHealthCheckQuery, IMessageResult<bool>>
    {
        private readonly IUnitOfWork uowUnitOfWork;

        public PassportHealthCheckQueryHandler([FromKeyedServices(DefaultKeyedServiceName.UnitOfWork)] IUnitOfWork uowUnitOfWork)
        {
            this.uowUnitOfWork = uowUnitOfWork;
        }

        public async ValueTask<IMessageResult<bool>> Handle(PassportHealthCheckQuery qryQuery, CancellationToken tknCancellation)
        {
            bool bIsHealthy = false;

            try
            {
                await uowUnitOfWork.TransactionAsync(() =>
                {
                    bIsHealthy = uowUnitOfWork.TryRollback();

                    return Task.CompletedTask;
                });
            }
            catch (Exception exException)
            {
                return new MessageResult<bool>(new MessageError() { Code = "EXCEPTION", Description = exException.Message });
            }

            return new MessageResult<bool>(bIsHealthy);
        }
    }
}
