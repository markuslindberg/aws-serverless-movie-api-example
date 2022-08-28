using MediatR;
using MovieApi.Domain;
using MovieApi.Responses;

namespace MovieApi.Requests;

public record GetMovieRequest(string MovieId) : IRequest<Response<Movie>>;