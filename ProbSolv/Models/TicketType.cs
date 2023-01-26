using System.ComponentModel;

namespace ProbSolv.Models
{
    public class TicketType
    {
        public int Id { get; set; }


        [DisplayName("Type")]
        public string Name { get; set; }


    }
}
