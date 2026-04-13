namespace paczkomat.Models
{
    public class SendPackRequest
    {
        public string ReceiverEmail { get; set; }
        public string ReceiverPhone { get; set; }
        public string Size { get; set; }
        public string PaczkomatName { get; set; }
     
    }
}