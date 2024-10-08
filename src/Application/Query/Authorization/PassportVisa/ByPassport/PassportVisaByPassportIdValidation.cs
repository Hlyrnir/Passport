﻿using Application.Common.Error;
using Application.Common.Result.Message;
using Application.Interface.Passport;
using Application.Interface.Result;
using Application.Interface.Time;
using Application.Interface.Validation;

namespace Application.Query.Authorization.PassportVisa.ByPassport
{
    internal class PassportVisaByPassportIdValidation : IValidation<PassportVisaByPassportIdQuery>
	{
		private readonly ITimeProvider prvTime;
		private readonly IPassportValidation srvValidation;

		private readonly IPassportRepository repoPassport;

		public PassportVisaByPassportIdValidation(IPassportRepository repoPassport, IPassportValidation srvValidation, ITimeProvider prvTime)
		{
			this.prvTime = prvTime;
			this.srvValidation = srvValidation;

			this.repoPassport = repoPassport;
		}

		async ValueTask<IMessageResult<bool>> IValidation<PassportVisaByPassportIdQuery>.ValidateAsync(PassportVisaByPassportIdQuery msgMessage, CancellationToken tknCancellation)
		{
			if (tknCancellation.IsCancellationRequested)
				return new MessageResult<bool>(DefaultMessageError.TaskAborted);

			srvValidation.ValidateGuid(msgMessage.PassportIdToFind, "Passport identifier");

			if (srvValidation.IsValid == true)
			{
				IRepositoryResult<bool> rsltPassport = await repoPassport.ExistsAsync(msgMessage.PassportIdToFind, tknCancellation);

				rsltPassport.Match(
					msgError => srvValidation.Add(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
					bResult =>
					{
						if (bResult == false)
							srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"Passport {msgMessage.PassportIdToFind} does not exist." });

						return bResult;
					});
			}

			return await Task.FromResult(
				srvValidation.Match(
					msgError => new MessageResult<bool>(msgError),
					bResult => new MessageResult<bool>(bResult))
				);
		}
	}
}