using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using JustWatchSearch.Models;
using JustWatchSearch.Services.JustWatch.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static JustWatchSearch.Services.JustWatch.Responses.SearchTitlesResponse;

namespace JustWatchSearch.Services.JustWatch;

public partial class JustwatchApiService : IJustwatchApiService
{
	private readonly GraphQLHttpClient _graphQLClient;
	private readonly ILogger<JustwatchApiService> _logger;
	private readonly ICurrencyConverter _currencyConverter;
	// Use local proxy for development:
	private readonly string _baseAddress = "http://localhost:8080";
	// Previous external CORS proxy (kept for reference):
	// private readonly string _baseAddress = "https://cors-proxy.cooks.fyi/https://apis.justwatch.com";

	public JustwatchApiService(ILogger<JustwatchApiService> logger, ICurrencyConverter currencyConverter)
	{
		_logger = logger;
		_currencyConverter = currencyConverter;
		_graphQLClient = new GraphQLHttpClient($"{_baseAddress}/graphql", new SystemTextJsonSerializer());
	}

	public async Task<SearchTitlesResponse> SearchTitlesAsync(string input, CancellationToken? token)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				_logger.LogWarning("SearchTitlesAsync called with null or empty input");
				return new SearchTitlesResponse();
			}

			if (_graphQLClient == null)
			{
				_logger.LogError("GraphQL client is not initialized");
				throw new InvalidOperationException("GraphQL client is not initialized");
			}

			var searchResult = await _graphQLClient.SendQueryAsync<SearchTitlesResponse>(
				JustWatchGraphQLQueries.GetSearchTitlesQuery(input), 
				token ?? default);

			if (searchResult == null)
			{
				_logger.LogWarning("Received null search result for input: {input}", input);
				return new SearchTitlesResponse();
			}

			return searchResult.Data ?? new SearchTitlesResponse();
		}
		catch (TaskCanceledException)
		{
			_logger.LogInformation("Search request was cancelled for input: {input}", input);
			throw;
		}
		catch (HttpRequestException ex)
		{
			_logger.LogError(ex, "HTTP error while searching for title: {input}", input);
			throw new InvalidOperationException($"Failed to connect to JustWatch API: {ex.Message}", ex);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Searching title {input} failed", input);
			throw new InvalidOperationException($"Failed to search for titles: {ex.Message}", ex);
		}
	}

	public async Task<GetOffersResponse?> GetTitleOffers(string id, IEnumerable<string> countries, CancellationToken? token)
	{
		_logger.LogInformation("Got Title Offer request {id}", id);
		try
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				_logger.LogWarning("GetTitleOffers called with null or empty id");
				return null;
			}

			if (countries == null || !countries.Any())
			{
				_logger.LogWarning("GetTitleOffers called with null or empty countries list for id: {id}", id);
				return null;
			}

			if (_graphQLClient == null)
			{
				_logger.LogError("GraphQL client is not initialized");
				return null;
			}

			var searchResult = await _graphQLClient.SendQueryAsync<GetOffersResponse>(
				JustWatchGraphQLQueries.GetTitleOffersQuery(id, countries), 
				token ?? default);

			if (searchResult == null)
			{
				_logger.LogWarning("Received null result for title offers request: {id}", id);
				return null;
			}

			return searchResult.Data;
		}
		catch (TaskCanceledException)
		{
			_logger.LogInformation("Get title offers request was cancelled for id: {id}", id);
			throw;
		}
		catch (HttpRequestException ex)
		{
			_logger.LogError(ex, "HTTP error while getting title offers for id: {id}", id);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Get title offers request {id} failed", id);
			return null;
		}
	}

	public async Task<UrlMetadataResponse?> GetUrlMetadataResponseAsync(string path)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				_logger.LogWarning("GetUrlMetadataResponseAsync called with null or empty path");
				return null;
			}

			using (var httpClient = new HttpClient())
			{
				httpClient.Timeout = TimeSpan.FromSeconds(30);
				string url = $"{_baseAddress}/content/urls?path={HttpUtility.UrlEncode(path)}";
				
				var response = await httpClient.GetAsync(url);
				
				if (response == null)
				{
					_logger.LogError("Received null response for URL metadata request: {path}", path);
					return null;
				}

				if (!response.IsSuccessStatusCode)
				{
					_logger.LogWarning("URL metadata request failed with status {StatusCode} for path: {path}", 
						response.StatusCode, path);
					return null;
				}

				var content = await response.Content.ReadAsStringAsync();
				
				if (string.IsNullOrWhiteSpace(content))
				{
					_logger.LogWarning("Received empty response for URL metadata request: {path}", path);
					return null;
				}

				var urlMetadataResponse = JsonSerializer.Deserialize<UrlMetadataResponse>(content);
				return urlMetadataResponse;
			}
		}
		catch (HttpRequestException ex)
		{
			_logger.LogError(ex, "HTTP error while getting URL metadata for path: {path}", path);
			return null;
		}
		catch (TaskCanceledException ex)
		{
			_logger.LogError(ex, "URL metadata request timed out for path: {path}", path);
			return null;
		}
		catch (JsonException ex)
		{
			_logger.LogError(ex, "Failed to deserialize URL metadata response for path: {path}", path);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error getting URL metadata for path: {path}", path);
			return null;
		}
	}

	public async Task<string[]> GetAvaibleLocales(string path)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				_logger.LogWarning("GetAvaibleLocales called with null or empty path");
				return Array.Empty<string>();
			}

			var urlMetadataResponse = await GetUrlMetadataResponseAsync(path);
			
			if (urlMetadataResponse == null)
			{
				_logger.LogWarning("URL metadata response is null for path: {path}", path);
				return Array.Empty<string>();
			}

			if (urlMetadataResponse.HrefLangTags == null || !urlMetadataResponse.HrefLangTags.Any())
			{
				_logger.LogInformation("No href lang tags found for path: {path}", path);
				return Array.Empty<string>();
			}

			return urlMetadataResponse.HrefLangTags
				.Where(tag => !string.IsNullOrWhiteSpace(tag?.Locale))
				.Select(tag => tag.Locale)
				.ToArray();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting available locales for path: {path}", path);
			return Array.Empty<string>();
		}
	}

	public async Task<IEnumerable<TitleOfferViewModel>?> GetAllOffers(string nodeId, string path, CancellationToken? token)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(nodeId))
			{
				_logger.LogWarning("GetAllOffers called with null or empty nodeId");
				return null;
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				_logger.LogWarning("GetAllOffers called with null or empty path for nodeId: {nodeId}", nodeId);
				return null;
			}

			if (_currencyConverter == null)
			{
				_logger.LogError("Currency converter is not initialized");
				return null;
			}

			await _currencyConverter.InitializeAsync();
			
			var locales = await GetAvaibleLocales(path);
			
			if (locales == null || locales.Length == 0)
			{
				_logger.LogWarning("No locales found for path: {path}", path);
				return Enumerable.Empty<TitleOfferViewModel>();
			}

			_logger.LogInformation("Got {count} locales for nodeId: {nodeId}", locales.Length, nodeId);

			var countries = locales
				.Where(locale => !string.IsNullOrWhiteSpace(locale))
				.Select(o => {
					var parts = o.Split("_");
					return parts.Length > 0 ? parts[parts.Length - 1] : null;
				})
				.Where(country => !string.IsNullOrWhiteSpace(country))
				.Cast<string>()
				.ToArray();

			if (countries.Length == 0)
			{
				_logger.LogWarning("No valid countries extracted from locales for nodeId: {nodeId}", nodeId);
				return Enumerable.Empty<TitleOfferViewModel>();
			}

			var titleOffer = await GetTitleOffers(nodeId, countries, token);
			
			if (titleOffer == null)
			{
				_logger.LogWarning("Title offers is null for nodeId: {nodeId}", nodeId);
				return null;
			}

			if (titleOffer.Offers == null || !titleOffer.Offers.Any())
			{
				_logger.LogInformation("No offers found for nodeId: {nodeId}", nodeId);
				return Enumerable.Empty<TitleOfferViewModel>();
			}

			return titleOffer.Offers
				.Where(item => item.Value != null)
				.SelectMany(item => item.Value
					.Where(o => o != null)
					.Select(o => new TitleOfferViewModel(
						o, 
						item.Key, 
						_currencyConverter.ConvertToUSD(o.Currency, o.RetailPriceValue ?? 0)
					)));
		}
		catch (TaskCanceledException)
		{
			_logger.LogInformation("GetAllOffers request was cancelled for nodeId: {nodeId}", nodeId);
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting all offers for nodeId: {nodeId}", nodeId);
			return null;
		}
	}

	public async Task<TitleNode?> GetTitle(string nodeId, CancellationToken? token = null)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(nodeId))
			{
				_logger.LogWarning("GetTitle called with null or empty nodeId");
				return null;
			}

			if (_graphQLClient == null)
			{
				_logger.LogError("GraphQL client is not initialized");
				return null;
			}

			var searchResult = await _graphQLClient.SendQueryAsync<TitleNodeWrapper>(
				JustWatchGraphQLQueries.GetTitleNode(nodeId), 
				token ?? default);

			if (searchResult == null)
			{
				_logger.LogWarning("Received null result for title node request: {nodeId}", nodeId);
				return null;
			}

			return searchResult.Data?.Node;
		}
		catch (TaskCanceledException)
		{
			_logger.LogInformation("Get title request was cancelled for nodeId: {nodeId}", nodeId);
			throw;
		}
		catch (HttpRequestException ex)
		{
			_logger.LogError(ex, "HTTP error while getting title for nodeId: {nodeId}", nodeId);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Get title request {nodeId} failed", nodeId);
			return null;
		}
	}
}