using System;
using CadetCampus;

static class RulesTests
{
    static int count;

    static void Check(bool result, string name)
    {
        if (!result) throw new Exception("FAILED: " + name);
        count++;
        Console.WriteLine("PASS: " + name);
    }

    static CampusRules ReadyForCircuit()
    {
        var r = new CampusRules();
        r.Interact(true, false);
        r.Interact(true, false);
        r.Interact(true, false);
        return r;
    }

    static void Main()
    {
        var r = new CampusRules();
        Check(r.Stage == MissionStage.Orientation && r.Credits == 0, "new session starts at check-in");
        Check(!r.Interact(false, false) && r.Stage == MissionStage.Orientation, "distant interaction cannot progress mission");
        Check(!r.ReachCheckpoint(true), "checkpoints cannot be claimed before a race");
        Check(r.Interact(true, false) && r.Credits == 100, "check-in awards 100 credits");
        Check(!r.Interact(true, true) && r.Stage == MissionStage.Collect, "parcel pickup requires leaving car");
        Check(r.Interact(true, false) && r.Stage == MissionStage.Deliver, "parcel pickup advances delivery");
        Check(!r.Interact(true, true) && r.Stage == MissionStage.Deliver, "delivery requires leaving car");
        Check(r.Interact(true, false) && r.Credits == 350, "delivery awards 250 credits");
        Check(!r.Interact(true, false), "race cannot start on foot");
        Check(r.Interact(true, true) && r.Remaining == CampusRules.CircuitLimit, "race starts with a fresh timer");
        Check(!r.ReachCheckpoint(false) && r.Checkpoint == 0, "race markers require driving");
        Check(!r.Interact(true, true) && r.Checkpoint == 0, "interaction cannot skip race markers");
        for (int i = 0; i < CampusRules.CheckpointCount - 1; i++) r.ReachCheckpoint(true);
        Check(r.CircuitRunning && r.Credits == 350, "partial circuit earns no race reward");
        r.ReachCheckpoint(true);
        Check(r.Stage == MissionStage.Complete && r.Credits == 850 && r.CircuitsWon == 1, "sixth checkpoint completes race and awards credits");
        Check(!r.ReachCheckpoint(true) && r.Credits == 850, "completed race cannot duplicate rewards");
        Check(r.Interact(true, true) && r.Checkpoint == 0 && r.Remaining == CampusRules.CircuitLimit, "completed race can be replayed");
        var timeout = ReadyForCircuit();
        timeout.Interact(true, true);
        timeout.ReachCheckpoint(true);
        Check(!timeout.Tick(-10) && timeout.Remaining == CampusRules.CircuitLimit, "negative time does not extend timer");
        Check(!timeout.Tick(CampusRules.CircuitLimit - 1) && timeout.Remaining == 1, "timer remains active before deadline");
        Check(timeout.Tick(1) && timeout.Stage == MissionStage.FindCar && timeout.Checkpoint == 0, "deadline fails race and clears checkpoint progress");
        Check(timeout.Credits == 350 && !timeout.Tick(1000), "timeout grants no reward and is reported only once");
        Check(timeout.Interact(true, true) && timeout.Checkpoint == 0 && timeout.Remaining == CampusRules.CircuitLimit, "failed race can restart cleanly");
        Console.WriteLine(count + " checks passed.");
    }
}
