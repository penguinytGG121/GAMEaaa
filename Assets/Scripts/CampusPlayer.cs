using UnityEngine;

namespace CadetCampus
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class CampusPlayer : MonoBehaviour
    {
        CharacterController controller;
        Transform model, leftLeg, rightLeg, leftArm, rightArm;
        float verticalSpeed, gait;

        public void Initialize(Vector3 spawn)
        {
            controller = GetComponent<CharacterController>();
            controller.height = 1.9f;
            controller.radius = .32f;
            controller.center = Vector3.up * .98f;
            controller.stepOffset = .35f;
            controller.slopeLimit = 48;
            gameObject.layer = 8;
            model = CreateCadet(transform, new Color(.14f, .28f, .24f), out leftLeg, out rightLeg, out leftArm, out rightArm);
            Teleport(spawn);
        }

        public static Transform CreateCadet(Transform parent, Color shirt, out Transform ll, out Transform rl, out Transform la, out Transform ra)
        {
            var root = new GameObject("Uniform and character").transform;
            root.SetParent(parent, false);
            var uniform = CampusWorld.Material("Cadet uniform", shirt);
            var trousers = CampusWorld.Material("Cadet trousers", new Color(.13f, .18f, .18f));
            var skin = CampusWorld.Material("Skin", new Color(.65f, .42f, .26f));
            var boots = CampusWorld.Material("Boots", new Color(.07f, .085f, .09f));
            CampusWorld.Part(root, "Torso", PrimitiveType.Cube, new Vector3(0, 1.18f, 0), new Vector3(.58f, .64f, .32f), uniform);
            CampusWorld.Part(root, "Belt", PrimitiveType.Cube, new Vector3(0, .9f, 0), new Vector3(.59f, .08f, .34f), boots);
            CampusWorld.Part(root, "Head", PrimitiveType.Sphere, new Vector3(0, 1.7f, 0), new Vector3(.38f, .43f, .38f), skin);
            CampusWorld.Part(root, "Beret", PrimitiveType.Sphere, new Vector3(0, 1.9f, 0), new Vector3(.42f, .16f, .42f), uniform);
            CampusWorld.Part(root, "Cap brim", PrimitiveType.Cube, new Vector3(0, 1.85f, .19f), new Vector3(.32f, .05f, .18f), uniform);
            var badge = CampusWorld.Material("Brass badge", new Color(.96f, .7f, .28f));
            CampusWorld.Part(root, "Badge", PrimitiveType.Cube, new Vector3(-.16f, 1.3f, .17f), new Vector3(.09f, .1f, .02f), badge);
            ll = Limb(root, "Left leg", new Vector3(-.17f, .88f, 0), new Vector3(.22f, .73f, .24f), trousers, boots, true);
            rl = Limb(root, "Right leg", new Vector3(.17f, .88f, 0), new Vector3(.22f, .73f, .24f), trousers, boots, true);
            la = Limb(root, "Left arm", new Vector3(-.4f, 1.48f, 0), new Vector3(.18f, .6f, .2f), uniform, skin, false);
            ra = Limb(root, "Right arm", new Vector3(.4f, 1.48f, 0), new Vector3(.18f, .6f, .2f), uniform, skin, false);
            return root;
        }

        static Transform Limb(Transform parent, string name, Vector3 origin, Vector3 size, UnityEngine.Material material, UnityEngine.Material endMaterial, bool leg)
        {
            Transform pivot = new GameObject(name + " pivot").transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = origin;
            CampusWorld.Part(pivot, name, PrimitiveType.Cube, Vector3.down * size.y * .5f, size, material);
            CampusWorld.Part(pivot, leg ? "Boot" : "Hand", PrimitiveType.Cube, new Vector3(0, -size.y, leg ? .06f : 0), leg ? new Vector3(.24f, .16f, .4f) : new Vector3(.16f, .17f, .16f), endMaterial);
            return pivot;
        }

        void Update()
        {
            var game = CampusGame.Instance;
            if (!game || !game.AcceptInput || game.ActiveCar) return;
            float x = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
            float z = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
            Vector3 forward = game.FollowCamera.transform.forward;
            forward.y = 0;
            forward.Normalize();
            Vector3 right = new Vector3(forward.z, 0, -forward.x);
            Vector3 direction = Vector3.ClampMagnitude(forward * z + right * x, 1);
            float speed = Input.GetKey(KeyCode.LeftShift) ? 9 : 4.6f;
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2;
            if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space)) verticalSpeed = 9;
            verticalSpeed -= 22 * Time.deltaTime;
            controller.Move((direction * speed + Vector3.up * verticalSpeed) * Time.deltaTime);
            if (direction.sqrMagnitude > .01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 14 * Time.deltaTime);
            gait += Time.deltaTime * speed * 1.8f * direction.magnitude;
            float swing = Mathf.Sin(gait) * 30 * direction.magnitude;
            leftLeg.localRotation = Quaternion.Euler(swing, 0, 0);
            rightLeg.localRotation = Quaternion.Euler(-swing, 0, 0);
            leftArm.localRotation = Quaternion.Euler(-swing * .7f, 0, -8);
            rightArm.localRotation = Quaternion.Euler(swing * .7f, 0, 8);
            if (transform.position.y < -8) Teleport(new Vector3(3, .2f, -143));
        }

        public void EnterVehicle()
        {
            controller.enabled = false;
            model.gameObject.SetActive(false);
        }

        public void ExitVehicle(Vector3 position, float heading)
        {
            transform.rotation = Quaternion.Euler(0, heading, 0);
            model.gameObject.SetActive(true);
            Teleport(position);
        }

        public void Teleport(Vector3 position)
        {
            controller.enabled = false;
            transform.position = position;
            verticalSpeed = 0;
            controller.enabled = true;
        }
    }
}
