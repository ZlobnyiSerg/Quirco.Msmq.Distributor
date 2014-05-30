using System;
using System.Collections.Generic;
using System.Messaging;
using System.Threading;
using System.Transactions;
using Common.Logging;

namespace Quirco.Msmq.Distributor
{
    public class MsmqDistributor : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger<MsmqDistributor>();

        private readonly string[] _destinationQueuesPath;
        private readonly MessageQueue _sourceQueue;
        private List<MessageQueue> _destQueues;

        private readonly Thread _job;

        public MsmqDistributor(string sourceQueuePath, params string[] destinationQueuesPath)
        {
            _destinationQueuesPath = destinationQueuesPath;
            _sourceQueue = new MessageQueue(sourceQueuePath, QueueAccessMode.Receive);                        
            _job = new Thread(Process);
            _job.Start();
            Log.Info("Distributor is started");
        }

        public void Process()
        {
            EnsureDestinationQueuesInitialized();
            Message msg;
            while ((msg = _sourceQueue.Receive()) != null)
            {
                Log.InfoFormat("New message {0} received, distributing to {1} queue(s)", msg.Id, _destQueues.Count);
                try
                {
                    TransactionScope scope = null;
                    if (_sourceQueue.Transactional)
                    {
                        scope = new TransactionScope(TransactionScopeOption.Required);
                    }                    
                    msg.Formatter = new BinaryMessageFormatter();
                    foreach (var queue in _destQueues)
                    {
                        queue.Send(msg, MessageQueueTransactionType.Automatic);
                    }
                    if (scope != null)
                    {
                        scope.Complete();
                        scope.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
        
        private void EnsureDestinationQueuesInitialized()
        {
            if (_destQueues == null)
            {
                _destQueues = new List<MessageQueue>(_destinationQueuesPath.Length);
                foreach (var targetQueuePath in _destinationQueuesPath)
                {
                    _destQueues.Add(new MessageQueue(targetQueuePath, QueueAccessMode.Send));
                }
            }
        }

        public void Dispose()
        {
            if (_job != null)
                _job.Abort();
            _sourceQueue.Dispose();
            if (_destQueues != null)
            {
                foreach (var queue in _destQueues)
                {
                    queue.Dispose();
                }
            }
        }
    }
}