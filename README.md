# Blood Hub — Sistema de Gestão de Doações de Sangue

Aplicação de console desenvolvida em **C# / .NET 10** sobre banco relacional **MySQL 8.4** em contêiner Docker. O acesso a dados é implementado com **ADO.NET puro** (`MySqlConnector`), sem a utilização de ORMs (como Entity Framework) ou micro-ORMs (como Dapper), atendendo aos requisitos acadêmicos da disciplina.

---

## 1. O Projeto e o Relacionamento entre Entidades

O sistema simula o controle sanitário de um hemocentro: gerencia o cadastro de **Doadores** e registra as **Doações** de sangue realizadas ao longo do tempo.

### Justificativa Funcional do Relacionamento (1:N)
O relacionamento entre `Doador` (1) e `Doacao` (N) possui justificativa clínica e sanitária real:
* A decisão de aceitar uma nova doação depende obrigatoriamente do **histórico de doações passadas** do doador.
* O sistema avalia se o intervalo mínimo de descanso sanitário foi respeitado e se o teto de doações por ano não foi excedido.
* Sem a associação direta entre a doação e o registro histórico do doador, o sistema seria incapaz de aplicar as regras de triagem do Ministério da Saúde.

### Regra de Integridade Referencial e Inativação
O histórico de doações possui valor médico-legal e sanitário inestimável para a rastreabilidade do sangue:
* A chave estrangeira `fk_doacoes_doador` é configurada com **`ON DELETE RESTRICT`**.
* O banco e a aplicação **bloqueiam a exclusão física** de qualquer doador que possua doações vinculadas.
* Quando a exclusão é bloqueada pelo banco, o sistema oferece a alternativa de **inativar o doador** (`ativo = false`), mantendo a integridade do histórico clínico.

---

## 2. Regras de Negócio e Elegibilidade Sanitária

A triagem clínica de novas doações avalia critérios obrigatórios:
1. **Status Cadastral:** O doador deve estar ativo no sistema.
2. **Faixa Etária Permitida:** Idade entre **16 e 69 anos** na data da doação.
3. **Peso Mínimo:** Peso na triagem de no mínimo **50,0 kg**.
4. **Intervalo Mínimo entre Doações Realizadas:**
   - **Homens:** mínimo de **60 dias** desde a última doação realizada.
   - **Mulheres:** mínimo de **90 dias** desde a última doação realizada.
5. **Limite Anual de Doações (últimos 365 dias):**
   - **Homens:** máximo de **4 doações** no período de 12 meses.
   - **Mulheres:** máximo de **3 doações** no período de 12 meses.
6. **Doações Recusadas:** Doações que não passaram na triagem podem ser registradas com o status `"Recusada"` e o respectivo motivo, mas **não contam** para o cálculo de intervalo nem para o limite anual.

---

## 3. Arquitetura MVC Adaptada ao Console

O projeto adota a separação estrita de responsabilidades em camadas desacopladas por interfaces:

```
src/BloodHub/
├── Modelos/          Doador, Doacao, ResultadoElegibilidade, ResumoPorTipoSanguineo, DoacoesPorMes
├── Data/             ConnectionFactory, DatabaseSettings
├── Repositorios/     IDoadorRepositorio / DoadorRepositorio
│                     IDoacaoRepositorio / DoacaoRepositorio
│                     IRelatorioRepositorio / RelatorioRepositorio
├── Servicos/         IElegibilidadeService / ElegibilidadeService
├── Controladores/    DoadorController, DoacaoController
├── Telas/            LeitorConsole, TelaDoador, TelaDoacao, TelaRelatorios, MenuPrincipal
├── Validacao/        Validadores, TradutorErroBanco
└── Program.cs
```

