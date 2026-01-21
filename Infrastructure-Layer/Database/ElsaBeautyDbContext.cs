﻿using Domain_Layer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;

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

                var guidListComparer = new ValueComparer<List<Guid>>(
                    (c1, c2) => (c1 ?? new List<Guid>()).SequenceEqual(c2 ?? new List<Guid>()),
                    c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c == null ? new List<Guid>() : c.ToList());

                entity.Property(c => c.ParticipantIds)
                      .HasConversion(
                          v => string.Join(',', v ?? new List<Guid>()),
                          v => v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(Guid.Parse)
                                .ToList())
                      .Metadata.SetValueComparer(guidListComparer);

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

            builder.Entity<BookingModel>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.HasOne(b => b.User)
                      .WithMany()
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Employee)
                      .WithMany()
                      .HasForeignKey(b => b.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Service)
                      .WithMany()
                      .HasForeignKey(b => b.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            base.OnModelCreating(builder);
        }
    }
}
