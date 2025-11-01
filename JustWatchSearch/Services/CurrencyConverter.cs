using System.Text.Json;
using System.Text.Json.Serialization;

namespace JustWatchSearch.Services;

public class CurrencyConverter : ICurrencyConverter
{
	private const string _apiUrl = "https://open.er-api.com/v6/latest/USD";
	private ExchangeRateResponse? _rates;
	private bool _initialized = false;
	private readonly object _lock = new object();

	public CurrencyConverter() { }

	public async Task InitializeAsync()
	{
		if (_initialized)
		{
			return;
		}

		lock (_lock)
		{
			if (_initialized)
			{
				return;
			}

			try
			{
				// Run synchronously in a lock to prevent race conditions
				Task.Run(async () => await FetchExchangeRatesAsync()).GetAwaiter().GetResult();
				_initialized = true;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"Failed to initialize currency converter: {ex.Message}");
				throw new InvalidOperationException("Failed to initialize currency converter", ex);
			}
		}
		
		await Task.CompletedTask;
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
				throw new InvalidOperationException("Currency converter not initialized. Call InitializeAsync first.");
			}

			if (_rates == null)
			{
				throw new InvalidOperationException("Exchange rates not available");
			}

			if (amount == 0)
			{
				return 0;
			}

			if (string.IsNullOrWhiteSpace(currencyCode))
			{
				Console.Error.WriteLine("Currency code is null or empty, returning placeholder value");
				return 999;
			}

			if (!_rates.Rates.ContainsKey(currencyCode))
			{
				Console.Error.WriteLine($"Currency code '{currencyCode}' not found in exchange rates, returning placeholder value");
				return 999;
			}

			decimal exchangeRate = _rates.Rates[currencyCode];
			
			if (exchangeRate <= 0)
			{
				Console.Error.WriteLine($"Invalid exchange rate for currency '{currencyCode}', returning placeholder value");
				return 999;
			}

			return Math.Round(amount / exchangeRate, 2);
		}
		catch (DivideByZeroException ex)
		{
			Console.Error.WriteLine($"Division by zero when converting currency: {ex.Message}");
			return 999;
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"Error converting currency: {ex.Message}");
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
