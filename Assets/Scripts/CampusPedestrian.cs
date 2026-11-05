using UnityEngine;

namespace CadetCampus
{
    public sealed class CampusPedestrian : MonoBehaviour
    {
        Vector3[] path;
        int waypoint, seed;
        Transform ll, rl, la, ra;
        float phase;

        public void Initialize(Vector3[] route, int index)
        {
            path = route;
            seed = index;
            waypoint = index % route.Length;
            transform.position = Vector3.Lerp(route[waypoint], route[(waypoint + 1) % route.Length], (index / route.Length) / 5f) + Vector3.up * .06f;
            waypoint = (waypoint + 1) % route.Length;
            CampusPlayer.CreateCadet(transform, index % 3 == 0 ? new Color(.74f, .72f, .56f) : new Color(.2f, .32f, .25f), out ll, out rl, out la, out ra);
            phase = index;
        }

        void Update()
        {
            var game = CampusGame.Instance;
            if (!game || !game.AcceptInput) return;
            Vector3 delta = path[waypoint] + Vector3.up * .06f - transform.position;
            if (delta.magnitude < .8f) { waypoint = (waypoint + 1) % path.Length; return; }
            float speed = 1.3f + (seed % 4) * .13f;
            // Pedestrians yield to the player and cars rather than blocking a drive.
            bool yield = Vector3.Distance(transform.position, game.Position) < 3.5f;
            foreach (var car in game.Cars)
                if (Vector3.Distance(transform.position, car.transform.position) < 4.5f) yield = true;
            if (yield) return;
            transform.position += delta.normalized * speed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(delta), Time.deltaTime * 5);
            phase += Time.deltaTime * speed * 3.5f;
            float swing = Mathf.Sin(phase) * 24;
            ll.localRotation = Quaternion.Euler(swing, 0, 0);
            rl.localRotation = Quaternion.Euler(-swing, 0, 0);
            la.localRotation = Quaternion.Euler(-swing, 0, -8);
            ra.localRotation = Quaternion.Euler(swing, 0, 8);
        }
    }
}
