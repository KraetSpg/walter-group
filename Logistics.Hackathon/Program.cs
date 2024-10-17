using System.Text.Json.Serialization;
using Logistics.Hackathon.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocument(configure =>
{
    configure.Title = "Hackathon API";
    configure.Version = "v1";
    configure.Description = "API for the Hackathon";
});
builder.Services
    .Configure<JsonOptions>(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();
app.UseOpenApi();

// Configure the HTTP request pipeline.
app.UseSwaggerUi3(configure =>
{
    configure.DocExpansion = "list";
});

app.MapGet("/health", () =>
    {
        app.Logger.LogInformation("Health request received");
        return Results.Ok();
    })
    .WithName("Health check")
    .WithOpenApi();

app.MapPost("/decide", (DecideRequest request) =>
    {
        app.Logger.LogInformation("Received DecideRequest");

        // Deserialize JSON data into a list of Offer objects
        List<Offer> offers = JsonConvert.DeserializeObject<List<Offer>>(JsonConvert.SerializeObject(request.Offers));

        // Sort the offers by Price and then by KmToCargo
        var sortedOffers = offers
            .OrderBy(o => o.Price)
            .ThenBy(o => o.KmToCargo)
            .ToList();

        // Select the best offer based on the sorting criteria
        var bestOffer = sortedOffers.FirstOrDefault();

        DecideResponse response = bestOffer != null ? new DeliverResponse(bestOffer.Uid) : new SleepResponse(1);

        app.Logger.LogInformation("Response: {response}", response);
        Console.WriteLine(response);

        return Results.Ok(response);
    })
    .WithName("Handle DecideRequest from Hackathon framework")
    .Produces<DeliverResponse>()
    .WithOpenApi();

app.Run();

public class Offer
{
    public int Uid { get; set; }
    public string Origin { get; set; }
    public string Dest { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public double EtaToCargo { get; set; }
    public double KmToCargo { get; set; }
    public double KmToDeliver { get; set; }
    public double EtaToDeliver { get; set; }
}
