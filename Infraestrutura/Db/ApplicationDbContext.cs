using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using mininal_api.Dominio.Entidades;
using BCrypt;

namespace mininal_api.Infraestrutura.Db
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IConfiguration _configuracaoAppSettings;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuracaoAppSettings)
            : base(options)
        {
            _configuracaoAppSettings = configuracaoAppSettings;
        }
        public DbSet<Administrador> Administradores { get; set; } = default!;
        public DbSet<Veiculo> Veiculos { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var hashedSenha = BCrypt.Net.BCrypt.HashPassword("123456");
            modelBuilder.Entity<Administrador>().HasData(
                new Administrador {
                    Id = 1, 
                    Email = "adm@teste.com",
                    Username = "Adm",
                    Senha = hashedSenha,
                    Perfil = "Adm"
                }
            );
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                var stringConexao = _configuracaoAppSettings.GetConnectionString("mysql")?.ToString();
                if (!string.IsNullOrEmpty(stringConexao))
                {
                    optionsBuilder.UseMySql(
                        stringConexao,
                        ServerVersion.AutoDetect(stringConexao)
                    );
                }
            }
        }
    }
}