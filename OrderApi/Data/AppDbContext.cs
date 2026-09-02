using Microsoft.EntityFrameworkCore;
using OrderApi.Models;

namespace OrderApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>()
            .Property(p => p.Preco)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Pedido>()
            .Property(p => p.ValorTotal)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Pedido>()
            .Property(p => p.Status)
            .HasConversion<string>();

        modelBuilder.Entity<ItemPedido>()
            .Property(i => i.PrecoUnitario)
            .HasColumnType("decimal(18,2)");
    }
}