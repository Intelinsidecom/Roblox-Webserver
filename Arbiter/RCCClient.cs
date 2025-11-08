using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter
{
    public class RCCClient : IDisposable
    {
        private readonly ChannelFactory<IRCCServiceSoap> _channelFactory;
        private IRCCServiceSoap? _channel;
        private readonly string _serviceUrl;

        public RCCClient(string serviceUrl)
        {
            _serviceUrl = serviceUrl;
            
            var binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 2147483647,
                MaxBufferSize = 2147483647,
                SendTimeout = TimeSpan.FromMinutes(10),
                ReceiveTimeout = TimeSpan.FromMinutes(10),
                OpenTimeout = TimeSpan.FromMinutes(1),
                CloseTimeout = TimeSpan.FromMinutes(1)
            };

            var endpoint = new EndpointAddress(serviceUrl);
            _channelFactory = new ChannelFactory<IRCCServiceSoap>(binding, endpoint);

            // Optional SOAP logging controlled by appsettings.json -> RCCService:SoapLogging
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .Build();

                var enabled = string.Equals(
                    config["RCCService:SoapLogging:Enabled"],
                    "true",
                    StringComparison.OrdinalIgnoreCase);

                if (enabled)
                {
                    _channelFactory.Endpoint.EndpointBehaviors.Add(new SoapLoggingBehavior());
                }
            }
            catch
            {
                // best-effort: ignore config/logging errors
            }
        }

        private IRCCServiceSoap Channel
        {
            get
            {
                if (_channel == null)
                {
                    _channel = _channelFactory.CreateChannel();
                }
                return _channel;
            }
        }

        public string HelloWorld()
        {
            return Channel.HelloWorld();
        }

        public string GetVersion()
        {
            return Channel.GetVersion();
        }

        public Status GetStatus()
        {
            return Channel.GetStatus();
        }

        public LuaValue[] BatchJob(Job job, ScriptExecution script)
        {
            return Channel.BatchJob(job, script);
        }

        public LuaValue[] BatchJobEx(Job job, ScriptExecution script)
        {
            return Channel.BatchJobEx(job, script);
        }

        public LuaValue[] OpenJob(Job job, ScriptExecution script)
        {
            return Channel.OpenJob(job, script);
        }

        public LuaValue[] OpenJobEx(Job job, ScriptExecution script)
        {
            return Channel.OpenJobEx(job, script);
        }

        public LuaValue[] Execute(string jobID, ScriptExecution script)
        {
            return Channel.Execute(jobID, script);
        }

        public LuaValue[] ExecuteEx(string jobID, ScriptExecution script)
        {
            return Channel.ExecuteEx(jobID, script);
        }

        public double RenewLease(string jobID, double expirationInSeconds)
        {
            return Channel.RenewLease(jobID, expirationInSeconds);
        }

        public void CloseJob(string jobID)
        {
            Channel.CloseJob(jobID);
        }

        public double GetExpiration(string jobID)
        {
            return Channel.GetExpiration(jobID);
        }

        public Job[] GetAllJobs()
        {
            return Channel.GetAllJobs();
        }

        public Job[] GetAllJobsEx()
        {
            return Channel.GetAllJobsEx();
        }

        public int CloseExpiredJobs()
        {
            return Channel.CloseExpiredJobs();
        }

        public int CloseAllJobs()
        {
            return Channel.CloseAllJobs();
        }

        public LuaValue[] Diag(int type, string jobID)
        {
            return Channel.Diag(type, jobID);
        }

        public LuaValue[] DiagEx(int type, string jobID)
        {
            return Channel.DiagEx(type, jobID);
        }

        public void Dispose()
        {
            if (_channel is IClientChannel clientChannel)
            {
                try
                {
                    if (clientChannel.State == CommunicationState.Faulted)
                    {
                        clientChannel.Abort();
                    }
                    else
                    {
                        clientChannel.Close();
                    }
                }
                catch
                {
                    clientChannel.Abort();
                }
            }

            _channelFactory?.Close();
        }
    }
}
