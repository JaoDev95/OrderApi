using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Models;
using OrderApi.Services;

namespace OrderApi.Controllers;

public record CriarPedidoRequest(int ClienteId, List<ItemPedidoRequest> Itens);
public record ItemPedidoRequest(int ProdutoId, int Quantidade);
public record AtualizarStatusRequest(StatusPedido NovoStatus);

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PedidoEventoService _eventoService;

    public PedidosController(AppDbContext context, PedidoEventoService eventoService)
    {
        _context = context;
        _eventoService = eventoService;
    }

    [HttpPost]
    public async Task<ActionResult<Pedido>> Create(CriarPedidoRequest request)
    {
        var pedido = new Pedido
        {
            ClienteId = request.ClienteId,
            Status = StatusPedido.Criado,
            Data = DateTime.UtcNow
        };

        decimal total = 0;

        foreach (var itemReq in request.Itens)
        {
            var produto = await _context.Produtos.FindAsync(itemReq.ProdutoId);
            if (produto is null)
                return BadRequest($"Produto {itemReq.ProdutoId} não encontrado.");

            var item = new ItemPedido
            {
                ProdutoId = produto.Id,
                Quantidade = itemReq.Quantidade,
                PrecoUnitario = produto.Preco
            };

            total += produto.Preco * itemReq.Quantidade;
            pedido.Itens.Add(item);
        }

        pedido.ValorTotal = total;

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        await _eventoService.RegistrarEventoAsync(
            pedido.Id,
            "PedidoCriado",
            $"Pedido criado com {pedido.Itens.Count} item(ns), total R$ {total:F2}");

        return CreatedAtAction(nameof(GetById), new { id = pedido.Id }, pedido);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Pedido>> GetById(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null) return NotFound();
        return pedido;
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> AtualizarStatus(int id, AtualizarStatusRequest request)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido is null) return NotFound();

        var statusAnterior = pedido.Status;
        pedido.Status = request.NovoStatus;
        await _context.SaveChangesAsync();

        await _eventoService.RegistrarEventoAsync(
            id,
            "StatusAtualizado",
            $"Status alterado de {statusAnterior} para {request.NovoStatus}");

        return NoContent();
    }

    [HttpGet("{id}/historico")]
    public async Task<ActionResult> GetHistorico(int id)
    {
        var pedidoExiste = await _context.Pedidos.AnyAsync(p => p.Id == id);
        if (!pedidoExiste) return NotFound();

        var historico = await _eventoService.ObterHistoricoAsync(id);
        return Ok(historico);
    }
}