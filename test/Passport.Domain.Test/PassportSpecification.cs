using FluentAssertions;

namespace Passport.Domain.Test
{
    public class PassportSpecification
    {
        [Fact]
        public void HasVisa_ShouldBeTrue_WhenPassportVisaExists()
        {
            // Arrange
            Domain.Aggregate.PassportVisa? ppVisa = DataFaker.PassportVisa.CreateDefault();

            IList<Guid> lstPassportVisaId = new List<Guid>()
            {
                ppVisa.Id
            };

            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault(lstPassportVisaId);

            // Act
            bool bResult = ppPassport.HasVisa(ppVisa);

            // Assert
            bResult.Should().BeTrue();
        }

        [Fact]
        public void TryAddVisa_ShouldBeTrue_WhenPassportVisaIdIsUnique()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            // Act
            bool bResult = ppPassport.TryAddVisa(ppVisa);

            // Assert
            bResult.Should().BeTrue();
        }

        [Fact]
        public void TryAddVisa_ShouldBeFalse_WhenPassportVisaIdIsNotUnique()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            IList<Guid> lstPassportVisaId = new List<Guid>()
            {
                ppVisa.Id
            };

            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault(lstPassportVisaId);

            // Act
            bool bResult = ppPassport.TryAddVisa(ppVisa);

