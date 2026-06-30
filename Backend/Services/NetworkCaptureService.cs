using SharpPcap;
using SharpPcap.LibPcap;
using PacketDotNet;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Services
{
    // Service qui tourne en arrière-plan et capture le trafic réseau en temps réel
    public class NetworkCaptureService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NetworkCaptureService> _logger;
        private ICaptureDevice? _device;

        // Dictionnaire pour agréger les connexions par IP source
        // Clé : IP source | Valeur : liste des timestamps de connexion
        private readonly Dictionary<string, List<DateTime>> _connectionTracker
            = new Dictionary<string, List<DateTime>>();

        // Dictionnaire pour compter les ports contactés par IP
        // Clé : IP source | Valeur : liste des ports contactés
        private readonly Dictionary<string, HashSet<int>> _portTracker
            = new Dictionary<string, HashSet<int>>();

        private readonly object _lock = new object();

        public NetworkCaptureService(
            IServiceScopeFactory scopeFactory,
            ILogger<NetworkCaptureService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // Point d'entrée du BackgroundService
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 NetworkCaptureService démarré.");

            try
            {
                // Récupérer la liste des interfaces réseau disponibles
                var devices = CaptureDeviceList.Instance;

                if (devices.Count == 0)
                {
                    _logger.LogWarning("⚠️ Aucune interface réseau trouvée.");
                    return;
                }

                // Utiliser la première interface disponible
                _device = devices[0];
                _logger.LogInformation($"📡 Interface réseau sélectionnée : {_device.Description}");

                // Configurer la capture
                _device.Open(DeviceModes.Promiscuous, 1000);
                _device.Filter = "tcp or udp";
                // Capturer uniquement TCP et UDP

                // Démarrer la capture en arrière-plan
                _device.OnPacketArrival += OnPacketArrival;
                _device.StartCapture();

                _logger.LogInformation("✅ Capture réseau démarrée.");

                // Lancer la tâche d'analyse périodique toutes les 5 secondes
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    await AnalyzeAggregatedTrafficAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Erreur NetworkCaptureService : {ex.Message}");
            }
            finally
            {
                StopCapture();
            }
        }

        // Appelé pour chaque paquet capturé
        private void OnPacketArrival(object sender, PacketCapture e)
        {
            try
            {
                var rawPacket = e.GetPacket();
                var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
                var ipPacket = packet.Extract<IPPacket>();

                if (ipPacket == null) return;

                var sourceIP = ipPacket.SourceAddress.ToString();
                var destinationIP = ipPacket.DestinationAddress.ToString();
                var now = DateTime.UtcNow;

                // Extraire le port destination selon le protocole
                int destinationPort = 0;
                

                var tcpPacket = packet.Extract<TcpPacket>();
                var udpPacket = packet.Extract<UdpPacket>();

                if (tcpPacket != null)
                {
                    destinationPort = tcpPacket.DestinationPort;
                    _ = "TCP"; // protocol ignoré dans l'agrégation
                }
                else if (udpPacket != null)
                {
                    destinationPort = udpPacket.DestinationPort;
                    _ = "UDP";
                }

                // Agréger les connexions par IP source (thread-safe)
                lock (_lock)
                {
                    // Tracker des connexions par seconde
                    if (!_connectionTracker.ContainsKey(sourceIP))
                        _connectionTracker[sourceIP] = new List<DateTime>();

                    _connectionTracker[sourceIP].Add(now);

                    // Tracker des ports contactés
                    if (!_portTracker.ContainsKey(sourceIP))
                        _portTracker[sourceIP] = new HashSet<int>();

                    if (destinationPort > 0)
                        _portTracker[sourceIP].Add(destinationPort);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Erreur parsing paquet : {ex.Message}");
            }
        }

        // Analyser le trafic agrégé toutes les 5 secondes
        private async Task AnalyzeAggregatedTrafficAsync()
        {
            Dictionary<string, List<DateTime>> connectionSnapshot;
            Dictionary<string, HashSet<int>> portSnapshot;

            // Copier et vider les trackers (thread-safe)
            lock (_lock)
            {
                connectionSnapshot = new Dictionary<string, List<DateTime>>(_connectionTracker);
                portSnapshot = new Dictionary<string, HashSet<int>>(_portTracker);
                _connectionTracker.Clear();
                _portTracker.Clear();
            }

            if (connectionSnapshot.Count == 0) return;

            using var scope = _scopeFactory.CreateScope();
            var mlService = scope.ServiceProvider.GetRequiredService<MLService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            foreach (var entry in connectionSnapshot)
            {
                var sourceIP = entry.Key;
                var connections = entry.Value;

                // Calculer les métriques sur la fenêtre de 5 secondes
                var connectionsPerSecond = connections.Count / 5.0f;
                var portsContacted = portSnapshot.ContainsKey(sourceIP)
                    ? portSnapshot[sourceIP].Count
                    : 0;

                // Créer un objet NetworkTraffic avec les métriques calculées
                var traffic = new NetworkTraffic
                {
                    SourceIP = sourceIP,
                    DestinationIP = "10.0.0.1",
                    // IP destination agrégée — à affiner selon ton réseau
                    SourcePort = 0,
                    DestinationPort = portSnapshot.ContainsKey(sourceIP)
                        ? portSnapshot[sourceIP].FirstOrDefault()
                        : 0,
                    Protocol = "TCP",
                    PacketSize = 1500,
                    // Taille moyenne estimée
                    ConnectionsPerSecond = connectionsPerSecond,
                    PortsContacted = portsContacted,
                    SessionDuration = 5.0f,
                    // Fenêtre d'analyse de 5 secondes
                    CapturedAt = DateTime.UtcNow,
                    AlertId = null
                };

                // Sauvegarder le trafic capturé
                dbContext.NetworkTraffics.Add(traffic);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation(
                    $"📊 {sourceIP} — {connectionsPerSecond:F1} conn/sec — {portsContacted} ports");

                // Envoyer au MLService pour analyse
                await mlService.AnalyzeTrafficAsync(traffic);
            }
        }

        // Arrêter proprement la capture
        private void StopCapture()
        {
            if (_device != null)
            {
                try
                {
                    _device.StopCapture();
                    _device.Close();
                    _logger.LogInformation("🛑 Capture réseau arrêtée.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erreur arrêt capture : {ex.Message}");
                }
            }
        }

        public override void Dispose()
        {
            StopCapture();
            base.Dispose();
        }
    }
}