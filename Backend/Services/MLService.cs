using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;

namespace Backend.Services
{
    // Classe représentant les données d'entrée pour ML.NET
    public class NetworkTrafficInput
    {
        [LoadColumn(0)]
        public float ConnectionsPerSecond { get; set; }

        [LoadColumn(1)]
        public float PacketSize { get; set; }

        [LoadColumn(2)]
        public float PortsContacted { get; set; }

        [LoadColumn(3)]
        public float SessionDuration { get; set; }

        [LoadColumn(4)]
        public float SourcePort { get; set; }

        [LoadColumn(5)]
        public float DestinationPort { get; set; }

        [LoadColumn(6)]
        public string Label { get; set; } = string.Empty;
        // Valeurs : "Normal", "DDoS", "Intrusion", "ScanPorts", "Malware"
    }

    // Classe représentant la prédiction de ML.NET
    public class NetworkTrafficPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedLabel { get; set; } = string.Empty;

        [ColumnName("Score")]
        public float[] Score { get; set; } = Array.Empty<float>();
    }

    public class MLService
    {
        private readonly AppDbContext _context;
        private readonly AlertService _alertService;
        private readonly IConfiguration _configuration;
        private readonly MLContext _mlContext;

        // Le moteur de prédiction — chargé une seule fois en mémoire
        private PredictionEngine<NetworkTrafficInput, NetworkTrafficPrediction>? _predictionEngine;
        private int _activeModelId = 0;

        public MLService(
            AppDbContext context,
            AlertService alertService,
            IConfiguration configuration)
        {
            _context = context;
            _alertService = alertService;
            _configuration = configuration;
            _mlContext = new MLContext(seed: 42);
        }

        // Charger le modèle ML actif depuis le fichier .zip
        public async Task LoadActiveModelAsync()
        {
            var activeModel = await _context.MLModels
                .FirstOrDefaultAsync(m => m.IsActive == true);

            if (activeModel == null)
            {
                Console.WriteLine("⚠️ Aucun modèle ML actif trouvé.");
                return;
            }

            var modelPath = activeModel.ModelFilePath;

            if (!File.Exists(modelPath))
            {
                Console.WriteLine($"⚠️ Fichier modèle introuvable : {modelPath}");
                Console.WriteLine("ℹ️ Le modèle sera entraîné au premier lancement.");
                return;
            }

            try
            {
                // Charger le modèle depuis le fichier .zip
                var loadedModel = _mlContext.Model.Load(modelPath, out _);

                // Créer le moteur de prédiction
                _predictionEngine = _mlContext.Model
                    .CreatePredictionEngine<NetworkTrafficInput, NetworkTrafficPrediction>(loadedModel);

                _activeModelId = activeModel.Id;

                Console.WriteLine($"✅ Modèle ML chargé : {activeModel.Version} ({activeModel.Algorithm})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors du chargement du modèle : {ex.Message}");
            }
        }

        // Analyser un flux réseau et détecter les anomalies
        public async Task<string> AnalyzeTrafficAsync(NetworkTraffic traffic)
        {
            // Si le modèle n'est pas chargé, charger le modèle actif
            if (_predictionEngine == null)
            {
                await LoadActiveModelAsync();
            }

            // Si toujours pas de modèle disponible
            if (_predictionEngine == null)
            {
                Console.WriteLine("⚠️ Aucun modèle disponible pour l'analyse.");
                return "Normal";
            }

            // Préparer les données d'entrée
            var input = new NetworkTrafficInput
            {
                ConnectionsPerSecond = traffic.ConnectionsPerSecond,
                PacketSize = traffic.PacketSize,
                PortsContacted = traffic.PortsContacted,
                SessionDuration = traffic.SessionDuration,
                SourcePort = traffic.SourcePort,
                DestinationPort = traffic.DestinationPort
            };

            // Faire la prédiction
            var prediction = _predictionEngine.Predict(input);
            var predictedLabel = prediction.PredictedLabel;

            // Calculer le score de confiance (valeur max dans le tableau Score)
            var confidence = prediction.Score.Length > 0
                ? prediction.Score.Max()
                : 0.0f;

            Console.WriteLine($"🔍 Prédiction : {predictedLabel} (confiance : {confidence:P0})");

            // Si ce n'est pas du trafic normal, créer une alerte
            if (predictedLabel != "Normal" && confidence >= 0.50f)
            {
                var severity = AlertService.DetermineSeverity(predictedLabel, confidence);

                var alertDto = new AlertCreateDto
                {
                    Type = predictedLabel,
                    SourceIP = traffic.SourceIP,
                    DestinationIP = traffic.DestinationIP,
                    Port = traffic.DestinationPort,
                    Protocol = traffic.Protocol,
                    Severity = severity,
                    Confidence = confidence,
                    MLModelId = _activeModelId
                };

                await _alertService.CreateAlertAsync(alertDto);
            }

            return predictedLabel;
        }

        // Entraîner un nouveau modèle ML.NET
        public async Task<MLModel?> TrainModelAsync(string datasetPath, string newVersion)
        {
            if (!File.Exists(datasetPath))
            {
                Console.WriteLine($"❌ Dataset introuvable : {datasetPath}");
                return null;
            }

            Console.WriteLine($"🚀 Début de l'entraînement du modèle {newVersion}...");

            try
            {
                // Charger le dataset CSV
                var data = _mlContext.Data.LoadFromTextFile<NetworkTrafficInput>(
                    datasetPath,
                    hasHeader: true,
                    separatorChar: ','
                );

                // Diviser en données d'entraînement (80%) et de test (20%)
                var split = _mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

                // Définir le pipeline ML
                var pipeline = _mlContext.Transforms
                    .Conversion.MapValueToKey("Label")
                    .Append(_mlContext.Transforms.Concatenate("Features",
                        nameof(NetworkTrafficInput.ConnectionsPerSecond),
                        nameof(NetworkTrafficInput.PacketSize),
                        nameof(NetworkTrafficInput.PortsContacted),
                        nameof(NetworkTrafficInput.SessionDuration),
                        nameof(NetworkTrafficInput.SourcePort),
                        nameof(NetworkTrafficInput.DestinationPort)
                    ))
                    .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                        labelColumnName: "Label",
                        featureColumnName: "Features"
                    ))
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

                // Entraîner le modèle
                var trainedModel = pipeline.Fit(split.TrainSet);

                // Évaluer le modèle sur les données de test
                var predictions = trainedModel.Transform(split.TestSet);
                var metrics = _mlContext.MulticlassClassification.Evaluate(predictions);

                Console.WriteLine($"✅ Entraînement terminé !");
                Console.WriteLine($"   Accuracy  : {metrics.MacroAccuracy:P2}");
                Console.WriteLine($"   Log-Loss  : {metrics.LogLoss:F4}");

                // Sauvegarder le modèle dans un fichier .zip
                var modelDir = "MLModels";
                Directory.CreateDirectory(modelDir);
                var modelPath = Path.Combine(modelDir, $"model_{newVersion}.zip");

                _mlContext.Model.Save(trainedModel, data.Schema, modelPath);
                Console.WriteLine($"💾 Modèle sauvegardé : {modelPath}");

                // Enregistrer le nouveau modèle dans la base de données
                var mlModel = new MLModel
                {
                    Version = newVersion,
                    Algorithm = "FastForest",
                    Accuracy = (float)metrics.MacroAccuracy,
                    Precision = (float)metrics.MacroAccuracy,
                    Recall = (float)metrics.MacroAccuracy,
                    F1Score = (float)metrics.MacroAccuracy,
                    TrainingDataset = Path.GetFileName(datasetPath),
                    TrainingDate = DateTime.UtcNow,
                    IsActive = false,
                    ModelFilePath = modelPath,
                    TotalAnomaliesDetected = 0,
                    Notes = $"Entraîné sur {Path.GetFileName(datasetPath)}"
                };

                _context.MLModels.Add(mlModel);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Modèle {newVersion} enregistré en base de données.");

                return mlModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur d'entraînement : {ex.Message}");
                return null;
            }
        }
    }
}