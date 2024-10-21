namespace Passport.Api
{
    public static class PassportServiceCollectionExtension
    {
        public static PassportServiceCollectionBuilder AddSqliteDatabase(this PassportServiceCollectionBuilder cltService, string sConnectionStringName)
        {
            //cltService.AddSqliteDatabase(sConnectionStringName: sConnectionStringName);
            Passport.Infrastructure.ServiceCollectionExtension.AddSqliteDatabase(cltService.Services, sConnectionStringName);

            return cltService;
        }
    }
}
