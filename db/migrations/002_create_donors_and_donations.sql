CREATE TABLE doador (
    id               INT AUTO_INCREMENT PRIMARY KEY,
    nome             VARCHAR(120) NOT NULL,
    cpf              CHAR(11) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    data_nascimento  DATE NOT NULL,
    sexo             CHAR(1) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    tipo_sanguineo   VARCHAR(3) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    telefone         VARCHAR(20) NULL,
    ativo            BOOLEAN NOT NULL DEFAULT TRUE,

    CONSTRAINT uq_doador_cpf UNIQUE (cpf),
    CONSTRAINT ck_doador_nome CHECK (CHAR_LENGTH(TRIM(nome)) > 0),
    CONSTRAINT ck_doador_cpf_tamanho CHECK (CHAR_LENGTH(cpf) = 11),
    CONSTRAINT ck_doador_sexo CHECK (sexo IN ('M', 'F')),
    CONSTRAINT ck_doador_tipo_sanguineo CHECK (
        tipo_sanguineo IN ('A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-')
    ),
    CONSTRAINT ck_doador_ativo CHECK (ativo IN (FALSE, TRUE))
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

CREATE TABLE doacao (
    id             INT AUTO_INCREMENT PRIMARY KEY,
    doador_id      INT NOT NULL,
    data_doacao    DATETIME NOT NULL,
    peso_kg        DECIMAL(5, 2) NOT NULL,
    volume_ml      INT NOT NULL,
    status         VARCHAR(10) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    motivo_recusa  VARCHAR(255) NULL,
    local_coleta   VARCHAR(120) NOT NULL,

    CONSTRAINT fk_doacao_doador FOREIGN KEY (doador_id)
        REFERENCES doador (id)
        ON DELETE RESTRICT
        ON UPDATE RESTRICT,
    CONSTRAINT ck_doacao_peso CHECK (peso_kg > 0),
    CONSTRAINT ck_doacao_volume CHECK (volume_ml >= 0),
    CONSTRAINT ck_doacao_status CHECK (status IN ('Realizada', 'Recusada')),
    CONSTRAINT ck_doacao_local CHECK (CHAR_LENGTH(TRIM(local_coleta)) > 0),

    INDEX ix_doacao_doador_data (doador_id, data_doacao),
    INDEX ix_doacao_data (data_doacao)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;
