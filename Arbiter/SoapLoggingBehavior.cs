using System;
using System.IO;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;

namespace RCCArbiter
{
    internal sealed class SoapLoggingBehavior : IEndpointBehavior
    {
        public SoapLoggingBehavior() { }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            // In some target frameworks, the property is ClientMessageInspectors.
            // Try to use it; if unavailable, fall back to MessageInspectors via dynamic.
            clientRuntime.ClientMessageInspectors.Add(new SoapMessageInspector());
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

        public void Validate(ServiceEndpoint endpoint) { }

        private sealed class SoapMessageInspector : IClientMessageInspector
        {
            private readonly object sync = new object();

            public SoapMessageInspector() { }

            public void AfterReceiveReply(ref Message reply, object correlationState)
            {
                try
                {
                    var buffer = reply.CreateBufferedCopy(int.MaxValue);
                    var copy = buffer.CreateMessage();
                    reply = buffer.CreateMessage();

                    var xml = MessageToString(copy);
                    // correlationState carries the log file path
                    if (correlationState is string path && !string.IsNullOrEmpty(path))
                    {
                        AppendSection(path, "RESPONSE", xml);
                    }
                }
                catch { /* best-effort logging */ }
            }

            public object BeforeSendRequest(ref Message request, System.ServiceModel.IClientChannel channel)
            {
                try
                {
                    var buffer = request.CreateBufferedCopy(int.MaxValue);
                    var copy = buffer.CreateMessage();
                    request = buffer.CreateMessage();

                    var xml = MessageToString(copy);
                    var path = CreateNewLogFilePath();
                    AppendSection(path, "REQUEST", xml);
                    return path; // pass to AfterReceiveReply
                }
                catch { /* best-effort logging */ }

                return null!;
            }

            private string MessageToString(Message message)
            {
                using var sw = new StringWriter();
                using var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8, OmitXmlDeclaration = false });
                message.WriteMessage(xw);
                xw.Flush();
                return sw.ToString();
            }

            private void AppendSection(string filePath, string direction, string xml)
            {
                var ts = DateTimeOffset.Now.ToString("o");
                var header = $"===== SOAP {direction} {ts} =====";
                try
                {
                    lock (sync)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                        File.AppendAllText(filePath, header + Environment.NewLine + xml + Environment.NewLine);
                    }
                }
                catch { /* ignore file IO errors */ }
            }

            private string CreateNewLogFilePath()
            {
                var baseDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
                var name = $"rcc_soap_{stamp}_{Guid.NewGuid():N}.log";
                return Path.Combine(baseDir, name);
            }
        }
    }
}
