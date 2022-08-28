using MediatR;
using MovieApi.Domain;
using MovieApi.Responses;

namespace MovieApi.Requests;

public record DeleteMovieRequest(string MovieId) : IRequest<Response<Movie>>;