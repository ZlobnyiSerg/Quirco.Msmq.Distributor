using System.Configuration;
using Quirco.Msmq.Distributor;
using Topshelf;

namespace Quirco.Msmq.Service
{
    public class MsmqDistributorService : ServiceControl
    {
        private MsmqDistributor _distributor;
        
        public bool Start(HostControl hostControl)
        {
            _distributor = new MsmqDistributor(ConfigurationManager.AppSettings["SourceQueue"], ConfigurationManager.AppSettings["DestQueues"].Split(';'));
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _distributor.Dispose();
            _distributor = null;
            return true;
        }
    }
}