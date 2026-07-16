﻿using Domain_Layer.Models;
using Infrastructure_Layer.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure_Layer.Database
{
    public class ElsaBeautyDbContext : IdentityDbContext<ApplicationUser>
    {
        public ElsaBeautyDbContext(DbContextOptions<ElsaBeautyDbContext> options) : base(options)
        {
        }

        public DbSet<ServiceModel> Services { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<BookingModel> Bookings { get; set; }
        public DbSet<ConversationModel> Conversations { get; set; }
        public DbSet<MessageModel> Messages { get; set; }
        public DbSet<NotificationModel> Notifications { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<EmployeeScheduleModel> EmployeeSchedules { get; set; }
        public DbSet<EmployeeWorkDayModel> EmployeeWorkDays { get; set; }

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
                entity.Ignore(n => n.User);
                entity.HasOne<ApplicationUser>()
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
                entity.Ignore(b => b.User);
                entity.Ignore(b => b.Employee);

                // Soft delete: lagra statusen som läsbar sträng ("Active"/"Cancelled").
                // Default-värdet backfyller befintliga rader till Active vid migrering.
                entity.Property(b => b.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(BookingStatus.Active);

                // Betalningsstatus som läsbar sträng, default None (betala på plats).
                entity.Property(b => b.PaymentStatus)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(PaymentStatus.None);

                entity.Property(b => b.AmountPaid)
                    .HasColumnType("decimal(10,2)");

                // Unikt (filtrerat) index: en Stripe-betalning får bara ge EN bokning.
                // Gör PI-vakten atomär — returflödet och webhooken kan tävla om samma
                // betalning, och utan indexet kunde båda passera exists-kontrollen.
                entity.Property(b => b.StripePaymentIntentId).HasMaxLength(255);
                entity.HasIndex(b => b.StripePaymentIntentId)
                    .IsUnique()
                    .HasFilter("[StripePaymentIntentId] IS NOT NULL");

                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(b => b.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Service)
                    .WithMany()
                    .HasForeignKey(b => b.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<UserRefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);

                entity.Property(rt => rt.TokenHash)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(rt => rt.UserId)
                    .IsRequired();

                entity.HasIndex(rt => rt.TokenHash);
                entity.HasIndex(rt => rt.UserId);
                entity.HasIndex(rt => new { rt.UserId, rt.RevokedAt });

                entity.Ignore(rt => rt.User);
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Ignore(rt => rt.IsExpired);
                entity.Ignore(rt => rt.IsRevoked);
                entity.Ignore(rt => rt.IsActive);
            });

            builder.Entity<EmployeeScheduleModel>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(s => s.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(s => s.EmployeeId);
            });

            builder.Entity<EmployeeWorkDayModel>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(w => w.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(w => new { w.EmployeeId, w.Date }).IsUnique();
            });

            base.OnModelCreating(builder);
        }
    }
}
