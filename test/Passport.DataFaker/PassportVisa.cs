namespace Passport.DataFaker
{
    public static class PassportVisa
    {
        public static Domain.Aggregate.PassportVisa CreateDefault()
        {
            Domain.Aggregate.PassportVisa? ppVisa = Domain.Aggregate.PassportVisa.Create(
                sName: Guid.NewGuid().ToString(),
                iLevel: 0);

            if (ppVisa is null)
                throw new NullReferenceException();

            return ppVisa;
        }

        public static Domain.Aggregate.PassportVisa CreateDefault(string sName, int iLevel)
        {
            Domain.Aggregate.PassportVisa? ppVisa = Domain.Aggregate.PassportVisa.Create(
                sName: sName,
                iLevel: iLevel);

            if (ppVisa is null)
                throw new NullReferenceException();

            return ppVisa;
        }
    }
}