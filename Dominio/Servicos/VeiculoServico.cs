using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using mininal_api.Dominio.Entidades;
using mininal_api.Dominio.Interfaces;
using mininal_api.Infraestrutura.Db;

namespace mininal_api.Dominio.Servicos
{
    public class VeiculoServico : IVeiculoServico
    {
        private readonly ApplicationDbContext _application;

        public VeiculoServico(ApplicationDbContext application)
        {
            _application = application;
        }

        public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
        {
            var query = _application.Veiculos.AsQueryable();
            if(!string.IsNullOrEmpty(nome))
            {
                query = query.Where(v => EF.Functions.Like(v.Nome.ToLower(), $"%{nome.ToLower()}%"));
            }

            if(!string.IsNullOrEmpty(marca))
            {
                query = query.Where(v => EF.Functions.Like(v.Marca.ToLower(), $"%{marca.ToLower()}%"));
            }
            
            int itensPorPagina = 10;

            if(pagina != null)
            {
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
            }
            
            return query.ToList();
        }

        public Veiculo? BuscaPorId(int id)
        {
            return _application.Veiculos.Where(v => v.Id == id).FirstOrDefault();
        }

        public void  Incluir(Veiculo veiculo)
        {
            _application.Veiculos.Add(veiculo);
            _application.SaveChanges();
        }

        public void Atualizar(Veiculo veiculo)
        {
            _application.Veiculos.Update(veiculo);
            _application.SaveChanges();
        }

        public void Apagar(Veiculo veiculo)
        {
            _application.Veiculos.Remove(veiculo);
            _application.SaveChanges();
        }
    }
}