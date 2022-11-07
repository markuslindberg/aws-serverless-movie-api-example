using Mediator;
using MovieApi.Domain;
using MovieApi.Responses;

namespace MovieApi.Requests;

public record GetMovieCharactersRequest(string MovieId) : IRequest<Response<List<Character>>>;