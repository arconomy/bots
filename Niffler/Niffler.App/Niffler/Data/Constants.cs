namespace Niffler.Data
{
    class Constants
    {

        // public static string ConnectionString = "data source=AFB\\sqlexpress;initial catalog=Botty;integrated security=True;";
        public static string DatabaseConnectionString = "Server=tcp:niffler.database.windows.net,1433;Initial Catalog=Niffler;Persist Security Info=False;User ID=niffler@niffler;Password=FoolsG0ld!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        public static string ServiceBusConnectionString = "Endpoint=sb://niffler.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=hZRIFvBXA2xLf+aV3DM3eKg11Fkq5GP7aB3xPg44MMk=";

    }
}
