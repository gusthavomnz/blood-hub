CREATE TABLE doacoes (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    doador_id   INT             NOT NULL,
    data_doacao DATE            NOT NULL,
    volume_ml   INT             NOT NULL,
    observacao  VARCHAR(500)    NULL,
    criado_em   DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_doacoes_doador FOREIGN KEY (doador_id) REFERENCES doadores(id)
        ON DELETE RESTRICT
        ON UPDATE RESTRICT,
    CONSTRAINT ck_doacoes_volume CHECK (volume_ml >= 0),
    INDEX ix_doacoes_doador_data (doador_id, data_doacao)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;
