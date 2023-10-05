using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Apache.NMS;
using Apache.NMS.ActiveMQ;

namespace OpenSense.NMS.ActiveMQ {
    public class Client : INotifyPropertyChanged, IDisposable {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private string host = "localhost";

        public string Host {
            get => host;
            set => SetProperty(ref host, value);
        }

        private int port = 61616;

        public int Port {
            get => port;
            set => SetProperty(ref port, value);
        }

        private string user = null; // same as ActiveMQConnection.DEFAULT_USER in Java

        public string User {
            get => user;
            set => SetProperty(ref user, value);
        }

        private string password = null; // same as ActiveMQConnection.DEFAULT_PASSWORD in Java

        public string Password {
            get => password;
            set => SetProperty(ref password, value);
        }

        private string scope = "DEFAULT_SCOPE";

        public string Scope {
            get => scope;
            set => SetProperty(ref scope, value);
        }

        private bool topic = true;

        public bool Topic {
            get => topic;
            set => SetProperty(ref topic, value);
        }

        private string BrokerUrl => $"tcp://{Host}:{Port}"; // Default value is the same as ConnectionFactory.DEFAULT_BROKER_URL

        private IConnection connection;

        private ISession session;

        private IDestination destination;

        private IMessageProducer producer;

        private bool initialized;

        private bool disposedValue;

        public Client() {

        }

        #region Dispose
        private void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    CloseConnection();
                }
                disposedValue = true;
            }
        }

        ~Client() {
            Dispose(disposing: false);
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        public void OpenConnection() {
            if (initialized) {
                throw new InvalidOperationException("The connection is already opened");
            }
            var connectionFactory = new ConnectionFactory(new Uri(BrokerUrl));
            connection = connectionFactory.CreateConnection(User, Password);//Even with both User and Password set to string.Empty, a NullReferenceException will be thrown.
            connection.Start();
            session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
            if (Topic) {
                destination = session.GetTopic(Scope);
            } else {
                destination = session.GetQueue(Scope);
            }
            producer = session.CreateProducer(destination);
            producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
            initialized = true;
        }

        public void CloseConnection() {
            connection?.Dispose();
            connection = null;
        }

        public void SendMessage(string message, params KeyValuePair<string, string>[] properties) {
            ThrowIfNotInitialized();
            if (message is null && properties.Length == 0) {
                throw new ArgumentException("Send nothing");
            }
            var rawMessage = (global::Apache.NMS.ActiveMQ.Commands.ActiveMQTextMessage)session.CreateTextMessage(message);
            foreach (var property in properties) {
                rawMessage.SetObjectProperty(property.Key, property.Value);
            }
            producer.Send(rawMessage);
        }

        /// <returns>An <see cref="IDisposable""/> object for unsubscribe and clean up.</returns>
        public IDisposable Subscribe(string messageSelector, EventHandler<Message> messageHandler) {
            ThrowIfNotInitialized();
            var consumer = session.CreateConsumer(destination, messageSelector);
            consumer.Listener += m => { 
                var rawMessage = (global::Apache.NMS.ActiveMQ.Commands.ActiveMQTextMessage)m;
                var properties = rawMessage.Properties.Keys
                    .OfType<string>()
                    .Select(k => new KeyValuePair<string, string>(k, rawMessage.Properties[k] as string))
                    .Where(p => p.Value != null)
                    .ToDictionary(p => p.Key, p => p.Value);
                var e = new Message(rawMessage.Text, properties);
                messageHandler(this, e);
            };
            return consumer;
        }

        private void ThrowIfNotInitialized() {
            if (!initialized) {
                throw new InvalidOperationException("No opened connection");
            }
        }
    }
}
