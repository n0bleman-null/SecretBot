using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot
{
    public static class Commands
    {
        public static async Task ExecuteCommand(Message message)
        {
            var ins = message.Text.Split().First();
            if (ins.Contains('@'))
                ins = ins.Remove(message.Text.IndexOf('@'));
            var action = (ins) switch
            { // TODO add commands...
                "/start" => StartCommandAsync(message),
                "/help" => HelpCommandAsync(message),
                "/game" => CreateGameCommandAsync(message),
                "/cancel" => CancelGameCommandAsync(message),
                "/join" => JoinGameCommandAsync(message),  
                "/leave" => LeaveGameCommandAsync(message),
                "/begin" => BeginGameCommandAsync(message),
                "/stop" => StopGameCommandAsync(message),
                "/test" => StopGameCommandAsync(message),
                _ => Usage(message)
            };
            await action;
        }

        // TODO make implementations
        private static Task StopGameCommandAsync(Message message)
        {
            throw new NotImplementedException();
        }

        private static Task BeginGameCommandAsync(Message message)
        {
            throw new NotImplementedException();
        }

        static async Task StartCommandAsync(Message message)
        {
            await Bot.Instance.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Started"
            );
        }
        
        static async Task HelpCommandAsync(Message message)
        {
            await Bot.Instance.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Help will appear here:"
            );
        }
        
        static async Task CreateGameCommandAsync(Message message)
        {
            if (message.Chat.Type == ChatType.Private)
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: $"You need to create game not in private chat"
                );
                return;
            }
            if (Games.Instance.ContainsKey(message.Chat.Id))
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Game is already created"
                );
            }
            else
            {
                Games.Instance.Add(message.Chat.Id, new Game(message.Chat.Id));
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Game created"
                );
            }
        }
        
        static async Task CancelGameCommandAsync(Message message)
        {
            if (message.Chat.Type == ChatType.Private)
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: $"You need to cancel game not in private chat"
                );
                return;
            }
            Games.Instance.Remove(message.Chat.Id);
            await Bot.Instance.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Game canceled..."
            );
        }
        
        static async Task JoinGameCommandAsync(Message message)
        {
            if (message.Chat.Type == ChatType.Private)
            {
                await Bot.Instance.SendTextMessageAsync(
                        chatId: message.Chat,
                        text: $"You need to join game not in private chat"
                    );
                return;
            }
            if (!Games.Instance.ContainsKey(message.Chat.Id))
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: $"You need to create game before join"
                );
                return;
            }
           
            var player = message.From;
            if (Games.Instance[message.Chat.Id].IsSubscribed(player))
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: player.Id,
                    text: $"You are already playing in chat {Games.Instance[message.Chat.Id].Players.Count}"
                    // text: $"You are already playing in chat {message.Chat.Title}"
                );
            }
            else
            {
                if (Games.Instance[message.Chat.Id].IsStarted)
                {
                    await Bot.Instance.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Wait for game end before join"
                    );
                    return;
                }
                Games.Instance[message.Chat.Id].Subscribe(player);
                await Bot.Instance.SendTextMessageAsync(
                    chatId: player.Id,
                    text: $"Joined to game in chat {Games.Instance[message.Chat.Id].Players.Count}"
                    // text: $"Joined to game in chat {message.Chat.Title}"
                );
            }
        }

        static async Task LeaveGameCommandAsync(Message message)
        {
            if (message.Chat.Type == ChatType.Private)
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: $"You need to leave game not in private chat"
                );
                return;
            }
            if (!Games.Instance.ContainsKey(message.Chat.Id))
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: $"Game is not created"
                );
                return;
            }
            if (Games.Instance[message.Chat.Id].IsStarted)
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Game is already started: can't leave"
                );
                return;
            }
            var player = message.From;
            Games.Instance[message.Chat.Id].Unsubscribe(player);
            await Bot.Instance.SendTextMessageAsync(
                chatId: player.Id,
                text: $"Exit game in chat {message.Chat.Title}"
            );
        }
        
        static async Task TestCommandAsync(Message message)
        {
            Player player = new Player(message.From);
            player.SendMessageAsync("df");
        }


        static async Task Usage(Message message)
        {
            if (message.Chat.Type == ChatType.Private)
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "type /help bitch"
                );
        }
    }
}