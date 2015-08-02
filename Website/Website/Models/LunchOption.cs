using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace Website.Models
{
    public class LunchOption
    {
        public int Id { get; set; }
        [Required, MaxLength(80), Index(IsUnique = true)]
        public string Name { get; set; }
    }
}