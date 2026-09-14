namespace SecondBrain.Application.DTOs;

public record SearchResultDto(string Type, Guid Id, string Title);

public record SearchResponse(string Query, List<SearchResultDto> Results);
