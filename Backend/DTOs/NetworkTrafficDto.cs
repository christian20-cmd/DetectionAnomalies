namespace Backend.DTOs
{
    // DTO pour envoyer les données de trafic au frontend (GET)
    public class NetworkTrafficResponseDto
    {
        public int Id { get; set; }
        public string SourceIP { get; set; } = string.Empty;
        public string DestinationIP { get; set; } = string.Empty;
        public int SourcePort { get; set; }
        public int DestinationPort { get; set; }
        public string Protocol { get; set; } = string.Empty;
        public float PacketSize { get; set; }
        public float ConnectionsPerSecond { get; set; }
        public int PortsContacted { get; set; }
        public float SessionDuration { get; set; }
        public DateTime CapturedAt { get; set; }

        // Si ce trafic a généré une alerte, on envoie son Id
        // null si trafic normal
        public int? AlertId { get; set; }
    }

    // DTO pour envoyer du trafic réseau (POST) — SharpPcap envoie ça
    public class NetworkTrafficCreateDto
    {
        public string SourceIP { get; set; } = string.Empty;
        public string DestinationIP { get; set; } = string.Empty;
        public int SourcePort { get; set; }
        public int DestinationPort { get; set; }
        public string Protocol { get; set; } = string.Empty;
        public float PacketSize { get; set; }
        public float ConnectionsPerSecond { get; set; }
        public int PortsContacted { get; set; }
        public float SessionDuration { get; set; }
    }
}