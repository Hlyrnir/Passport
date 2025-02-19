using Dapper;
using System;
using System.Data;

namespace Passport.Infrastructure.TypeHandler
{
    internal sealed class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
    {
        public override DateTimeOffset Parse(object oData)
        {
            if (oData is string sOffset)
                return DateTimeOffset.Parse(sOffset);

            return DateTimeOffset.MaxValue;
        }

        public override void SetValue(IDbDataParameter dpParameter, DateTimeOffset dtOffset)
        {
            dpParameter.Value = dtOffset.ToString("O");
            dpParameter.DbType = DbType.String;
        }
    }
}
