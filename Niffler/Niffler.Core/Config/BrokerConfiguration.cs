using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Core.Config
{
    class BrokerConfiguration
    {
        private string BrokerId;
        private string BrokerName;
        private int PipSize;

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
