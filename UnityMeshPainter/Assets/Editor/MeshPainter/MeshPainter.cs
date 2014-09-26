/// MeshPainter Unity tool to paint mesh on other mesh. Find under Window/Tools/MeshPainter.
/// Made by Cyril Carincotte
/// contact : ccarincotte@gmail.com
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


[System.Serializable()]
public class MeshPainter : EditorWindow
{
    /// <summary>
    ///     Instancies mesh to paint following a brush type.
    /// </summary>
    /// <param name="center">
    ///     Position of click.
    /// </param>
    public delegate void BrushDelegate(Vector3 center);


#region Mesh informations
    /// <summary>
    /// Map GameObject to paint triangles count.
    /// </summary>
    [SerializeField]
    public SerializableDictionary<GameObject, int> triangles = new SerializableDictionary<GameObject, int>();

    /// <summary>
    /// Map GameObject to paint vertices count.
    /// </summary>
    [SerializeField]
    public SerializableDictionary<GameObject, int> vertices = new SerializableDictionary<GameObject, int>();

    /// <summary>
    /// How many object have been generated.
    /// </summary>
    public int nbObjectGenerated = 0;

    /// <summary>
    /// How mane triangles have been generated.
    /// </summary>
    public int nbTriangles = 0;

    /// <summary>
    /// How many vertices have been generated.
    /// </summary>
    public int nbVertices = 0;
#endregion

    /// <summary>
    /// All objects to paint.
    /// </summary>
    public GameObject[] toPaint = new GameObject[1];

    /// <summary>
    /// Value for random for each GameObject to paint.
    /// </summary>
    public float[] randValue = new float[1];

    /// <summary>
    /// Parent for each generated objects.
    /// </summary>
    public GameObject parent = null;

    /// <summary>
    /// tag used for ignoring other mesh.
    /// </summary>
    public string tag = "Untagged";

    /// <summary>
    /// Speed of refresh of brush.
    /// </summary>
    public float speedBrush = 0.5f;

    /// <summary>
    /// Max scale value.
    /// </summary>
    public float maxScale = 1.0f;

    /// <summary>
    /// Min scale value.
    /// </summary>
    public float minScale = 0.01f;

    /// <summary>
    /// Min Y rotation.
    /// </summary>
    public float minRotY = -180.0f;

    /// <summary>
    /// Max Y rotation.
    /// </summary>
    public float maxRotY = 180.0f;

    /// <summary>
    /// Min X rotation.
    /// </summary>
    public float minRotX = -180.0f;

    /// <summary>
    /// Max X rotation.
    /// </summary>
    public float maxRotX = 180.0f;

    /// <summary>
    /// Min Z rotation.
    /// </summary>
    public float minRotZ = -180.0f;

    /// <summary>
    /// Max Z rotation.
    /// </summary>
    public float maxRotZ = 180.0f;

    /// <summary>
    /// Tool is active.
    /// </summary>
    public bool activated = false;

    /// <summary>
    ///     All bursh functions
    /// </summary>
    public BrushDelegate[] brushes;

    /// <summary>
    /// List of all objects generated.
    /// </summary>
    private List<GameObject> _previousCreated = new List<GameObject>();

    private Vector3 lastPos = Vector3.zero;

    /// <summary>
    /// Time of last draw.
    /// </summary>
    private float _lastDraw = 0.0f;

    /// <summary>
    /// size of array of to paint GameObject.
    /// </summary>
    private int _arraySize = 1;

    /// <summary>
    /// Id of next generated object.
    /// </summary>
    private int _instanceId = 0;

    /// <summary>
    /// Generated object before release it on Scene.
    /// </summary>
    private GameObject _nextInstance = null;

    /// <summary>
    /// Click is down.
    /// </summary>
    private bool _isDrawing = false;

    /// <summary>
    ///     Window instance.
    /// </summary>
    private static MeshPainter _painter;

#region Editor values.
    /// <summary>
    ///     Describe Brush currently used
    /// </summary>
    /// <value>
    ///     None = No brush used : you draw on the mouse only.
    ///     Circle = a cricle brush. (On going)
    ///     Square = a square brush. (On going)
    ///     Mesh = use a mesh as brush. (not yet)
    /// </value>
    private enum BrushType
    {
        None = 0,
        Circle,
        Square,
        Mesh
    };

    /// <summary>
    ///     Which kind of brush currently used.
    /// </summary>
    private BrushType currentBrush = 0;

    private float rayCastdistance = 10.0f;

