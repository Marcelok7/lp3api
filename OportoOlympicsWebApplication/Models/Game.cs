public class Game
{
    public Guid Id { get; set; }
    public string GroupId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Location { get; set; }
    public int Capacity { get; set; }
    public string Sport { get; set; }
    public bool Active { get; set; }
    public int EventId { get; set; }
}