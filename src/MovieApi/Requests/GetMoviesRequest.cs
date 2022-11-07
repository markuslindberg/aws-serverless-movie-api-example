using Mediator;
using MovieApi.Domain;
using MovieApi.Responses;

namespace MovieApi.Requests;

public record GetMoviesRequest(string Category, int? YearMin = null, int? YearMax = null) : IRequest<Response<List<Movie>>>;