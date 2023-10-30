using Bot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var usersContexts = new Dictionary<long, UserContext>();

var botClient = new TelegramBotClient("1826487742:AAEOYKJ-_nyzzUtSw8coWK89zdGAyqZfFE4");

using CancellationTokenSource cts = new ();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new ()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    var mapMenuInlineKeyboard = new InlineKeyboardMarkup(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData(text: "Добавить цель", callbackData: "map_creating_goals"),
        },
        new[]
        {
            InlineKeyboardButton.WithCallbackData(text: "Добавить негативную мысль", callbackData: "map_creating_negative_thoughts"),
        },
        new[]
        {
            InlineKeyboardButton.WithCallbackData(text: "Добавить поддерживающую мысль", callbackData: "map_creating_supporting_thoughts"),
        },
        new[]
        {
            InlineKeyboardButton.WithCallbackData(text: "Добавить достижение", callbackData: "map_creating_achievements"),
        },
        new[]
        {
            InlineKeyboardButton.WithCallbackData(text: "Добавить аффирмацию", callbackData: "map_creating_affirmations"),
        },
        new[]
        {
            InlineKeyboardButton.WithCallbackData(text: "Добавить успех", callbackData: "map_creating_self_success"),
        }
    });
    var goToCreateMapInlineKeyboard = new InlineKeyboardMarkup(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "start_map_creating")
        }
    });
    switch (update.Type)
    {
        case UpdateType.Message:
            if (!usersContexts.ContainsKey(update.Message!.From!.Id))
                usersContexts[update.Message.From.Id] = new UserContext();
            break;
        case UpdateType.CallbackQuery:
            if (!usersContexts.ContainsKey(update.CallbackQuery!.From.Id))
                usersContexts[update.CallbackQuery!.From.Id] = new UserContext();
            break;
    }
    if (update.Type == UpdateType.CallbackQuery)
    {
        if (update.CallbackQuery?.Data == "start_map_creating")
        {
            const string startMapCreatingText = "Давай попробуем создать карту поддерживающих мыслей. " +
                                                "Нажимай на кнопки и вперёд!";
            await botClient.SendTextMessageAsync(
                update.CallbackQuery.From.Id,
                startMapCreatingText,
                replyMarkup: mapMenuInlineKeyboard,
                cancellationToken: cancellationToken
            );
            return;
        }
        if (update.CallbackQuery?.Data == "map_creating_goals")
        {
            var goalsText = "Давай определимся с целями, которые ты хочешь достичь " +
                            "с помощью самоподдержки.\nЭто могут быть, например: улучшение " +
                            "самооценки, развитие оптимизма или преодоление негативных мыслей.\n\n" +
                            $"Твои цели: \n---------------\n{string.Join("\n\n", usersContexts[update.CallbackQuery.From.Id].ThoughtsMap.Goals)}\n" +
                            "---------------\n\nДобавим ещё?";
            usersContexts[update.CallbackQuery.From.Id].State = State.AddGoal;
            await botClient.SendTextMessageAsync(
                update.CallbackQuery.From.Id,
                goalsText,
                replyMarkup: goToCreateMapInlineKeyboard,
                cancellationToken: cancellationToken
            );
            return;
        }

        if (update.CallbackQuery?.Data == "map_creating_negative_thoughts")
        {
            var negativeThoughtsText = "Определи типичные негативные мысли или убеждения, которые возникают у тебя. " +
                                       "Это могут быть самокритичные мысли, сомнения в собственных способностях или предубеждения.\n\n" +
                                       $"Твои мысли: \n---------------\n{string.Join("\n\n", usersContexts[update.CallbackQuery.From.Id].ThoughtsMap.NegativeThoughts)}\n" +
                                       "---------------\n\nДобавим ещё?";
            usersContexts[update.CallbackQuery.From.Id].State = State.AddNegativeThoughts;
            await botClient.SendTextMessageAsync(
                update.CallbackQuery.From.Id,
                negativeThoughtsText,
                replyMarkup: goToCreateMapInlineKeyboard,
                cancellationToken: cancellationToken
            );
            return;
        }

        if (update.CallbackQuery?.Data == "map_creating_supporting_thoughts")
        {
            var supportingThoughtsText = "Напиши положительные утверждения или убеждения, которые могут подтвердить " +
                                         "твою ценность, достоинства и возможности.\n\n" +
                                         $"Твои утверждения: \n---------------\n{string.Join("\n\n", usersContexts[update.CallbackQuery.From.Id].ThoughtsMap.SupportingThoughts)}\n" +
                                         "---------------\n\nДобавим ещё?";
            usersContexts[update.CallbackQuery.From.Id].State = State.AddSupportingThoughts;
            await botClient.SendTextMessageAsync(
                update.CallbackQuery.From.Id,
                supportingThoughtsText,
                replyMarkup: goToCreateMapInlineKeyboard,
                cancellationToken: cancellationToken
            );
            return;
        }

        if (update.CallbackQuery?.Data == "map_creating_achievements")
        {
            var achievementsThoughtsText = "Включите в свой план поддерживающих мыслей достижения, которыми вы " +
                                           "гордитесь, и положительные качества, которыми обладаете. Например, вы " +
                                           "можете написать о своих прошлых успехах, мудрых решениях или любых других " +
                                           "характеристиках, которые вам нравятся в себе.\n\n" +
                                           $"Твои достижения: \n---------------\n{string.Join("\n\n", usersContexts[update.CallbackQuery.From.Id].ThoughtsMap.Achievements)}\n" +
                                           "---------------\n\nДобавим ещё?";
            usersContexts[update.CallbackQuery.From.Id].State = State.AddAchievements;
            await botClient.SendTextMessageAsync(
                update.CallbackQuery.From.Id,
                achievementsThoughtsText,
                replyMarkup: goToCreateMapInlineKeyboard,
                cancellationToken: cancellationToken
            );
            return;
        }

        if (update.CallbackQuery?.Data == "map_creating_affirmations")
        {
            var achievementsThoughtsText = "Включите в свой план позитивные утверждения или аффирмации, " +
                                           "которые помогут вам укрепить позитивное мышление. Это могут быть фразы " +
                                           "вроде \"Я ценен/ценна\", \"У меня есть сила преодолеть трудности\" или " +
                                           "\"Я достоин/достойна любви и заботы\"\n\n" +
                                           $"Твои утверждения: \n---------------\n{string.Join("\n\n", usersContexts[update.CallbackQuery.From.Id].ThoughtsMap.Affirmations)}\n" +
                                           "---------------\n\nДобавим ещё?";
            usersContexts[update.CallbackQuery.From.Id].State = State.AddAffirmations;
            await botClient.SendTextMessageAsync(
                update.CallbackQuery.From.Id,
                achievementsThoughtsText,
                replyMarkup: goToCreateMapInlineKeyboard,
                cancellationToken: cancellationToken
            );
            return;
        }

        if (update.CallbackQuery?.Data == "map_creating_self_success")
        {
            var achievementsThoughtsText = "Включи в свой план визуализацию успеха. Это означает представление " +
                                           "себя в ситуациях, где ты чувствуешь себя сильным, уверенным и счастливым. " +
                                           "Визуализация может помочь укрепить позитивные мысли и создать позитивные эмоции.\n\n" +
                                           $"Твои успехи: \n---------------\n{string.Join("\n\n", usersContexts[update.CallbackQuery.From.Id].ThoughtsMap.SelfSuccess)}\n" +
                                           "---------------\n\nДобавим ещё?";
            usersContexts[update.CallbackQuery.From.Id].State = State.AddSuccess;
            await botClient.SendTextMessageAsync(
                update.CallbackQuery.From.Id,
                achievementsThoughtsText,
                replyMarkup: goToCreateMapInlineKeyboard,
                cancellationToken: cancellationToken
            );
            return;
        }
        return;
    }

    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    if (messageText == "/start")
    {
        usersContexts[chatId] = new UserContext();
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Начать", callbackData: "start_map_creating"),
            }
        });
        var welcomeText = "Привет!\nЯ - бот поддержки, и я постараюсь быть тебе полезным в трудные минуты жизни.";
        await botClient.SendTextMessageAsync(chatId, welcomeText, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
        return;
    }

    if (usersContexts[chatId].State == State.AddGoal)
    {
        const string startMapCreatingText = "Давай попробуем создать карту поддерживающих мыслей. " +
                                            "Нажимай на кнопки и вперёд!";
        usersContexts[chatId].ThoughtsMap.Goals.Add(messageText);
        usersContexts[chatId].State = State.Map;
        await botClient.SendTextMessageAsync(
            chatId,
            startMapCreatingText,
            replyMarkup: mapMenuInlineKeyboard,
            cancellationToken: cancellationToken);
        return;
    }

    if (usersContexts[chatId].State == State.AddNegativeThoughts)
    {
        const string startMapCreatingText = "Давай попробуем создать карту поддерживающих мыслей. " +
                                            "Нажимай на кнопки и вперёд!";

        usersContexts[chatId].ThoughtsMap.NegativeThoughts.Add(messageText);
        usersContexts[chatId].State = State.Map;
        await botClient.SendTextMessageAsync(
            chatId,
            startMapCreatingText,
            replyMarkup: mapMenuInlineKeyboard,
            cancellationToken: cancellationToken);
        return;
    }

    if (usersContexts[chatId].State == State.AddSupportingThoughts)
    {
        const string startMapCreatingText = "Давай попробуем создать карту поддерживающих мыслей. " +
                                            "Нажимай на кнопки и вперёд!";

        usersContexts[chatId].ThoughtsMap.SupportingThoughts.Add(messageText);
        usersContexts[chatId].State = State.Map;
        await botClient.SendTextMessageAsync(
            chatId,
            startMapCreatingText,
            replyMarkup: mapMenuInlineKeyboard,
            cancellationToken: cancellationToken);
        return;
    }
    if (usersContexts[chatId].State == State.AddAchievements)
    {
        const string startMapCreatingText = "Давай попробуем создать карту поддерживающих мыслей. " +
                                            "Нажимай на кнопки и вперёд!";

        usersContexts[chatId].ThoughtsMap.Achievements.Add(messageText);
        usersContexts[chatId].State = State.Map;
        await botClient.SendTextMessageAsync(
            chatId,
            startMapCreatingText,
            replyMarkup: mapMenuInlineKeyboard,
            cancellationToken: cancellationToken);
        return;
    }

    if (usersContexts[chatId].State == State.AddAffirmations)
    {
        const string startMapCreatingText = "Давай попробуем создать карту поддерживающих мыслей. " +
                                            "Нажимай на кнопки и вперёд!";

        usersContexts[chatId].ThoughtsMap.Affirmations.Add(messageText);
        usersContexts[chatId].State = State.Map;
        await botClient.SendTextMessageAsync(
            chatId,
            startMapCreatingText,
            replyMarkup: mapMenuInlineKeyboard,
            cancellationToken: cancellationToken);
        return;
    }

    if (usersContexts[chatId].State == State.AddSuccess)
    {
        const string startMapCreatingText = "Давай попробуем создать карту поддерживающих мыслей. " +
                                            "Нажимай на кнопки и вперёд!";

        usersContexts[chatId].ThoughtsMap.SelfSuccess.Add(messageText);
        usersContexts[chatId].State = State.Map;
        await botClient.SendTextMessageAsync(
            chatId,
            startMapCreatingText,
            replyMarkup: mapMenuInlineKeyboard,
            cancellationToken: cancellationToken);
        return;
    }

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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