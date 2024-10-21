namespace Passport.Domain.Test
{
    public class PassportVisaSpecification
    {
        [Theory]
        [InlineData(false, null)]
        [InlineData(false, "")]
        [InlineData(false, " ")]
        [InlineData(false, "This is not a valid name.")]
        [InlineData(true, "VALID_VISA_NAME")]
        public void ChangeName_ShouldSucceed_WhenNameIsValid(bool bResult, string? sName)
        {
            // Arrange
            bool bIsChanged = false;

            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            // Act
            bIsChanged = ppVisa.TryChangeName(sName!);

            // Assert
            Assert.Equal(bResult, bIsChanged);
        }

        [Theory]
        [InlineData(false, (-1))]
        [InlineData(true, 0)]
        [InlineData(false, int.MinValue)]
        [InlineData(true, int.MaxValue)]
        public void ChangeLevel_ShouldSucceed_WhenLevelIsValid(bool bResult, int iLevel)
        {
            // Arrange
            bool bIsChanged = false;

            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            // Act
            bIsChanged = ppVisa.TryChangeLevel(iLevel);

            // Assert
            Assert.Equal(bResult, bIsChanged);
        }
    }
}
