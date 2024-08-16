using MongoDB.Driver;
using SmartComponents.LocalEmbeddings;
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<LocalEmbedder>();

var SpaceNewsApiOriginPolicy = "_spaceNewsApiOriginPolicy";

var allowedOrigin = builder.Environment.IsDevelopment() ?
    builder.Configuration.GetValue<string>("AllowedOrigin") :
    builder.Configuration.GetValue<string>("ALLOWED_ORIGIN");

builder.Services.AddCors(options =>
{
    if (allowedOrigin is not null)
    {
        options.AddPolicy(name: SpaceNewsApiOriginPolicy,
            policy =>
            {
                policy.WithOrigins(allowedOrigin);
            });
    }
});

builder.Services.AddControllers();

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();

var connectionString = builder.Environment.IsDevelopment() ?
     builder.Configuration.GetConnectionString("SpaceNews") :
      builder.Configuration.GetValue<string>("DB_CONNECTION_STRING");

var conn = new MongoClient(connectionString);
var database = conn.GetDatabase("SpaceNews");
builder.Services.AddSingleton(database);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors(SpaceNewsApiOriginPolicy);
app.UseAuthorization();

app.UseFastEndpoints();
app.UseSwaggerGen();
app.Run();
