using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DataAccessLayer.Models;
using DataAccessLayer.Repository;

namespace MiddleWare.BackgroundServices
{
    public class ChatCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatCleanupBackgroundService> _logger;
        private readonly HttpClient _httpClient;

        public ChatCleanupBackgroundService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<ChatCleanupBackgroundService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ChatCleanupBackgroundService started.");

            // Run initial check and table verification shortly after startup
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                await RunChatCleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during initial chat cleanup check.");
            }

            // Run periodically every 24 hours
            using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunChatCleanupAsync(stoppingToken);
            }
        }

        public async Task RunChatCleanupAsync(CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogInformation("Running scheduled 30-day inactive chat cleanup task...");

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<EFContexts>();
                var configRepo = new ConfigRepository(dbContext);

                // Auto-ensure default config keys and table exist
                await configRepo.EnsureDefaultConfigsAsync();

                // Read configurable inactivity window in days
                int inactiveDays = await configRepo.GetConfigIntAsync("InactiveChatPurgeDays", 30);
                if (inactiveDays <= 0) inactiveDays = 30;

                var now = DateTime.UtcNow;
                var thresholdDate = now.AddDays(-inactiveDays);

                _logger.LogInformation("Inactive chat purge threshold: {Days} days (Older than {Threshold:yyyy-MM-dd HH:mm:ss} UTC)", inactiveDays, thresholdDate);

                // 1. Database cleanup: mark older active chat history records as inactive
                var staleChats = await dbContext.ChatHistories
                    .Where(c => c.ChatTime < thresholdDate && (c.IsActive == true || c.IsActive == null))
                    .ToListAsync(stoppingToken);

                int dbUpdatedCount = 0;
                if (staleChats.Any())
                {
                    foreach (var chat in staleChats)
                    {
                        chat.IsActive = false;
                        chat.Updated = now;
                    }
                    dbUpdatedCount = await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Database cleanup: Marked {Count} stale chat history record(s) as inactive.", staleChats.Count);
                }
                else
                {
                    _logger.LogInformation("Database cleanup: No active chat records older than {Days} days found.", inactiveDays);
                }

                // 2. TalkJS REST API Purge: Delete/archive conversations from TalkJS cloud servers
                string? talkJsAppId = _configuration["TalkJS:AppId"] ?? "tKzUD2dn";
                string? talkJsSecret = _configuration["TalkJS:SecretKey"] ?? "sk_test_R1ul8bBmiFIAsBG9C0CYsIDzK2R8ka2V";

                if (!string.IsNullOrWhiteSpace(talkJsAppId) && !string.IsNullOrWhiteSpace(talkJsSecret))
                {
                    int talkJsPurgedCount = await PurgeTalkJsInactiveConversationsAsync(talkJsAppId, talkJsSecret, thresholdDate, stoppingToken);
                    _logger.LogInformation("TalkJS cleanup: Purged {Count} inactive conversation(s) from TalkJS.", talkJsPurgedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing the chat cleanup task.");
            }
        }

        private async Task<int> PurgeTalkJsInactiveConversationsAsync(
            string appId,
            string secretKey,
            DateTime thresholdDate,
            CancellationToken stoppingToken)
        {
            int purgedCount = 0;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.talkjs.com/v1/{appId}/conversations?limit=30");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

                var response = await _httpClient.SendAsync(request, stoppingToken);
                if (!response.IsSuccessStatusCode)
                {
                    string errContent = await response.Content.ReadAsStringAsync(stoppingToken);
                    _logger.LogWarning("TalkJS list conversations returned HTTP {Status}: {Content}", response.StatusCode, errContent);
                    return 0;
                }

                string json = await response.Content.ReadAsStringAsync(stoppingToken);
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("data", out var dataArray) || dataArray.ValueKind != JsonValueKind.Array)
                {
                    return 0;
                }

                long thresholdUnixMs = new DateTimeOffset(thresholdDate).ToUnixTimeMilliseconds();

                foreach (var conv in dataArray.EnumerateArray())
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    string? convId = null;
                    if (conv.TryGetProperty("id", out var idProp))
                    {
                        convId = idProp.GetString();
                    }

                    if (string.IsNullOrWhiteSpace(convId)) continue;

                    long lastActivityTime = 0;

                    if (conv.TryGetProperty("lastMessage", out var lastMsg) && lastMsg.ValueKind == JsonValueKind.Object && lastMsg.TryGetProperty("createdAt", out var createdProp))
                    {
                        lastActivityTime = createdProp.GetInt64();
                    }
                    else if (conv.TryGetProperty("createdAt", out var convCreatedProp))
                    {
                        lastActivityTime = convCreatedProp.GetInt64();
                    }

                    // If conversation last message or creation is older than the threshold, delete it
                    if (lastActivityTime > 0 && lastActivityTime < thresholdUnixMs)
                    {
                        bool deleted = await DeleteTalkJsConversationAsync(appId, secretKey, convId, stoppingToken);
                        if (deleted)
                        {
                            purgedCount++;
                            _logger.LogInformation("Purged TalkJS conversation: {ConversationId}", convId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error communicating with TalkJS REST API during inactive chat purge.");
            }

            return purgedCount;
        }

        private async Task<bool> DeleteTalkJsConversationAsync(string appId, string secretKey, string conversationId, CancellationToken stoppingToken)
        {
            try
            {
                var delRequest = new HttpRequestMessage(HttpMethod.Delete, $"https://api.talkjs.com/v1/{appId}/conversations/{conversationId}");
                delRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

                var delResponse = await _httpClient.SendAsync(delRequest, stoppingToken);
                return delResponse.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete TalkJS conversation {ConversationId}", conversationId);
                return false;
            }
        }
    }
}
