using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

enum UserState
{
    WaitingForName,
    WaitingForPhone,
    WaitingForService,
    WaitingForTime,
    Completed
}

class UserSession
{
    public UserState State { get; set; } = UserState.WaitingForName;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Service { get; set; }
    public string? Time { get; set; }
}

class Program
{
    static void Main()
    {
        var botClient = new TelegramBotClient("7861550980:AAGcGeuMdwTlbzErt9-9wS5qCo_VBeoCRE8");

        var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        Console.WriteLine("🤖 Бот запущен! Нажмите Enter для остановки.");
        Console.ReadLine();
        cts.Cancel();
    }
    // Хранилище всех сессий (обязательно вне методов!)
    static Dictionary<long, UserSession> sessions = new();
    // Основная логика обработки сообщений
    static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message || message.Text is null)
            return;

        var chatId = message.Chat.Id;
        var text = message.Text;


        
        // Команда /start — сбрасываем состояние и спрашиваем ФИО
        if (text == "/start")
        {
            sessions[chatId] = new UserSession();
            await bot.SendTextMessageAsync(
                chatId,
                "Привет! Введите своё ФИО:",
                cancellationToken: cancellationToken
            );
            return;
        }

        // Если пользователя нет в словаре — создаём сессию
        if (!sessions.ContainsKey(chatId))
        {
            sessions[chatId] = new UserSession();
            await bot.SendTextMessageAsync(
                chatId,
                "Добро пожаловать! Введите ваше ФИО:",
                cancellationToken: cancellationToken
            );
            return;
        }

        var session = sessions[chatId];

        switch (session.State)
        {
            case UserState.WaitingForName:
                session.FullName = text;
                session.State = UserState.WaitingForPhone;
                await bot.SendTextMessageAsync(
                    chatId,
                    "Введите номер телефона:",
                    cancellationToken: cancellationToken
                );
                break;

            case UserState.WaitingForPhone:
                session.Phone = text;
                session.State = UserState.WaitingForService;
                await bot.SendTextMessageAsync(
                    chatId,
                    "Выберите услугу:\n1. Парикмахер\n2. Маникюр",
                    cancellationToken: cancellationToken
                );
                break;

            case UserState.WaitingForService:
                if (text == "1")
                    session.Service = "Парикмахер";
                else if (text == "2")
                    session.Service = "Маникюр";
                else
                {
                    await bot.SendTextMessageAsync(
                        chatId,
                        "Пожалуйста, выберите 1 или 2.",
                        cancellationToken: cancellationToken
                    );
                    return;
                }

                session.State = UserState.WaitingForTime;
                await bot.SendTextMessageAsync(
                    chatId,
                    "Введите время в формате: 2025-03-30 16:00",
                    cancellationToken: cancellationToken
                );
                break;

            case UserState.WaitingForTime:
                session.Time = text;
                session.State = UserState.Completed;

                await bot.SendTextMessageAsync(
                    chatId,
                    $"✅ Вы успешно записаны!\n\n" +
                    $"Имя: {session.FullName}\n" +
                    $"Телефон: {session.Phone}\n" +
                    $"Услуга: {session.Service}\n" +
                    $"Время: {session.Time}",
                    cancellationToken: cancellationToken
                );

                // 👉 Здесь позже добавим отправку на API CRM, если нужно

                // Сбрасываем сессию
                sessions.Remove(chatId);
                break;

            default:
                // Если состояние завершено - просим начать заново
                await bot.SendTextMessageAsync(
                    chatId,
                    "Чтобы начать сначала, введите /start",
                    cancellationToken: cancellationToken
                );
                break;
        }
    }

    // Обработка ошибок
    static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"❌ Ошибка: {exception.Message}");
        return Task.CompletedTask;
    }
}
