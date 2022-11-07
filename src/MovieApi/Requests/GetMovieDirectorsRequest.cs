using Mediator;
using MovieApi.Domain;
using MovieApi.Responses;

namespace MovieApi.Requests;

public record GetMovieDirectorsRequest(string MovieId) : IRequest<Response<List<Director>>>;