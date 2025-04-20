using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PersonalFinanceManager.API.Model;
using PersonalFinanceManager.API.Services;
using PersonalFinanceManager.Infrastructure.Repositories;
using PersonalFinanceManager.Infrastructure.Services;

namespace PersonalFinanceManager.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GmailApiController : ControllerBase
    {
        private readonly IGmailService _gmailService;
        private readonly TransactionService _transactionService;
        private readonly GmailServiceSettings _gmailSettings;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GmailApiController> _logger;

        private readonly string _credentialPath = "credentials.json";

        public GmailApiController(IGmailService gmailService,
                               TransactionService transactionService,
                               IOptions<GmailServiceSettings> gmailOptions,
                               IConfiguration configuration,
                               ILogger<GmailApiController> logger)
        {
            _gmailService = gmailService;
            _transactionService = transactionService;
            _gmailSettings = gmailOptions.Value;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("initiate-oauth")]
        public async Task<IActionResult> InitiateOAuth()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized access: UserId is null or empty.");
                    return Unauthorized();
                }

                var redirectUri = _configuration["GmailApiSettings:RedirectUri"];
                _logger.LogInformation($"Initiating OAuth with redirectUri: {redirectUri}");

                var authUrl = await _gmailService.InitiateOAuthFlowAsync(userId, _credentialPath, redirectUri);
                return Ok(new { AuthUrl = authUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating OAuth flow.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback(string code, string state, string? error)
        {
            _logger.LogInformation("Callback received: code={Code}, state={State}, error={Error}", code, state, error);

            try
            {
                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogWarning("OAuth error received: {Error}", error);
                    var errorRedirectUri = _configuration["GmailApiSettings:Callback"]
                                           ?? "https://localhost:7204/GmailManagement/callback";
                    return Redirect($"{errorRedirectUri}?error={error}");
                }

                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
                {
                    _logger.LogWarning("Missing code or state. code={Code}, state={State}", code, state);
                    var errorRedirectUri = _configuration["GmailApiSettings:Callback"]
                                           ?? "https://localhost:7204/GmailManagement/callback";
                    return Redirect($"{errorRedirectUri}?error=Invalid code or state");
                }

                try
                {
                    var redirectUri = _configuration["GmailApiSettings:RedirectUri"]
                                      ?? "http://localhost:8000/api/GmailApi/callback";
                    _logger.LogInformation("Exchanging code for token. State={State}, RedirectUri={RedirectUri}", state, redirectUri);

                    var userCredential = await _gmailService.ExchangeCodeForTokenAsync(state, _credentialPath, code, redirectUri);

                    _logger.LogInformation("Token exchange successful. Extracting transactions for state={State}", state);
                    var transactions = await _gmailService.ExtractTransactionsAsync(state, userCredential, 10);

                    _logger.LogInformation("Saving {Count} transactions to database for user {UserId}", transactions.Count, state);
                    await _transactionService.SaveTransactions(transactions, Int16.Parse(state));

                    var successMessage = "Connect to Gmail successfully";
                    _logger.LogInformation("Redirecting to MVC with success message.");

                    var successRedirectUri = _configuration["GmailApiSettings:Callback"]
                                             ?? "https://localhost:7204/AccountManagement/Reauthorize";

                    return Redirect($"{successRedirectUri}?successMessage={Uri.EscapeDataString(successMessage)}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during Gmail callback processing.");
                    var errorRedirectUri = _configuration["GmailApiSettings:RedirectUri"]
                                           ?? "http://localhost:8000/api/gmailApi/callback";

                    return Redirect($"{errorRedirectUri}?error={Uri.EscapeDataString(ex.Message)}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unexpected error occurred in Callback.");
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
    }
}