using System;
using BCrypt.Net;

class HashPasswordHelper
{
    static void Main(string[] args)
    {
        string password = "Password123!";
        
        // Generar hash con BCrypt usando WorkFactor 11 (igual que el backend)
        string hash = BCrypt.Net.BCrypt.HashPassword(password, 11);
        
        Console.WriteLine("Password: " + password);
        Console.WriteLine("Hash: " + hash);
        Console.WriteLine();
        Console.WriteLine("Para verificar:");
        bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
        Console.WriteLine("Verificación: " + (isValid ? "✓ VÁLIDO" : "✗ INVÁLIDO"));
    }
}
