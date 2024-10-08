﻿using Application.Command.Authorization.PassportHolder.ConfirmEmailAddress;
using Application.Common.Error;
using Application.Interface.Result;
using Application.Interface.Time;
using Application.Interface.Validation;
using ApplicationTest.Common;
using Domain.Interface.Authorization;
using DomainFaker;
using FluentAssertions;
using Xunit;

namespace ApplicationTest.Command.Authorization.PassportHolder.ConfirmEmailAddress
{
    public sealed class ConfirmEmailAddressValidationSpecification : IClassFixture<PassportFixture>
	{
		private readonly PassportFixture fxtAuthorizationData;
		private readonly ITimeProvider prvTime;

		public ConfirmEmailAddressValidationSpecification(PassportFixture fxtAuthorizationData)
		{
			this.fxtAuthorizationData = fxtAuthorizationData;
			prvTime = fxtAuthorizationData.TimeProvider;
		}

		[Fact]
		public async Task Update_ShouldReturnTrue_WhenPassportExists()
		{
			// Arrange
			IPassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault(fxtAuthorizationData.PassportSetting);
			await fxtAuthorizationData.PassportHolderRepository.InsertAsync(ppHolder, prvTime.GetUtcNow(), CancellationToken.None);

			ConfirmEmailAddressCommand cmdUpdate = new ConfirmEmailAddressCommand()
			{
				ConcurrencyStamp = ppHolder.ConcurrencyStamp,
				EmailAddress = $"{Guid.NewGuid()}@passport.org",
				PassportHolderId = ppHolder.Id,
				RestrictedPassportId = Guid.NewGuid()
			};

			IValidation<ConfirmEmailAddressCommand> hndlValidation = new ConfirmEmailAddressValidation(
				repoHolder: fxtAuthorizationData.PassportHolderRepository,
				srvValidation: fxtAuthorizationData.PassportValidation,
				prvTime: prvTime);

			// Act
			IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
				msgMessage: cmdUpdate,
				tknCancellation: CancellationToken.None);

			// Assert
			rsltValidation.Match(
				msgError =>
				{
					msgError.Should().BeNull();

					return false;
				},
				bResult =>
				{
					bResult.Should().BeTrue();

					return true;
				});

			// Clean up
			await fxtAuthorizationData.PassportHolderRepository.DeleteAsync(ppHolder, CancellationToken.None);
		}

		[Fact]
		public async Task Update_ShouldReturnMessageError_WhenPassportDoesNotExist()
		{
			// Arrange
			ConfirmEmailAddressCommand cmdUpdate = new ConfirmEmailAddressCommand()
			{
				ConcurrencyStamp = Guid.NewGuid().ToString(),
				EmailAddress = $"{Guid.NewGuid()}@passport.org",
				PassportHolderId = Guid.NewGuid(),
				RestrictedPassportId = Guid.NewGuid()
			};

			IValidation<ConfirmEmailAddressCommand> hndlValidation = new ConfirmEmailAddressValidation(
				repoHolder: fxtAuthorizationData.PassportHolderRepository,
				srvValidation: fxtAuthorizationData.PassportValidation,
				prvTime: prvTime);

			// Act
			IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
				msgMessage: cmdUpdate,
				tknCancellation: CancellationToken.None);

			// Assert
			rsltValidation.Match(
				msgError =>
				{
					msgError.Should().NotBeNull();
					msgError.Code.Should().Be(ValidationError.Code.Method);
					msgError.Description.Should().Contain($"Passport holder {cmdUpdate.PassportHolderId} does not exist.");

					return false;
				},
				bResult =>
				{
					bResult.Should().BeFalse();

					return true;
				});
		}
	}
}