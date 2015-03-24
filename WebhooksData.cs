using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Webhooks
{
    public class WebhooksData
    {
        public string Description { get; set; }
        public string Url { get; set; }
        public bool Create { get; set; }
        public bool Update { get; set; }
    }
}