            // Assert
            bResult.Should().BeFalse();
        }

        [Fact]
        public void RemoveVisa_ShouldBeTrue_WhenPassportVisaExists()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            IList<Guid> lstPassportVisaId = new List<Guid>()
            {
                ppVisa.Id
            };

            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault(lstPassportVisaId);

            // Act
            bool bResult = ppPassport.TryRemoveVisa(ppVisa);

            // Assert
            bResult.Should().BeTrue();
        }

        [Fact]
        public void RemoveVisa_ShouldBeFalse_WhenPassportVisaDoesNotExist()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            IList<Guid> lstPassportVisaId = new List<Guid>()
            {
                Guid.NewGuid()
            };

            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault(lstPassportVisaId);

            // Act
            bool bResult = ppPassport.TryRemoveVisa(ppVisa);

            // Assert
            bResult.Should().BeFalse();
        }

        [Fact]
        public void TryDisable_ShouldBeTrue_WhenPassportIsEnabledAndIsNotExpired()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthorizedPassport = DataFaker.Passport.CreateDefault();

            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();
            DateTimeOffset dtEnabledAt = DataFaker.Passport.LastCheckedAt;

            ppPassport.TryEnable(ppAuthority, dtEnabledAt);
            ppAuthorizedPassport.TryEnable(ppAuthority, dtEnabledAt);

            // Act
            DateTimeOffset dtDisabledAt = DataFaker.Passport.LastCheckedAt.AddDays(5);

            bool bResult = ppPassport.TryDisable(ppAuthorizedPassport, dtDisabledAt);

            // Assert
            ppAuthorizedPassport.IsEnabled.Should().BeTrue();
            bResult.Should().BeTrue();
            ppPassport.IsEnabled.Should().BeFalse();
            ppPassport.LastCheckedAt.Should().Be(dtDisabledAt);
            ppPassport.LastCheckedBy.Should().Be(ppAuthorizedPassport.Id);
            ppPassport.IsExpired(dtDisabledAt.AddDays(1));
        }

        [Fact]
        public void TryDisable_ShouldBeFalse_WhenPassportIsDisabledAndIsNotExpired()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthorizedPassport = DataFaker.Passport.CreateDefault();

            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();
            DateTimeOffset dtEnabledAt = DataFaker.Passport.LastCheckedAt;

            ppPassport.TryEnable(ppAuthority, dtEnabledAt);

            // Act
            DateTimeOffset dtDisabledAt = DataFaker.Passport.LastCheckedAt.AddDays(5);

            bool bResult = ppPassport.TryDisable(ppAuthorizedPassport, dtDisabledAt);

            // Assert
            ppAuthorizedPassport.IsEnabled.Should().BeFalse();
            bResult.Should().BeFalse();
        }

        [Fact]
        public void TryDisable_ShouldBeFalse_WhenPassportIsEnabledAndIsExpired()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthorizedPassport = DataFaker.Passport.CreateDefault();

            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();
            DateTimeOffset dtEnabledAt = DataFaker.Passport.LastCheckedAt;

            ppPassport.TryEnable(ppAuthority, dtEnabledAt);
            ppAuthorizedPassport.TryEnable(ppAuthority, dtEnabledAt);

            // Act
            DateTimeOffset dtDisabledAt = ppPassport.ExpiredAt.AddDays(1);

            bool bResult = ppPassport.TryDisable(ppAuthorizedPassport, dtDisabledAt);

            // Assert
            ppAuthorizedPassport.IsEnabled.Should().BeTrue();
            bResult.Should().BeFalse();
        }

        [Fact]
        public void TryEnable_ShouldBeTrue_WhenPassportIsEnabledAndAuthorized()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();

            // Act
            DateTimeOffset dtEnabledAt = DataFaker.Passport.LastCheckedAt.AddDays(1);

            bool bResult = ppPassport.TryEnable(ppAuthority, dtEnabledAt);

            // Assert
            bResult.Should().BeTrue();
        }

        [Fact]
        public void TryEnable_ShouldBeFalse_WhenPassportIsDisabledAndAuthorized()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();

            DateTimeOffset dtDisabledAt = ppAuthority.LastCheckedAt.AddDays(-1);
            ppAuthority.TryDisable(ppAuthority, dtDisabledAt);

            // Act
            DateTimeOffset dtEnabledAt = DataFaker.Passport.LastCheckedAt;

            bool bResult = ppPassport.TryEnable(ppAuthority, dtEnabledAt);

            // Assert
            bResult.Should().BeFalse();
        }

        [Fact]
        public void TryEnable_ShouldBeFalse_WhenPassportIsEnabledAndIsNotAuthorized()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateDefault();

            // Act
            DateTimeOffset dtEnabledAt = DataFaker.Passport.LastCheckedAt;

            bool bResult = ppPassport.TryEnable(ppAuthority, dtEnabledAt);

            // Assert
            bResult.Should().BeFalse();
        }

        [Fact]
        public void TryExtendTerm_ShouldBeTrue_WhenDateIsLater()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            // Act
            DateTimeOffset dtExtendedAt = DataFaker.Passport.LastCheckedAt;
            DateTimeOffset dtDate = ppPassport.ExpiredAt.AddDays(1);

            bool bResult = ppPassport.TryExtendTerm(dtDate, dtExtendedAt, ppPassport.Id);

            // Assert
            bResult.Should().BeTrue();
            ppPassport.LastCheckedAt.Should().Be(dtExtendedAt);
            ppPassport.LastCheckedBy.Should().Be(ppPassport.Id);
            ppPassport.IsExpired(dtDate).Should().BeTrue();
        }

        [Fact]
        public void TryExtendTerm_ShouldBeFalse_WhenDateIsGone()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            // Act
            DateTimeOffset dtExtendedAt = DataFaker.Passport.LastCheckedAt;
            DateTimeOffset dtDate = ppPassport.ExpiredAt.AddDays(-1);

            bool bResult = ppPassport.TryExtendTerm(dtDate, dtExtendedAt, ppPassport.Id);

            // Assert
            bResult.Should().BeFalse();
        }

        [Fact]
        public void TryJoinToAuthority_ShouldBeTrue_WhenPassportIsEnabledAndAuthorized()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();

            DateTimeOffset dtEnabledAt = DataFaker.Passport.LastCheckedAt.AddDays(1);
            ppPassport.TryEnable(ppAuthority, dtEnabledAt);

            // Act
            DateTimeOffset dtJoinedAt = DataFaker.Passport.LastCheckedAt.AddDays(1);

            bool bResult = ppPassport.TryJoinToAuthority(ppAuthority, dtJoinedAt);

            // Assert
            bResult.Should().BeTrue();
            ppPassport.IsAuthority.Should().BeTrue();
            ppPassport.IsEnabled.Should().BeTrue();
            ppPassport.LastCheckedAt.Should().Be(dtJoinedAt);
            ppPassport.LastCheckedBy.Should().Be(ppAuthority.Id);
        }

        [Fact]
        public void TryJoinToAuthority_ShouldBeFalse_WhenPassportIsDisabledAndAuthorized()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();

            DateTimeOffset dtDisabledAt = ppAuthority.LastCheckedAt.AddDays(-1);
            ppAuthority.TryDisable(ppAuthority, dtDisabledAt);

            // Act
            DateTimeOffset dtJoinedAt = DataFaker.Passport.LastCheckedAt;

            bool bResult = ppPassport.TryJoinToAuthority(ppAuthority, dtJoinedAt);

            // Assert
            bResult.Should().BeFalse();
        }

        [Fact]
        public void TryJoinToAuthority_ShouldBeFalse_WhenPassportIsEnabledAndIsNotAuthorized()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateDefault();

            // Act
            DateTimeOffset dtJoinedAt = DataFaker.Passport.LastCheckedAt;

            bool bResult = ppPassport.TryJoinToAuthority(ppAuthority, dtJoinedAt);

            // Assert
            bResult.Should().BeFalse();
        }

        [Fact]
        public void TryJoinToAuthority_ShouldBeFalse_WhenPassportIsDisabled()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();

            // Act
            DateTimeOffset dtJoinedAt = DataFaker.Passport.LastCheckedAt;

            bool bResult = ppPassport.TryJoinToAuthority(ppAuthority, dtJoinedAt);

            // Assert
            bResult.Should().BeFalse();
        }

        [Fact]
        public void TryJoinToAuthority_ShouldBeFalse_WhenPassportIsExpired()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();

            DateTimeOffset dtEnabledAt = DataFaker.Passport.LastCheckedAt.AddDays(1);
            ppPassport.TryEnable(ppAuthority, dtEnabledAt);

            // Act
            DateTimeOffset dtJoinedAt = ppPassport.ExpiredAt.AddDays(1);

            bool bResult = ppPassport.TryJoinToAuthority(ppAuthority, dtJoinedAt);

            // Assert
            bResult.Should().BeFalse();
        }

        [Fact]
        public void TryReset_ShouldBeTrue_WhenPassportIsEnabledAndAuthorized()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateAuthority();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();

            // Act
            DateTimeOffset dtResetAt = DataFaker.Passport.LastCheckedAt.AddDays(1);

            bool bResult = ppPassport.TryReset(ppAuthority, dtResetAt);

            // Assert
            bResult.Should().BeTrue();
            ppPassport.IsAuthority.Should().BeFalse();
            ppPassport.IsEnabled.Should().BeFalse();
            ppPassport.LastCheckedAt.Should().Be(dtResetAt);
            ppPassport.LastCheckedBy.Should().Be(ppAuthority.Id);
            ppPassport.IsExpired(dtResetAt).Should().BeTrue();
        }

        [Fact]
        public void TryReset_ShouldBeFalse_WhenPassportIsDisabledAndAuthorized()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateAuthority();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();

            DateTimeOffset dtDisabledAt = ppAuthority.LastCheckedAt.AddDays(-1);
            ppAuthority.TryDisable(ppAuthority, dtDisabledAt);

            // Act
            DateTimeOffset dtResetAt = DataFaker.Passport.LastCheckedAt;

            bool bResult = ppPassport.TryReset(ppAuthority, dtResetAt);

            // Assert
            bResult.Should().BeFalse();
        }

        [Fact]
        public void TryReset_ShouldBeFalse_WhenPassportIsEnabledAndIsNotAuthorized()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateAuthority();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateDefault();

            // Act
            DateTimeOffset dtResetAt = DataFaker.Passport.LastCheckedAt;

            bool bResult = ppPassport.TryReset(ppAuthority, dtResetAt);

            // Assert
            bResult.Should().BeFalse();
        }
    }
}
