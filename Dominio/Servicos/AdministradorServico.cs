using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mininal_api.Dominio.DTOs;
using mininal_api.Dominio.Entidades;
using mininal_api.Infraestrutura.Db;
using mininal_api.Infraestrutura.Interfaces;

namespace mininal_api.Dominio.Servicos
{
    public class AdministradorServico : IAdministradorServicos
    {
        private readonly ApplicationDbContext _application;
        public AdministradorServico(ApplicationDbContext application)
        {
            _application = application;
        }

        public Administrador Incluir(Administrador administrador)
        {
            _application.Administradores.Add(administrador);
            _application.SaveChanges();

            return administrador;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _application.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
            return adm;            
        }

        public List<Administrador> Todos(int? pagina)
        {
            var query = _application.Administradores.AsQueryable();
            
            int itensPorPagina = 10;

            if(pagina != null)
            {
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
            }
            
            return query.ToList();
        }

        public Administrador? BuscaPorId(int id)
        {
            return _application.Administradores.Where(a => a.Id == id).FirstOrDefault();
        }
    }
}