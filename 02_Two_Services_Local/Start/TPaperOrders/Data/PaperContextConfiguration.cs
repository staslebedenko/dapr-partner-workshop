using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TPaperOrders
{
    public static class PaperContextConfiguration
    {
        public static void Configure(ModelBuilder modelBuilder)
        {
            EntityTypeBuilder<Delivery> delivery = modelBuilder.Entity<Delivery>();
            delivery.Property(p => p.Id).ValueGeneratedOnAdd();
            delivery.Property(p => p.Number);
            delivery.Property(p => p.ClientId);
            delivery.Property(p => p.EdiOrderId);
            delivery.Property(p => p.ProductId);
            delivery.Property(p => p.Notes);

            EntityTypeBuilder<EdiOrder> order = modelBuilder.Entity<EdiOrder>();
            order.Property(p => p.Id).ValueGeneratedOnAdd();
            order.Property(p => p.ClientId);
            order.Property(p => p.Quantity);
            order.Property(p => p.ProductCode);
            order.Property(p => p.Notes);
            order.Property(p => p.DeliveryId);
            order.HasOne(p => p.Delivery)
                .WithMany(p => p.Orders)
                .HasForeignKey(p => p.DeliveryId)
                .OnDelete(DeleteBehavior.Restrict);

            EntityTypeBuilder<Client> client = modelBuilder.Entity<Client>();
            client.Property(p => p.Id).ValueGeneratedOnAdd();
            client.Property(p => p.Name);
            client.Property(p => p.EdiClientId);
            client.HasData(new Client { Id = 1, EdiClientId = 1, Name = "Test client" });

            EntityTypeBuilder<Product> product = modelBuilder.Entity<Product>();
            product.Property(p => p.Id).ValueGeneratedOnAdd();
            product.Property(p => p.Name);
            product.Property(p => p.ExternalCode);
            product.HasData(new Product { Id = 1, Name = "Sample", ExternalCode = 1});

            EntityTypeBuilder<Inventory> inventory = modelBuilder.Entity<Inventory>();
            inventory.Property(p => p.Id).ValueGeneratedOnAdd();
            inventory.Property(p => p.Number);
            inventory.Property(p => p.ProductId);
        }
    }
}
