using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
    public class TransaccionIncommModel
	{
        
		public string issuerId { get; set; }
		public string correlationId { get; set; }
		public string issueDate { get; set; }
		public string phoneNumber { get; set; }
		public string source { get; set; }
		public string ip { get; set; }
		public string amount { get; set; }
		public string issuerLogin { get; set; } 
		public string email { get; set; } 
		public string eanCode { get; set; }
		public string zipCode { get; set; }
		public string channel { get; set; }

	}
}
