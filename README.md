# Blood Hub

Aplicação console em C# / .NET 10 com MySQL, via Docker.

## Como rodar

Requer [Docker](https://docs.docker.com/get-started/get-docker/) instalado e aberto.

```bash
docker compose run --rm --build app
```

O banco sobe automaticamente. Para sair da aplicação, digite `sair`.

O banco continua rodando em segundo plano depois que a aplicação fecha. Para desligá-lo:

```bash
docker compose down
```

## Migrations

Coloque os arquivos `.sql` em `db/migrations/`. Eles rodam em ordem alfabética, então use prefixo numérico: `001_...sql`, `002_...sql`.

Os scripts só rodam quando o banco é criado pela primeira vez. Para recriar o banco e rodar tudo de novo (apaga os dados):

```bash
docker compose down -v
```

## Variáveis de ambiente

Opcional. O `docker-compose.yml` já tem valores padrão para tudo. Para alterá-los:

```bash
cp .env.example .env
```

Deixar credenciais padrão no compose não é a prática mais correta. Fiz isso apenas para facilitar rodar o projeto.
