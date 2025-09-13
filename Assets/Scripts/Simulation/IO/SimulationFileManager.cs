using SweetSpin.Simulation.Data;
using System;
using System.IO;
using UnityEngine;

namespace SweetSpin.Simulation.IO
{
    /// <summary>
    /// Handles persistence operations for simulation data including JSON serialization,
    /// timestamped file generation, results archival, and cleanup utilities.
    /// Manages the SimulationResults directory structure and file lifecycle.
    /// </summary>
    public static class SimulationFileManager
    {
        private const string RESULTS_FOLDER = "Assets/SimulationResults";

        /// <summary>
        /// Saves a simulation report to a JSON file
        /// </summary>
        /// <returns>The full path of the saved file</returns>
        public static string SaveSimulationReport(SimulationReport report)
        {
            // Ensure the results directory exists
            if (!Directory.Exists(RESULTS_FOLDER))
            {
                Directory.CreateDirectory(RESULTS_FOLDER);
            }

            // Generate filename with reverse timestamp
            string filename = GenerateFilename();
            string fullPath = Path.Combine(RESULTS_FOLDER, filename);

            // Convert to JSON with pretty printing
            string json = JsonUtility.ToJson(report, true);

            // Save to file
            try
            {
                File.WriteAllText(fullPath, json);
                Debug.Log($"Simulation saved to: {fullPath}");
                return fullPath;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save simulation: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Generates a filename with reverse timestamp
        /// </summary>
        private static string GenerateFilename()
        {
            // Format: yyyyMMdd_HHmmss.json (reverse order for better sorting)
            DateTime now = DateTime.Now;
            return $"{now:yyyyMMdd_HHmmss}.json";
        }

        /// <summary>
        /// Loads a simulation report from a JSON file
        /// </summary>
        public static SimulationReport LoadSimulationReport(string filepath)
        {
            if (!File.Exists(filepath))
            {
                Debug.LogError($"File not found: {filepath}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(filepath);
                return JsonUtility.FromJson<SimulationReport>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load simulation: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets all simulation files in the results folder
        /// </summary>
        public static string[] GetAllSimulationFiles()
        {
            if (!Directory.Exists(RESULTS_FOLDER))
            {
                return new string[0];
            }

            return Directory.GetFiles(RESULTS_FOLDER, "*.json");
        }

        /// <summary>
        /// Deletes old simulation files, keeping only the most recent N files
        /// </summary>
        public static void CleanupOldSimulations(int keepCount = 50)
        {
            var files = GetAllSimulationFiles();

            if (files.Length <= keepCount)
                return;

            // Sort by creation time (oldest first)
            Array.Sort(files, (a, b) => File.GetCreationTime(a).CompareTo(File.GetCreationTime(b)));

            // Delete oldest files
            int deleteCount = files.Length - keepCount;
            for (int i = 0; i < deleteCount; i++)
            {
                try
                {
                    File.Delete(files[i]);
                    Debug.Log($"Deleted old simulation: {Path.GetFileName(files[i])}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to delete file: {e.Message}");
                }
            }
        }
    }
}