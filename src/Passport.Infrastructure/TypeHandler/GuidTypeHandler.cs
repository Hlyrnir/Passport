using Dapper;
using System;
using System.Data;

namespace Passport.Infrastructure.TypeHandler
{
    internal sealed class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object oData)
        {
            if (oData is string sId)
                return Guid.Parse(sId);

            return Guid.Empty;
        }

        public override void SetValue(IDbDataParameter dpParameter, Guid guId)
        {
            dpParameter.Value = guId.ToString();
            dpParameter.DbType = DbType.String;
        }
    }
}
