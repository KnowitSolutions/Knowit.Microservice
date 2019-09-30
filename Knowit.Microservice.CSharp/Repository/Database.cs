﻿using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class Database : DbContext
    {
        public Database(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Entity> Entity { get; set; }
    }
}
