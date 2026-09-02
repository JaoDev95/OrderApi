Order API — Sistema de Pedidos com SQL Server + MongoDB

API REST em ASP.NET Core que gerencia clientes, produtos e pedidos, combinando dois bancos de dados de propósitos diferentes: SQL Server para dados transacionais e MongoDB para histórico de eventos.

Por que dois bancos?
SQL Server (relacional): clientes, produtos e pedidos têm relações bem definidas (um pedido pertence a um cliente, tem vários itens, cada item referencia um produto) e precisam de consistência forte. Isso pede um banco relacional, com chaves estrangeiras garantindo a integridade dos dados.
MongoDB (NoSQL): cada mudança de status de um pedido vira um evento de auditoria. Esse tipo de dado é majoritariamente escrita, não tem relações fixas, e cresce rápido com o tempo — um banco de documentos encaixa melhor aqui do que forçar uma tabela de log no relacional.
Arquitetura
Cliente / Produto / Pedido / ItemPedido  →  SQL Server (via Entity Framework Core)
Eventos do Pedido (PedidoCriado, StatusAtualizado, ...)  →  MongoDB (via MongoDB.Driver)

Quando um pedido é criado ou muda de status, a API grava o dado "oficial" no SQL Server e, na mesma operação, registra um evento no MongoDB descrevendo o que aconteceu — sem misturar as responsabilidades dos dois bancos.

Endpoints
Método	Rota	Descrição
GET	/api/clientes	Lista clientes
POST	/api/clientes	Cria cliente
GET	/api/produtos	Lista produtos
POST	/api/produtos	Cria produto
GET	/api/produtos/{id}	Busca produto por Id
POST	/api/pedidos	Cria pedido (calcula total, grava no SQL + evento no Mongo)
GET	/api/pedidos/{id}	Busca pedido com seus itens (SQL Server)
PUT	/api/pedidos/{id}/status	Atualiza status (grava novo evento no Mongo)
GET	/api/pedidos/{id}/historico	Histórico completo de eventos do pedido (MongoDB)

O endpoint GET /api/pedidos/{id}/historico é o que evidencia a integração: ele confirma que o pedido existe no SQL Server e devolve seu histórico de eventos vindo do MongoDB.

Stack
ASP.NET Core 8 (Controllers)
Entity Framework Core + SQL Server (LocalDB em desenvolvimento)
MongoDB.Driver + MongoDB Atlas (free tier)
Swagger / OpenAPI para documentação e testes manuais
Decisões técnicas
Preço "congelado" no pedido: ItemPedido.PrecoUnitario guarda o preço do produto no momento da compra, não uma referência ao preço atual — evita que o histórico de um pedido mude se o preço do produto for atualizado depois.
Enum de status como texto: StatusPedido é salvo como string no banco (HasConversion<string>()) e serializado como string na API (JsonStringEnumConverter), deixando o dado legível tanto no banco quanto na API, em vez de números arbitrários.
Ciclos de referência tratados: como Pedido e ItemPedido referenciam um ao outro, o serializador JSON usa ReferenceHandler.IgnoreCycles para evitar loop infinito na resposta.
Como rodar localmente
Pré-requisitos
.NET 8 SDK
SQL Server LocalDB (ou outra instância SQL Server)
Uma conta no MongoDB Atlas (free tier) ou MongoDB local
Configuração

Este projeto usa appsettings.Development.json (não versionado) para guardar as credenciais reais de conexão. Crie esse arquivo na raiz do projeto com:

json
{
  "ConnectionStrings": {
    "SqlServer": "Server=(localdb)\\mssqllocaldb;Database=OrderApiDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "MongoDb": "mongodb+srv://SEU_USUARIO:SUA_SENHA@SEU_CLUSTER.mongodb.net/?appName=SeuApp"
  },
  "MongoDatabaseName": "OrderApiEvents"
}
Passos
Restaura os pacotes e aplica as migrations:
   dotnet restore
   dotnet ef database update
Roda a API:
   dotnet run
Acessa o Swagger em https://localhost:{porta}/swagger para testar os endpoints
Próximos passos (evolução planejada)
Autenticação com JWT
Testes automatizados (xUnit)
Deploy da API no Azure App Service
Docker Compose subindo API + SQL Server + MongoDB juntos