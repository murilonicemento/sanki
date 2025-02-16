# API Sanki

## Visão Geral

Esta API tem como objetivo gerenciar resumos e flashcards para auxiliar no aprendizado e revisão de conteúdo. Com ela, é possível criar, listar, atualizar e deletar resumos e flashcards, além de possibilitar a revisão baseada em espaçamento (Spaced Repetition). Os flashcards são gerados automaticamente por inteligência artificial com base nos resumos fornecidos pelo usuário, garantindo uma experiência de estudo otimizada e personalizada.

## Tecnologias Utilizadas

- **Linguagem:** C#/.NET
- **Framework:** ASP NET Core
- **Banco de Dados:** PostgreSQL
- **Autenticação:** JWT
- **Padrão Arquitetural:** RESTful
- **Containerização:** Docker Compose

## Instalação e Configuração

1. Clone o repositório:

   ```bash
   git clone https://github.com/murilonicemento/sanki.git
   ```

2. Acesse o diretório do projeto:

   ```bash
   cd sanki
   ```

3. Instale as dependências:

   ```bash
   dotnet restore
   ```

4. Configure a string de conexão no `appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=SEU_SERVIDOR;Database=NOME_DO_BANCO;Username=USUARIO;Password=SENHA;"
     }
   },
     "Jwt": {
    "Issuer": "http://localhost:5265",
    "Audience": "http://localhost:4200",
    "EXPIRATION_MINUTES": 1,
    "Key": "your secret key"
   },
   "RefreshToken": {
    "EXPIRATION_MINUTES": 60
   },
   "Gemini": {
    "ApiKey": "your api key",
    "Url": "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent"
   }
   ```

5. Suba o banco de dados com Docker Compose:

   ```bash
   docker-compose up -d
   ```

6. Execute as migrações para criar o banco de dados:

   ```bash
   dotnet ef database update
   ```

7. Inicie a API:

   ```bash
   dotnet run
   ```

## Endpoints Principais

### Usuário

- `POST /api/user` - Cria um novo usuário.

### Autenticação

- `POST /api/auth/login` - Autentica o usuário e retorna um token JWT.
- `POST /api/auth/GenerateNewToken` - Gera um novo token JWT.

### Resumos

- `GET /api/resume` - Lista todos os resumos do usuário autenticado.
- `POST /api/resume` - Cria um novo resumo.
- `PUT /api/resume/{id}` - Atualiza um resumo existente.
- `DELETE /api/resume/{id}` - Remove um resumo.

### Flashcards

- `GET /api/flashcard` - Lista todos os flashcards do usuário autenticado.
- `POST /api/flashcard` - Cria flashcards.

### Reviews

- `POST /api/review` - Salva nova data de revisão.

## Autenticação e Segurança

A API utiliza JWT (JSON Web Token) para autenticação. Para acessar os endpoints protegidos, é necessário incluir um token válido no cabeçalho da requisição:

```http
Authorization: Bearer SEU_TOKEN_AQUI
```

## Contribuição

Para contribuir com o projeto:

1. Faça um fork do repositório.
2. Crie uma branch para sua feature (`git checkout -b minha-feature`).
3. Faça commit das suas alterações (`git commit -m 'Adiciona nova feature'`).
4. Faça push para a branch (`git push origin minha-feature`).
5. Abra um Pull Request.

## Licença

Este projeto está licenciado sob a [MIT License](LICENSE).
