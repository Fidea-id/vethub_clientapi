﻿using FluentEmail.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Emails
{
    public class EmailSenderData
    {
        public string To { get; set; }
        public List<Address> CC { get; set; }
        public List<Address> BCC { get; set; }
        public string Subject { get; set; }
        public List<Attachment> Attachments { get; set; }
        public object EmailData { get; set; }

        public EmailSenderData()
        {
            CC = new List<Address>();
            BCC = new List<Address>();
            Attachments = new List<Attachment>();
            EmailData = new object();
        }

    }
}
