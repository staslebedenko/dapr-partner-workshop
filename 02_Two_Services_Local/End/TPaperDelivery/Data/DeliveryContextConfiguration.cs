using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TPaperDelivery
{
    public static class DeliveryContextConfiguration
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
            delivery.HasIndex(u => u.EdiOrderId).IsUnique();

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
