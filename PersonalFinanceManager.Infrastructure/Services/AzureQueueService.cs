using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Infrastructure.Services
{
    public class AzureQueueService: IQueueService
    {
        private readonly QueueClient _queueClient;
        private readonly bool _enabled;
        private readonly ILogger<AzureQueueService> _logger;
        public AzureQueueService(IConfiguration configuration, ILogger<AzureQueueService> logger) 
        {
            _logger = logger;
            _enabled = configuration.GetValue<bool>("AzureQueue:Enabled", false);
            if (_enabled)
            {
                var connectString = configuration["AzureQueue:ConnectionString"];
                var queueName = configuration["AzureQueue:QueueName"];

                if (string.IsNullOrEmpty(connectString)|| string.IsNullOrEmpty(queueName))
                {
                    _enabled = false;
                    _logger.LogWarning("Azure Queue disabled due to missing ConnectionString or QueueName");
                }
                else
                {
                    _queueClient = new QueueClient(connectString, queueName);
                    _queueClient.CreateIfNotExists();
                    _logger.LogInformation("Azure Queue initialized with queue name {QueueName}", queueName);
                }
            }
        }

        public async Task SendMessageAsync<T>(T message)
        {
            if (!_enabled) 
            {
                _logger.LogInformation("Azure Queue is disabled, skipping message");
                return;
            }

            try
            {
                var json = JsonConvert.SerializeObject(message, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                await _queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(json)));
                _logger.LogInformation("Message sent to queue: {Json}", json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to queue");
                throw; // Có thể tùy chỉnh: throw hoặc bỏ qua
            }
            
        }
    }
}
