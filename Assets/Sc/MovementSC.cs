using UnityEngine;

public class MovementSC : MonoBehaviour
{
    Animator anim;

    public float kecepatan = 4f;
    public float kecepatanPutar = 10f;

    float comboAtk = 0f;
    float lastAtk = 0f;

    public float jedaHit = 0.5f;
    float timer;

    [Header("Lompat")]
    public float durasiLompat = 1f;      // samakan dengan panjang clip lompat
    public float jedaLompat = 0.2f;      // jeda minimal antar lompat
    bool sedangLompat;
    float timerLompat;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (timer > 0f) timer -= Time.deltaTime;

        // Timer cadangan: kalau Animation Event reset_lompat gagal, tetap otomatis selesai
        if (sedangLompat)
        {
            timerLompat -= Time.deltaTime;
            if (timerLompat <= 0f) reset_lompat();
        }

        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        Transform cam = Camera.main.transform;
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 arah = camForward * v + camRight * h;
        bool bergerak = arah.sqrMagnitude > 0.01f;

        if (bergerak)
        {
            arah.Normalize();
            Quaternion target = Quaternion.LookRotation(arah);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, kecepatanPutar * Time.deltaTime);
            transform.position += arah * kecepatan * Time.deltaTime;
        }

        anim.SetBool("jalan", bergerak);

        // ---------- LOMPAT ----------
        if (Input.GetKeyDown(KeyCode.Space) && !sedangLompat)
        {
            sedangLompat = true;
            timerLompat = durasiLompat;
            anim.SetBool("lompat", true);
        }

        // ---------- SERANG ----------
        if (Input.GetButtonDown("Fire1"))
        {
            lastAtk = Time.time;
            comboAtk++;
            comboAtk = Mathf.Clamp(comboAtk, 0, 3);
            anim.SetFloat("serang", comboAtk);
        }

        if (Time.time - lastAtk >= 1f && comboAtk != 0)
        {
            comboAtk = 0;
            anim.SetFloat("serang", comboAtk);
        }
    }

    public void TerimaHit()
    {
        if (timer > 0f) return;
        timer = jedaHit;

        anim.ResetTrigger("hit");
        anim.SetTrigger("hit");
        Debug.Log("Hero kena hit!");
    }

    // Dipanggil dari Animation Event di akhir clip lompat (atau otomatis oleh timer)
    public void reset_lompat()
    {
        sedangLompat = false;
        anim.SetBool("lompat", false);
    }
}