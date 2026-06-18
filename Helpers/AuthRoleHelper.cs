namespace WebColegio.Helpers
{
    public static class AuthRoleHelper
    {
        /// <summary>
        /// Nombre de rol para claims del menú lateral (_Layout usa Admin, Cajero, Docente, etc.).
        /// </summary>
        public static string ResolverNombreRol(string? nombreRolApi, int idRol)
        {
            var normalizado = NormalizarNombreRol(nombreRolApi);
            if (!string.IsNullOrWhiteSpace(normalizado))
                return normalizado;

            return idRol switch
            {
                1 => "Admin",
                2 => "Cajero",
                3 => "Docente",
                4 => "Tutor",
                5 => "Secretaria",
                6 => "Admin",
                _ => $"Rol_{idRol}"
            };
        }

        private static string? NormalizarNombreRol(string? nombreRol)
        {
            if (string.IsNullOrWhiteSpace(nombreRol))
                return null;

            var n = nombreRol.Trim();
            return n.ToLowerInvariant() switch
            {
                "admin" or "administrador" or "administrator" => "Admin",
                "cajero" or "cajera" => "Cajero",
                "docente" or "profesor" or "profesora" => "Docente",
                "secretaria" or "secretaría" or "secretario" => "Secretaria",
                "director" or "directora" => "Director",
                "usersystem" or "user system" or "sistema" => "UserSystem",
                "tutor" or "tutora" => "Tutor",
                "inventario" => "Secretaria",
                _ => n
            };
        }
    }
}
