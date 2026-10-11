using UnityEngine;

public class SkeletonSC : MonoBehaviour
{
    enum State { Patroli, Kejar, Serang }
    State state = State.Patroli;

    Animator anim;
    Transform hero;

    [Header("Titik patroli")]
    public Transform movePoint;
    Transform[] points;
    int indexTujuan = 0;
    int arah = 1;
    public bool bolakBalik = false;
    public float jarakSampai = 0.3f;
    public float waktuTunggu = 0.3f;
    float timerTunggu;
    bool menunggu;

    [Header("Gerak")]
    public float kecepatanPatroli = 2f;
    public float kecepatanKejar = 3.5f;
    public float kecepatanPutar = 6f;

    [Header("Deteksi hero")]
    public float jarakLihat = 10f;
    [Range(0, 180)] public float sudutPandang = 60f;   // setengah sudut kerucut pandang (60 = total 120°)
    public float jarakKehilangan = 15f;                // di luar jarak ini, berhenti mengejar
    public float tinggiMata = 1.5f;
    public LayerMask layerHalangan = ~0;               // layer yang bisa menghalangi pandangan

    [Header("Serang")]
    public float jarakSerang = 1.8f;
    public float cooldownSerang = 1.5f;
    public int damage = 10;
    float timerSerang;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();

        // cari hero: tag Player dulu, kalau tidak ada pakai nama
        GameObject h = null;
        try { h = GameObject.FindWithTag("Player"); } catch { }
        if (h == null) h = GameObject.Find("Lady Pirate");
        if (h != null) hero = h.transform;
        else Debug.LogError("Hero tidak ditemukan! Beri tag Player atau cek nama 'Lady Pirate'.");

        if (movePoint == null)
        {
            GameObject mp = GameObject.Find("movePoint");
            if (mp != null) movePoint = mp.transform;
        }
        if (movePoint != null)
        {
            points = new Transform[movePoint.childCount];
            for (int i = 0; i < points.Length; i++)
                points[i] = movePoint.GetChild(i);
        }
    }

    void Update()
    {
        if (hero == null) return;

        float jarak = Vector3.Distance(transform.position, hero.position);
        timerSerang -= Time.deltaTime;

        switch (state)
        {
            case State.Patroli:
                SetSerang(false);
                if (BisaMelihatHero()) state = State.Kejar;
                else Patroli();
                break;

            case State.Kejar:
                SetSerang(false);
                if (jarak > jarakKehilangan) { state = State.Patroli; break; }
                if (jarak <= jarakSerang) { state = State.Serang; break; }
                Kejar();
                break;

            case State.Serang:
                if (jarak > jarakSerang + 0.5f) { state = State.Kejar; SetSerang(false); break; }
                Serang();
                break;
        }
    }

    // ---------- PATROLI ----------
    void Patroli()
    {
        if (points == null || points.Length == 0) { SetJalan(false); return; }

        if (menunggu)
        {
            timerTunggu -= Time.deltaTime;
            if (timerTunggu <= 0f) menunggu = false;
            SetJalan(false);
            return;
        }

        Vector3 selisih = points[indexTujuan].position - transform.position;
        selisih.y = 0f;

        if (selisih.magnitude <= jarakSampai)
        {
            GantiTujuan();
            if (waktuTunggu > 0f) { menunggu = true; timerTunggu = waktuTunggu; }
            return;
        }

        GerakKe(selisih, kecepatanPatroli);
    }

    void GantiTujuan()
    {
        if (bolakBalik)
        {
            indexTujuan += arah;
            if (indexTujuan >= points.Length) { arah = -1; indexTujuan = Mathf.Max(points.Length - 2, 0); }
            else if (indexTujuan < 0) { arah = 1; indexTujuan = Mathf.Min(1, points.Length - 1); }
        }
        else indexTujuan = (indexTujuan + 1) % points.Length;
    }

    // ---------- KEJAR ----------
    void Kejar()
    {
        Vector3 selisih = hero.position - transform.position;
        selisih.y = 0f;
        GerakKe(selisih, kecepatanKejar);
    }

    // ---------- SERANG ----------
    void Serang()
    {
        SetJalan(false);
        SetSerang(true);   // Bool tetap true selama dalam jarak serang

        Vector3 selisih = hero.position - transform.position;
        selisih.y = 0f;
        if (selisih.sqrMagnitude > 0.001f)
        {
            Quaternion t = Quaternion.LookRotation(selisih);
            transform.rotation = Quaternion.Slerp(transform.rotation, t, kecepatanPutar * Time.deltaTime);
        }

        if (timerSerang <= 0f)
        {
            timerSerang = cooldownSerang;

            MovementSC hh = hero.GetComponentInParent<MovementSC>();
            if (hh == null) hh = hero.GetComponent<MovementSC>();
            if (hh != null) hh.TerimaHit();

            Debug.Log("Skeleton menyerang hero!");
        }
    }

    // ---------- UTILITAS ----------
    bool BisaMelihatHero()
    {
        Vector3 dariMata = transform.position + Vector3.up * tinggiMata;
        Vector3 keHero = (hero.position + Vector3.up * 1f) - dariMata;

        if (keHero.magnitude > jarakLihat) return false;

        Vector3 datar = keHero; datar.y = 0f;
        if (Vector3.Angle(transform.forward, datar) > sudutPandang) return false;

        // cek tidak ada tembok di antara
        if (Physics.Raycast(dariMata, keHero.normalized, out RaycastHit hit, jarakLihat, layerHalangan))
        {
            if (hit.transform != hero && !hit.transform.IsChildOf(hero)) return false;
        }
        return true;
    }

    void GerakKe(Vector3 selisih, float kecepatan)
    {
        if (selisih.sqrMagnitude < 0.0001f) return;
        Quaternion target = Quaternion.LookRotation(selisih);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, kecepatanPutar * Time.deltaTime);
        transform.position += selisih.normalized * kecepatan * Time.deltaTime;
        SetJalan(true);
    }

    void SetSerang(bool nilai)
    {
        if (anim != null) anim.SetBool("isSerang", nilai);
    }

    void SetJalan(bool nilai)
    {
        if (anim != null) anim.SetBool("isJalan", nilai);
    }

    // Garis bantu di Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, jarakLihat);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, jarakSerang);
    }
}