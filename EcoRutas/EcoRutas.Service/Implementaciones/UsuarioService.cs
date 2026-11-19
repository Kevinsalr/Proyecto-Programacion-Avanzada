using System;
using System.Collections.Generic;
using System.Linq;
using EcoRutas.Data.Entities;
using EcoRutas.Repository.Interfaces;
using EcoRutas.Service.DTOs;
using EcoRutas.Service.Exceptions;
using EcoRutas.Service.Interfaces;
using Microsoft.AspNet.Identity;

namespace EcoRutas.Service.Implementaciones
{
    public class UsuarioService : IUsuarioService
    {
        private const string RolAdministrador = "Administrador";
        private const string RolGuia = "Guia";
        private static readonly string[] RolesValidos = { "Administrador", "Guia", "Asociado" };

        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsuarioService(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        // ---------- Consultas ----------

        public IList<UsuarioListadoDto> Listar(string busqueda, string rol, bool? activo)
        {
            var consulta = string.IsNullOrWhiteSpace(rol)
                ? _uow.Usuarios.Consultar()
                : _uow.Usuarios.ConsultarPorRol(rol);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                consulta = consulta.Where(u => u.NombreCompleto.Contains(texto)
                    || u.Email.Contains(texto) || u.CodigoUsuario.Contains(texto));
            }

            if (activo.HasValue)
            {
                var valor = activo.Value;
                consulta = consulta.Where(u => u.Activo == valor);
            }

            var nombresRoles = _uow.Usuarios.ObtenerNombresRoles();

            // Proyección: solo se leen las columnas que muestra la lista.
            var usuarios = consulta.OrderBy(u => u.NombreCompleto)
                .Select(u => new
                {
                    u.Id,
                    u.CodigoUsuario,
                    u.NombreCompleto,
                    u.Email,
                    u.Activo,
                    u.UltimaConexion,
                    RolId = u.Roles.Select(r => r.RoleId).FirstOrDefault()
                }).ToList();

            return usuarios.Select(u => new UsuarioListadoDto
            {
                Id = u.Id,
                CodigoUsuario = u.CodigoUsuario,
                NombreCompleto = u.NombreCompleto,
                Email = u.Email,
                Activo = u.Activo,
                UltimaConexion = u.UltimaConexion,
                Rol = u.RolId != null && nombresRoles.ContainsKey(u.RolId) ? nombresRoles[u.RolId] : null
            }).ToList();
        }

        public UsuarioGuardarDto ObtenerParaEditar(string id)
        {
            var usuario = ObtenerUsuario(id);
            return new UsuarioGuardarDto
            {
                Id = usuario.Id,
                CodigoUsuario = usuario.CodigoUsuario,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
                Rol = _userManager.GetRoles(usuario.Id).FirstOrDefault(),
                Activo = usuario.Activo
            };
        }

        // ---------- Comandos ----------

        public string Crear(UsuarioGuardarDto dto, string password)
        {
            ValidarCampos(dto);

            if (string.IsNullOrWhiteSpace(password))
                throw new ValidacionException("La contraseña es obligatoria.");

            var email = dto.Email.Trim();
            if (_userManager.FindByEmail(email) != null)
                throw new ConflictoException("Ya existe un usuario con ese correo electrónico.");

            var usuario = new ApplicationUser
            {
                CodigoUsuario = GenerarCodigo(),
                NombreCompleto = dto.NombreCompleto.Trim(),
                UserName = email,
                Email = email,
                Activo = dto.Activo
            };

            // Identity guarda la contraseña cifrada y valida sus reglas (IdentityConfig).
            VerificarResultado(_userManager.Create(usuario, password));

            var resultadoRol = _userManager.AddToRole(usuario.Id, dto.Rol);
            if (!resultadoRol.Succeeded)
            {
                _userManager.Delete(usuario);
                VerificarResultado(resultadoRol);
            }

            return usuario.Id;
        }

