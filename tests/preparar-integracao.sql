-- Executar uma vez pelo cliente MySQL dentro do container, com as credenciais padrão.
CREATE DATABASE IF NOT EXISTS bloodhub_tests CHARACTER SET utf8mb4;
GRANT SELECT, INSERT, UPDATE, DELETE ON bloodhub_tests.* TO 'bloodhub'@'%';
USE bloodhub_tests;
SOURCE /docker-entrypoint-initdb.d/002_create_doadores.sql;
SOURCE /docker-entrypoint-initdb.d/003_create_doacoes.sql;
SOURCE /docker-entrypoint-initdb.d/004_add_colunas_doacao.sql;
