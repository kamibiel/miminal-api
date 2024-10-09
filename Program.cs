using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mininal_api.Dominio.DTOs;
using mininal_api.Dominio.Entidades;
using mininal_api.Dominio.Interfaces;
using mininal_api.Dominio.ModelViews;
using mininal_api.Dominio.Servicos;
using mininal_api.Infraestrutura.Db;
using mininal_api.Infraestrutura.Interfaces;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServicos, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options => 
{
    options.UseMySql(
      builder.Configuration.GetConnectionString("mysql"),
      ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();
#endregion

# region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores
app.MapPost("/administradores/login", async ([FromBody] LoginDTO loginDTO, IAdministradorServicos administradorServico, ApplicationDbContext dbContext) =>
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
}).WithTags("Administradores");
#endregion

#region Veiculos
app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => 
{
    var veiculo = new Veiculo {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoServico.Incluir(veiculo);
    
    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);

    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if(veiculo == null) return Results.NotFound("Veiculo não existe!");
    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if(veiculo == null) return Results.NotFound("Veiculo não existe!");

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca =veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);
    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if(veiculo == null) return Results.NotFound("Veiculo não existe!");

    veiculoServico.Apagar(veiculo);
    return Results.NoContent();
}).WithTags("Veiculos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
#endregion
