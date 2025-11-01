using System.Text.Json;
using System.Text.Json.Serialization;

namespace JustWatchSearch.Services;

public class CurrencyConverter : ICurrencyConverter
{
	private const string _apiUrl = "https://open.er-api.com/v6/latest/USD";
	private ExchangeRateResponse? _rates;
	private bool _initialized = false;
	private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
	private readonly ILogger<CurrencyConverter>? _logger;

	public CurrencyConverter(ILogger<CurrencyConverter>? logger = null)
	{
		_logger = logger;
	}

	public async Task InitializeAsync()
	{
		if (_initialized)
		{
			return;
		}

		await _initLock.WaitAsync();
		try
		{
			if (_initialized)
			{
				return;
			}

			await FetchExchangeRatesAsync();
			_initialized = true;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "Failed to initialize currency converter");
			throw new InvalidOperationException("Failed to initialize currency converter", ex);
		}
		finally
		{
			_initLock.Release();
		}
	}

	private async Task FetchExchangeRatesAsync()
	{
		try
		{
			using (HttpClient client = new HttpClient())
			{
				client.Timeout = TimeSpan.FromSeconds(30);
				
				HttpResponseMessage response = await client.GetAsync(_apiUrl);
				
				if (response == null)
				{
					throw new InvalidOperationException("Received null response from exchange rate API");
				}

				if (!response.IsSuccessStatusCode)
				{
					throw new HttpRequestException($"Exchange rate API returned status code: {response.StatusCode}");
				}

				string json = await response.Content.ReadAsStringAsync();
				
				if (string.IsNullOrWhiteSpace(json))
				{
					throw new InvalidOperationException("Received empty response from exchange rate API");
				}

				_rates = JsonSerializer.Deserialize<ExchangeRateResponse>(json);
				
				if (_rates == null)
				{
					throw new InvalidOperationException("Failed to deserialize exchange rate response");
				}

				if (_rates.Rates == null || _rates.Rates.Count == 0)
				{
					throw new InvalidOperationException("Exchange rate response contains no rates");
				}
			}
		}
		catch (HttpRequestException ex)
		{
			throw new InvalidOperationException("Failed to fetch exchange rates from API", ex);
		}
		catch (TaskCanceledException ex)
		{
			throw new InvalidOperationException("Exchange rate request timed out", ex);
		}
		catch (JsonException ex)
		{
			throw new InvalidOperationException("Failed to parse exchange rate response", ex);
		}
	}

	public decimal ConvertToUSD(string? currencyCode, decimal amount)
	{
		try
		{
			if (!_initialized)
			{
				_logger?.LogError("Currency converter not initialized");
				throw new InvalidOperationException("Currency converter not initialized. Call InitializeAsync first.");
			}

			if (_rates == null)
			{
				_logger?.LogError("Exchange rates not available");
				throw new InvalidOperationException("Exchange rates not available");
			}

			if (amount == 0)
			{
				return 0;
			}

			if (string.IsNullOrWhiteSpace(currencyCode))
			{
				_logger?.LogWarning("Currency code is null or empty, returning placeholder value");
				return 999;
			}

			if (!_rates.Rates.ContainsKey(currencyCode))
			{
				_logger?.LogWarning("Currency code '{CurrencyCode}' not found in exchange rates, returning placeholder value", currencyCode);
				return 999;
			}

			decimal exchangeRate = _rates.Rates[currencyCode];
			
			if (exchangeRate <= 0)
			{
				_logger?.LogWarning("Invalid exchange rate for currency '{CurrencyCode}', returning placeholder value", currencyCode);
				return 999;
			}

			return Math.Round(amount / exchangeRate, 2);
		}
		catch (DivideByZeroException ex)
		{
			_logger?.LogError(ex, "Division by zero when converting currency");
			return 999;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "Error converting currency");
			return 999;
		}
	}
}

public class ExchangeRateResponse
{
	[JsonPropertyName("base_code")]
	public string? BaseCode { get; set; }

	[JsonPropertyName("rates")]
	public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();
}
