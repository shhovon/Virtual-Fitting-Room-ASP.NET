﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VFR_Project.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class VFREntities : DbContext
    {
        public VFREntities()
            : base("name=VFREntities")
        {
        }

        public virtual DbSet<UserPhoto> UserPhotos { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPhoto>()
               .Property(e => e.ImageData)
               .IsRequired();

            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Garment> Garments { get; set; }
        public virtual DbSet<UserPhoto> UserPhotoes { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<VirtualFitting> VirtualFittings { get; set; }
    }
}
