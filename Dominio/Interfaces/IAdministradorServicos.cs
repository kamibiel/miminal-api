using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mininal_api.Dominio.DTOs;
using mininal_api.Dominio.Entidades;

namespace mininal_api.Infraestrutura.Interfaces
{
    public interface IAdministradorServicos
    {
       Administrador? Login(LoginDTO loginDTO);
       Administrador Incluir(Administrador administrador);
       List<Administrador> Todos(int? pagina);
       Administrador? BuscaPorId(int id);
    }
}