-- Carga inicial de dados de exemplo para testes das regras de elegibilidade e relatórios
-- Cobre: doador apto, intervalo em vigor, limite anual atingido, doação recusada e doador inativo.

INSERT INTO doadores (id, nome, cpf, data_nascimento, tipo_sanguineo, fator_rh, sexo, peso, telefone, email, endereco, ativo) VALUES
(1, 'Carlos Eduardo Silva',    '12345678909', '1995-04-12', 'O',  '+', 'M', 75.50, '11987654321', 'carlos.silva@email.com',   'Rua das Flores, 123 - Centro', 1),
(2, 'Mariana Souza Santos',    '98765432100', '1998-09-20', 'A',  '+', 'F', 62.00, '11912345678', 'mariana.santos@email.com', 'Av. Paulista, 1000 - Bela Vista', 1),
(3, 'Juliana Costa Ferreira',  '11122233396', '1990-03-15', 'B',  '+', 'F', 68.00, '21998877665', 'juliana.costa@email.com',  'Rua Barão do Rio Branco, 45 - Botafogo', 1),
(4, 'Roberto Alves Lima',      '44455566619', '1982-11-05', 'AB', '-', 'M', 80.00, '31988776655', 'roberto.lima@email.com',   'Rua Bahia, 500 - Savassi', 1),
(5, 'Fernanda Ribeiro Gomes',  '77788899941', '1997-07-28', 'O',  '-', 'F', 56.50, '41977665544', 'fernanda.gomes@email.com', 'Rua XV de Novembro, 250 - Batel', 0);

INSERT INTO doacoes (id, doador_id, data_doacao, peso_kg, volume_ml, status, motivo_recusa, local_coleta, observacao) VALUES
-- Doador 1: Homem com doação há mais de 60 dias (APTO a doar hoje)
(1, 1, '2026-06-15', 75.50, 450, 'Realizada', NULL, 'Hemocentro Central', 'Doação voluntária regular'),

-- Doador 2: Mulher com doação há menos de 90 dias (REPROVADA por intervalo mínimo se doar hoje)
(2, 2, '2026-10-18', 62.00, 450, 'Realizada', NULL, 'Unidade Móvel Praça da Sé', 'Primeira doação da campanha'),

-- Doador 3: Mulher com 3 doações realizadas nos últimos 365 dias (REPROVADA por limite anual se doar hoje)
(3, 3, '2026-01-10', 68.00, 450, 'Realizada', NULL, 'Hemocentro Regional', 'Doação de rotina'),
(4, 3, '2026-04-20', 67.50, 450, 'Realizada', NULL, 'Hemocentro Regional', 'Doação de reposição'),
(5, 3, '2026-08-01', 68.00, 450, 'Realizada', NULL, 'Hemocentro Central',  'Campanha de inverno'),

-- Doador 4: Homem com 1 doação realizada e 1 doação RECUSADA na triagem (APTO a doar se peso normal, pois recusada não conta)
(6, 4, '2026-07-10', 80.00, 450, 'Realizada', NULL, 'Hemocentro Central', 'Doação regular'),
(7, 4, '2026-10-25', 48.00, 0,   'Recusada',  'Peso abaixo do mínimo permitido (50 kg)', 'Posto de Coleta Norte', 'Reprovado na triagem física'),

-- Doador 5: Mulher inativa com histórico sanitário preservado (bloqueia DELETE com RESTRICT; reprova se tentar nova doação)
(8, 5, '2025-11-15', 56.50, 450, 'Realizada', NULL, 'Hemocentro Central', 'Doação do ano anterior');
