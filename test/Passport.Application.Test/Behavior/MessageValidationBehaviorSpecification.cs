using FluentAssertions;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Result;
using Passport.Application.Test.Fake.Command;
using Passport.Application.Validation;

namespace Passport.Application.Test.Behavior
{
    public class MessageValidationBehaviorSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public MessageValidationBehaviorSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Validation_ShouldReturnTrue_WhenIsValid()
        {
            string sTest = "TEST_CONTENT";
            IMessageError msgError = new MessageError() { Code = "TEST_VALIDATION", Description = "Message is not valid." };

            FakeCommand cmdFake = new FakeCommand()
            {
                Test = sTest,
                RestrictedPassportId = Guid.NewGuid()
            };

            FakeCommandHandler msgHandler = new FakeCommandHandler();

            IValidation<FakeCommand> msgValidation = new FakeValidation(sTest, msgError);

            MessageValidationBehaviour<FakeCommand, FakeResult> msgBehavior = new MessageValidationBehaviour<FakeCommand, FakeResult>(msgValidation: msgValidation);

            // Act
            IMessageResult<FakeResult> rsltValidation = await msgBehavior.Handle(cmdFake, CancellationToken.None, (FakeCommand msgMessage, CancellationToken tknCancellation) => { return msgHandler.Handle(msgMessage, tknCancellation); });

            //Assert
            rsltValidation.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                msgResult =>
                {
                    msgResult.Should().NotBeNull();

                    return true;
                });
        }

        [Fact]
        public async Task Validation_ShouldReturnMessageError_WhenIsInvalid()
        {
            string sTest = "TEST_CONTENT";
            IMessageError msgValidationError = new MessageError() { Code = "TEST_VALIDATION", Description = "Message is not valid." };

            FakeCommand cmdFake = new FakeCommand()
            {
                Test = "INVALID_CONTENT",
                RestrictedPassportId = Guid.NewGuid()
            };

            FakeCommandHandler msgHandler = new FakeCommandHandler();

            IValidation<FakeCommand> msgValidation = new FakeValidation(sTest, msgValidationError);

            MessageValidationBehaviour<FakeCommand, FakeResult> msgBehavior = new MessageValidationBehaviour<FakeCommand, FakeResult>(msgValidation: msgValidation);

            // Act
            IMessageResult<FakeResult> rsltValidation = await msgBehavior.Handle(cmdFake, CancellationToken.None, (FakeCommand msgMessage, CancellationToken tknCancellation) => { return msgHandler.Handle(msgMessage, tknCancellation); });

            //Assert
            rsltValidation.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Should().Be(msgValidationError);

                    return false;
                },
                msgResult =>
                {
                    msgResult.Should().BeNull();

                    return true;
                });
        }
    }
}