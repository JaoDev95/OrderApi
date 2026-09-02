using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OrderApi.Models;

public class PedidoEvento
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public int PedidoId { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Detalhes { get; set; }
}