using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Core.Config
{
    public class BrokerConfiguration
    {
        public string BrokerId { get; }
        public string BrokerName { get; }
        public int PipSize { get; }

        public BrokerConfiguration(String BrokerId)
        {
            this.BrokerId = BrokerId;

            switch (BrokerId)
            {
                case "01":
                    BrokerName = "Pepperstone";
                    PipSize = 1;
                    break;
                case "02":
                    BrokerName = "IC Markets";
                    PipSize = 10;
                    break;
            };
        }
    }
}
