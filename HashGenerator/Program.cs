using System;

namespace HashGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var password = "Admin123!";
            
            // Generar 2 hashes diferentes
            var hash1 = BCrypt.Net.BCrypt.HashPassword(password, 11);
            var hash2 = BCrypt.Net.BCrypt.HashPassword(password, 11);
            
            Console.WriteLine("=== HASHES GENERADOS CON BCrypt.Net ===");
            Console.WriteLine($"Password: {password}");
            Console.WriteLine();
            Console.WriteLine($"Hash Admin: {hash1}");
            Console.WriteLine($"Verifica: {BCrypt.Net.BCrypt.Verify(password, hash1)}");
            Console.WriteLine();
            Console.WriteLine($"Hash Viewer: {hash2}");
            Console.WriteLine($"Verifica: {BCrypt.Net.BCrypt.Verify(password, hash2)}");
            Console.WriteLine();
            Console.WriteLine("=== COPIAR ESTOS SQL ===");
            Console.WriteLine($"UPDATE users SET password_hash = '{hash1}' WHERE email = 'admin@openuptool.com';");
            Console.WriteLine($"UPDATE users SET password_hash = '{hash2}' WHERE email = 'viewer@openuptool.com';");
        }
    }
}