    /// <summary>
    /// Scroll value.
    /// </summary>
    private Vector2 _scroll = Vector2.zero;

    /// <summary>
    /// Array foldout is open.
    /// </summary>
    private bool _arraysFoldout = true;

    /// <summary>
    /// MeshPainter use normal to place objects.
    /// </summary>
    private bool _useNormal = true;

    /// <summary>
    /// Control button is down.
    /// </summary>
    private bool _control = false;
#endregion

    [MenuItem("Window/Tools/MeshPainter")]
    public static void GetWindow()
    {
        MeshPainter window = GetWindow<MeshPainter>();
        window.title = "Mesh Painter";
        //MeshPainter._window = window;
    }

    public static MeshPainter Window
    {
        get
        {
            if (MeshPainter._painter != null)
                return (MeshPainter._painter);
            MeshPainter._painter = GetWindow<MeshPainter>();
            MeshPainter._painter.brushes = new BrushDelegate[4] { MeshPainter._painter.InstanciateNext, MeshPainter._painter.InstanciateNext,
                                                                  MeshPainter._painter.InstanciateNext, MeshPainter._painter.InstanciateNext};
            return (MeshPainter._painter);
        }
    }

    protected void PaintMesh(Event ev) // Button 0 pressed paint Mesh on scene
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(ev.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo) == true)
        {
            if (this._nextInstance == null)
                return;
            this._nextInstance.transform.position += (hitInfo.point - this.lastPos);
            if (this._useNormal == true)
            {
                Vector3 interpolatedNormal;

                interpolatedNormal = hitInfo.normal;
                this._nextInstance.transform.rotation = Quaternion.FromToRotation(this._nextInstance.transform.up, interpolatedNormal) * this._nextInstance.transform.rotation;
            }
            this.lastPos = hitInfo.point;
            if (hitInfo.collider.tag != this.tag ||
                Time.realtimeSinceStartup - this._lastDraw < this.speedBrush)
                return;
            this.nbObjectGenerated += 1;
            if (this.triangles[this.toPaint[this._instanceId]] == -1)
                MeshPainter.GetTrianglesVertices(this._nextInstance, this.toPaint[this._instanceId]);
            this.nbTriangles += this.triangles[this.toPaint[this._instanceId]];
            this.nbVertices += this.vertices[this.toPaint[this._instanceId]];
            MeshPainter.Window.Repaint();
            MeshPainter.Window.brushes[(int)this.currentBrush](hitInfo.point);
            this._lastDraw = Time.realtimeSinceStartup;
        }
    }

    protected void InstanciateNext(Vector3 position)
    {
        GameObject prefab = null;
        float rand = Random.Range(0.0f, 1.1f);
        float tmp = 0.0f;
        MeshPainter painter = MeshPainter.Window;

        for (int i = 0, size = painter.randValue.Length; i < size; ++i)
        {
            tmp += painter.randValue[i];
            if (tmp > rand)
            {
                prefab = painter.toPaint[i];
                painter._instanceId = i;
                break;
            }
        }
        if (prefab == null)
            return;
        GameObject obj = GameObject.Instantiate(prefab, position, Quaternion.identity) as GameObject;

        if (painter.parent != null)
            obj.transform.parent = painter.parent.transform;
        if (painter._useNormal == false)
            obj.transform.Rotate(Random.Range(painter.minRotX, painter.maxRotX),
                                 Random.Range(painter.minRotY, painter.maxRotY),
                                 Random.Range(painter.minRotZ, painter.maxRotZ));
        else
        {
            obj.transform.Rotate(0.0f,
                                 Random.Range(painter.minRotY, painter.maxRotY),
                                 0.0f);
        }
        float scale = Random.Range(painter.minScale, painter.maxScale);
        obj.transform.localScale = new Vector3(scale, scale, scale);
        painter._previousCreated.Add(painter._nextInstance);
        painter._nextInstance = obj;
    }

    protected static void UnDo()
    {
        if (MeshPainter.Window._previousCreated.Count == 0)
            return;
        GameObject go = MeshPainter.Window._previousCreated[MeshPainter.Window._previousCreated.Count - 1];

        MeshPainter.Window._previousCreated.Remove(go);
        DestroyImmediate(go);
    }

    public static void OnScene(SceneView view)
    {
        MeshPainter painter = MeshPainter.Window;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        Event ev = Event.current;

        //if (ev.isKey == true)
        //{
        //    if (ev.keyCode == KeyCode.Z && ev.control == true)
        //    {
        //        MeshPainter.UnDo();
        //        ev.Use();
        //    }
        //}
        if (ev.isMouse == true)
        {
            if (ev.type == EventType.MouseDown && ev.button == 0) // LEFT BUTTON DOWN
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(ev.mousePosition);
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo) == true)
                    MeshPainter.Window.brushes[(int)painter.currentBrush](hitInfo.point);
                else
                    MeshPainter.Window.brushes[(int)painter.currentBrush](Vector3.zero);
                painter._isDrawing = true;
                painter._lastDraw = Time.realtimeSinceStartup;
                painter.lastPos = hitInfo.point;
                //MeshPainter.nbObjectGenerated = 0;
                //MeshPainter.nbTriangles = 0;
                //MeshPainter.nbVertices = 0;
            }
            else if (ev.type == EventType.MouseUp && ev.button == 0) // LEFT BUTTON UP
            {
                //if (Time.realtimeSinceStartup - painter._lastDraw < painter.speedBrush)
                //{
                    GameObject.DestroyImmediate(painter._nextInstance);
                //}
                //else
                //    MeshPainter.PaintMesh(ev);
                //painter._nextInstance = null;
                painter._isDrawing = false;
            }
            if (ev.alt == true)
                return;
            else if (painter._isDrawing == true &&
                     painter.activated == true)
            {
                painter.PaintMesh(ev);
                //ev.Use();
            }
        }
        if (ev.type == EventType.Layout)
            HandleUtility.AddDefaultControl(controlID);
    }

    protected static void GetTrianglesVertices(GameObject go, GameObject key)
    {
        MeshFilter[] renderers = go.GetComponentsInChildren<MeshFilter>();
        SkinnedMeshRenderer[] skinRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();
        int nbTriangles = 0;
        int nbVertices = 0;

        for (int i = 0, size = renderers.Length; i < size; ++i)
        {
            nbTriangles += renderers[i].sharedMesh.triangles.Length;
            nbVertices += renderers[i].sharedMesh.vertexCount;
        }
        for (int i = 0, size = skinRenderers.Length; i < size; ++i)
        {
            nbTriangles += skinRenderers[i].sharedMesh.triangles.Length;
            nbVertices += skinRenderers[i].sharedMesh.vertexCount;
        }
        MeshPainter.Window.triangles[key] = nbTriangles;
        MeshPainter.Window.vertices[key] = nbVertices;
    }

    void OnGUI()
    {
        MeshPainter painter = MeshPainter.Window;
        GUIContent content = new GUIContent();
        int newSize = 0;
        bool active = painter.activated;

        painter._scroll = EditorGUILayout.BeginScrollView(painter._scroll, false, false);
        content.text = "Active";
        painter.activated = EditorGUILayout.Toggle(content, painter.activated);
        if (painter.activated != active)
        {
            if (active == false)
            {
                SceneView.onSceneGUIDelegate += OnScene;
            }
            else
            {
                SceneView.onSceneGUIDelegate -= OnScene;
            }
        }
        content.text = "Parent of objects in Scene";
        painter.parent = EditorGUILayout.ObjectField(content, painter.parent, typeof(GameObject), true) as GameObject;
        content.text = "Object to Paint";
        painter._arraysFoldout = EditorGUILayout.Foldout(painter._arraysFoldout, content);
        #region Array foldout
        if (painter._arraysFoldout)
        {
            ++EditorGUI.indentLevel;
            GameObject go;
            content.text = "Arrays size of Objects";
            newSize = EditorGUILayout.IntField(content, painter._arraySize);
            if (newSize != painter._arraySize)
            {
                int i = 0;
                GameObject[] tmp = new GameObject[newSize];
                float[] randTmp = new float[newSize];

                for (int size = Mathf.Min(newSize, painter.toPaint.Length); i < size; ++i)
                {
                    tmp[i] = painter.toPaint[i];
                    randTmp[i] = painter.randValue[i];
                }
                for (int size = randTmp.Length; i < size; ++i)
                    randTmp[i] = 0.0f;
                painter.toPaint = tmp;
                painter.randValue = randTmp;
                painter._arraySize = newSize;
            }
            for (int i = 0; i < newSize; ++i)
            {
                content.text = "Object " + i.ToString();
                GUILayout.BeginHorizontal();
                {
                    go = painter.toPaint[i];
                    painter.toPaint[i] = EditorGUILayout.ObjectField(content, painter.toPaint[i], typeof(GameObject)) as GameObject;
                    if (go != painter.toPaint[i])
                    {
                        painter.triangles.Remove(painter.toPaint[i]);
                        painter.vertices.Remove(painter.toPaint[i]);
                        if (painter.toPaint[i] != null)
                        {
                            painter.triangles[painter.toPaint[i]] = -1;
                            painter.vertices[painter.toPaint[i]] = -1;
                        }
                        //    this.GetTrianglesVertices(MeshPainter.toPaint[i]);
                    }
                    painter.randValue[i] = EditorGUILayout.FloatField((painter.randValue[i] * 100.0f)) / 100.0f;
                    content.text = "%";
                    EditorGUILayout.LabelField(content, GUILayout.MaxWidth(20));
                }
                GUILayout.EndHorizontal();
            }
            --EditorGUI.indentLevel;
        }
        #endregion
        content.text = "Tag to draw on";
        painter.tag = EditorGUILayout.TagField(content, painter.tag);
        content.text = "Scale range";
        GUILayout.BeginHorizontal();
        {
            painter.minScale = EditorGUILayout.FloatField(content, painter.minScale);
            painter.maxScale = EditorGUILayout.FloatField(painter.maxScale);
        }
        GUILayout.EndHorizontal();


        content.text = "Rotation Y";
        GUILayout.BeginHorizontal();
        {
            //EditorGUILayout.MinMaxSlider(content, ref MeshPainter.minRotY, ref MeshPainter.maxRotY, -360.0f, 360.0f);
            painter.minRotY = EditorGUILayout.FloatField(content, painter.minRotY);
            painter.maxRotY = EditorGUILayout.FloatField(painter.maxRotY);
        }
        GUILayout.EndHorizontal();

        content.text = "Follow normal";
        painter._useNormal = EditorGUILayout.Toggle(content, painter._useNormal);
        if (painter._useNormal == false)
        {
            //EditorGUILayout.MinMaxSlider(content, ref MeshPainter.minScale, ref MeshPainter.maxScale, 0.01f, 5.0f);
            content.text = "Rotation X";
            GUILayout.BeginHorizontal();
            {
                //EditorGUILayout.MinMaxSlider(content, ref MeshPainter.minRotX, ref MeshPainter.maxRotX, -360.0f, 360.0f);
                painter.minRotX = EditorGUILayout.FloatField(content, painter.minRotX);
                painter.maxRotX = EditorGUILayout.FloatField(painter.maxRotX);
            }

            GUILayout.EndHorizontal();
            content.text = "Rotation Z";
            GUILayout.BeginHorizontal();
            {
                //EditorGUILayout.MinMaxSlider(content, ref MeshPainter.minRotZ, ref MeshPainter.maxRotZ, -360.0f, 360.0f);
                painter.minRotZ = EditorGUILayout.FloatField(content, painter.minRotZ);
                painter.maxRotZ = EditorGUILayout.FloatField(painter.maxRotZ);
            }
            GUILayout.EndHorizontal();
        }

        content.text = "Brush type : ";
        painter.currentBrush = (BrushType)EditorGUILayout.EnumPopup(content, painter.currentBrush);

        content.text = "Refresh rate of brush";
        painter.speedBrush = EditorGUILayout.FloatField(content, painter.speedBrush);


        content.text = "Brush raycast distance : ";
        painter.rayCastdistance = EditorGUILayout.FloatField(content, painter.rayCastdistance);


        content.text = "Objects generated : " + painter.nbObjectGenerated;
        EditorGUILayout.LabelField(content);
        content.text = "Triangles generated : " + painter.nbTriangles;
        EditorGUILayout.LabelField(content);
        content.text = "Vertices generated : " + painter.nbVertices;
        EditorGUILayout.LabelField(content);

        content.text = "Reset information generated";
        if (GUILayout.Button(content) == true)
        {
            painter.ClearInfo();
        }
        content.text = "Destroy generated objects";
        if (GUILayout.Button(content) == true)
        {
            painter.DestroyPrevious();
        }

        EditorGUILayout.EndScrollView();
    }

    public void DestroyPrevious()
    {
        foreach (GameObject instance in this._previousCreated)
        {
            GameObject.DestroyImmediate(instance);
        }
        this.ClearInfo();
    }

    public void ClearInfo()
    {
        this.nbObjectGenerated = 0;
        this.nbTriangles = 0;
        this.nbVertices = 0;
        this._previousCreated.Clear();
    }

    public void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= OnScene;
    }
}
