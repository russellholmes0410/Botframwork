﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.StreamingExtensions.Transport
{
    /// <summary>
    /// Implemented by servers compatible with the Bot Framework Protocol 3 with Streaming Extensions.
    /// </summary>
    public interface IStreamingTransportServer
    {
        /// <summary>
        /// An event used to signal when the underlying connection has disconnected.
        /// </summary>
        event DisconnectedEventHandler Disconnected;

        /// <summary>
        /// Gets or sets the Id of this transport server.
        /// </summary>
        /// <value>
        /// The ID of this transport server, useful for making sure responses are sent over the correct transport.
        /// </value>
        Guid Id { get; set; }

        /// <summary>
        /// Gets the base URL of the RemoteHost this server is connected to.
        /// </summary>
        /// <value>
        /// The base URL of the RemoteHost this server is connected to.
        /// </value>
        string RemoteHost { get; }

        /// <summary>
        /// Used to establish the connection used by this server and begin listening for incoming messages.
        /// </summary>
        /// <returns>A <see cref="Task"/> to handle the server listen operation.</returns>
        Task StartAsync();

        /// <summary>
        /// Task used to send data over this server connection.
        /// </summary>
        /// <param name="request">The <see cref="StreamingRequest"/> to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to signal this operation should be cancelled.</param>
        /// <returns>A <see cref="Task"/> of type <see cref="ReceiveResponse"/> handling the send operation.</returns>
        Task<ReceiveResponse> SendAsync(StreamingRequest request, CancellationToken cancellationToken = default(CancellationToken));
    }
}
