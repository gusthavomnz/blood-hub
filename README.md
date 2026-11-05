# Blood Hub

Aplicação console em C# / .NET 10 com MySQL 8.4, via Docker. O acesso ao banco usa ADO.NET com MySqlConnector, sem ORM.

## Como rodar

Requer [Docker](https://docs.docker.com/get-started/get-docker/) instalado e aberto.

Se acabou de instalar o Docker e o terminal não reconhece `docker`, reabra o terminal para atualizar o caminho dos programas.

```bash
docker compose run --rm --build app
```

O banco sobe automaticamente. O menu inicial reúne o CRUD de doadores; use a opção de saída ou digite `sair` para encerrar.

O banco continua rodando em segundo plano depois que a aplicação fecha. Para desligá-lo:

```bash
docker compose down
```

## Migrations

Coloque os arquivos `.sql` em `db/migrations/`. Eles rodam em ordem alfabética, então use prefixo numérico: `001_...sql`, `002_...sql`.

Os scripts só rodam automaticamente quando o volume do banco é criado pela primeira vez. Adicionar um arquivo nesta pasta não atualiza um banco já existente.

Para um banco criado antes da implementação de doadores, inicie o serviço e aplique somente a migration nova. No PowerShell, a partir da raiz do repositório:

```powershell
docker compose up -d --wait db
Get-Content -Raw db/migrations/002_create_donors_and_donations.sql | docker compose exec -T db sh -c 'MYSQL_PWD="$MYSQL_PASSWORD" exec mysql --default-character-set=utf8mb4 -u"$MYSQL_USER" "$MYSQL_DATABASE"'
```

Esse comando cria as tabelas de domínio sem apagar a tabela de exemplo nem os dados existentes. Execute-o uma única vez se as tabelas `doador` e `doacao` ainda não existirem. O projeto ainda não possui um executor automático de migrations ou controle de versões aplicadas.

Para recriar intencionalmente um ambiente descartável do zero, o comando abaixo apaga o volume e todos os seus dados; a próxima inicialização executará todos os scripts:

```bash
docker compose down -v
```

### Estrutura do domínio

- `001_create_health_check_history.sql`: tabela de exemplo da infraestrutura inicial.
- `002_create_donors_and_donations.sql`: tabelas `doador` e `doacao`, relacionamento 1:N, CPF único e índices para histórico e período.

Uma doação pertence a um doador. A chave estrangeira usa `ON DELETE RESTRICT`: um doador com qualquer doação vinculada não pode ser excluído. A inativação preserva o cadastro e o histórico.

## Tarefa 2: dados e validação de doadores

A implementação aproveita `Data/ConnectionFactory.OpenAsync()`. As responsabilidades ficam separadas:

| Parte | Responsabilidade |
| --- | --- |
| `Models/Doador` e `Models/Doacao` | Representar as duas entidades e o relacionamento |
| `Regras/Validadores` | Normalizar e validar os dados cadastrais sem acessar o banco ou o console |
| `Data/DoadorRepositorio` | Executar o CRUD com SQL parametrizado e mapear resultados para objetos |
| `Data/TradutorErroBanco` | Converter erros de banco em mensagens que a tela pode apresentar |

O CPF é armazenado como texto de 11 dígitos, preservando zeros iniciais. Os validadores verificam formato, dígitos verificadores, nome obrigatório, nascimento não futuro, sexo e os oito tipos sanguíneos. Telefone é opcional. Os limites de texto acompanham os tamanhos das colunas do banco.

Calcular idade e validar o cadastro não significa decidir se alguém pode doar. Peso, intervalo, limite anual e demais critérios de elegibilidade pertencem à tarefa 5. O CRUD deve preservar cadastros e histórico mesmo quando a pessoa não pode doar hoje.

### Contratos para integrar as telas

Os métodos de acesso ao banco são assíncronos e devem ser chamados com `await`:

| Método | Operação |
| --- | --- |
| `InserirAsync(doador)` | Cadastrar e devolver o ID gerado |
| `ListarAsync()` | Listar cadastros, incluindo sua situação ativa/inativa |
| `BuscarPorIdAsync(id)` | Buscar um doador ou devolver `null` |
| `BuscarPorCpfAsync(cpf)` | Buscar pelo CPF normalizado ou devolver `null` |
| `BuscarPorNomeAsync(nome)` | Buscar parte do nome com `LIKE` parametrizado |
| `AtualizarAsync(doador)` | Atualizar o cadastro pelo ID |
| `InativarAsync(id)` | Marcar o cadastro como inativo, preservando histórico |
| `ExcluirAsync(id)` | Excluir somente quando não houver doações vinculadas |

Uma lista vazia representa uma busca sem resultados; falha de banco deve seguir o tratamento de erros. Atualizar um doador mantendo seu próprio CPF é permitido, mas usar o CPF de outro cadastro é bloqueado pela restrição única.

A tabela de doações já permite integrar as próximas partes do grupo. O CRUD de doações, os relatórios e o serviço de elegibilidade serão desenvolvidos nas tarefas correspondentes.

## Variáveis de ambiente

Opcional. O `docker-compose.yml` já tem valores padrão para tudo. Para alterá-los:

```bash
cp .env.example .env
```

Se a porta `3306` estiver indisponível no computador, altere `MYSQL_PORT` no `.env`, por exemplo para `33306`. A aplicação dentro do Docker continua usando a porta interna `3306` automaticamente. O `.env` é local e não é enviado ao Git.

Deixar credenciais padrão no compose não é a prática mais correta. Fiz isso apenas para facilitar rodar o projeto.

## Verificações

Com o SDK .NET 10 instalado, compile a solução e rode as verificações de CPF, normalização, limites e idade. Essa etapa não precisa de banco:

```powershell
dotnet build BloodHub.slnx
dotnet run --project tests/BloodHub.Checks
```

Para verificar o repositório com MySQL real, prepare uma vez o banco isolado `bloodhub_tests`. Os comandos abaixo usam o usuário padrão `bloodhub`; se ele foi alterado no `.env`, ajuste a permissão no script de preparação. O script não altera o banco da aplicação:

```powershell
docker compose up -d --wait db
Get-Content -Raw tests/preparar-integracao.sql | docker compose exec -T db sh -c 'MYSQL_PWD="$MYSQL_ROOT_PASSWORD" exec mysql --default-character-set=utf8mb4 -uroot'
$env:MYSQL_PORT = "33306" # use a porta definida no seu .env; o padrão do compose é 3306
$env:BLOODHUB_TEST_DATABASE = "bloodhub_tests"
dotnet run --project tests/BloodHub.Checks -- --integration
```

As verificações de integração recusam nomes sem o sufixo `_tests` e bancos que já contenham doadores. Exercitam o CRUD, CPF duplicado no cadastro e na atualização, pesquisa literal com `LIKE`, inativação repetida e exclusão bloqueada por doações. Os registros sintéticos criados são removidos ao terminar.

Para rodar a aplicação C# diretamente no computador com o banco do Docker, defina `MYSQL_PORT` no terminal conforme o `.env` e execute `dotnet run --project src/BloodHub`. A leitura automática do arquivo `.env` é feita pelo Docker Compose, não pela aplicação C#.