        public void Editar(UsuarioGuardarDto dto, string usuarioActualId)
        {
            var usuario = ObtenerUsuario(dto.Id);
            ValidarCampos(dto);

            var rolActual = _userManager.GetRoles(usuario.Id).FirstOrDefault();

            // Regla: el administrador no puede quitarse su propio acceso.
            if (usuario.Id == usuarioActualId && (dto.Rol != RolAdministrador || !dto.Activo))
                throw new ValidacionException("No puede quitarse el rol de administrador ni inactivar su propia cuenta.");

            // Regla: un guía con salidas futuras no puede dejar de ser guía ni inactivarse.
            if (rolActual == RolGuia && (dto.Rol != RolGuia || !dto.Activo)
                && _uow.Usuarios.TieneSalidasFuturas(usuario.Id))
                throw new ConflictoException(
                    "El guía tiene salidas futuras asignadas. Reasígnelas a otro guía antes de cambiarle el rol o inactivarlo.");

            var email = dto.Email.Trim();
            var otro = _userManager.FindByEmail(email);
            if (otro != null && otro.Id != usuario.Id)
                throw new ConflictoException("Ya existe otro usuario con ese correo electrónico.");

            usuario.NombreCompleto = dto.NombreCompleto.Trim();
            usuario.Email = email;
            usuario.UserName = email;
            usuario.Activo = dto.Activo;
            VerificarResultado(_userManager.Update(usuario));

            if (rolActual != dto.Rol)
            {
                if (rolActual != null)
                    VerificarResultado(_userManager.RemoveFromRole(usuario.Id, rolActual));
                VerificarResultado(_userManager.AddToRole(usuario.Id, dto.Rol));
            }
        }

        public void CambiarEstado(string id, bool activo, string usuarioActualId)
        {
            var usuario = ObtenerUsuario(id);

            if (!activo)
            {
                if (usuario.Id == usuarioActualId)
                    throw new ValidacionException("No puede inactivar su propia cuenta.");

                if (_userManager.IsInRole(usuario.Id, RolGuia) && _uow.Usuarios.TieneSalidasFuturas(usuario.Id))
                    throw new ConflictoException(
                        "El guía tiene salidas futuras asignadas. Reasígnelas a otro guía antes de inactivarlo.");
            }

            usuario.Activo = activo;
            _uow.Guardar();
        }

        public void Eliminar(string id, string usuarioActualId)
        {
            var usuario = ObtenerUsuario(id);

            if (usuario.Id == usuarioActualId)
                throw new ValidacionException("No puede eliminar su propia cuenta.");

            if (_uow.Usuarios.TieneDependencias(usuario.Id))
                throw new ConflictoException(
                    "El usuario tiene salidas, reservas o reseñas asociadas y no puede eliminarse. Puede inactivarlo.");

            VerificarResultado(_userManager.Delete(usuario));   // también borra su rol
        }

        // ---------- Auxiliares ----------

        private ApplicationUser ObtenerUsuario(string id)
        {
            var usuario = string.IsNullOrWhiteSpace(id) ? null : _uow.Usuarios.ObtenerPorId(id);
            if (usuario == null)
                throw new NoEncontradoException("El usuario solicitado no existe.");
            return usuario;
        }

        private string GenerarCodigo()
        {
            string codigo;
            do
            {
                codigo = "USR-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            } while (_uow.Usuarios.ExisteCodigo(codigo));
            return codigo;
        }

        private static void ValidarCampos(UsuarioGuardarDto dto)
        {
            if (dto == null) throw new ValidacionException("Datos del usuario incompletos.");

            Requerido(dto.NombreCompleto, "El nombre completo");
            Requerido(dto.Email, "El correo electrónico");

            if (dto.NombreCompleto.Trim().Length > 120)
                throw new ValidacionException("El nombre no puede superar 120 caracteres.");
            if (!RolesValidos.Contains(dto.Rol))
                throw new ValidacionException("Seleccione un rol válido.");
        }

        private static void Requerido(string valor, string campo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ValidacionException(campo + " es obligatorio.");
        }

        // Convierte los errores de Identity (contraseña débil, correo repetido...) en errores de negocio.
        private static void VerificarResultado(IdentityResult resultado)
        {
            if (!resultado.Succeeded)
                throw new ValidacionException(string.Join(" ", resultado.Errors));
        }
    }
}
