# AgroSolutions Users API

API REST para gerenciamento de usuários do sistema AgroSolutions, desenvolvida para o projeto HACKATHON 8NETT.

## Descrição

API responsável pelo gerenciamento completo do ciclo de vida de usuários, incluindo registro, autenticação via JWT e gestão de credenciais. Implementa princípios de Clean Architecture com separação clara de responsabilidades entre as camadas.

## Tecnologias

- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core 8.0
- PostgreSQL
- JWT Bearer Authentication
- Swagger/OpenAPI
- C# 12

## Arquitetura

O projeto segue os princípios da Clean Architecture, organizado em quatro camadas principais:

### Core
Camada de domínio que contém as entidades de negócio e interfaces de repositórios. Não possui dependências externas.

- **Entities**: Entidades de domínio (User)
- **ValueObjects**: Objetos de valor (Email, Password)
- **Interfaces/Repositories**: Contratos para acesso a dados

### Application
Camada de aplicação que contém a lógica de negócio e casos de uso.

- **Services**: Serviços de aplicação (UserAppService)
- **DTOs**: Objetos de transferência de dados
- **Contracts**: Interfaces de serviços (IAuthService, ICurrentUserService)
- **Exceptions**: Exceções customizadas de negócio

### Infrastructure
Camada de infraestrutura que implementa os detalhes técnicos e persistência.

- **Persistence**: Contexto do EF Core e configurações de entidades
- **Repository**: Implementação dos repositórios
- **Services**: Implementação de serviços de infraestrutura (AuthService, CurrentUserService)

### AgroSolutions.Users
Camada de apresentação que expõe a API REST.

- **Controllers**: Endpoints da API
- **Middlewares**: Middlewares customizados
- **Program.cs**: Configuração da aplicação

## Estrutura de Pastas

```
agro-solutions-users/
├── AgroSolutions.Users/       # API Web (Presentation Layer)
│   ├── Controllers/           # Controladores REST
│   ├── Middlewares/           # Middlewares customizados
│   └── Program.cs             # Configuração e startup
├── Application/               # Camada de Aplicação
│   ├── Contracts/             # Interfaces de serviços
│   ├── DTOs/                  # Data Transfer Objects
│   ├── Exceptions/            # Exceções de negócio
│   └── Services/              # Serviços de aplicação
├── Core/                      # Camada de Domínio
│   ├── Entities/              # Entidades de domínio
│   ├── Interfaces/            # Contratos de repositórios
│   └── ValueObjects/          # Objetos de valor
└── Infrastructure/            # Camada de Infraestrutura
    ├── Persistence/           # Contexto e configurações do EF Core
    └── Services/              # Implementações de serviços
```

## Pré-requisitos

- .NET 8.0 SDK ou superior
- PostgreSQL 12 ou superior
- IDE compatível (Visual Studio 2022, Visual Studio Code, Rider)

## Configuração

### 1. Connection String

Edite o arquivo `appsettings.json` ou `appsettings.Development.json` e configure a string de conexão do PostgreSQL:

```json
{
  "ConnectionStrings": {
    "ConnectionString": "Host=localhost;Port=5432;Database=agrosolutions_users;Username=seu_usuario;Password=sua_senha"
  }
}
```

### 2. Configuração JWT

Configure as chaves de autenticação JWT no arquivo de configuração:

```json
{
  "Jwt": {
    "Key": "sua_chave_secreta_com_minimo_32_caracteres",
    "Issuer": "AgroSolutions.API",
    "Audience": "AgroSolutions.Client"
  }
}
```

### 3. Migrations

Execute as migrations para criar o banco de dados:

```bash
cd AgroSolutions.Users
dotnet ef database update
```

## Execução

### Modo Development

```bash
cd AgroSolutions.Users
dotnet run
```

A API estará disponível em:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

### Modo Production

```bash
cd AgroSolutions.Users
dotnet publish -c Release -o ./publish
cd publish
dotnet AgroSolutions.Users.dll
```

## Endpoints da API

### Autenticação

#### POST /api/auth/login
Autentica um usuário e retorna um token JWT.

