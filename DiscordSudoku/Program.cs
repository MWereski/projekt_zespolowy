using System;
using Discord;
using Discord.Net;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using DiscordSudoku.Services;

namespace DiscordSudoku
{
    class Program
    {

        // setup our fields we assign later
        private readonly IConfiguration _config;
        private DiscordSocketClient _client;
        private InteractionService _commands;
        private ulong _testGuildId;
        public static Task Main(string[] args) => new Program().MainAsync();



        public async Task MainAsync()
        {

            var token = Properties.Resources.Token;

            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();
                
                var commands = services.GetRequiredService<InteractionService>();
                _client = client;
                
                _commands = commands;

                client.Log += LogAsync;
                commands.Log += LogAsync;
                client.Ready += ReadyAsync;

                await client.LoginAsync(TokenType.Bot, token);
                await client.StartAsync();

                await services.GetRequiredService<CommandHandler>().InitializeAsync();

                await Task.Delay(Timeout.Infinite);
            }

        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            if (IsDebug())
            {
                System.Console.WriteLine($"In debug mode, adding commands to {_testGuildId}...");
                await _commands.RegisterCommandsToGuildAsync(_testGuildId);
            }
            else
            {
                await _commands.RegisterCommandsGloballyAsync(true);
            }
            Console.WriteLine($"Connected as -> [{_client.CurrentUser}] :)");
        }

        private ServiceProvider ConfigureServices()
        {
            DiscordSocketConfig config = new()
            {
                UseInteractionSnowflakeDate = false
            };
            var a = new DiscordSocketClient(config);

            return new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton<DiscordSocketClient>(a)
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();
        }

        static bool IsDebug()
        {
#if DEBUG
            return true;
#else
                return false;
#endif
        }

        public Program()
        {
            var _builder = new ConfigurationBuilder();
       
            _config = _builder.Build();
            _testGuildId = 1072785311303741501;
        }

    }

}
