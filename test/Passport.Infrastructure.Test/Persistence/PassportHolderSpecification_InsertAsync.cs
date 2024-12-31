using FluentAssertions;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportHolderRepositorySpecification_InsertAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportHolderRepositorySpecification_InsertAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            this.prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task InsertAsync_ShouldReturnTrue_WhenHolderIsCreated()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault();

            // Act
            RepositoryResult<bool> rsltHolder = await fxtPassport.PassportHolderRepository.InsertAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltHolder.Match<bool>(
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
            await fxtPassport.PassportHolderRepository.DeleteAsync(ppHolder.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task InsertAsync_ShouldReturnRepositoryError_WhenHolderIdExistsAndEmailAddressIsDifferent()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault();

            PassportHolderTransferObject dtoHolder = ppHolder.MapToTransferObject();
            PassportHolderTransferObject dtoHolderToCreate = new PassportHolderTransferObject()
            {
                ConcurrencyStamp = dtoHolder.ConcurrencyStamp,
                CultureName = dtoHolder.CultureName,
                EmailAddressIsConfirmed = dtoHolder.EmailAddressIsConfirmed,
                EmailAddress = "another@passport.org",
                FirstName = dtoHolder.FirstName,
                Id = dtoHolder.Id,
                LastName = dtoHolder.LastName,
                PhoneNumber = dtoHolder.PhoneNumber,
                PhoneNumberIsConfirmed = dtoHolder.PhoneNumberIsConfirmed,
                SecurityStamp = dtoHolder.SecurityStamp
            };

            await fxtPassport.PassportHolderRepository.InsertAsync(dtoHolder, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltPassport = await fxtPassport.PassportHolderRepository.InsertAsync(dtoHolderToCreate, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltPassport.Match<bool>(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Could not create holder {dtoHolderToCreate.Id}.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportHolderRepository.DeleteAsync(dtoHolder, CancellationToken.None);
        }

        [Fact]
        public async Task InsertAsync_ShouldReturnRepositoryError_WhenEmailAddressExistsAndHolderIdIsDifferent()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault();

            PassportHolderTransferObject dtoHolder = ppHolder.MapToTransferObject();
            PassportHolderTransferObject dtoHolderToCreate = new PassportHolderTransferObject()
            {
                ConcurrencyStamp = dtoHolder.ConcurrencyStamp,
                CultureName = dtoHolder.CultureName,
                EmailAddressIsConfirmed = dtoHolder.EmailAddressIsConfirmed,
                EmailAddress = dtoHolder.EmailAddress,
                FirstName = dtoHolder.FirstName,
                Id = Guid.NewGuid(),
                LastName = dtoHolder.LastName,
                PhoneNumber = dtoHolder.PhoneNumber,
                PhoneNumberIsConfirmed = dtoHolder.PhoneNumberIsConfirmed,
                SecurityStamp = dtoHolder.SecurityStamp
            };

            await fxtPassport.PassportHolderRepository.InsertAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltPassport = await fxtPassport.PassportHolderRepository.InsertAsync(dtoHolderToCreate, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltPassport.Match<bool>(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Could not create holder {dtoHolderToCreate.Id}.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportHolderRepository.DeleteAsync(dtoHolder, CancellationToken.None);
        }
    }
}
