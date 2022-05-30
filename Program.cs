var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddGraphQLServer()
    .AddQueryType<QueryType>()
    .AddMutationType<MutationType>()
    .AddGlobalObjectIdentification();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapGraphQL();
});

app.Run();


public class Test
{
    public int Id { get; set; }

    public string Name { get; set; }
}

public class Resolver
{
    public Test GetTestById(int id)
    {
        return new()
        {
            Id = id,
            Name = "Some random test"
        };
    }

    public IEnumerable<Test> GetTests()
    {
        return new Test[]
        {
            new()
            {
                Id = 1,
                Name = "Some random test"
            },
            new()
            {
                Id = 2,
                Name = "Some random test"
            }
        };
    }

    public bool DoTests(int[] ids)
    {
        return true;
    }
}

public class TestType : ObjectType<Test>
{
    protected override void Configure(IObjectTypeDescriptor<Test> descriptor)
    {
        descriptor
            .ImplementsNode()
            .IdField(f => f.Id)
            .ResolveNodeWith<Resolver>(r => r.GetTestById(default));
    }
}

public class QueryType : ObjectType
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Field("tests").Type<NonNullType<ListType<NonNullType<TestType>>>>().ResolveWith<Resolver>(r => r.GetTests());
    }
}

public class MutationType : ObjectType
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Field("doTest")
            .Argument("ids", descriptor => descriptor.Type<NonNullType<ListType<NonNullType<IdType>>>>().ID(nameof(Test)))
            .Type<BooleanType>()
            .ResolveWith<Resolver>(r => r.DoTests(default));
    }
}