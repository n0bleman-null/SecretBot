using System;
using System.Linq;
using System.Threading.Tasks;
using SecretHitlerBot;
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
                "/stats" => StatsCommandAsync(message),
                _ => Usage(message)
            };
            await action;
        }

        private static async Task StatsCommandAsync(Message message)
        {
            if (message.Chat.Type != ChatType.Private)
            {
                await Bot.Instance.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: $"Вам патрэбна атрымлiваць статыстыку ў прыватным чаце"
                );
                return;
            }

            using (var db = new SechitContext())
            {
                await Bot.Instance.SendTextMessageAsync(
                    message.Chat.Id,
                    $"Статыстыка гульца:\n" +
                    $"Гульняў згуляна: {db.Playergames.Count(p => p.Player == db.Players.Single(p => p.PlayerId == message.From.Id))}\n" +
                    $"Гульняў выйграна: {(from playergame in db.Playergames join game in db.Games on playergame.GameId equals game.Id join player in db.Players on playergame.PlayerId equals player.Id select new {Игрок = player.PlayerId,                           Роль = playergame.Role, Итог = game.Winner}).Count(p => p.Игрок == message.From.Id && p.Роль == p.Итог)}");
            }
        }
        
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
            if (Games.Instance[message.Chat.Id].Players.Count > 10)
            {
                await Games.Instance[message.Chat.Id].SendToChatAsync($"Занадта гульцоў (максiмум 10) ");
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
            using (var db = new SechitContext())
            {
                if (db.Players.FirstOrDefault(p => p.PlayerId == message.From.Id) is null)
                    db.Players.Add(new PlayerDB {PlayerId = message.From.Id});
                db.SaveChanges();
            }
        }
        
        static async Task HelpCommandAsync(Message message)
        {
            await Bot.Instance.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: @"Афіцыйныя правілы гульні: http://www.secrethitler.com/assets/Secret_Hitler_Rules.pdf" // TODO write help
            );
        }
        
        static async Task CreateGameCommandAsync(Message message)
        {
            using (var db = new SechitContext())
            {
                if (db.Chats.FirstOrDefault(c => c.ChatId == message.Chat.Id) is null)
                    db.Chats.Add(new ChatDB {ChatId = message.Chat.Id});
                db.SaveChanges();
            }

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