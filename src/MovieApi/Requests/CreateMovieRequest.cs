using Mediator;
using MovieApi.Domain;
using MovieApi.Responses;

namespace MovieApi.Requests;

public record CreateMovieRequest(Movie Movie) : IRequest<Response<Movie>>;