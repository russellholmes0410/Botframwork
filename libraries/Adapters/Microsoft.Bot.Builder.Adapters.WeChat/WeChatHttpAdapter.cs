﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response;
using Microsoft.Bot.Builder.Adapters.WeChat.TaskExtensions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    /// <summary>
    /// Represents a adapter that can connect a bot to WeChat endpoint.
    /// </summary>
    public class WeChatHttpAdapter : BotAdapter, IWeChatHttpAdapter
    {
        private readonly IWeChatMessageMapper _wechatMessageMapper;
        private readonly WeChatClient _wechatClient;
        private readonly ILogger _logger;
        private readonly string _appId;
        private readonly string _encodingAESKey;
        private readonly string _token;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IHostedService _backgroundService;

        public WeChatHttpAdapter(
                    IConfiguration configuration,
                    IWeChatMessageMapper wechatMessageMapper = null,
                    WeChatClient wechatClient = null,
                    BotStateSet botStateSet = null,
                    ILogger logger = null,
                    IBackgroundTaskQueue backgroundTaskQueue = null,
                    IHostedService backgroundService = null,
                    Func<ITurnContext, Exception, Task> onTurnError = null)
        {
            var uploadTemporaryMedia = configuration.GetSection("WeChatSetting")?.GetValue<bool>("UploadTemporaryMedia") ?? true;
            var appSecret = configuration.GetSection("WeChatSetting").GetSection("AppSecret")?.Value;

            _appId = configuration.GetSection("WeChatSetting").GetSection("AppId")?.Value;
            _encodingAESKey = configuration.GetSection("WeChatSetting").GetSection("EncodingAESKey")?.Value;
            _token = configuration.GetSection("WeChatSetting").GetSection("Token")?.Value;
            _logger = logger ?? NullLogger.Instance;
            _wechatClient = wechatClient ?? new WeChatClient(_appId, appSecret, logger);
            _wechatMessageMapper = wechatMessageMapper ?? new WeChatMessageMapper(wechatClient, uploadTemporaryMedia, logger);
            _taskQueue = backgroundTaskQueue ?? BackgroundTaskQueue.Instance;
            _backgroundService = backgroundService;

            if (onTurnError == null)
            {
                OnTurnError = async (context, exception) =>
                {
                    await context.SendActivityAsync(Constants.DefaultErrorMessage);
                };
            }

            if (botStateSet != null)
            {
                Use(new AutoSaveStateMiddleware(botStateSet));
            }
        }

        /// <summary>
        /// Process the request from WeChat.
        /// </summary>
        /// <param name="wechatRequest">Request message entity from wechat.</param>
        /// <param name="callback"> Bot callback handler.</param>
        /// <param name="secretInfo">Secret info for verify the request.</param>
        /// <param name="passiveResponse">Marked the message whether it needs passive reply or not. </param>
        /// <param name="cancellationToken">Cancellation Token of this Task.</param>
        /// <returns>Response message entity.</returns>
        public async Task<object> ProcessWeChatRequest(IRequestMessageBase wechatRequest, BotCallbackHandler callback, SecretInfo secretInfo, bool passiveResponse, CancellationToken cancellationToken)
        {
            var activity = await _wechatMessageMapper.ToConnectorMessage(wechatRequest);
            BotAssert.ActivityNotNull(activity);
            using (var context = new TurnContext(this, activity as Activity))
            {
                try
                {
                    var responses = new Dictionary<string, List<Activity>>();
                    context.TurnState.Add(Constants.TurnResponseKey, responses);
                    await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);
                    var key = $"{activity.Conversation.Id}:{activity.Id}";
                    try
                    {
                        var activities = responses.ContainsKey(key) ? responses[key] : new List<Activity>();
                        var response = await ProcessBotResponse(activities, secretInfo, wechatRequest.FromUserName, passiveResponse);
                        return response;
                    }
                    catch (Exception e)
                    {
                        // TODO: exception handling when send message to wechat api failed.
                        _logger.LogError(e, "Failed to process bot response");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    // exception handing when bot throw an exception.
                    await OnTurnError(context, ex);
                    return null;
                }
            }
        }

        // Does not support by wechat.
        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // Does not support by wechat.
        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            var resourceResponses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {
                switch (activity.Type)
                {
                    case ActivityTypes.Message:
                    case ActivityTypes.EndOfConversation:
                        var conversation = activity.Conversation ?? new ConversationAccount();
                        var key = $"{conversation.Id}:{activity.ReplyToId}";
                        var responses = turnContext.TurnState.Get<Dictionary<string, List<Activity>>>(Constants.TurnResponseKey);
                        if (responses.ContainsKey(key))
                        {
                            responses[key].Add(activity);
                        }
                        else
                        {
                            responses[key] = new List<Activity> { activity };
                        }

                        break;
                    default:
                        _logger.LogInformation(
                            $"WeChatAdapter.SendActivities(): Activities of type '{activity.Type}' aren't supported.");
                        break;
                }

                resourceResponses.Add(new ResourceResponse(activity.Id));
            }

            return Task.FromResult(resourceResponses.ToArray());
        }

        /// <summary>
        /// Process the request from WeChat.
        /// </summary>
        /// <param name="httpRequest">The request sent from WeChat.</param>
        /// <param name="httpResponse">Http response object of current request.</param>
        /// <param name="bot">The bot instance.</param>
        /// <param name="secretInfo">Secret info for verify the request.</param>
        /// <param name="passiveResponse">If using passvice response mode, if set to true, user can only get one reply.</param>
        /// <param name="cancellationToken">Cancellation Token of this Task.</param>
        /// <returns>Task running result.</returns>
        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, SecretInfo secretInfo, bool passiveResponse, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogInformation("Receive a new request from WeChat.");
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            if (secretInfo == null)
            {
                throw new ArgumentNullException(nameof(secretInfo));
            }

            VerificationHelper.Check(secretInfo.Signature, secretInfo.Timestamp, secretInfo.Nonce, _token);

            secretInfo.Token = _token;
            secretInfo.EncodingAESKey = _encodingAESKey;
            secretInfo.AppId = _appId;
            var postDataDocument = XmlHelper.Convert(httpRequest.Body);
            var wechatRequest = GetRequestMessage(postDataDocument, secretInfo);

            try
            {
                // Reply WeChat(User) request have two ways, set response in http response or use background task to process the request async.
                if (!passiveResponse)
                {
                    // Running a background task, Get bot response and parse it from activity to wechat response message
                    if (_backgroundService == null)
                    {
                        throw new ArgumentNullException("AdapterBackgroundService can not be null.");
                    }

                    _taskQueue.QueueBackgroundWorkItem(async (ct) =>
                    {
                        await ProcessWeChatRequest(
                                        wechatRequest,
                                        bot.OnTurnAsync,
                                        secretInfo,
                                        passiveResponse,
                                        ct).ConfigureAwait(false);
                    });
                }
                else
                {
                    var wechatResponse = await ProcessWeChatRequest(
                                wechatRequest,
                                bot.OnTurnAsync,
                                secretInfo,
                                passiveResponse,
                                cancellationToken).ConfigureAwait(false);
                    httpResponse.StatusCode = (int)HttpStatusCode.OK;
                    httpResponse.ContentType = "text/xml";

                    var xmlString = WeChatMessageFactory.ConvertResponseToXml(wechatResponse);
                    var requestBytes = Encoding.UTF8.GetBytes(xmlString);
                    httpResponse.Body.Write(requestBytes, 0, requestBytes.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Process wechat request failed.");
            }
        }

        /// <summary>
        /// Parse the XDocument to RequestMessage, decrypt it if needed.
        /// </summary>
        /// <param name="postDataDocument">XDocument from WeChat Request.</param>
        /// <param name="secretInfo">The secretInfo used to decrypt the message.</param>
        /// <returns>Decrypted WeChat RequestMessage instance.</returns>
        public IRequestMessageBase GetRequestMessage(XDocument postDataDocument, SecretInfo secretInfo)
        {
            // decrypt xml document message and parse to message
            var postDataStr = postDataDocument.ToString();
            var decryptDoc = postDataDocument;

            if (secretInfo != null
                && !string.IsNullOrWhiteSpace(secretInfo.Token)
                && postDataDocument.Root.Element("Encrypt") != null
                && !string.IsNullOrEmpty(postDataDocument.Root.Element("Encrypt").Value))
            {
                var msgCrype = new MessageCryptography(secretInfo);
                var msgXml = msgCrype.DecryptMessage(postDataStr);

                decryptDoc = XDocument.Parse(msgXml);
            }

            var requestMessage = WeChatMessageFactory.GetRequestEntity(decryptDoc, _logger);

            return requestMessage;
        }

        /// <summary>
        /// Get the respone from bot for the wechat request.
        /// </summary>
        /// <param name="activities">List of bot activities.</param>
        /// <param name="secretInfo">Secret info for verify the request.</param>
        /// <param name="openId">User's open id from WeChat.</param>
        /// <param name="passiveResponse">If using passvice response mode, if set to true, user can only get one reply.</param>
        /// <returns>Bot response message.</returns>
        private async Task<object> ProcessBotResponse(List<Activity> activities, SecretInfo secretInfo, string openId, bool passiveResponse = false)
        {
            object response = null;
            foreach (var activity in activities)
            {
                if (activity != null && activity.Type == ActivityTypes.Message)
                {
                    if (activity.ChannelData != null)
                    {
                        if (passiveResponse)
                        {
                            response = activity.ChannelData;
                        }
                        else
                        {
                            await SendMessageToWeChat(activity.ChannelData).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        var resposneList = await _wechatMessageMapper.ToWeChatMessages(activity, secretInfo).ConfigureAwait(false);

                        // Passive Response can only response one message per turn, retrun the last acitvity as the response.
                        if (passiveResponse)
                        {
                            response = resposneList.LastOrDefault();
                        }
                        else
                        {
                            await SendMessageToWechat(resposneList, openId).ConfigureAwait(false);
                        }
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Send raw channel data to WeChat.
        /// </summary>
        /// <param name="channelData">Raw channel data.</param>
        /// <returns>Task running result.</returns>
        private async Task SendMessageToWeChat(object channelData)
        {
            try
            {
                await _wechatClient.SendMessageToUser(channelData).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Send channelData to wechat failed");
            }
        }

        /// <summary>
        /// Send response based on message type.
        /// </summary>
        /// <param name="responseList">Response message list.</param>
        /// <param name="openId">User's open id from WeChat.</param>
        /// <returns>Task running result.</returns>
        private async Task SendMessageToWechat(IList<IResponseMessageBase> responseList, string openId)
        {
            foreach (var response in responseList)
            {
                try
                {
                    switch (response.MsgType)
                    {
                        case ResponseMessageType.Text:
                            var textResponse = response as TextResponse;
                            await _wechatClient.SendTextAsync(openId, textResponse.Content);
                            break;

                        case ResponseMessageType.Image:
                            var imageResposne = response as ImageResponse;
                            await _wechatClient.SendImageAsync(openId, imageResposne.Image.MediaId);
                            break;

                        case ResponseMessageType.News:
                            var newsResponse = response as NewsResponse;
                            await _wechatClient.SendNewsAsync(openId, newsResponse.Articles);
                            break;

                        case ResponseMessageType.Music:
                            var musicResponse = response as MusicResponse;
                            var music = musicResponse.Music;
                            await _wechatClient.SendMusicAsync(openId, music.Title, music.Description, music.MusicUrl, music.HQMusicUrl, music.ThumbMediaId);
                            break;

                        case ResponseMessageType.MPNews:
                            var mpnewsResponse = response as MPNewsResponse;
                            await _wechatClient.SendMPNewsAsync(openId, mpnewsResponse.MediaId);
                            break;

                        case ResponseMessageType.Video:
                            var videoResposne = response as VideoResponse;
                            var video = videoResposne.Video;
                            await _wechatClient.SendVideoAsync(openId, video.MediaId, video.Title, video.Description);
                            break;

                        case ResponseMessageType.Voice:
                            var voiceResponse = response as VoiceResponse;
                            var voice = voiceResponse.Voice;
                            await _wechatClient.SendVoiceAsync(openId, voice.MediaId);
                            break;
                        case ResponseMessageType.LocationMessage:
                            var locationResponse = response as ResponseMessage;

                            // Currently not supported by wechat api.
                            // TODO: find another way to send location message. perhaps using map service.
                            break;
                        case ResponseMessageType.SuccessResponse:
                        case ResponseMessageType.Unknown:
                        case ResponseMessageType.NoResponse:
                        default:
                            _logger.LogInformation("Get an unsupported messaged.");
                            break;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Send response to wechat failed");
                }
            }
        }
    }
}