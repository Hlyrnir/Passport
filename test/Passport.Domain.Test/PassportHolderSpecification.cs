namespace Passport.Domain.Test
{
    public class PassportHolderSpecification
    {
        private readonly IPassportSetting ppSetting;

        public PassportHolderSpecification()
        {
            ppSetting = DataFaker.PassportSetting.Create();
        }

        [Theory]
        [InlineData(false, null)]
        [InlineData(false, "")]
        [InlineData(false, " ")]
        [InlineData(false, "en")]
        [InlineData(false, "enGB")]
        [InlineData(false, "en|GB")]
        [InlineData(false, "This is not a valid culture name.")]
        [InlineData(true, "en-gb")]
        [InlineData(true, "en-GB")]
        public void ChangeCultureNameShouldSucceed(bool bResult, string? sCultureName)
        {
            // Arrange
            bool bIsChanged = false;

            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault(ppSetting);

            // Act
            bIsChanged = ppHolder.TryChangeCultureName(sCultureName!);

            // Assert
            Assert.Equal(bResult, bIsChanged);
        }

        [Fact]
        public void ChangeCultureNameShouldBeCorrectFormatted()
        {
            // Arrange
            bool bIsChanged = false;

            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault(ppSetting);

            // Act
            bIsChanged = ppHolder.TryChangeCultureName("DE-CH");

            // Assert
            Assert.True(bIsChanged);
            Assert.Equal("de-CH", ppHolder.CultureName);
        }

        [Theory]
        [InlineData(false, null)]
        [InlineData(false, "")]
        [InlineData(false, " ")]
        [InlineData(false, "This is not a valid e-mail address.")]
        [InlineData(false, "example@notcomplete")]
        [InlineData(false, "default.example@notcomplete")]
        //[InlineData(false, "examp!e@not.complete")]
        [InlineData(true, "example@not.complete")]
        [InlineData(true, "default@example.ch")]
        [InlineData(true, "default@example.co.uk")]
        [InlineData(true, "firstname.lastname@example.com")]
        [InlineData(true, "example@valid.com")]
        public void ChangeEmailAddressShouldSucceed(bool bResult, string? sEmailAddress)
        {
            // Arrange
            bool bIsChanged = false;

            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault(ppSetting);

            // Act
            bIsChanged = ppHolder.TryChangeEmailAddress(sEmailAddress!, ppSetting);

            // Assert
            Assert.Equal(bResult, bIsChanged);
        }
    }
}
