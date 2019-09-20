using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace Microsoft.Bot.Builder.Teams
{
    public class TeamsHelpers
    {
        public T NotifyUser<T>(T replyActivity)
            where T : IMessageActivity
        {
            TeamsChannelData channelData = replyActivity.GetChannelData<TeamsChannelData>() == null ?
                new TeamsChannelData() :
                replyActivity.GetChannelData<TeamsChannelData>();
            channelData.Notification = new NotificationInfo
            {
                Alert = true,
            };
            replyActivity.ChannelData = channelData;

            return replyActivity;
        }

        public ChannelInfo GetGeneralChannel(T replyActivity)
            where T : IMessageActivity
        {
            if (this.turnContext.Activity.ChannelData != null)
            {
                TeamsChannelData channelData = this.turnContext.Activity.GetChannelData<TeamsChannelData>();

                if (channelData != null && channelData.Team != null)
                {
                    return new ChannelInfo
                    {
                        Id = channelData.Team.Id,
                    };
                }

                throw new ArgumentException("Failed to process channel data in Activity. ChannelData is missing Team property.");
            }
            else
            {
                throw new ArgumentException("ChannelData missing in Activity");
            }
        }
    }
}
