using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using OpenSense.NMS.ActiveMQ;

namespace OpenSense.VHMessage {
    /// <summary>
    /// Mimic the implemention of VHMsg.cs
    /// </summary>
    public class VirtualHumanMessageClient : IDisposable {

        private Client mqClient;
        private List<IDisposable> subscriptions = new List<IDisposable>();

        public VirtualHumanMessageClient() {
            mqClient = new Client() {
                //TODO: MQ settings goes here
            };
        }

        public void OpenConnection() {
            mqClient.OpenConnection();
        }

        /// <remarks>
        /// A method for unsubscribe is not provided, since it is not needed in this project
        /// </remarks>
        public void Subscribe(EventHandler<Message> messageHandler) {
            void handler(object? sender, Message message) {
                var parts = message.TextMessage.Split(new[] { ' ' }, 2);
                Trace.Assert(parts.Length == 2);//If no arg, the last element will be empty string
                var op = parts.First();
                var arg = parts.Last();
                var argDecoded = HttpUtility.UrlDecode(arg, Encoding.UTF8);
                var propDecoded = message.Properties
                    .ToDictionary(p => p.Key, p => HttpUtility.UrlDecode(p.Value, Encoding.UTF8));
                var decoded = new Message($"{op} {argDecoded}", propDecoded);
                messageHandler(sender, decoded);
            }
            var subscription = mqClient.Subscribe($"ELVISH_SCOPE = '{mqClient.Scope}' AND MESSAGE_PREFIX LIKE '%'", handler);//subscribe all
            subscriptions.Add(subscription);
        }

        public void SendMessage(string message) {
            Trace.Assert(message != null);
            var parts = message!.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            var op = parts.First().Trim();
            var arg = parts.ElementAtOrDefault(1)?.Trim();
            var argEncoded = arg is null ? null : HttpUtility.UrlEncode(arg, Encoding.UTF8);
            var properties = new Dictionary<string, string>() {
                { "ELVISH_SCOPE", HttpUtility.UrlEncode(mqClient.Scope, Encoding.UTF8) },
                { "MESSAGE_PREFIX", HttpUtility.UrlEncode(op, Encoding.UTF8) },
            };
            var newMessage = $"{op} {argEncoded}";//if arg is null, then there is always a trailing space
            mqClient.SendMessage(newMessage, properties.ToArray());
        }

        #region IDisposable
        private bool disposedValue;

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    subscriptions.ForEach(s => s.Dispose());
                    mqClient.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
