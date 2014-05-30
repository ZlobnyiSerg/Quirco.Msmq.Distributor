using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Quirco.Msmq.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.RunAsLocalSystem();
                x.StartAutomatically();
                x.SetDescription("Quirco MSMQ Distributor service");
                x.SetDisplayName("Quirco MSMQ Distributor");
                x.SetServiceName("Quirco.MSMQ.Disctributor");
                x.Service<MsmqDistributorService>();
            });
        }
    }
}
