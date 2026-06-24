using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
public class CatController : MonoBehaviour
{
    [SerializeField] float          moveSpeed = 3f;
    [SerializeField] SpriteRenderer spriteRenderer;

    Rigidbody2D     _rb;
    Vector2         _target;
    bool            _moving;
    ClickableObject _pendingInteraction;

    void Awake()
    {
        _rb              = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
        _target          = transform.position;
    }

    void Update()
    {
        // right-click — move freely
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 w           = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _target             = new Vector2(w.x, w.y);
            _moving             = true;
            _pendingInteraction = null;
        }

        // left-click — interact with world objects (skip if over UI)
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var     hit   = Physics2D.Raycast(new Vector2(world.x, world.y), Vector2.zero);

            if (hit.collider != null)
            {
                var clickable = hit.collider.GetComponent<ClickableObject>();
                if (clickable != null) { WalkThenInteract(clickable); return; }

                var mouseCtrl = hit.collider.GetComponent<MouseController>();
                if (mouseCtrl != null) mouseCtrl.HandleClick();
            }
        }
    }

    public void WalkThenInteract(ClickableObject target)
    {
        _target             = target.transform.position;
        _moving             = true;
        _pendingInteraction = target;
    }

    void FixedUpdate()
    {
        if (!_moving) { _rb.velocity = Vector2.zero; return; }

        Vector2 dir       = _target - (Vector2)transform.position;
        float   threshold = _pendingInteraction != null ? 1.2f : 0.08f;

        if (dir.magnitude < threshold)
        {
            _moving      = false;
            _rb.velocity = Vector2.zero;
            if (_pendingInteraction != null)
            {
                _pendingInteraction.Interact();
                _pendingInteraction = null;
            }
            return;
        }

        _rb.velocity = dir.normalized * moveSpeed;
        if (spriteRenderer != null) spriteRenderer.flipX = dir.x < 0;
    }
}