### Princípios Arquiteturais:
* **View (`Telas/`):** Camada exclusiva de I/O no terminal (menus, leitura tolerante a falhas com `LeitorConsole` e renderização de tabelas com `TelaRelatorios`). Nunca acessa banco nem escreve SQL.
* **Controller (`Controladores/`):** Orquestra o fluxo de dados, validações e persistência. **Não utiliza `Console` diretamente**, comunicando-se com a View através de parâmetros e retornos.
* **Model e Domain Services (`Modelos/` e `Servicos/`):** Regras de negócio de elegibilidade clínica isoladas de banco e console.
* **Data Access (`Repositorios/`):** Comandos SQL 100% parametrizados via ADO.NET (`MySqlCommand`, `MySqlDataReader`), gerenciamento seguro de conexões com `await using` e prevenção contra SQL Injection.

---

## 4. Como Rodar a Aplicação

### Pré-requisitos
* [Docker Desktop](https://docs.docker.com/get-started/get-docker/) instalado e em execução.
* [.NET 10 SDK](https://dotnet.microsoft.com/) (fixado via `global.json` para execução local/testes).

### Execução via Docker (Recomendado)
Para subir o banco MySQL 8.4 e iniciar a aplicação no terminal:

```powershell
docker compose run --rm --build app
```

Para encerrar os contêineres preservando os dados gravados no volume:
```powershell
docker compose down
```

Para reiniciar o banco descartando o volume e recriando do zero com os dados iniciais:
```powershell
docker compose down -v
```

---

## 5. Carga Inicial e Cenários para Demonstração

O banco é inicializado automaticamente na primeira criação do volume pelo script `db/migrations/005_seed_data.sql`, cobrindo todos os cenários para teste da banca avaliadora:

| ID | Doador | Sexo / Tipo | Histórico | Situação no Sistema |
| :-: | :--- | :-: | :--- | :--- |
| **1** | Carlos Eduardo Silva | M / O+ | Doou há > 140 dias | **Apto a doar hoje** (intervalo > 60d respeitado). |
| **2** | Mariana Souza Santos | F / A+ | Doou há 20 dias | **Reprovada por intervalo** (mulheres exigem 90d). |
| **3** | Juliana Costa Ferreira | F / B+ | 3 doações nos últimos 365d | **Reprovada por limite anual** (máximo 3 doações/ano). |
| **4** | Roberto Alves Lima | M / AB- | 1 doação realizada e 1 recusada | **Apto a doar** (a doação recusada não consome intervalo). |
| **5** | Fernanda Ribeiro Gomes | F / O- | 1 doação passada / **Inativa** | **Reprova nova doação** e **bloqueia exclusão física** por integridade referencial. |

### Consultas Analíticas (`GROUP BY`):
No submenu **Relatórios e Estatísticas**, estão disponíveis:
* **Total por tipo sanguíneo (`GROUP BY`):** consolida bolsas coletadas e volume em mL por tipo/fator Rh (`O+`, `A+`, `B+`, `AB-`, `O-`).
* **Doações realizadas por mês (`GROUP BY`):** consolida coletas por Ano e Mês no histórico.

---

## 6. Divisão de Tarefas da Equipe

| Integrante | Responsabilidade | Principais Entregáveis |
| :--- | :--- | :--- |
| **1: Infraestrutura e Integração** | Ambiente, Docker, scripts e junção | `docker-compose.yml`, migrations SQL, `global.json`, `README.md`, `ConnectionFactory`, integração e carga inicial. |
| **2: Doador (dados e validação)** | Persistência e integridade do doador | `DoadorRepositorio`, `Validadores` (CPF, idades, tamanhos) e `TradutorErroBanco`. |
| **3: Interface do Console** | Experiência de terminal e relatórios | `LeitorConsole`, `TelaRelatorios` (formatação de tabelas), `DoadorController` e menus do sistema. |
| **4: Doação (dados e consultas)** | Persistência e agregações analíticas | `DoacaoRepositorio` (CRUD, JOINs, filtros), `RelatorioRepositorio` (`GROUP BY`) e `DoacaoController`. |
| **5: Doação (regras e interface)** | Motor clínico e telas de doação | `ElegibilidadeService` (idade, peso, intervalos, limite anual), `TelaDoacao` e consulta de aptos hoje. |
