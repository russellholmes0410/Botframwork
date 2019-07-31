﻿using System.Xml.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event.Common
{
    /// <summary>
    /// Scan QR code.
    /// </summary>
    [XmlRoot("xml")]
    public class ScanEvent : RequestEventWithEventKey
    {
        /// <summary>
        /// Gets event, EventType: scan.
        /// </summary>
        /// <value>
        /// EventType: scan.
        /// </value>
        public override string Event
        {
            get { return EventType.Scan; }
        }

        /// <summary>
        /// Gets or sets Ticket.
        /// </summary>
        /// <value>
        /// Use to get QR code picture.
        /// </value>
        [XmlElement(ElementName = "Ticket")]
        public string Ticket { get; set; }
    }
}