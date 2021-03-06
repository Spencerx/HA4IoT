﻿using System;
using System.IO;
using System.Text;
using Windows.Web.Http;
using HA4IoT.Api.Configuration;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Net.Http;
using HA4IoT.Net.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Api
{
    public class HttpServerService : ServiceBase, IApiAdapter
    {
        private readonly HttpServer _httpServer;
        private readonly ILogger _log;

        private const string ApiBaseUri = "/api/";
        private const string AppBaseUri = "/app/";
        private const string ManagementAppBaseUri = "/managementApp/";

        public HttpServerService(IConfigurationService configurationService, IApiDispatcherService apiDispatcherService, ILogService logService)
        {
            if (configurationService == null) throw new ArgumentNullException(nameof(configurationService));
            if (apiDispatcherService == null) throw new ArgumentNullException(nameof(apiDispatcherService));
            _log = logService.CreatePublisher(nameof(HttpServerService)) ?? throw new ArgumentNullException(nameof(logService));

            _httpServer = new HttpServer(logService);

            _httpServer.HttpRequestReceived += OnHttpRequestReceived;
            _httpServer.WebSocketConnected += AttachWebSocket;

            var configuration = configurationService.GetConfiguration<HttpServerServiceConfiguration>("HttpServerService");
            _httpServer.BindAsync(configuration.Port).GetAwaiter().GetResult();

            apiDispatcherService.RegisterAdapter(this);
        }

        public event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;

        public void NotifyStateChanged(IComponent component)
        {
        }

        private void OnHttpRequestReceived(object sender, HttpRequestReceivedEventArgs e)
        {
            if (e.Context.Request.Uri.StartsWith(ApiBaseUri, StringComparison.OrdinalIgnoreCase))
            {
                e.IsHandled = true;
                OnApiRequestReceived(e.Context);
            }
            else if (e.Context.Request.Uri.StartsWith(AppBaseUri, StringComparison.OrdinalIgnoreCase))
            {
                e.IsHandled = true;
                OnAppRequestReceived(e.Context, StoragePath.AppRoot);
            }
            else if (e.Context.Request.Uri.StartsWith(ManagementAppBaseUri, StringComparison.OrdinalIgnoreCase))
            {
                e.IsHandled = true;
                OnAppRequestReceived(e.Context, StoragePath.ManagementAppRoot);
            }
        }

        private void OnApiRequestReceived(HttpContext context)
        {
            IApiCall apiCall = CreateApiContext(context);
            if (apiCall == null)
            {
                context.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            var eventArgs = new ApiRequestReceivedEventArgs(apiCall);
            RequestReceived?.Invoke(this, eventArgs);

            if (!eventArgs.IsHandled)
            {
                context.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            context.Response.StatusCode = HttpStatusCode.Ok;
            if (eventArgs.ApiContext.Result == null)
            {
                eventArgs.ApiContext.Result = new JObject();
            }

            var apiResponse = new ApiResponse
            {
                ResultCode = apiCall.ResultCode,
                Result = apiCall.Result,
                ResultHash = apiCall.ResultHash
            };

            var json = JsonConvert.SerializeObject(apiResponse);
            context.Response.Body = Encoding.UTF8.GetBytes(json);
            context.Response.MimeType = MimeTypeProvider.Json;
        }

        private static void OnAppRequestReceived(HttpContext context, string rootDirectory)
        {
            string filename;
            if (!TryGetFilename(context, rootDirectory, out filename))
            {
                context.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            if (File.Exists(filename))
            {
                context.Response.Body = File.ReadAllBytes(filename);
                context.Response.MimeType = MimeTypeProvider.GetMimeTypeFromFilename(filename);
            }
            else
            {
                context.Response.StatusCode = HttpStatusCode.NotFound;
            }
        }

        private static bool TryGetFilename(HttpContext context, string rootDirectory, out string filename)
        {
            var relativeUrl = context.Request.Uri;
            relativeUrl = relativeUrl.TrimStart('/');
            relativeUrl = relativeUrl.Substring(relativeUrl.IndexOf('/') + 1);

            if (relativeUrl.EndsWith("/") || relativeUrl == string.Empty)
            {
                relativeUrl += "Index.html";
            }

            relativeUrl = relativeUrl.Trim('/');
            relativeUrl = relativeUrl.Replace("/", @"\");

            filename = Path.Combine(rootDirectory, relativeUrl);
            return true;
        }

        private void AttachWebSocket(object sender, WebSocketConnectedEventArgs e)
        {
            // Accept each URI at the moment.
            e.IsHandled = true;

            e.WebSocketClientSession.MessageReceived += OnWebSocketMessageReceived;
            e.WebSocketClientSession.Closed += (_, __) => e.WebSocketClientSession.MessageReceived -= OnWebSocketMessageReceived;
        }

        private void OnWebSocketMessageReceived(object sender, WebSocketMessageReceivedEventArgs e)
        {
            var requestMessage = JObject.Parse(((WebSocketTextMessage)e.Message).Text);
            var apiRequest = requestMessage.ToObject<ApiRequest>();

            var context = new ApiCall(apiRequest.Action, apiRequest.Parameter, apiRequest.ResultHash);

            var eventArgs = new ApiRequestReceivedEventArgs(context);
            RequestReceived?.Invoke(this, eventArgs);

            if (!eventArgs.IsHandled)
            {
                context.ResultCode = ApiResultCode.ActionNotSupported;
            }

            var apiResponse = new ApiResponse
            {
                ResultCode = context.ResultCode,
                Result = context.Result,
                ResultHash = context.ResultHash
            };

            var jsonResponse = JObject.FromObject(apiResponse);
            jsonResponse["CorrelationId"] = requestMessage["CorrelationId"];

            e.WebSocketClientSession.SendAsync(jsonResponse.ToString()).Wait();
        }

        private ApiCall CreateApiContext(HttpContext context)
        {
            try
            {
                string bodyText;

                // Parse a special query parameter.
                if (!string.IsNullOrEmpty(context.Request.Query) && context.Request.Query.StartsWith("body=", StringComparison.OrdinalIgnoreCase))
                {
                    bodyText = context.Request.Query.Substring("body=".Length);
                }
                else
                {
                    bodyText = Encoding.UTF8.GetString(context.Request.Body ?? new byte[0]);
                }

                var action = context.Request.Uri.Substring("/api/".Length);
                var parameter = string.IsNullOrEmpty(bodyText) ? new JObject() : JObject.Parse(bodyText);

                return new ApiCall(action, parameter, null);
            }
            catch (Exception)
            {
                _log.Verbose("Received a request with no valid JSON request.");
                return null;
            }
        }
    }
}
