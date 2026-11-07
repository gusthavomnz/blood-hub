# Blood Hub

Aplicação de console em C#/.NET 10 com MySQL 8.4 no Docker. O acesso usa ADO.NET por meio do MySqlConnector, sem ORM.

## Como rodar

Requer Docker instalado e em execução.

```powershell
docker compose run --rm --build app
```

O comando sobe o banco e abre o menu principal. Para parar o banco sem apagar os dados:

```powershell
docker compose down
```

## Banco de dados

As migrations de `db/migrations` são aplicadas automaticamente somente quando o volume do MySQL é criado. A estrutura usa `doadores` e `doacoes`; cada doação referencia seu doador por chave estrangeira com `ON DELETE RESTRICT`.

Se o banco já tiver um volume criado antes dessas migrations, aplique os arquivos em ordem sem remover o volume. No PowerShell:

```powershell
docker compose up -d --wait db
Get-Content -Raw db/migrations/002_create_doadores.sql | docker compose exec -T db sh -c 'MYSQL_PWD="$MYSQL_ROOT_PASSWORD" exec mysql --default-character-set=utf8mb4 -uroot "$MYSQL_DATABASE"'
Get-Content -Raw db/migrations/003_create_doacoes.sql | docker compose exec -T db sh -c 'MYSQL_PWD="$MYSQL_ROOT_PASSWORD" exec mysql --default-character-set=utf8mb4 -uroot "$MYSQL_DATABASE"'
```

Execute cada migration uma vez e somente se as tabelas correspondentes ainda não existirem. O projeto não tem executor nem registro automático de migrations. `docker compose down -v` remove todos os dados do volume; use apenas ao recriar intencionalmente um banco descartável.

Se a porta local `3306` estiver indisponível, defina `MYSQL_PORT` no `.env`, por exemplo `33306`. O app dentro do Docker continua conectando à porta interna `3306`. O `.env` é local e ignorado pelo Git.

## Organização do código

| Pasta/arquivo | Responsabilidade |
| --- | --- |
| `Modelos` | Entidades `Doador` e `Doacao`, com o vínculo entre elas |
| `Repositorios/DoadorRepositorio` | CRUD e buscas com comandos SQL parametrizados |
| `Validacao/Validadores` | Normalização e validação cadastral antes de persistir |
| `Validacao/TradutorErroBanco` | Mensagens claras para CPF repetido e exclusão bloqueada |
| `Telas` | Leitura do console, menu, CRUD de doadores e listagens |
| `db/migrations` | Tabelas, índices, restrições e chave estrangeira |

O CPF é validado pelo formato e pelos dígitos verificadores e fica armazenado como 11 dígitos, preservando zeros à esquerda. A validação também verifica campos obrigatórios, tamanhos, data de nascimento, sexo, tipo sanguíneo e fator Rh. Telefone, e-mail e endereço são opcionais. O cadastro não aplica a faixa de idade nem o limite de peso para doação; esses critérios pertencem à regra de elegibilidade da tarefa 5.

Um doador com doações vinculadas não pode ser excluído. A interface oferece inativação quando o banco bloqueia a exclusão, preservando o histórico. O CPF único e a chave estrangeira protegem os dados também no próprio banco.

O repositório mantém os métodos assíncronos `InserirAsync`, `ListarTodosAsync`, `BuscarPorIdAsync`, `BuscarPorCpfAsync`, `BuscarPorNomeAsync`, `AtualizarAsync`, `InativarAsync` e `ExcluirAsync`. Atualizações normalizam os dados novamente e a busca por nome usa `LIKE` parametrizado com curingas tratados como texto literal.

## Variáveis de ambiente

O `docker-compose.yml` fornece valores padrão. Para personalizar a instalação local, copie `.env.example` para `.env` e ajuste as variáveis. A aplicação iniciada diretamente no computador não carrega `.env` automaticamente; o arquivo é lido pelo Docker Compose.
