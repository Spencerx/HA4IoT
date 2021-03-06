﻿using System;
using HA4IoT.Contracts.ExternalServices.Twitter;
using HA4IoT.Contracts.Scripting;
using MoonSharp.Interpreter;

namespace HA4IoT.ExternalServices.Twitter
{
    public class TwitterClientScriptProxy : IScriptProxy
    {
        private readonly ITwitterClientService _twitterClientService;

        [MoonSharpHidden]
        public TwitterClientScriptProxy(ITwitterClientService twitterClientService)
        {
            _twitterClientService = twitterClientService ?? throw new ArgumentNullException(nameof(twitterClientService));
        }

        [MoonSharpHidden]
        public string Name => "twitter";

        public void Tweet(string message)
        {
            _twitterClientService.TryTweet(message);
        }
    }
}
