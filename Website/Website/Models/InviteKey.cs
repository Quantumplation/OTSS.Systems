﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class InviteKey
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public int UseCount { get; set; }
    }
}