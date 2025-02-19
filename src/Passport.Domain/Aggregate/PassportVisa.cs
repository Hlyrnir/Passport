using System;

namespace Passport.Domain.Aggregate
{
    public sealed class PassportVisa
    {
        private string sConcurrencyStamp;
        private Guid guId;
        private int iLevel;
        private string sName;

        private PassportVisa(
            string sConcurrencyStamp,
            Guid guId,
            int iLevel,
            string sName)
        {
            this.sConcurrencyStamp = sConcurrencyStamp;
            this.guId = guId;
            this.iLevel = iLevel;
            this.sName = sName;
        }

        public string ConcurrencyStamp { get => sConcurrencyStamp; private set => sConcurrencyStamp = value; }
        public Guid Id { get => guId; private set => guId = value; }
        public int Level { get => iLevel; private set => iLevel = value; }
        public string Name { get => sName; private set => sName = value; }

        public bool TryChangeName(string sName)
        {
            if (string.IsNullOrWhiteSpace(sName) == true)
                return false;

            Span<char> cNormalizedName = sName.ToCharArray();

            foreach (char cActual in cNormalizedName)
            {
                if (char.IsWhiteSpace(cActual) == true)
                    return false;
            }

            this.sName = sName;

            return true;
        }

        public bool TryChangeLevel(int iLevel)
        {
            if (iLevel < 0)
                return false;

            this.iLevel = iLevel;

            return true;
        }

        public static PassportVisa? Initialize(
            string sConcurrencyStamp,
            Guid guId,
            string sName,
            int iLevel)
        {
            PassportVisa ppPassport = new PassportVisa(
                sConcurrencyStamp: sConcurrencyStamp,
                guId: guId,
                sName: sName,
                iLevel: iLevel);

            if (ppPassport.TryChangeName(sName) == false)
                return null;

            if (ppPassport.TryChangeLevel(iLevel) == false)
                return null;

            return ppPassport;
        }

        public static PassportVisa? Create(
            string sName,
            int iLevel)
        {
            return Initialize(
                sConcurrencyStamp: Guid.NewGuid().ToString(),
                guId: Guid.NewGuid(),
                sName: sName,
                iLevel: iLevel);
        }
    }
}
