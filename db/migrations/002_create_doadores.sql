CREATE TABLE doadores (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    nome            VARCHAR(120)    NOT NULL,
    cpf             VARCHAR(14)     NOT NULL UNIQUE,
    data_nascimento DATE            NOT NULL,
    tipo_sanguineo  VARCHAR(3)      NOT NULL,
    fator_rh        CHAR(1)         NOT NULL,
    sexo            CHAR(1)         NOT NULL,
    peso            DECIMAL(5,2)    NOT NULL,
    telefone        VARCHAR(20)     NOT NULL,
    email           VARCHAR(150)    NULL,
    endereco        VARCHAR(255)    NULL,
    ativo           TINYINT(1)      NOT NULL DEFAULT 1,
    criado_em       DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    atualizado_em   DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
