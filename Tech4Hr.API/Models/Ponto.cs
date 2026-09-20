namespace Tech4Hr.API.Models
{
    public class Ponto
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public TimeSpan Entrada { get; set; }
        public TimeSpan Saida { get; set; }
        public int FuncionarioId { get; set; }
    }
}
