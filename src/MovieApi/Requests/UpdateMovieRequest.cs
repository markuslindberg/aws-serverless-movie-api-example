using Mediator;
using MovieApi.Domain;
using MovieApi.Responses;

namespace MovieApi.Requests;

public record UpdateMovieRequest(string MovieId, Movie Movie) : IRequest<Response<Movie>>;