
using Microsoft.EntityFrameworkCore;
using STBEverywhere_back_APICompte.Repository.IRepository;
using STBEverywhere_back_APICompte.Repository;
using STBEverywhere_back_APICompte;
using STBEverywhere_Back_SharedModels.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

  builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.Parse("8.0.0-mysql") // Mets la version exacte de MySQL ici
    ));
builder.Services.AddScoped<ICompteRepository, CompteRepository>();
builder.Services.AddScoped<IVirementRepository, VirementRepository>();

builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddControllers().AddNewtonsoftJson();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var AllowedOrigins = builder.Configuration.GetValue<string>("AllowedOrigins")!.Split(",");
builder.Services.AddCors(Options =>
{
    Options.AddDefaultPolicy(policy => {
        policy.WithOrigins(AllowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    //context.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
