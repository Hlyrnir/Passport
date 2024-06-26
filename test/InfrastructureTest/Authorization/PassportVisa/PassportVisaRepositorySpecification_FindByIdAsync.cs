﻿using Application.Interface.Result;
using Domain.Interface.Authorization;
using DomainFaker;
using FluentAssertions;
using Infrastructure.Error;
using InfrastructureTest.Authorization.Common;
using InfrastructureTest.Common;
using Xunit;

namespace InfrastructureTest.Passport.PassportVisa
{
    public class PassportVisaRepositorySpecification_FindByIdAsync : IClassFixture<PassportFixture>
	{
		private readonly PassportFixture fxtAuthorizationData;

		private readonly ITimeProvider prvTime;

		public PassportVisaRepositorySpecification_FindByIdAsync(PassportFixture fxtAuthorizationData)
		{
			this.fxtAuthorizationData = fxtAuthorizationData;
			this.prvTime = fxtAuthorizationData.TimeProvider;
		}

		[Fact]
		public async Task Find_ShouldFindVisa_WhenVisaIdExists()
		{
			// Arrange
			IPassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

			await fxtAuthorizationData.PassportVisaRepository.InsertAsync(ppVisa, prvTime.GetUtcNow(), CancellationToken.None);

			// Act
			IRepositoryResult<IPassportVisa> rsltVisa = await fxtAuthorizationData.PassportVisaRepository.FindByIdAsync(ppVisa.Id, CancellationToken.None);

			// Assert
			rsltVisa.Match(
				msgError =>
				{
					msgError.Should().BeNull();

					return false;
				},
				ppPassportInRepository =>
				{
					ppPassportInRepository.Should().NotBeNull();
					ppPassportInRepository.Should().BeEquivalentTo(ppVisa);

					return true;
				});

			// Clean up
			await fxtAuthorizationData.PassportVisaRepository.DeleteAsync(ppVisa, CancellationToken.None);
		}

		[Fact]
		public async Task Find_ShouldReturnRepositoryError_WhenVisaIdDoesNotExist()
		{
			// Arrange
			Guid guVisaId = Guid.NewGuid();

			// Act
			IRepositoryResult<IPassportVisa> rsltVisa = await fxtAuthorizationData.PassportVisaRepository.FindByIdAsync(guVisaId, CancellationToken.None);

			// Assert
			rsltVisa.Match(
				msgError =>
				{
					msgError.Should().NotBeNull();
					msgError.Code.Should().Be(PassportError.Code.Method);
					msgError.Description.Should().Be($"No data for {guVisaId} has been found.");

					return false;
				},
				ppVisaInRepository =>
				{
					ppVisaInRepository.Should().BeNull();

					return true;
				});
		}
	}
}
