﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models
{
    public class MailSettings
    {
        public string Mail { get; set; }
        public string DisplayName { get; set; }

        public string Password { get; set; }

        public string Host { get; set; }

        public string Port { get; set; }
    }
}