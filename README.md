# MovieApi

Serverless movie api example using .net 8, AWS Lambda, DynamoDB, API Gateway and AWS SAM.

[![Main Workflow](https://github.com/markuslindberg/serverless-movie-api-example/actions/workflows/pipeline.yaml/badge.svg)](https://github.com/markuslindberg/serverless-movie-api-example/actions/workflows/pipeline.yaml)

## Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
* [AWS Lambda for .NET Core](https://github.com/aws/aws-lambda-dotnet)
* [AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html)
* [Amazon.Lambda.Tools](https://github.com/aws/aws-extensions-for-dotnet-cli)
* [Powertools for AWS Lambda (.NET)](https://docs.powertools.aws.dev/lambda/dotnet/)

## Endpoints

List of REST endpoints exposed in API Gateway:

* [API Documentation](https://bump.sh/markuslindberg/doc/movieapi)
* [OpenAPI Specification](src/MovieApi/Schemas/openapi.yaml)

| Paths | Method | Description|
| :---  | :---   | :---       |
|/movies|GET|Get all movies|
|/movies|POST|Create a movie|
|/movies/{movieId}|GET|Get a movie|
|/movies/{movieId}|PUT|Update a movie|
|/movies/{movieId}|DELETE|Delete a movie|
|/movies/{movieId}/characters|GET|Get movie characters|
|/movies/{movieId}/directors|GET|Get movie directors|
|/characters/{characterId}/movies|GET|Get character movies|
|/directors/{directorId}/movies|GET|Get director movies|

## Single Table Data Model

The domain objects in this example are stored in a single table, consisting of three entity types: MOVIE, CHARACTER, DIRECTOR.

![Single Table](images/MoviesTable.png)

| Access Patterns |Table/GSI/LSI|Key Condition|Filter Expression| Example|
| :---        | :---         | :---     | :---    |:---|
|Get movie for a given movieId|Table|pk=movieId and sk=movieId|-|pk="MOVIE#DieHard" and sk="MOVIE#DieHard"|
|Get characters for a given movieId|Table|pk=movieId and sk begins_with "CHARACTER#"|-|pk="MOVIE#DieHard" and sk begins_with "CHARACTER#"|
|Get directors for a given movieId|Table|pk=movieId and sk begins_with "DIRECTOR#"|-|pk="MOVIE#DieHard" and sk begins_with "DIRECTOR#"|

### Global Secondary Index (GSI1)

![GSI1](images/MoviesTableGSI1.png)

| Access Patterns |Table/GSI/LSI|Key Condition|Filter Expression| Example|
| :---        | :---         | :---     | :---    |:---|
|Get movies for a given characterId|GSI1|gsi1pk=characterId and SK begins_with "MOVIE#"|-|gsi1pk="CHARACTER#JohnMcClane" and gsi1sk begins_with "MOVIE#"|
|Get movies for a given directorId|GSI1|gsi1pk=directorId and SK begins_with "MOVIE#"|-|gsi1pk="DIRECTOR#JohnMcTiernan" and gsi1sk begins_with "MOVIE#"|

### Global Secondary Index (GSI2)

![GSI2](images/MoviesTableGSI2.png)

| Access Patterns |Table/GSI/LSI|Key Condition|Filter Expression| Example|
| :---        | :---         | :---     | :---    |:---|
|Get movies for a given category and year range|GSI2|gsi2pk=category and gsi2sk between year and year|-|gsi2pk="Action" and gsi2sk between 1988 and 1990|

## Observability
Using AWS X-Ray for tracing and structured logging in AWS CloudWatch. 

## Quality 

### Documentation

* **API documentation** in OpenAPI Specification v3.0.x.
* **Technical documentation** TODO class, deployment, dependency diagrams.
* **Code documentation** TODO inline code comments.

### Testing
Testing of Lambda functions and DynamoDB table using xUnit and Verify. Verify is a great library to assert complex test responses and easily setup and modify the expected results using the configured diff tool.

* [xUnit](https://xunit.net/)
* [Verify](https://github.com/VerifyTests/Verify)
* [Bogus](https://github.com/bchavez/Bogus)

### Static Code Analyzers
Using .NET source code analysis which is enabled by default, and the following third-party analyzers:

* [SecurityCodeScan.VS2019](https://security-code-scan.github.io/)
* [SonarAnalyzer.CSharp](https://github.com/SonarSource/sonar-dotnet)
* [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)

### Software Bill Of Materials (SBOM)
Generating SPDX 2.2 compatible SBOM using [Microsoft SBOM Tool](https://github.com/microsoft/sbom-tool) in GitHub Actions. The SBOM is saved as an artifact, and uploaded to GitHub's dependency submission API.

## References
1. [Velocity Template Language (VTL) reference](https://velocity.apache.org/engine/devel/vtl-reference.html)
2. [YAML over multiple lines](https://stackoverflow.com/questions/3790454/how-do-i-break-a-string-in-yaml-over-multiple-lines)
3. [JSON schema draft 4](https://datatracker.ietf.org/doc/html/draft-zyp-json-schema-04)
4. [OpenAPI specification v3.0.0](https://spec.openapis.org/oas/v3.0.0)
5. [API Gateway template variables](https://docs.aws.amazon.com/apigateway/latest/developerguide/api-gateway-mapping-template-reference.html)
6. [API Gateway proxy vs non-proxy Lambda integration](https://medium.com/@lakshmanLD/lambda-proxy-vs-lambda-integration-in-aws-api-gateway-3a9397af0e6d)
7. [.NET source code analysis](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview)
8. [AWS Serverless Application Model](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/what-is-sam.html)
9. [Building Lambda functions with C#](https://docs.aws.amazon.com/lambda/latest/dg/lambda-csharp.html)
10. [Amazon DynamoDB Developer Guide](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Introduction.html)
11. [OpenAPI.Tools](https://openapi.tools/)