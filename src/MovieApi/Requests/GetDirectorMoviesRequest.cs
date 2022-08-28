using MediatR;
using MovieApi.Responses;

namespace MovieApi.Requests;

public record GetDirectorMoviesRequest(string DirectorId) : IRequest<Response<List<string>>>;