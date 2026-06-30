Scénario global du projet
Contexte
Le système surveille en continu le trafic réseau d'une organisation. Chaque paquet ou connexion réseau passe par le système qui décide en temps réel si le comportement est normal ou anormal.

Les acteurs
Le réseau génère du trafic (utilisateurs, serveurs, équipements). Le système ML.NET analyse ce trafic. Les administrateurs reçoivent les alertes et consultent les rapports.

Le scénario pas à pas
Étape 1 — Collecte des données

Le système capture en temps réel les données du trafic réseau : adresses IP source/destination, ports utilisés, volume de données, protocoles, durée des connexions, fréquence des requêtes.
Étape 2 — Prétraitement

Les données brutes sont nettoyées et transformées en features numériques exploitables par le modèle ML. Par exemple : nombre de connexions par seconde, ratio upload/download, etc.
Étape 3 — Analyse par ML.NET

Le modèle (entraîné au préalable sur des datasets de trafic normal et anormal) analyse chaque flux et lui attribue une classification : normal, intrusion, DDoS, scan de ports, ou malware.
Étape 4 — Déclenchement d'alertes

Si une anomalie est détectée, le système génère immédiatement une alerte en temps réel avec le type de menace, l'IP concernée, le niveau de criticité et l'horodatage.
Étape 5 — Rapport d'analyse

Le système produit des rapports périodiques (quotidiens, hebdomadaires) avec des statistiques sur les anomalies détectées, les tendances, les IPs suspectes récurrentes.
Étape 6 — Interface d'administration

Un dashboard ASP.NET Core permet aux admins de consulter les alertes en cours, l'historique, les rapports, et de configurer les seuils de détection.

Les types d'anomalies détectées

Intrusion : tentative d'accès non autorisé à un système
DDoS : volume de requêtes anormalement élevé vers une cible
Scan de ports : balayage systématique des ports d'une machine
Malware : comportement réseau caractéristique d'un logiciel malveillant


DetectionAnomaliesReseau/
│
├── DetectionAnomaliesReseau.sln
│
├── Backend/
│   ├── Controllers/
│   │   ├── AlertsController.cs
│   │   ├── NetworkTrafficController.cs
│   │   ├── ReportsController.cs
│   │   ├── MLModelsController.cs
│   │   ├── UsersController.cs
│   │   └── BlockedIPsController.cs
│   │
│   ├── Models/
│   │   ├── Alert.cs
│   │   ├── NetworkTraffic.cs
│   │   ├── Report.cs
│   │   ├── MLModel.cs
│   │   ├── User.cs
│   │   └── BlockedIP.cs
│   │
│   ├── Data/
│   │   └── AppDbContext.cs
│   │
│   ├── Services/
│   │   ├── MLService.cs
│   │   ├── NetworkCaptureService.cs
│   │   ├── AlertService.cs
│   │   ├── ReportService.cs
│   │   └── AuthService.cs
│   │
│   ├── Hubs/
│   │   └── AlertHub.cs
│   │
│   ├── DTOs/
│   │   ├── AlertDto.cs
│   │   ├── NetworkTrafficDto.cs
│   │   └── UserDto.cs
│   │
│   ├── Migrations/
│   │
│   ├── MLModels/
│   │   └── (fichiers .zip des modèles ML.NET)
│   │
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
│
└── Frontend/
    ├── public/
    │
    ├── src/
    │   ├── components/
    │   │   ├── common/
    │   │   │   ├── Navbar.jsx
    │   │   │   ├── Sidebar.jsx
    │   │   │   └── LoadingSpinner.jsx
    │   │   │
    │   │   ├── alerts/
    │   │   │   ├── AlertCard.jsx
    │   │   │   ├── AlertList.jsx
    │   │   │   └── AlertBadge.jsx
    │   │   │
    │   │   ├── charts/
    │   │   │   ├── AnomaliesChart.jsx
    │   │   │   └── ThreatTypesChart.jsx
    │   │   │
    │   │   └── dashboard/
    │   │       ├── StatsCard.jsx
    │   │       └── RecentAlerts.jsx
    │   │
    │   ├── pages/
    │   │   ├── Dashboard.jsx
    │   │   ├── Alerts.jsx
    │   │   ├── NetworkTraffic.jsx
    │   │   ├── Reports.jsx
    │   │   ├── MLModels.jsx
    │   │   ├── BlockedIPs.jsx
    │   │   └── Login.jsx
    │   │
    │   ├── services/
    │   │   ├── api.js
    │   │   ├── alertService.js
    │   │   ├── reportService.js
    │   │   └── signalrService.js
    │   │
    │   ├── hooks/
    │   │   ├── useAlerts.js
    │   │   └── useSignalR.js
    │   │
    │   ├── context/
    │   │   └── AuthContext.jsx
    │   │
    │   ├── utils/
    │   │   ├── formatDate.js
    │   │   └── severityColors.js
    │   │
    │   ├── App.jsx
    │   ├── main.jsx
    │   └── index.css
    │
    ├── tailwind.config.js
    ├── postcss.config.js
    ├── vite.config.js
    └── package.json


