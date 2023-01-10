using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<AppDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/load-data/{length:int}", async ([FromRoute] int length, AppDbContext context) =>
{
    if(length <= 0)
        return Results.BadRequest("Length data must be greater than 0.");

    Random rnd = new Random();

    for (int i = 0; i < length; i++)
    {
        var user = new Users {
            Name = "Name " + i.ToString(),
            LastName = "LastName_" + i.ToString(),
            Age = rnd.Next(10, 100)
        };

        await context.AddAsync(user);
        await context.SaveChangesAsync();
    }
    return Results.Ok("The data has been loaded.");
});


app.MapGet("/get-data/page/{page:int}/take/{take:int}", async ([FromRoute] int page, [FromRoute] int take, AppDbContext context) =>
{
    if(page < 0)
        return Results.BadRequest("Page must be greater than 0.");

    var pageResults = (decimal)take;
    var pageCount = Math.Ceiling((context.Users.CountAsync().Result / pageResults));

    var data = await context.Users
    .Skip((page - 1) * (int)pageResults)
    .Take((int)pageResults)
    .ToListAsync();

    return Results.Ok(new DataResponse { Data = data, CurrentPage = page, Pages = (int)pageCount });
});

app.Run();

internal record DataResponse()
{
    public IEnumerable<object>? Data { get; set; }
    public int CurrentPage { get; set; }
    public int Pages { get; set; }
}