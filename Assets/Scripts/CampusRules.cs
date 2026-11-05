using System;

namespace CadetCampus
{
    // Pure rules shared by the game and the command-line regression checks.
    public enum MissionStage { Orientation, Collect, Deliver, FindCar, Circuit, Complete }

    public sealed class CampusRules
    {
        public const float CircuitLimit = 110f;
        public const int CheckpointCount = 6;
        public MissionStage Stage { get; private set; }
        public int Credits { get; private set; }
        public int Checkpoint { get; private set; }
        public float Remaining { get; private set; }
        public int CircuitsWon { get; private set; }
        public bool CircuitRunning { get { return Stage == MissionStage.Circuit; } }

        public bool Interact(bool atTarget, bool inCar)
        {
            if (!atTarget) return false;
            switch (Stage)
            {
                case MissionStage.Orientation:
                    Stage = MissionStage.Collect;
                    Credits += 100;
                    return true;
                case MissionStage.Collect:
                    if (inCar) return false;
                    Stage = MissionStage.Deliver;
                    return true;
                case MissionStage.Deliver:
                    if (inCar) return false;
                    Stage = MissionStage.FindCar;
                    Credits += 250;
                    return true;
                case MissionStage.FindCar:
                case MissionStage.Complete:
                    if (!inCar) return false;
                    StartCircuit();
                    return true;
                default:
                    return false;
            }
        }

        void StartCircuit()
        {
            Stage = MissionStage.Circuit;
            Remaining = CircuitLimit;
            Checkpoint = 0;
        }

        public bool ReachCheckpoint(bool inCar)
        {
            if (!CircuitRunning || !inCar) return false;
            Checkpoint++;
            if (Checkpoint >= CheckpointCount)
            {
                Stage = MissionStage.Complete;
                CircuitsWon++;
                Credits += 500;
            }
            return true;
        }

        // Returns true only on timeout. A retry cannot retain stale checkpoints.
        public bool Tick(float seconds)
        {
            if (!CircuitRunning) return false;
            Remaining = Math.Max(0f, Remaining - Math.Max(0f, seconds));
            if (Remaining > 0f) return false;
            Stage = MissionStage.FindCar;
            Checkpoint = 0;
            return true;
        }
    }
}
