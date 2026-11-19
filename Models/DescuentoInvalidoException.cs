using System;

namespace PrototipoCRUD.Models
{
    public class DescuentoInvalidoException : Exception
    {
        public DescuentoInvalidoException(string mensaje) : base(mensaje) { }
    }
}