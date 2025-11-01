using GraphQL;
using System.Diagnostics.Metrics;

namespace JustWatchSearch.Services.JustWatch;

public static class JustWatchGraphQLQueries
{
	public static GraphQLRequest GetTitleOffersQuery(string nodeId, IEnumerable<string> countries)
	{
		return new GraphQLRequest
		{
			OperationName = "GetTitleOffers",
			Query = _GetTitleOffersQuery(countries),
			Variables = new
			{
				Country = "US",
				filterBuy = new { },
				language = "en",
				nodeId,
				platform = "WEB"
			}
		};
	}

	public static GraphQLRequest GetSearchTitlesQuery(string input, string country = "US")
	{
		return new GraphQLRequest
		{
			OperationName = "GetSearchTitles",
			Query = JustWatchGraphQLQueries._getSearchTitlesQuery,
			Variables = new
			{
				searchTitlesFilter = new
				{
					searchQuery = input
				},
				country = country,
				language = "en",
				first = 10,
				formatPoster = "JPG",
				formatOfferIcon = "PNG",
				profile = "S718",
				backdropProfile = "S1920",
				filter = new
				{
				}
			}
		};
	}

	public static GraphQLRequest GetUpcomingTitlesQuery(string country = "US", int first = 20)
	{
		return new GraphQLRequest
		{
			OperationName = "GetUpcomingTitles",
			Query = JustWatchGraphQLQueries._getUpcomingTitlesQuery,
			Variables = new
			{
				country = country,
				language = "en",
				first = first,
				formatPoster = "JPG",
				profile = "S718",
				backdropProfile = "S1920",
				releaseDateFrom = DateTime.UtcNow.ToString("yyyy-MM-dd"),
				releaseDateUntil = DateTime.UtcNow.AddMonths(6).ToString("yyyy-MM-dd")
			}
		};
	}

	public static GraphQLRequest GetPopularTitlesQuery(string country = "US", int first = 20)
	{
		return new GraphQLRequest
		{
			OperationName = "GetPopularTitles",
			Query = JustWatchGraphQLQueries._getPopularTitlesQuery,
			Variables = new
			{
				country = country,
				language = "en",
				first = first,
				formatPoster = "JPG",
				profile = "S718",
				backdropProfile = "S1920"
			}
		};
	}


	public static GraphQLRequest GetTitleNode(string nodeId)
	{

		return new GraphQLRequest
		{
			OperationName = "GetTitleNode",
			Query = _GetTitleNodeQuery,
			Variables = new
			{
				Country = "US",
				filterBuy = new { },
				language = "en",
				nodeId,
				platform = "WEB",
				formatPoster = "JPG",
				formatOfferIcon = "PNG",
				profile = "S718",
				backdropProfile = "S1920",
			}
		};
	}


	#region stringQueries
	
	public const string _getUpcomingTitlesQuery = @"
query GetUpcomingTitles(
  $country: Country!,
  $language: Language!,
  $first: Int!,
  $formatPoster: ImageFormat,
  $profile: PosterProfile,
  $backdropProfile: BackdropProfile,
  $releaseDateFrom: Date!,
  $releaseDateUntil: Date!
) {
  popularTitles(
    country: $country
    first: $first
    sortBy: POPULAR
    sortRandomSeed: 0
    filter: {
      releaseDateFrom: $releaseDateFrom
      releaseDateUntil: $releaseDateUntil
    }
  ) {
    edges {
      ...SearchTitleGraphql
      __typename
    }
    __typename
  }
}

fragment SearchTitleGraphql on PopularTitlesEdge {
  node {
    id
    objectId
    objectType
    content(country: $country, language: $language) {
      title
      fullPath
      originalReleaseYear
      originalReleaseDate
      runtime
      shortDescription
      genres {
        shortName
        __typename
      }
      externalIds {
        imdbId
        tmdbId
        __typename
      }
      posterUrl(profile: $profile, format: $formatPoster)
      backdrops(profile: $backdropProfile, format: $formatPoster) {
        backdropUrl
        __typename
      }
      __typename
    }
    __typename
  }
  __typename
}";

