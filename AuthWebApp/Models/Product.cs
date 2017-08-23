﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AuthWebApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string nome { get; set; }
        public string descricao { get; set; }
        public string cor { get; set; }
        [StringLength(450)]
        [Index(IsUnique = true)]
        [Required]
        public string modelo { get; set; }
        [StringLength(450)]
        [Index(IsUnique = true)]
        [Required]
        public string codigo { get; set; }
        public decimal preco { get; set; }
        public decimal peso { get; set; }
        public decimal altura { get; set; }
        public decimal largura { get; set; }
        public decimal comprimento { get; set; }
        public decimal diametro { get; set; }
        public string url { get; set; }
    }
}