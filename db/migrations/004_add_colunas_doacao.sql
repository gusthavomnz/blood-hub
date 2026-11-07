ALTER TABLE doacoes
    ADD COLUMN peso_kg DECIMAL(5,2) NOT NULL DEFAULT 50.00 AFTER data_doacao,
    ADD COLUMN status VARCHAR(20) NOT NULL DEFAULT 'Realizada' AFTER volume_ml,
    ADD COLUMN motivo_recusa VARCHAR(255) NULL AFTER status,
    ADD COLUMN local_coleta VARCHAR(120) NOT NULL DEFAULT 'Hemocentro Central' AFTER motivo_recusa,
    ADD CONSTRAINT ck_doacoes_status CHECK (status IN ('Realizada', 'Recusada')),
    ADD CONSTRAINT ck_doacoes_peso CHECK (peso_kg > 0);
