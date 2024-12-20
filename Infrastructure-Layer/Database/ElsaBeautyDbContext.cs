using Domain_Layer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure_Layer.Database
{
    public class ElsaBeautyDbContext : IdentityDbContext<UserModel>
    {
        public ElsaBeautyDbContext(DbContextOptions<ElsaBeautyDbContext> options) : base(options)
        {
        }

        public DbSet<UserModel> User { get; set; }
        public DbSet<ServiceModel> Services { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<BookingModel> Bookings { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ServiceModel>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<ServiceModel>()
                .HasOne(s => s.Category)
                .WithMany(c => c.Services)
                .HasForeignKey(s => s.CategoryId);

            base.OnModelCreating(builder);
        }
    }
}
