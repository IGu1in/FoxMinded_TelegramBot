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
        private List<Models.User> users;
        private readonly string _token = "5018725104:AAHtSpg7ykLoorww0dAQ2gfl4SQlPzJ7wQY";

        public UAHBot()
        {
            users = new List<Models.User>();
        }

        public async Task BotAsync()
        {
            var botClient = new TelegramBotClient(_token);
            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };
            botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: cts.Token);
            var me = await botClient.GetMeAsync(cancellationToken: cts.Token);
            Console.WriteLine(me.FirstName);
            Console.ReadLine();
            cts.Cancel();
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                  => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(errorMessage);

            return Task.CompletedTask;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Models.User user = new Models.User();
            bool isNewUser = true;
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
                if (person.Id == chatId)
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
                    case var value when new Regex(@"[0-3]\d\.[0,1]\d\.[1,2]\d{3}").IsMatch(value):
                        user.Data = messageText;
                        var course = user.GetCourse();
                        await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: course + "\n" ,
                           replyMarkup: new ReplyKeyboardRemove(),
                           cancellationToken: cancellationToken);
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Choose a currency:\n",
                            replyMarkup: replyKeyboardMarkup,
                            cancellationToken: cancellationToken);
                        user.Currency = null;
                        user.Data = null;

                        break;

                    default:                        
                        await botClient.SendTextMessageAsync(
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
                        await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: "Enter the date in the format dd.mm.yyyy:\n",
                           replyMarkup: new ReplyKeyboardRemove(),
                           cancellationToken: cancellationToken);
                        user.Currency = "USD";

                        break;

                    default:
                        await botClient.SendTextMessageAsync(
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