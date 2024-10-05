using Microsoft.EntityFrameworkCore;
using mininal_api.Dominio.DTOs;
using mininal_api.Infraestrutura.Db;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options => 
{
    options.UseMySql(
      builder.Configuration.GetConnectionString("mysql"),
      ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", async (LoginDTO loginDTO, ApplicationDbContext dbContext) =>
{
    // Busca o administrador pelo email ou username
    var administrador = await dbContext.Administradores
        .FirstOrDefaultAsync(a => a.Email == loginDTO.Email || a.Username == loginDTO.Username);

    // Verifica se o administrador foi encontrado e se a senha está correta
    if (administrador != null)
    {
        bool senhaValida = BCrypt.Net.BCrypt.Verify(loginDTO.Senha, administrador.Senha);
        
        if (senhaValida) // Verifica se a senha é válida
        {
            return Results.Ok("Login com sucesso");
        }
    }
    
    return Results.Unauthorized();
});

app.Run();
