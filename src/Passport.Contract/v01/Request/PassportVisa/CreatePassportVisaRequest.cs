namespace Passport.Contract.v01.Request.PassportVisa
{
    public sealed class CreatePassportVisaRequest
    {
        public required string Name { get; init; }
        public required int Level { get; init; }
    }
}