	public const string _getPopularTitlesQuery = @"
query GetPopularTitles(
  $country: Country!,
  $language: Language!,
  $first: Int!,
  $formatPoster: ImageFormat,
  $profile: PosterProfile,
  $backdropProfile: BackdropProfile
) {
  popularTitles(
    country: $country
    first: $first
    sortBy: POPULAR
    sortRandomSeed: 0
    filter: {}
  ) {
    edges {
      ...PopularTitleGraphql
      __typename
    }
    __typename
  }
}

fragment PopularTitleGraphql on PopularTitlesEdge {
  node {
    id
    objectId
    objectType
    content(country: $country, language: $language) {
      title
      fullPath
      originalReleaseYear
      originalReleaseDate
      runtime
      shortDescription
      genres {
        shortName
        __typename
      }
      externalIds {
        imdbId
        tmdbId
        __typename
      }
      posterUrl(profile: $profile, format: $formatPoster)
      backdrops(profile: $backdropProfile, format: $formatPoster) {
        backdropUrl
        __typename
      }
      __typename
    }
    __typename
  }
  __typename
}";

	public const string _getSearchTitlesQuery = @"
           query GetSearchTitles(
  $searchTitlesFilter: TitleFilter!,
  $country: Country!,
  $language: Language!,
  $first: Int!,
  $formatPoster: ImageFormat,
  $profile: PosterProfile,
  $backdropProfile: BackdropProfile,
) {
  popularTitles(
    country: $country
    filter: $searchTitlesFilter
    first: $first
    sortBy: POPULAR
    sortRandomSeed: 0
  ) {
    edges {
      ...SearchTitleGraphql
      __typename
    }
    __typename
  }
}

fragment SearchTitleGraphql on PopularTitlesEdge {
  node {
    id
    objectId
    objectType
    content(country: $country, language: $language) {
      title
      fullPath
      originalReleaseYear
      originalReleaseDate
      runtime
      shortDescription
      genres {
        shortName
        __typename
      }
      externalIds {
        imdbId
        tmdbId
        __typename
      }
      posterUrl(profile: $profile, format: $formatPoster)
      backdrops(profile: $backdropProfile, format: $formatPoster) {
        backdropUrl
        __typename
      }
      __typename
    }
    __typename
  }
  __typename
}";

	private static string _GetTitleNodeQuery => @"
query GetTitleNode(
  $nodeId: ID!, 
  $language: Language!, 
  $country: Country!,
  $formatPoster: ImageFormat,
  $profile: PosterProfile,
  $backdropProfile: BackdropProfile) {
  node(id: $nodeId) {
 
   ... on MovieOrShow {
    id
    objectId
    objectType
    content(country: $country, language: $language) {
      title
      fullPath
      originalReleaseYear
      originalReleaseDate
      runtime
      shortDescription
      genres {
        shortName
        __typename
      }
      externalIds {
        imdbId
        tmdbId
        __typename
      }
      posterUrl(profile: $profile, format: $formatPoster)
      backdrops(profile: $backdropProfile, format: $formatPoster) {
        backdropUrl
        __typename
      }
      __typename
    }

    __typename
  }
}
}
";



	private static string _GetTitleOffersQuery(IEnumerable<string> countries)
	{
		string query = @"
query GetTitleOffers($nodeId: ID!, $language: Language!, $filterBuy: OfferFilter!, $platform: Platform! = WEB) {
  node(id: $nodeId) {

    ... on MovieOrShowOrSeasonOrEpisode {
      
      ";

		foreach (var country in countries)
		{
			query += $@"
      {country.ToLower()}: offers(country: {country}, platform: $platform, filter: $filterBuy) {{
        ...TitleOffer
        __typename
      }}";
		}
		query += @"
    }
  }
}

fragment TitleOffer on Offer {
  id
  presentationType
  monetizationType
  retailPrice(language: $language)
  retailPriceValue
  currency
  lastChangeRetailPriceValue
  type
  package {
    id
    packageId
    clearName
    technicalName
    icon(profile: S100)
    __typename
  }
  standardWebURL
  elementCount
  availableTo
  deeplinkRoku: deeplinkURL(platform: ROKU_OS)
  subtitleLanguages
  videoTechnology
  audioTechnology
  audioLanguages
  __typename
}";

		return query;
	}

	#endregion
}