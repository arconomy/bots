using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Common
{
    class Notifications
    {
         protected void ToSlack(string Message, string Channel)
        {

            if (Channel == "")
            {
                Channel = "daily-levels";
            }
            System.Net.WebClient WC = new System.Net.WebClient();
            string Webhook = "https://hooks.slack.com/services/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";
            string Response = WC.UploadString(Webhook, "{\"text\": \"" + Message + "\", \"channel\": \"#" + Channel + "\"}");

        }
    }
}
