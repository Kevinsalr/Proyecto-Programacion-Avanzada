using System;

namespace EcoRutas.Service.Exceptions
{
    // Base de las excepciones de negocio. En la entrega final un filtro las traducirá a códigos HTTP.
    public abstract class NegocioException : Exception
    {
        protected NegocioException(string mensaje) : base(mensaje) { }
    }
    public class ValidacionException : NegocioException
    {
        public ValidacionException(string mensaje) : base(mensaje) { }
    }
    public class NoEncontradoException : NegocioException
    {
        public NoEncontradoException(string mensaje) : base(mensaje) { }
    }
    public class AccesoDenegadoException : NegocioException
    {
        public AccesoDenegadoException(string mensaje) : base(mensaje) { }
    }
    public class ConflictoException : NegocioException
    {
        public ConflictoException(string mensaje) : base(mensaje) { }
    }
}