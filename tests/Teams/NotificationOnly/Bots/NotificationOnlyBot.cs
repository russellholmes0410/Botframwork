﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class NotificationOnlyBot : TeamsActivityHandler
    {
        protected override async Task OnTeamsMembersAddedAsync(IList<ChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                var replyActivity = MessageFactory.Text($"{member.Id} was added to the team");
                replyActivity.ApplyConversationReference(turnContext.Activity.GetConversationReference());

                var channelId = turnContext.Activity.Conversation.Id.Split(";")[0];
                replyActivity.Conversation.Id = channelId;
                var resourceResponse = await turnContext.SendActivityAsync(replyActivity, cancellationToken);
            }
        }

        protected override async Task OnTeamsMembersRemovedAsync(IList<ChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                var replyActivity = MessageFactory.Text($"{member.Id} was removed to the team");
                replyActivity.ApplyConversationReference(turnContext.Activity.GetConversationReference());

                var channelId = turnContext.Activity.Conversation.Id.Split(";")[0];
                replyActivity.Conversation.Id = channelId;
                var resourceResponse = await turnContext.SendActivityAsync(replyActivity, cancellationToken);
            }
        }
    }
}