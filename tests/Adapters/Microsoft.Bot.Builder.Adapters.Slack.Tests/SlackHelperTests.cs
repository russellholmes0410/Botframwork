﻿// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Slack.Tests
{
    public class SlackHelperTests
    {
        public const string ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU";

        [Fact]
        public void ActivityToSlackShouldReturnNullWithNullActivity()
        {
            Assert.Null(SlackHelper.ActivityToSlack(null));
        }

        [Fact]
        public void ActivityToSlackShouldReturnMessage()
        {
            var activity = new Activity
            {
                Timestamp = new DateTimeOffset(),
                Text = "Hello!",
                Attachments = new List<Attachment>
                {
                    new Attachment(name: "image", thumbnailUrl: ImageUrl),
                },
                Conversation = new ConversationAccount(id: "testId"),
            };

            var message = SlackHelper.ActivityToSlack(activity);

            Assert.Equal(activity.Conversation.Id, message.channel);
            Assert.Equal(activity.Attachments[0].Name, message.attachments[0].author_name);
        }

        [Fact]
        public void ActivityToSlackShouldReturnMessageFromChannelData()
        {
            var messageText = "Hello from message";

            var activity = new Activity
            {
                Timestamp = new DateTimeOffset(),
                Text = "Hello!",
                Recipient = new ChannelAccount("testRecipientId"),
                ChannelData = new NewSlackMessage
                {
                    text = messageText,
                    Ephemeral = "testEphimeral",
                    IconUrl = new Uri(ImageUrl),
                },
                Conversation = new ConversationAccount(id: "testId"),
            };

            var message = SlackHelper.ActivityToSlack(activity);

            Assert.Equal(messageText, message.text);
            Assert.False(message.AsUser);
        }

        [Fact]
        public void ActivityToSlackShouldReturnMessageWithThreadTS()
        {
            var serializeConversation = "{\"id\":\"testId\",\"thread_ts\":\"0001-01-01T00:00:00+00:00\"}";

            var activity = new Activity
            {
                Timestamp = new DateTimeOffset(),
                Text = "Hello!",
                Conversation = JsonConvert.DeserializeObject<ConversationAccount>(serializeConversation),
            };

            var message = SlackHelper.ActivityToSlack(activity);

            Assert.Equal(activity.Conversation.Id, message.channel);
            Assert.Equal(activity.Conversation.Properties["thread_ts"], message.thread_ts);
        }

        [Fact]
        public void GetMessageFromSlackEventShouldReturnNull()
        {
            Assert.Null(SlackHelper.GetMessageFromSlackEvent(null));
        }

        [Fact]
        public void GetMessageFromSlackEventShouldReturnMessage()
        {
            var json = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\MessageBody.json");
            dynamic slackEvent = JsonConvert.DeserializeObject(json);

            var message = SlackHelper.GetMessageFromSlackEvent(slackEvent);

            Assert.Equal(slackEvent["event"].text.Value, message.text);
            Assert.Equal(slackEvent["event"].user.Value, message.user);
        }
    }
}