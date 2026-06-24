using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CatController : MonoBehaviour
{
    [SerializeField] float          moveSpeed = 3f;
    [SerializeField] SpriteRenderer spriteRenderer;

    Rigidbody2D _rb;
    Vector2     _target;
    bool        _moving;

    void Awake()
    {
        _rb              = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
        _target          = transform.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 w = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _target   = new Vector2(w.x, w.y);
            _moving   = true;
        }
    }

    void FixedUpdate()
    {
        if (!_moving) { _rb.velocity = Vector2.zero; return; }
        Vector2 dir = _target - (Vector2)transform.position;
        if (dir.magnitude < 0.08f)
        {
            _moving      = false;
            _rb.velocity = Vector2.zero;
            return;
        }
        _rb.velocity = dir.normalized * moveSpeed;
        if (spriteRenderer != null) spriteRenderer.flipX = dir.x < 0;
    }
}
