using UnityEngine;

public class Camera_sc : MonoBehaviour
{
    GameObject hero;

    public float jarak = 5f;
    public float tinggiTarget = 1f;      // titik yang dilihat (misal setinggi dada/kepala hero)
    public float sensitivitas = 3f;
    public float minPitch = -30f;
    public float maxPitch = 60f;

    float yaw;
    float pitch;

    void Start()
    {
        hero = GameObject.Find("Lady Pirate");

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()   // pakai LateUpdate supaya kamera jalan setelah hero bergerak
    {
        Mouse_camera();
    }

    void Mouse_camera()
    {
        yaw += Input.GetAxis("Mouse X") * sensitivitas;
        pitch -= Input.GetAxis("Mouse Y") * sensitivitas;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotasi = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 target = hero.transform.position + Vector3.up * tinggiTarget;

        transform.position = target - rotasi * Vector3.forward * jarak;
        transform.rotation = rotasi;
    }
}
