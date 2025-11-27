#!/usr/bin/env dotnet-script
#r "nuget: BCrypt.Net-Next, 4.0.3"

using BCrypt.Net;

// Generar hashes para Admin123!
var password = "Admin123!";
var hash1 = BCrypt.HashPassword(password, 11);
var hash2 = BCrypt.HashPassword(password, 11);

Console.WriteLine("=== HASHES GENERADOS ===");
Console.WriteLine($"Password: {password}");
Console.WriteLine();
Console.WriteLine($"Hash 1 (Admin): {hash1}");
Console.WriteLine($"Verificación: {BCrypt.Verify(password, hash1)}");
Console.WriteLine();
Console.WriteLine($"Hash 2 (Viewer): {hash2}");
Console.WriteLine($"Verificación: {BCrypt.Verify(password, hash2)}");
Console.WriteLine();
Console.WriteLine("=== SQL UPDATE STATEMENTS ===");
Console.WriteLine($"UPDATE users SET password_hash = '{hash1}' WHERE email = 'admin@openuptool.com';");
Console.WriteLine($"UPDATE users SET password_hash = '{hash2}' WHERE email = 'viewer@openuptool.com';");
