namespace OrderApi.Models;

public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public DateTime Data { get; set; } = DateTime.UtcNow;
    public StatusPedido Status { get; set; } = StatusPedido.Criado;
    public decimal ValorTotal { get; set; }

    public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
}