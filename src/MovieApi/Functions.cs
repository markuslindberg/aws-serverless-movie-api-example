using System.Diagnostics;
using System.Text.Json;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MovieApi.Domain;
using MovieApi.Requests;
using MovieApi.Responses;
using Serilog;
using Serilog.Context;

namespace MovieApi;

public class Functions
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private bool _isColdStart = true;

    public Functions()
        : this(Startup.Configure().BuildServiceProvider())
    {
    }

    public Functions(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger>();
    }

    public async Task<Response<Movie>> GetMovie(GetMovieRequest request, ILambdaContext context)
    {
        return await InvokeHandler<GetMovieRequest, Movie>(request, context);
    }

    public async Task<Response<List<Movie>>> GetMovies(GetMoviesRequest request, ILambdaContext context)
    {
        return await InvokeHandler<GetMoviesRequest, List<Movie>>(request, context);
    }

    public async Task<Response<Movie>> CreateMovie(CreateMovieRequest request, ILambdaContext context)
    {
        return await InvokeHandler<CreateMovieRequest, Movie>(request, context);
    }

    public async Task<Response<Movie>> UpdateMovie(UpdateMovieRequest request, ILambdaContext context)
    {
        return await InvokeHandler<UpdateMovieRequest, Movie>(request, context);
    }

    public async Task<Response<Movie>> DeleteMovie(DeleteMovieRequest request, ILambdaContext context)
    {
        return await InvokeHandler<DeleteMovieRequest, Movie>(request, context);
    }

    public async Task<Response<List<Character>>> GetMovieCharacters(GetMovieCharactersRequest request, ILambdaContext context)
    {
        return await InvokeHandler<GetMovieCharactersRequest, List<Character>>(request, context);
    }

    public async Task<Response<List<Director>>> GetMovieDirectors(GetMovieDirectorsRequest request, ILambdaContext context)
    {
        return await InvokeHandler<GetMovieDirectorsRequest, List<Director>>(request, context);
    }

    public async Task<Response<List<string>>> GetCharacterMovies(GetCharacterMoviesRequest request, ILambdaContext context)
    {
        return await InvokeHandler<GetCharacterMoviesRequest, List<string>>(request, context);
    }

    public async Task<Response<List<string>>> GetDirectorMovies(GetDirectorMoviesRequest request, ILambdaContext context)
    {
        return await InvokeHandler<GetDirectorMoviesRequest, List<string>>(request, context);
    }

    private async Task<Response<TResult>> InvokeHandler<TRequest, TResult>(TRequest request, ILambdaContext context)
        where TRequest : IRequest<Response<TResult>>
    {
        using (LogContext.PushProperty("RequestId", context.AwsRequestId))
        using (LogContext.PushProperty("FunctionArn", context.InvokedFunctionArn))
        using (LogContext.PushProperty("ColdStart", _isColdStart))
        {
            _isColdStart = false;
            var sw = Stopwatch.StartNew();

            try
            {
                var mediator = _serviceProvider.GetRequiredService<IMediator>();
                var response = await mediator.Send<Response<TResult>>(request);

                _logger
                    .ForContext("Request", request)
                    .ForContext("Response", JsonSerializer.Serialize(response))
                    .Information("Function completed in {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                _logger
                    .ForContext("Request", request)
                    .Error(ex, "Function failed after {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
                return new Response<TResult>(500, "Function failed with exception");
            }
        }
    }
}