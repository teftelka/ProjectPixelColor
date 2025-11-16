using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionDrag : MonoBehaviour
{
    [Header("UI")]
    public Canvas canvas;                 // Screen Space - Overlay
    public RectTransform selectionBox;    // UI Image прямоугольника

    private Vector2 startMousePosScreen;
    private bool dragging;

    void Awake()
    {
        if (selectionBox) selectionBox.gameObject.SetActive(false);
    }

    void Update()
    {
        // игнор клика по UI (если нужно)
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            dragging = true;
            startMousePosScreen = Input.mousePosition;
            if (selectionBox) selectionBox.gameObject.SetActive(true);
            UpdateSelectionBox(Input.mousePosition);
        }
        else if (dragging && Input.GetMouseButton(0))
        {
            UpdateSelectionBox(Input.mousePosition);
        }
        else if (dragging && Input.GetMouseButtonUp(0))
        {
            dragging = false;
            if (selectionBox) selectionBox.gameObject.SetActive(false);

            // здесь запускаем выбор объектов
            //SelectInsideScreenRect(startMousePosScreen, (Vector2)Input.mousePosition);
        }
    }

    void UpdateSelectionBox(Vector2 currentMouseScreen)
    {
        if (!selectionBox || !canvas) return;

        RectTransform canvasRect = canvas.transform as RectTransform;

        Rect screenRect = GetScreenRect(startMousePosScreen, currentMouseScreen);
        // в локальные координаты канваса
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, new Vector2(screenRect.xMin, screenRect.yMin), canvas.worldCamera, out Vector2 bl);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, new Vector2(screenRect.xMax, screenRect.yMax), canvas.worldCamera, out Vector2 tr);

        Vector2 size = tr - bl;
        selectionBox.anchoredPosition = bl;
        selectionBox.sizeDelta = size;
        selectionBox.pivot = Vector2.zero; // важно: чтобы anchoredPosition был левым-нижним углом
    }

    static Rect GetScreenRect(Vector2 p1, Vector2 p2)
    {
        // нормализуем любую диагональ в прямоугольник (xMin<=xMax, yMin<=yMax)
        Vector2 min = Vector2.Min(p1, p2);
        Vector2 max = Vector2.Max(p1, p2);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    // ====== ВЫБОР ОБЪЕКТОВ (ниже 2 варианта) ======

    // ВАРИАНТ A (простой): 2D/“точечная” проверка по экрану
    /*void SelectInsideScreenRect(Vector2 start, Vector2 end)
    {
        Rect r = GetScreenRect(start, end);
        var cam = Camera.main;
        var all = FindObjectsOfType<Selectable>(); // см. класс ниже

        foreach (var s in all)
        {
            Vector3 sp = cam.WorldToScreenPoint(s.transform.position);
            bool inside = r.Contains(sp, true);
            s.SetSelected(inside);
        }
    }*/

    // ВАРИАНТ B (3D/объёмно): тест коллайдера в усечённой пирамиде выделенного прямоугольника
    // Вызови вместо SelectInsideScreenRect, если нужен объёмный выбор:
    /*
    void SelectInsideScreenRect_3D(Vector2 start, Vector2 end)
    {
        var cam = Camera.main;
        Rect r = GetScreenRect(start, end);

        // переводим экранный прямоугольник в нормализованные координаты viewport [0..1]
        Vector2 vpMin = new Vector2(r.xMin / Screen.width, r.yMin / Screen.height);
        Vector2 vpMax = new Vector2(r.xMax / Screen.width, r.yMax / Screen.height);

        // строим матрицу проекции подпрямоугольника в текущей камере
        Matrix4x4 proj = Matrix4x4.Perspective(cam.fieldOfView, cam.aspect, cam.nearClipPlane, cam.farClipPlane);
        // шейпим её под выделенный viewport
        var m = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
        // вместо громоздкого матем-перехода проще собрать плоскости вручную:
        Plane[] planes = GetFrustumFromViewportRect(cam, vpMin, vpMax);

        var all = FindObjectsOfType<Selectable>();
        foreach (var s in all)
        {
            var col = s.GetComponent<Collider>();
            if (!col) { s.SetSelected(false); continue; }
            bool inside = GeometryUtility.TestPlanesAABB(planes, col.bounds);
            s.SetSelected(inside);
        }
    }

    static Plane[] GetFrustumFromViewportRect(Camera cam, Vector2 vpMin, Vector2 vpMax)
    {
        // 8 углов фрустума получаем лучами из камеры
        Vector3[] corners = new Vector3[8];
        // near
        corners[0] = cam.ViewportToWorldPoint(new Vector3(vpMin.x, vpMin.y, cam.nearClipPlane));
        corners[1] = cam.ViewportToWorldPoint(new Vector3(vpMax.x, vpMin.y, cam.nearClipPlane));
        corners[2] = cam.ViewportToWorldPoint(new Vector3(vpMax.x, vpMax.y, cam.nearClipPlane));
        corners[3] = cam.ViewportToWorldPoint(new Vector3(vpMin.x, vpMax.y, cam.nearClipPlane));
        // far
        corners[4] = cam.ViewportToWorldPoint(new Vector3(vpMin.x, vpMin.y, cam.farClipPlane));
        corners[5] = cam.ViewportToWorldPoint(new Vector3(vpMax.x, vpMin.y, cam.farClipPlane));
        corners[6] = cam.ViewportToWorldPoint(new Vector3(vpMax.x, vpMax.y, cam.farClipPlane));
        corners[7] = cam.ViewportToWorldPoint(new Vector3(vpMin.x, vpMax.y, cam.farClipPlane));

        // строим 6 плоскостей (порядок нормалей наружу)
        Plane[] p = new Plane[6];
        p[0] = new Plane(corners[0], corners[1], corners[2]); // near
        p[1] = new Plane(corners[6], corners[5], corners[4]); // far
        p[2] = new Plane(corners[0], corners[3], corners[7]); // left
        p[3] = new Plane(corners[2], corners[1], corners[5]); // right
        p[4] = new Plane(corners[1], corners[0], corners[4]); // bottom
        p[5] = new Plane(corners[3], corners[2], corners[6]); // top
        return p;
    }
    */
}
