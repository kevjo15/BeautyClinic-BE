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


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

        }
    }
}
