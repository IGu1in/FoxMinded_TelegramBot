using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TGBot
{
    public class UAHBot
    {
        List<Models.User> users;
        readonly string _token = "5018725104:AAHtSpg7ykLoorww0dAQ2gfl4SQlPzJ7wQY";

        public UAHBot()
        {
            users = new List<Models.User>();
        }

        public async void BotAsync()
        {
            var botClient = new TelegramBotClient(_token);

            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };
            botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: cts.Token);
            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
            cts.Cancel();
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                  => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);

            return Task.CompletedTask;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Models.User user = new Models.User();
            bool isNewUser = true;
            Message sentMessage;
            ReplyKeyboardMarkup replyKeyboardMarkup = new
            (new[]
            {
                new KeyboardButton[] { "USD" },
            })
            {
                ResizeKeyboard = true
            };

            if (update.Type != UpdateType.Message)
                return;

            if (update.Message.Type != MessageType.Text)
                return;

            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            foreach(var person in users)
            {
                if (person.ID == chatId)
                {
                    user = person;
                    isNewUser = false;
                }
            }

            if (isNewUser)
            {
                user = new Models.User(chatId);
                users.Add(user);
            }


            if (user.Currency != null)
            {
                switch (messageText)
                {
                    case var someVal when new Regex(@"[0-3]\d\.[0,1]\d\.[1,2]\d{3}").IsMatch(someVal):
                        user.Data = messageText;
                        var course = user.GetCourse();
                        sentMessage = await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: course + "\n" ,
                           replyMarkup: new ReplyKeyboardRemove(),
                           cancellationToken: cancellationToken);
                        sentMessage = await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Choose a currency:\n",
                            replyMarkup: replyKeyboardMarkup,
                            cancellationToken: cancellationToken);
                        user.Currency = null;
                        user.Data = null;
                        break;

                    default:
                        
                        sentMessage = await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Enter the date in the correct format:\n",
                            replyMarkup: new ReplyKeyboardRemove(),
                            cancellationToken: cancellationToken);

                        break;
                }
            }
            else
            {
                switch (messageText)
                {
                    case "USD":
                        sentMessage = await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: "Enter the date in the format dd.mm.yyyy:\n",
                           replyMarkup: new ReplyKeyboardRemove(),
                           cancellationToken: cancellationToken);
                        user.Currency = "USD";
                        break;

                    default:
                        sentMessage = await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Choose a currency:\n",
                            replyMarkup: replyKeyboardMarkup,
                            cancellationToken: cancellationToken);

                        break;
                }
            }
        }
    }
}