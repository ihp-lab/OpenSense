using System;
using System.Collections.Generic;

namespace OpenSense.NMS.ActiveMQ {
    public class Message : EventArgs {

        public string TextMessage { get; set; }

        public IDictionary<string, string> Properties { get; set; }

        public Message(string textMessage, IDictionary<string, string> properties) {
            TextMessage = textMessage;
            Properties = properties;
        }
    }
}
