//using Amazon;
//using Amazon.Runtime;
//using Amazon.SQS;
//using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;

using System.Configuration;

namespace SharedLibrary.AzureStorage
{
    public class AzureSQSHelper
    {
        ///////////////////////////////////////////////////////////////////////
        //                           Fields                                  //
        ///////////////////////////////////////////////////////////////////////

        public CloudQueue             queue              { get; set; }   // Azure simple queue service reference
        //public GetQueueUrlResponse    queueurl           { get; set; }   // Azure queue url
        //public ReceiveMessageRequest  rcvMessageRequest  { get; set; }   // Azure receive message request
        //public ReceiveMessageResponse rcvMessageResponse { get; set; }   // Azure receive message response
        //public DeleteMessageRequest   delMessageRequest  { get; set; }   // Azure delete message request

        public bool IsValid                              { get; set; }   // True when the queue is OK
                                                         
        public int ErrorCode                             { get; set; }   // Last error code
        public string ErrorMessage                       { get; set; }   // Last error message

        public const int e_Exception = -1;

        private Object deletionLock  = new Object ();

        ///////////////////////////////////////////////////////////////////////
        //                    Methods & Functions                            //
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Class constructor
        /// </summary>
        public AzureSQSHelper ()
        {
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        public AzureSQSHelper(string queuename, int maxnumberofmessages, String azureSQSConnectionString)
        {
            OpenQueue(queuename, maxnumberofmessages, azureSQSConnectionString);
        }

        /// <summary>
        /// The method clears the error information associated with the queue
        /// </summary>
        private void ClearErrorInfo()
        {
            ErrorCode = 0;
            ErrorMessage = string.Empty;
        }

        /// <summary>
        /// The method opens the queue
        /// </summary>
        public bool OpenQueue(string queuename, int maxnumberofmessages, String azureSQSConnectionString)
        {
            ClearErrorInfo();

            IsValid = false;

            if (!string.IsNullOrWhiteSpace(queuename))
            {
                // Checking for the need to use provided credentials instead of reading from app.Config
                try
                {
                    //String connString = ConfigurationManager.AppSettings["StorageConnectionString"].ToString();
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(azureSQSConnectionString);

                    // Create the queue client
                    CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

                    // Retrieve a reference to a queue
                    queue = queueClient.GetQueueReference(queuename);
                    Console.WriteLine(queue.Name);
                    Console.WriteLine(queue.Uri);
                    Console.WriteLine(queue.ServiceClient.ToString());

                    // Create the queue if it doesn't already exist
                    queue.CreateIfNotExists();

                    IsValid = true;
                }
                catch (Exception ex)
                {
                    ErrorCode = e_Exception;
                    ErrorMessage = ex.Message;
                }
            }

            return IsValid;
        }

        /// <summary>
        /// Returns the approximate number of queued messages
        /// </summary>
        public int ApproximateNumberOfMessages()
        {
            ClearErrorInfo();

            int result = 0;
            try
            {
                // Fetch the queue attributes.
                queue.FetchAttributes();

                // Retrieve the cached approximate message count.
                result = queue.ApproximateMessageCount.Value;

                // Display number of messages.
                Console.WriteLine("Number of messages in queue: " + result);
                
            }
            catch (Exception ex)
            {
                ErrorCode = e_Exception;
                ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// The method loads a one or more messages from the queue
        /// </summary>
        public bool DeQueueMessages()
        {
            ClearErrorInfo();

            bool result = false;
            try
            {
                IEnumerable<CloudQueueMessage> messages = queue.GetMessages(32);
                result = true;
            }
            catch (Exception ex)
            {
                ErrorCode = e_Exception;
                ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// The method deletes the message from the queue
        /// </summary>
        public bool DeleteMessage(CloudQueueMessage message)
        {
            lock (deletionLock)
            {
                ClearErrorInfo();

                bool result = false;
                try
                {
                    queue.DeleteMessage(message);
                    result = true;
                }
                catch (Exception ex)
                {
                    ErrorCode = e_Exception;
                    ErrorMessage = ex.Message;
                }

                return result;
            }
        }

        /// <summary>
        /// Inserts a message in the queue
        /// </summary>
        public bool EnqueueMessage(string msgbody)
        {
            ClearErrorInfo();

            bool result = false;
            try
            {
                CloudQueueMessage message = new CloudQueueMessage(msgbody);
                queue.AddMessage(message);
                result = true;
            }
            catch (Exception ex)
            {
                ErrorCode = e_Exception;
                ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Inserts a message in the queue and retries when an error is detected
        /// </summary>
        public bool EnqueueMessage(string msgbody, int maxretries)
        {
            // Insert domain info into queue
            bool result = false;
            int retrycount = maxretries;
            while (true)
            {
                // Try the insertion
                if (EnqueueMessage(msgbody))
                {
                    result = true;
                    break;
                }

                // Retry
                retrycount--;
                if (retrycount <= 0)
                    break;
                Thread.Sleep (new Random ().Next (500, 2000));
            }

            // Return
            return result;
        }

        public bool AnyMessageReceived ()
        {
            try
            {
                //if (rcvMessageResponse == null)
                //    return false;

                var messageResults = queue.GetMessages(32);
  
                if (messageResults != null && messageResults.FirstOrDefault () != null)
                {
                    return true;
                }
            }
            catch 
            {
                // Nothing to do here                
            }
  
            return false;
        }

        public void ClearQueue()
        {
            do
            {
                // Dequeueing Messages
                if (!DeQueueMessages())
                {
                    // Checking for the need to abort (queue error)
                    if (!String.IsNullOrWhiteSpace (ErrorMessage))
                    {
                        return; // Abort
                    }

                    continue; // Continue in case de dequeue fails, to make sure no message will be kept in the queue
                }

                // Retrieving Message Results
                var resultMessages = queue.GetMessages(32);

                // Checking for no message dequeued
                if (resultMessages.Count() == 0)
                {
                    break; // Breaks loop
                }

                // Iterating over messages of the result to remove it
                foreach (CloudQueueMessage message in resultMessages)
                {
                    // Deleting Message from Queue
                    DeleteMessage(message);
                }

            } while (true);
        }

        public void ClearQueues (List<String> queueNames, String azureSQSConnectionString)
        {
            // Iterating over queues
            foreach (string queueName in queueNames)
            {
                OpenQueue (queueName, 10, azureSQSConnectionString);

                do
                {
                    // Dequeueing Messages
                    if (!DeQueueMessages())
                    {
                        continue; // Continue in case de dequeue fails, to make sure no message will be kept in the queue
                    }

                    // Retrieving Message Results
                    var resultMessages = queue.GetMessages(32);

                    // Checking for no message dequeued
                    if (resultMessages.Count() == 0)
                    {
                        break;
                    }

                    // Iterating over messages of the result to remove it
                    foreach (CloudQueueMessage message in resultMessages)
                    {
                        // Deleting Message from Queue
                        DeleteMessage(message);
                    }

                } while (true);
            }
        }

        public IEnumerable<CloudQueueMessage> GetDequeuedMessages ()
        {
            return queue.GetMessages(32);
        }
    }
}
