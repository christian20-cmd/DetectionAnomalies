namespace Backend.DTOs
{
    // DTO pour envoyer une alerte au frontend (GET)
    public class AlertResponseDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string SourceIP { get; set; } = string.Empty;
        public string DestinationIP { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Protocol { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public DateTime DetectedAt { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // On envoie juste la version du modèle, pas tout l'objet MLModel
        public string MLModelVersion { get; set; } = string.Empty;
    }

    // DTO pour créer une alerte (POST) — React ou ML.NET envoie ça
    public class AlertCreateDto
    {
        public string Type { get; set; } = string.Empty;
        public string SourceIP { get; set; } = string.Empty;
        public string DestinationIP { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Protocol { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public int MLModelId { get; set; }
    }

    // DTO pour résoudre une alerte (PUT) — l'admin marque comme traitée
    public class AlertResolveDto
    {
        public bool IsResolved { get; set; }
    }
}