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
        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _application.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
            return adm;
            
        }
    }
}