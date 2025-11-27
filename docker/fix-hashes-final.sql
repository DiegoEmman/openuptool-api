-- Actualizar hashes generados con BCrypt.Net WorkFactor 11
UPDATE users SET password_hash = '$2a$11$TpxL4BUA9sGUXtCnQnYeD.dCzyvnU4q2ghle//J7jmB09CklHojwy' WHERE email = 'admin@openuptool.com';
UPDATE users SET password_hash = '$2a$11$VWK/CudNHMCW5uJS3TF.o.Igqo4tD7XvKf7g93xTR/N8EgBlidZBK' WHERE email = 'viewer@openuptool.com';

-- Verificar
SELECT email, LEFT(password_hash, 30) as hash_start FROM users;
