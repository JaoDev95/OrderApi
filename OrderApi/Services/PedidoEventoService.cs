using MongoDB.Driver;
using OrderApi.Data;
using OrderApi.Models;

namespace OrderApi.Services;

public class PedidoEventoService
{
    private readonly IMongoCollection<PedidoEvento> _eventos;

    public PedidoEventoService(MongoContext context)
    {
        _eventos = context.Database.GetCollection<PedidoEvento>("PedidoEventos");
    }

    public async Task RegistrarEventoAsync(int pedidoId, string tipoEvento, string? detalhes = null)
    {
        var evento = new PedidoEvento
        {
            PedidoId = pedidoId,
            TipoEvento = tipoEvento,
            Detalhes = detalhes,
            Timestamp = DateTime.UtcNow
        };

        await _eventos.InsertOneAsync(evento);
    }

    public async Task<List<PedidoEvento>> ObterHistoricoAsync(int pedidoId)
    {
        return await _eventos
            .Find(e => e.PedidoId == pedidoId)
            .SortBy(e => e.Timestamp)
            .ToListAsync();
    }
}