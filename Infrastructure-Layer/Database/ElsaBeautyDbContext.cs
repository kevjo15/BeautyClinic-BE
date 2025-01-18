﻿﻿﻿﻿﻿using Domain_Layer.Models;
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
        public DbSet<ConversationModel> Conversations { get; set; }
        public DbSet<MessageModel> Messages { get; set; }
        public DbSet<NotificationModel> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ServiceModel>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<ServiceModel>()
                .HasOne(s => s.Category)
                .WithMany(c => c.Services)
                .HasForeignKey(s => s.CategoryId);

            builder.Entity<ConversationModel>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasMany(c => c.Messages)
                      .WithOne()
                      .HasForeignKey(m => m.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<MessageModel>(entity =>
            {
                entity.HasKey(m => m.Id);
            });

            builder.Entity<NotificationModel>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.HasOne(n => n.User)
                      .WithMany()
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.Booking)
                      .WithMany()
                      .HasForeignKey(n => n.BookingId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            base.OnModelCreating(builder);
        }
    }
}
