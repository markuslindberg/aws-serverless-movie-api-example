using MediatR;
using MovieApi.Responses;

namespace MovieApi.Requests;

public record GetCharacterMoviesRequest(string CharacterId) : IRequest<Response<List<string>>>;