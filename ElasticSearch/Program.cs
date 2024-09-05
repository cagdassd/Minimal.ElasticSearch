using Elastic.Clients.Elasticsearch;
using System.Reflection.Metadata;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

ElasticsearchClientSettings settings = new(new Uri("http://localhost:9200"));
settings.DefaultIndex("products");
ElasticsearchClient client = new(settings);

client.IndexAsync("products").GetAwaiter().GetResult();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapPost("/products/create",async (CreateProductDto request, CancellationToken cancellationToken) =>
{
    Product product = new()
    {
        Name = request.Name,
        Price = request.Price,
        Stock = request.Stock,
        Description = request.Description
    };

    CreateRequest<Product> createRequest = new(product.Id.ToString())
    {
        Document = product
    };
    CreateResponse cresponse = await client.CreateAsync(createRequest, cancellationToken);
    return Results.Ok(cresponse.Id);
});

app.MapGet("/products/getall", async (CancellationToken cancellationToken) =>
{
    SearchResponse<Product> response = await client.SearchAsync<Product>("products", cancellationToken);

    return Results.Ok(response.Documents);
});
    
app.Run();


class Product
{
    public Product()
    {
        Id = Guid.NewGuid();
    }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Description { get; set; }
}

record CreateProductDto
(
    string Name,
    decimal Price,
    int Stock,
    string Description
);
