﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilderSamples.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;

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

        protected override async Task OnTeamsMembersRemovedAsync(IList<ChannelAccount> membersRemoved, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersRemoved)
            {
                var replyActivity = MessageFactory.Text($"{member.Id} was removed to the team");
                replyActivity.ApplyConversationReference(turnContext.Activity.GetConversationReference());

                var channelId = turnContext.Activity.Conversation.Id.Split(";")[0];
                replyActivity.Conversation.Id = channelId;
                var resourceResponse = await turnContext.SendActivityAsync(replyActivity, cancellationToken);
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
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

        protected override async Task OnMembersRemovedAsync(IList<ChannelAccount> membersRemoved, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersRemoved)
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
