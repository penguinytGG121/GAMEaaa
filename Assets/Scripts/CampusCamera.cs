using UnityEngine;

namespace CadetCampus
{
    [RequireComponent(typeof(Camera), typeof(AudioListener))]
    public sealed class CampusCamera : MonoBehaviour
    {
        CampusGame game;
        float yaw, pitch = 19;
        Vector3 smoothedTarget;

        public void Initialize(CampusGame owner)
        {
            game = owner;
            var cameraComponent = GetComponent<Camera>();
            cameraComponent.fieldOfView = 62;
            cameraComponent.nearClipPlane = .15f;
            cameraComponent.farClipPlane = 800;
            cameraComponent.backgroundColor = new Color(.58f, .77f, .81f);
            cameraComponent.clearFlags = CameraClearFlags.Skybox;
            gameObject.tag = "MainCamera";
            smoothedTarget = owner.Player.transform.position + Vector3.up * 1.4f;
        }

        void LateUpdate()
        {
            if (!game.Started)
            {
                float t = Time.unscaledTime * .025f;
                transform.position = new Vector3(Mathf.Sin(t) * 210, 140, -205 + Mathf.Cos(t) * 20);
                transform.LookAt(new Vector3(0, 0, 5));
                return;
            }
            if (!game.AcceptInput) return;
            yaw += Input.GetAxis("Mouse X") * 3;
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * 2.5f, -12, 65);
            bool driving = game.ActiveCar;
            if (driving && Mathf.Abs(Input.GetAxis("Mouse X")) < .02f && game.ActiveCar.SpeedKph > 5)
                yaw = Mathf.LerpAngle(yaw, game.Heading, Time.deltaTime * 1.3f);
            Vector3 target = game.Focus.position + Vector3.up * (driving ? 1.5f : 1.45f);
            smoothedTarget = Vector3.Lerp(smoothedTarget, target, 1 - Mathf.Exp(-12 * Time.deltaTime));
            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0);
            float distance = driving ? 8 : 5.1f;
            Vector3 offset = orbit * new Vector3(driving ? .6f : .55f, .4f, -distance);
            RaycastHit hit;
            float length = offset.magnitude;
            if (Physics.SphereCast(smoothedTarget, .22f, offset.normalized, out hit, length, 1 << 0, QueryTriggerInteraction.Ignore))
                length = Mathf.Max(.5f, hit.distance - .12f);
            transform.position = smoothedTarget + offset.normalized * length;
            transform.LookAt(smoothedTarget + orbit * Vector3.right * .35f);
            GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, driving ? 66 + game.ActiveCar.SpeedKph * .1f : 62, Time.deltaTime * 3);
        }
    }
}
