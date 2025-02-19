using System;
using System.Collections.Generic;
using System.Linq;

namespace Passport.Domain.Aggregate
{
    public sealed class Passport
    {
        private string sConcurrencyStamp;
        private DateTimeOffset dtExpiredAt;
        private Guid guHolderId;
        private Guid guId;
        private bool bIsAuthority;
        private bool bIsEnabled;
        private Guid guIssuedBy;
        private DateTimeOffset dtLastCheckedAt;
        private Guid guLastCheckedBy;
        private IList<Guid> lstVisaId;

        private Passport(
            string sConcurrencyStamp,
            DateTimeOffset dtExpiredAt,
            Guid guHolderId,
            Guid guId,
            bool bIsAuthority,
            bool bIsEnabled,
            Guid guIssuedBy,
            DateTimeOffset dtLastCheckedAt,
            Guid guLastCheckedBy,
            IList<Guid> lstVisaId)
        {
            this.sConcurrencyStamp = sConcurrencyStamp;
            this.dtExpiredAt = dtExpiredAt;
            this.guHolderId = guHolderId;
            this.guId = guId;
            this.bIsAuthority = bIsAuthority;
            this.bIsEnabled = bIsEnabled;
            this.guIssuedBy = guIssuedBy;
            this.dtLastCheckedAt = dtLastCheckedAt;
            this.guLastCheckedBy = guLastCheckedBy;
            this.lstVisaId = lstVisaId;
        }

        public string ConcurrencyStamp { get => sConcurrencyStamp; }
        public DateTimeOffset ExpiredAt { get => dtExpiredAt; private set => dtExpiredAt = value; }
        public Guid HolderId { get => guHolderId; }
        public Guid Id { get => guId; }
        public bool IsAuthority { get => bIsAuthority; }
        public bool IsEnabled { get => bIsEnabled; }
        public Guid IssuedBy { get => guIssuedBy; }
        public DateTimeOffset LastCheckedAt { get => dtLastCheckedAt; }
        public Guid LastCheckedBy { get => guLastCheckedBy; }
        public IEnumerable<Guid> VisaId { get => lstVisaId.AsEnumerable(); }

        public bool TryDisable(Passport ppPassport, DateTimeOffset dtDisabledAt)
        {
            if (ppPassport.IsEnabled == false)
                return false;

            if (dtExpiredAt < dtDisabledAt)
                return false;

            dtExpiredAt = dtDisabledAt;
            bIsEnabled = false;
            dtLastCheckedAt = dtDisabledAt;
            guLastCheckedBy = ppPassport.Id;

            return true;
        }

        public bool TryEnable(Passport ppPassport, DateTimeOffset dtEnabledAt)
        {
            if (ppPassport.IsAuthority == false)
                return false;

            if (ppPassport.IsEnabled == false)
                return false;

            if (Equals(ppPassport.Id, this.Id))
                return false;

            bIsEnabled = true;
            dtLastCheckedAt = dtEnabledAt;
            guLastCheckedBy = ppPassport.Id;

            return true;
        }

        public bool TryExtendTerm(DateTimeOffset dtDate, DateTimeOffset dtCheckedAt, Guid guCheckedBy)
        {
            if (DateTimeOffset.Compare(dtDate, dtExpiredAt) < 0)
                return false;

            dtExpiredAt = dtDate;
            dtLastCheckedAt = dtCheckedAt;
            guLastCheckedBy = guCheckedBy;

            return true;
        }

        public bool HasVisa(PassportVisa ppVisa)
        {
            if (ppVisa is null)
                return false;

            return TryFindVisa(ppVisa, out _);
        }

        public bool IsExpired(DateTimeOffset dtDate)
        {
            if (DateTimeOffset.Compare(dtDate, dtExpiredAt) < 0)
                return false;

            return true;
        }

        public bool TryJoinToAuthority(Passport ppPassport, DateTimeOffset dtJointedAt)
        {
            if (ppPassport.IsAuthority == false)
                return false;

            if (ppPassport.IsEnabled == false)
                return false;

            if (this.IsEnabled == false)
                return false;

            if (dtExpiredAt < dtJointedAt)
                return false;

            bIsAuthority = true;
            dtLastCheckedAt = dtJointedAt;
            guLastCheckedBy = ppPassport.Id;

            return true;
        }

        public bool TryReset(Passport ppPassport, DateTimeOffset dtResetAt)
        {
            if (ppPassport.IsAuthority == false)
                return false;

            if (ppPassport.IsEnabled == false)
                return false;

            bIsAuthority = false;
            bIsEnabled = false;
            dtExpiredAt = dtResetAt;
            dtLastCheckedAt = dtResetAt;
            guLastCheckedBy = ppPassport.Id;

            return true;
        }

        public bool TryAddVisa(PassportVisa ppVisa)
        {
            if (ppVisa is null)
                return false;

            if (TryFindVisa(ppVisa, out _) == false)
            {
                lstVisaId.Add(ppVisa.Id);
                return true;
            }

            return false;
        }

        public bool TryRemoveVisa(PassportVisa ppVisa)
        {
            if (ppVisa is null)
                return false;

            if (TryFindVisa(ppVisa, out int iIndex) == true)
            {
                lstVisaId.RemoveAt(iIndex);
                return true;
            }

            return false;
        }

        private bool TryFindVisa(PassportVisa ppVisa, out int iIndex)
        {
            for (iIndex = 0; iIndex < lstVisaId.Count; iIndex++)
            {
                if (Guid.Equals(lstVisaId[iIndex], ppVisa.Id))
                {
                    return true;
                }
            }

            iIndex = (-1);

            return false;
        }

        public static Passport? Initialize(
            string sConcurrencyStamp,
            DateTimeOffset dtExpiredAt,
            Guid guHolderId,
            Guid guId,
            bool bIsAuthority,
            bool bIsEnabled,
            Guid guIssuedBy,
            DateTimeOffset dtLastCheckedAt,
            Guid guLastCheckedBy,
            IList<Guid> lstPassportVisaId)
        {
            if (string.IsNullOrWhiteSpace(sConcurrencyStamp) == true)
                return null;

            if (dtExpiredAt == default)
                return null;

            if (guHolderId == default)
                return null;

            if (guId == default)
                return null;

            if (guIssuedBy == default)
                return null;

            if (dtLastCheckedAt == default)
                return null;

            if (guLastCheckedBy == default)
                return null;

            return new Passport(
                sConcurrencyStamp: sConcurrencyStamp,
                dtExpiredAt: dtExpiredAt,
                guHolderId: guHolderId,
                guId: guId,
                bIsAuthority: bIsAuthority,
                bIsEnabled: bIsEnabled,
                guIssuedBy: guIssuedBy,
                dtLastCheckedAt: dtLastCheckedAt,
                guLastCheckedBy: guLastCheckedBy,
                lstVisaId: lstPassportVisaId);
        }

        public static Passport? Create(
            DateTimeOffset dtExpiredAt,
            Guid guHolderId,
            Guid guIssuedBy,
            DateTimeOffset dtLastCheckedAt)
        {
            return Initialize(
                sConcurrencyStamp: Guid.NewGuid().ToString(),
                dtExpiredAt: dtExpiredAt,
                guHolderId: guHolderId,
                guId: Guid.NewGuid(),
                bIsAuthority: false,
                bIsEnabled: false,
                guIssuedBy: guIssuedBy,
                dtLastCheckedAt: dtLastCheckedAt,
                guLastCheckedBy: guIssuedBy,
                lstPassportVisaId: new List<Guid>());
        }
    }
}