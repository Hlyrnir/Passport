﻿using Application.Command.PhysicalData.TimePeriod.Update;
using Application.Common.Error;
using Application.Interface.Result;
using Contract.v01.Request.TimePeriod;
using Mediator;
using Presentation.Common;

namespace Presentation.Endpoint.PhysicalData
{
    public static class UpdateTimePeriodEndpoint
	{
		public const string Name = "UpdateTimePeriod";

		public static void AddUpdateTimePeriodEndpoint(this IEndpointRouteBuilder epBuilder)
		{
			epBuilder.MapPut(
				EndpointRoute.TimePeriod.Update, UpdateTimePeriod)
				.RequireAuthorization(EndpointAuthorization.Passport)
				.WithName(Name)
				.WithTags("TimePeriod")
				.Produces(StatusCodes.Status401Unauthorized)
				.Produces(StatusCodes.Status403Forbidden)
				.Produces<bool>(StatusCodes.Status200OK)
				.Produces<string>(StatusCodes.Status400BadRequest)
				.WithApiVersionSet(EndpointVersion.VersionSet)
				.HasApiVersion(1.0);
		}

		public static async Task<IResult> UpdateTimePeriod(
			UpdateTimePeriodRequest rqstTimePeriod,
			HttpContext httpContext,
			ISender mdtMediator,
			CancellationToken tknCancellation)
		{
			Guid guPassportId = Guid.Empty;

			if (httpContext.TryParsePassportId(out guPassportId) == false)
				return Results.BadRequest("Passport could not be identified.");

			UpdateTimePeriodCommand cmdUpdate = rqstTimePeriod.MapToCommand(guPassportId);

			IMessageResult<bool> mdtResult = await mdtMediator.Send(cmdUpdate, tknCancellation);

			return mdtResult.Match(
				msgError =>
				{
					if (msgError.Equals(AuthorizationError.PassportVisa.VisaDoesNotExist) == true)
						return Results.Forbid();

					return Results.BadRequest($"{msgError.Code}: {msgError.Description}");
				},
				bResult => TypedResults.Ok(bResult));
		}

		private static UpdateTimePeriodCommand MapToCommand(this UpdateTimePeriodRequest rqstTimePeriod, Guid guPassportId)
		{
			return new UpdateTimePeriodCommand()
			{
				RestrictedPassportId = guPassportId,
				ConcurrencyStamp=rqstTimePeriod.ConcurrencyStamp,
				TimePeriodId = rqstTimePeriod.TimePeriodId,
				PhysicalDimensionId = rqstTimePeriod.PhysicalDimensionId,
				Magnitude = rqstTimePeriod.Magnitude,
				Offset = rqstTimePeriod.Offset
			};
		}
	}
}