**Request Body:**
```json
{
  "email": "usuario@exemplo.com",
  "password": "senha123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Usuários

#### POST /api/users/register
Registra um novo usuário no sistema.

**Request Body:**
```json
{
  "email": "usuario@exemplo.com",
  "password": "senha123"
}
```

**Response:**
```json
{
  "message": "Usuário registrado com sucesso"
}
```

#### PUT /api/users/change-password
Altera a senha do usuário autenticado. Requer autenticação.

**Headers:**
```
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "currentPassword": "senha_atual",
  "newPassword": "nova_senha"
}
```

**Response:**
```
204 No Content
```

## Autenticação e Autorização

A API utiliza autenticação baseada em JWT (JSON Web Tokens). O fluxo é o seguinte:

1. O cliente envia credenciais para `/api/auth/login`
2. A API valida as credenciais e retorna um token JWT
3. O cliente inclui o token no header `Authorization: Bearer {token}` nas requisições subsequentes
4. Endpoints protegidos validam o token antes de processar a requisição

### Configuração do Token

- **Algoritmo**: HS256 (HMAC SHA256)
- **Validações**: Issuer, Audience, Lifetime, Signing Key
- **Claims**: UserId, Email

## Banco de Dados

### Tecnologia
PostgreSQL com Entity Framework Core como ORM.

### Entidades

#### Users
```
Id (int, PK)
Email (string)
Password (string, hashed)
CreatedAt (datetime)
```

### Configurações

- **Email**: Value Object com validação de formato
- **Password**: Value Object com hash automático usando ASP.NET Core Identity PasswordHasher
- **Índices**: Email (único)

## Value Objects

### Email
Garante que o endereço de e-mail seja válido e normalizado.

### Password
Encapsula a lógica de hash e verificação de senhas, utilizando o PasswordHasher do ASP.NET Core Identity para segurança robusta.

## Tratamento de Exceções

A API implementa tratamento centralizado de exceções através de middlewares:

### Exceções Customizadas

- **BusinessException**: Erros de regra de negócio (400 Bad Request)
- **ValidationException**: Erros de validação (400 Bad Request)
- **NotFoundException**: Recurso não encontrado (404 Not Found)
- **ConflictException**: Conflito de dados (409 Conflict)
- **UnauthorizedException**: Não autorizado (401 Unauthorized)

### Formato de Resposta de Erro

```json
{
  "message": "Descrição do erro",
  "correlationId": "guid-correlacao",
  "statusCode": 400
}
```

## Middlewares

### ExceptionHandlingMiddleware
Captura e trata todas as exceções da aplicação, retornando respostas padronizadas.

### CorrelationMiddleware
Adiciona um ID de correlação a cada requisição para rastreabilidade em logs.

### ValidateUserExistsMiddleware
Valida a existência do usuário autenticado antes de processar requisições protegidas.

### StatusCodePagesExtensions
Trata códigos de status HTTP específicos com respostas customizadas.

## Pacotes NuGet Principais

### AgroSolutions.Users
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.0)
- Npgsql.EntityFrameworkCore.PostgreSQL (8.0.11)
- Swashbuckle.AspNetCore (6.6.2)
- Microsoft.EntityFrameworkCore.Design (8.0.0)

### Infrastructure
- Microsoft.EntityFrameworkCore (8.0.22)
- Npgsql.EntityFrameworkCore.PostgreSQL (8.0.11)
- System.IdentityModel.Tokens.Jwt (8.15.0)
- Microsoft.EntityFrameworkCore.Tools (8.0.22)

## Injeção de Dependências

A aplicação utiliza o container de DI nativo do ASP.NET Core:

```csharp
// Repositórios
services.AddScoped<IUserRepository, UserRepository>();

// Serviços de Aplicação
services.AddScoped<UserAppService>();
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<ICurrentUserService, CurrentUserService>();

// Contexto
services.AddDbContext<ApplicationDbContext>();
```

## Boas Práticas Implementadas

- Clean Architecture com separação clara de responsabilidades
- SOLID principles
- Repository Pattern para abstração de acesso a dados
- Value Objects para encapsulamento de lógica de domínio
- DTOs para transferência de dados entre camadas
- Tratamento centralizado de exceções
- Autenticação segura com JWT
- Hash de senhas com algoritmos robustos
- Validação de dados em múltiplas camadas
- Middlewares para concerns transversais
- Documentação automática com Swagger/OpenAPI

## Desenvolvimento

### Adicionar Migration

```bash
cd AgroSolutions.Users
dotnet ef migrations add NomeDaMigration
```

### Reverter Migration

```bash
cd AgroSolutions.Users
dotnet ef migrations remove
```

### Atualizar Banco de Dados

```bash
cd AgroSolutions.Users
dotnet ef database update
```

## Ambiente de Desenvolvimento

### Visual Studio 2022
1. Abrir `AgroSolutions.Users.sln`
2. Configurar `AgroSolutions.Users` como projeto de inicialização
3. Pressionar F5 para executar

### Visual Studio Code
1. Abrir a pasta do projeto
2. Configurar `launch.json` e `tasks.json`
3. Pressionar F5 para executar

### Rider
1. Abrir `AgroSolutions.Users.sln`
2. Executar configuração padrão

## Segurança

- Senhas são hasheadas usando ASP.NET Core Identity PasswordHasher
- Tokens JWT com expiração configurável
- Validação de entrada em todas as camadas
- HTTPS habilitado por padrão
- Proteção contra CORS configurável
- Headers de segurança implementados via middlewares

## Performance

- Uso de async/await para operações I/O
- DbContext com escopo por requisição
- Queries otimizadas com Entity Framework Core
- Connection pooling do Npgsql

## Testes

A estrutura do projeto facilita a implementação de testes unitários e de integração:

- Camada Core pode ser testada isoladamente
- Serviços de Application podem usar mocks de repositórios
- Controllers podem ser testados com TestServer

## Versionamento

A API utiliza versionamento semântico (SemVer):
- Versão atual: 1.0.0

## Licença

Este projeto foi desenvolvido para o HACKATHON 8NETT.

## Suporte

Para questões técnicas ou reportar problemas, abra uma issue no repositório do projeto.
