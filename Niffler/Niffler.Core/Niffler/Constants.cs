using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 

namespace Niffler
{
    class Constants
    {
         
         // public static string ConnectionString = "data source=AFB\\sqlexpress;initial catalog=Botty;integrated security=True;";
        public static string DatabaseConnectionString = "Server=tcp:niffler.database.windows.net,1433;Initial Catalog=Niffler;Persist Security Info=False;User ID=niffler@niffler;Password=FoolsG0ld!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        public static string ServiceBusConnectionString = "DefaultEndpointsProtocol=https;AccountName=niffler;AccountKey=GebWl8fp38g1runleQO4lq+T9+UGTWuLzrE/Kfw4a7ldc+zgoPt/+FWEblnZ1kM0KFzo+vYOhkrwN0aENQy2JA==;EndpointSuffix=core.windows.net";

    }
}
