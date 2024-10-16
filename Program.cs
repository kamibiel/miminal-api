using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using mininal_api.Dominio.DTOs;
using mininal_api.Dominio.Entidades;
using mininal_api.Dominio.Enuns;
using mininal_api.Dominio.Interfaces;
using mininal_api.Dominio.ModelViews;
using mininal_api.Dominio.Servicos;
using mininal_api.Infraestrutura.Db;
using mininal_api.Infraestrutura.Interfaces;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt:Key").Value;
if(string.IsNullOrEmpty(key)) key = "123456";

builder.Services.AddAuthentication(option => {
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option => {
    option.TokenValidationParameters = new TokenValidationParameters{
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorServicos, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme{
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insita o token JWT aqui"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(
      builder.Configuration.GetConnectionString("MySql"),
      ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    );
});

var app = builder.Build();
#endregion

# region Home
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
#endregion

#region Administradores
string GerarTokenJwt(Administrador administrador)
{
    if(string.IsNullOrEmpty(key)) 
        throw new InvalidOperationException("JWT Key is not configured properly.");

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim("Email", administrador.Email),
        new Claim("Perfil", administrador.Perfil),
        new Claim (ClaimTypes.Role, administrador.Perfil),
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

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
            string token = GerarTokenJwt(administrador);
            return Results.Ok(new AdministradorLogado 
            {
                Email = administrador.Email,
                Perfil = administrador.Perfil,
                Token = token
            });
        }
    }
    return Results.Unauthorized();
}).AllowAnonymous().WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServicos administradorServico) =>
{
    var adms = new List<AdministradorModelView>();
    var administradores = administradorServico.Todos(pagina);
    foreach(var adm in administradores)
    {
        adms.Add(new AdministradorModelView{
            Id = adm.Id,
            Email = adm.Email,
            Username = adm.Username,
            Perfil = adm.Perfil
        });
    }
    return Results.Ok(adms);
}).RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
    .WithTags("Administradores");

app.MapGet("/administrador/{id}", ([FromRoute] int id, IAdministradorServicos administradorServicos) =>
{
    var administrador = administradorServicos.BuscaPorId(id);
    if (administrador == null) return Results.NotFound("Administrador não existe!");
    return Results.Ok(new AdministradorModelView{
            Id = administrador.Id,
            Email = administrador.Email,
            Username = administrador.Username,
            Perfil = administrador.Perfil
        });
}).RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
    .WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServicos administradorServico, ApplicationDbContext dbContext) =>
{
    var validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(administradorDTO.Email))
    {
        validacao.Mensagens.Add("O e-mail não pode ser vazio.");
    }

    if (string.IsNullOrEmpty(administradorDTO.Username))
    {
        validacao.Mensagens.Add("O usuário não pode ser vazio.");
    }

    if (string.IsNullOrEmpty(administradorDTO.Senha))
    {
        validacao.Mensagens.Add("A senha não pode ficar em branco");
    }

    if (administradorDTO.Perfil == null)
    {
        validacao.Mensagens.Add("O perfil tem que ser preenchido.");
    }

    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }
    
    var administrador = new Administrador
    {
        Email = administradorDTO.Email,
        Username = administradorDTO.Username,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
    };

    administradorServico.Incluir(administrador);
    return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView{
            Id = administrador.Id,
            Email = administrador.Email,
            Username = administrador.Username,
            Perfil = administrador.Perfil
        });
}).RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
    .WithTags("Administradores");
#endregion

#region Veiculos
ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(veiculoDTO.Nome))
    {
        validacao.Mensagens.Add("O nome não pode ser vazio.");
    }

    if (string.IsNullOrEmpty(veiculoDTO.Marca))
    {
        validacao.Mensagens.Add("A marca não pode ficar em branco.");
    }

    if (veiculoDTO.Ano < 1950)
    {
        validacao.Mensagens.Add("Veículo muito antigo, aceito somente anos superiores a 1950.");
    }

    return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoServico.Incluir(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
    .WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);

    return Results.Ok(veiculos);
}).RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
    .WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound("Veiculo não existe!");
    return Results.Ok(veiculo);
}).RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
    .WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound("Veiculo não existe!");

    var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);
    return Results.Ok(veiculo);
}).RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
    .WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound("Veiculo não existe!");

    veiculoServico.Apagar(veiculo);
    return Results.NoContent();
}).RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
    .WithTags("Veiculos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion
