# TvMazeApi

## Overview

The TvMaze solution consists of a worker service `ScrapperWorker` which can run as a background or a scheduled service, and a RESTApi `TvShowApi` which allows querying the application's local storage. Both applications are written in .NET Core 6.

The API is using the new Minimal APIs template, which provides a simple and clean way to build APIs, and the whole application is contained in a single file.

The scrapper service consists of a worker service, which runs indefinitely, and the `TvShowScrapperService` which contains the scrapper logic. It depends on `TvMazeApiClient` which is a facade for the 3rd party API, and the repository project, responsible for the database access.

On a special note, the `Polly` library was used to handle transient errors while calling the external API. In case of a transient error (usually 5xx) or 429 (TooManyRequests), `Polly` will automatically retry, and increase the waiting time within each attempt.

The API contains 3 endpoints:

- `GET ​/shows`: has two mandatory query string parameters: `pageNumber` and `pageSize`. This endpoint allows querying the shows and its casts by page and page size.
- `GET ​/shows​/{id}`: Allows querying a specific show. Added for testing purposes.
- `POST ​/shows`: Allows saving a show. Added for testing purposes.

To persist the data, `LiteDB` was used. [LiteDB](https://www.litedb.org/) is Embedded NoSQL database for .NET. The database file is stored in the solution root folder and is shared across the 2 main projects. A DB file with some scraped data is available in the code repository.

## Tests

The 2 main components of this application are tested through de projects: `ScrapperWorker.Tests` and `TvShowApi.Tests`.

- `ScrapperWorker.Tests`: the scrapper application is extensively covered by unit tests. All interactions with external APIs and database are mocked.
- `TvShowApi.Tests`: the API is extensively covered by unit tests as well, but since its logic is rather simple, the interaction with the repository layer is not mocked, and an in-memory version of the database is used instead.

## Usage

To run either the scrapper service or the API, just set the desired project as a start-up project, and run it.

The scrapper application can be triggered, stopped, and resumed at any time. It continues to scrap from the last processed page, and if a page is partially processed, it can continue without problems.

The API supports OpenAPI Specification (Swagger), and when launching it, the swagger UI will be loaded. Or it can be accessed via: `https://{API_ENDPOINT}/swagger/index.html`.

## Future improvements

Here are some suggestions for future improvements:

- Database: for this application, a simple document storage was used: `LiteDB`. For a real project, a more powerful, scalable and performance database should be used.
- Performance: due to a low rate limit of the TV Maze API, it's not possible to parallelize the scrapper service. If the external API supports a higher rate limit, the performance can be largely improved, especially for the cast lookup, which could be parallelized.
- Tests: despite the coverage of unit tests, it's important to have integration and/or E2E tests that target a fully running application.
