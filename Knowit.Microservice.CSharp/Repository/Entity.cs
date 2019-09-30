﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Repository
{
    public class Entity
    {
        public Guid Id { get; set; }
        
        [Required] public string Name { get; set; } = string.Empty;
    }
}
