using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Sindzi.Common.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Sindzi.Bot;

public class BotStarterService : IHostedService
{
    private readonly string BotToken;
    private readonly MistralRequestService _mistralRequestService;

    public BotStarterService(IOptions<TelegramBotOptions> telegramBotOptions, MistralRequestService mistralRequestService)
    {
        BotToken = telegramBotOptions.Value.Token;
        _mistralRequestService = mistralRequestService;
    }

    public async Task StartAsync(CancellationToken token)
    {
        var botClient = new TelegramBotClient(BotToken);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message },
            DropPendingUpdates = true,
        };

        using var cts = new CancellationTokenSource();

        botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cts.Token
        );

        var me = await botClient.GetMe();
        Console.WriteLine($"{me.FirstName} started!");
    }

    public Task StopAsync(CancellationToken token)
    {
        Console.WriteLine("Telegram Bot stopped...");
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update.Message is null)
        {
            return;
        }

        var messageUpdate = update.Message;
        var chatId = messageUpdate.Chat.Id;
        if (messageUpdate.Text == null)
        {
            return;
        }

        var message = messageUpdate.Text.ToLower();

        try
        {
            if (message.StartsWith("sindzi"))
            {
                var response = await _mistralRequestService.GetResponseAsync(messageUpdate.Text);

                if (string.IsNullOrWhiteSpace(response))
                {
                    await botClient.SendMessage(chatId, "Sorry, I don't know what to do with.");
                    return;
                }

                await botClient.SendMessage(chatId, response);
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in message handling: {ex.Message}");
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiEx => $"Telegram API Error: [{apiEx.ErrorCode}] {apiEx.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}
