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
                _ => Usage(message)
            };
            await action;
        }

        // TODO make implementations
        private static async Task StopGameCommandAsync(Message message)
        {
            await Bot.Instance.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Усе гульні спыненыя"
            );
            if (Games.Instance.ContainsKey(message.Chat.Id))
                Games.Instance.Remove(message.Chat.Id);
        }

        private static async Task BeginGameCommandAsync(Message message)
        {
            if (Games.Instance[message.Chat.Id].Players.Count < 5)
            {
                await Games.Instance[message.Chat.Id].SendToChatAsync($"Недастаткова гульцоў (неабходна мінімум 5) - зараз {Games.Instance[message.Chat.Id].Players.Count}");
                return;
            }
            await Games.Instance[message.Chat.Id].SendToChatAsync("Гульня пачалася");
            await Games.Instance[message.Chat.Id].State.Step();
        }

        static async Task StartCommandAsync(Message message)
        {
            await Bot.Instance.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: @"Бот для гульні ""Сакрэтны гітлер"", каб пачаць гульню дадайце бота ў чат"
            );
        }
        
        static async Task HelpCommandAsync(Message message)
        {
            await Bot.Instance.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Дапамога: "// TODO write help
            );
        }
        
        static async Task CreateGameCommandAsync(Message message)
        {
            if (message.Chat.Type == ChatType.Private)
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: $"Вам патрэбна ствараць гульню не ў прыватным чаце"
                );
                return;
            }
            if (Games.Instance.ContainsKey(message.Chat.Id))
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Гульня ўжо створана"
                );
            }
            else
            {
                Games.Instance.Add(message.Chat.Id, new Game(message.Chat.Id));
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Гульня створана"
                );
            }
        }
        
        static async Task CancelGameCommandAsync(Message message)
        {
            if (message.Chat.Type == ChatType.Private)
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: $"Вам патрэбна адмяняць гульню не ў прыватным чаце"
                );
                return;
            }
            Games.Instance.Remove(message.Chat.Id);
            await Bot.Instance.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Гульня адменена..."
            );
        }
        
        static async Task JoinGameCommandAsync(Message message)
        {
            if (message.Chat.Type == ChatType.Private)
            {
                await Bot.Instance.SendTextMessageAsync(
                        chatId: message.Chat,
                        text: $"Вам патрэбна далучыцца да гульні не ў прыватным чаце"
                    );
                return;
            }
            if (!Games.Instance.ContainsKey(message.Chat.Id))
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: $"Вам патрэбна стварыць гульню, перш чым далучыцца"
                );
                return;
            }
           
            var player = message.From;
            if (Games.Instance[message.Chat.Id].IsSubscribed(player))
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: player.Id,
                    text: $"Вы ўжо гуляеце ў чаце {message.Chat.Title}"
                );
            }
            else
            {
                if (Games.Instance[message.Chat.Id].IsStarted)
                {
                    await Bot.Instance.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Дачакайцеся заканчэння гульні, перш чым далучыцца"
                    );
                    return;
                }
                Games.Instance[message.Chat.Id].Subscribe(player);
                await Bot.Instance.SendTextMessageAsync(
                    chatId: player.Id,
                    text: $"Далучыўся да гульні ў чаце {message.Chat.Title}"
                );
            }
        }

        static async Task LeaveGameCommandAsync(Message message)
        {
            if (message.Chat.Type == ChatType.Private)
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: $"Вам трэба пакінуць гульню не ў прыватным чаце"
                );
                return;
            }
            if (!Games.Instance.ContainsKey(message.Chat.Id))
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: $"Гульня не створана"
                );
                return;
            }
            if (Games.Instance[message.Chat.Id].IsStarted)
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Гульня ўжо пачата: нельга сыходзіць"
                );
                return;
            }
            var player = message.From;
            Games.Instance[message.Chat.Id].Unsubscribe(player);
            await Bot.Instance.SendTextMessageAsync(
                chatId: player.Id,
                text: $"Выхад з гульні ў чаце {message.Chat.Title}"
            );
        }

        static async Task Usage(Message message)
        {
            //
            if (message.Chat.Type == ChatType.Private)
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Каб атрымаць дапамогу, напішыце /help"
                );
        }
    }
}