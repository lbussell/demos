using System.Text.Json.Serialization;

// var builder = WebApplication.CreateSlimBuilder(args);
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();

// Uncomment for native AOT
// builder.Services.ConfigureHttpJsonOptions(options =>
// {
//     options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
// });

var app = builder.Build();

app.MapHealthChecks("/healthz");

app.MapGet("/releases", async () => await ReleaseReport.Generator.MakeReportAsync());

app.Run();

// Uncomment for native AOT
// [JsonSerializable(typeof(ReportJson.Report))]
// internal partial class AppJsonSerializerContext : JsonSerializerContext
// {
// }
