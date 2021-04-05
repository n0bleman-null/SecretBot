﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;


namespace TelegramBot
{
    public sealed class Bot // Singleton
    {
        private static readonly Lazy<TelegramBotClient> Lazy =
            new Lazy<TelegramBotClient>(() => new TelegramBotClient(token: Configuration.BotToken));
        public static TelegramBotClient Instance => Lazy.Value;
        private Bot()
        { }
    }
    
    public sealed class Games // Singleton
    {
        private static readonly Lazy<Dictionary<long, Game>> Lazy =
            new Lazy<Dictionary<long, Game>>(() => new Dictionary<long, Game>());
        public static Dictionary<long, Game> Instance => Lazy.Value;
        private Games()
        { }
    }
    
    class Program
    {
        static async Task Main(string[] args)
        {
            Bot.Instance.OnMessage += BotOnMessageReceived;
            
            Bot.Instance.StartReceiving();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            Bot.Instance.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text)
                return;

            if (message.Text.StartsWith('/'))
            {
                await Commands.ExecuteCommand(message);
            }
        }
    }
}