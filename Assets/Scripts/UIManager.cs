using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading.Tasks;
using TMPro;
using System;
using UnityEngine.Events;

[Serializable]
public class ColorEvent:UnityEvent<Color>{}

public class UIManager : MonoBehaviour
{
    #region  UI Elements
    [SerializeField] private Camera cam;
    [SerializeField] private Slider Zoom;
    [SerializeField] private List<Slider> RotationSlider;
    [SerializeField] private List<Slider> fersnelSliders;
    [SerializeField] private Toggle toggle;
    [SerializeField] private Toggle MouseRotation;
    [SerializeField] private Dropdown dropdown;
  
    #endregion
    
    #region Complext Variables
    [SerializeField] private List<Shader> ShaderList;
    [SerializeField] List<Renderer> rend;
    [SerializeField] RectTransform rect;
     [SerializeField] Image ColorPickerImage;
    [SerializeField] Texture2D ColorTexture;
    #endregion  

    #region  Game Objects
    GameObject ReferanceObject;
    #endregion
    
    #region  Vectors
    public Vector3 offset;
    Vector3 facingDirection;
    Vector3 PreviousDirection;
    Quaternion rotation;
    #endregion

    #region SimpleVariables
    [SerializeField] int speed;
    float elapsedTime;
    float desiredTime;

    bool canRotate;
    #endregion

    #region Events
    public ColorEvent onColorSelect;
    #endregion
    [SerializeField] float minsize;
    [SerializeField] float maxsize;

    // Start is called before the first frame update
    private void Start() {
        cam = Camera.main;
        ReferanceObject = GameObject.FindGameObjectWithTag("ViewableObject");
        populateRenderer();
        desiredTime = 1;
        minsize = 0.2f;
        maxsize = 3;
        canRotate = true;
        ColorTexture = ColorPickerImage.mainTexture as Texture2D;
        
        Zoom.maxValue = maxsize;
        Zoom.minValue = minsize;
        
        rotation = Quaternion.identity;
        cam.transform.position = ReferanceObject.transform.position;
        cam.transform.Rotate(new Vector3(1,0,0),facingDirection.y);
        cam.transform.Rotate(new Vector3(0,1,0),-facingDirection.x, Space.World);
        cam.transform.Translate(new Vector3(0,0,-5));


        //This is assigning a Delegate or an action to the callback methof og onValueChanged.
        toggle.onValueChanged.AddListener(delegate{
            ToggleShader(toggle);
        });
        MouseRotation.onValueChanged.AddListener(delegate{
            ToggleRotation(MouseRotation);
        });

        dropdown.onValueChanged.AddListener(delegate{
            DropDownChange(dropdown);
        });
        rotationSliderDelegate(RotationSlider);
      
    }

    private void Update() {
        //here we are checking if the mouse pointer is over the game. If So we dont want to interact with the game using the Mouse so we just return instantly
        fersnelSliderFunc(fersnelSliders);
        //fersnelRendererFunc();
        ColorPicker();
        if(EventSystem.current.IsPointerOverGameObject())
            return;

        if(canRotate)
            Rotation();
        
    }

    #region  MouseRotation
    public void Rotation()
    {
        
        if(Input.GetMouseButtonDown(0)){
            PreviousDirection = cam.ScreenToViewportPoint(Input.mousePosition);
        }
        if(Input.GetMouseButton(0)){
            
            facingDirection = cam.ScreenToViewportPoint(Input.mousePosition) - PreviousDirection;

            cam.transform.position = ReferanceObject.transform.position;
            cam.transform.Rotate(new Vector3(1,0,0),facingDirection.y * 180);
            cam.transform.Rotate(new Vector3(0,1,0),-facingDirection.x * 180, Space.World);
            cam.transform.Translate(new Vector3(0,0,-5));
            PreviousDirection = cam.ScreenToViewportPoint(Input.mousePosition);
        }
            
    }
    #endregion
 
    #region Buttons
 
    public void ZoomFunc(){
        cam.orthographicSize = Zoom.value;   
    } 
    public void populateRenderer()
    {
        Debug.Log(ReferanceObject.transform.childCount);
        for (var i = 0; i < ReferanceObject.transform.GetChild(0).childCount; i++)
        {
           rend.Add(ReferanceObject.transform.GetChild(0).GetChild(i).GetComponent<Renderer>());
        }    
    }
    public void rotationSliderDelegate(List<Slider> SlidersToInit){
        for (var i = 0; i < SlidersToInit.Count; i++)
        {
            SlidersToInit[i].onValueChanged.AddListener(delegate{
                RotateAroundReferance();
            });
        }
    }

    public void ToggleShader(Toggle toggle)
    {
        Debug.Log(ReferanceObject.transform.GetChild(0).childCount);
        if(toggle.isOn){
            for (var i = 0; i < ReferanceObject.transform.GetChild(0).childCount; i++)
            {
                rend[i].material.shader = ShaderList[0];
            }
        }else{
            for (var i = 0; i < ReferanceObject.transform.GetChild(0).childCount; i++)
            {
                rend[i].material.shader = ShaderList[1];
            }
        }
        
    }
    public void ToggleRotation(Toggle mouseRotation){
        if(mouseRotation.isOn)
            canRotate = true;
        else
            canRotate = false;
    }
    public void DropDownChange(Dropdown drop){
       //dropdown.onValueChanged;
       Debug.Log(drop.value); 

       switch (dropdown.value)
       {
           //inside of this switch case we are going to run our lerp function to rotate the camera to a certain point.
            case 0:
                rotation.eulerAngles = new Vector3(0,0,0);
                cam.transform.SetPositionAndRotation(new Vector3(0,0,-5f),rotation);
                break;
           case 1:
                rotation.eulerAngles = new Vector3(0,-90,0);
                cam.transform.SetPositionAndRotation(new Vector3(5f,0,0f),rotation);               
                break;
            case 2:
                rotation.eulerAngles = new Vector3(0,90,0);
                cam.transform.SetPositionAndRotation(new Vector3(-5f,0,0f),rotation);               
                break;
            case 3:
                rotation.eulerAngles = new Vector3(0,180,0);
                cam.transform.SetPositionAndRotation(new Vector3(0,0,5f),rotation);               
               break;
            case 4:
                rotation.eulerAngles = new Vector3(90,0,0);
                cam.transform.SetPositionAndRotation(new Vector3(0f,5,0f),rotation);
                break;
            case 5:
                rotation.eulerAngles = new Vector3(-90,0,0);
                cam.transform.SetPositionAndRotation(new Vector3(0f,-5,0f),rotation);
                break;
           default:
           Debug.LogError("Somehow You have Selected Something That isnt Possible");
               break;
       }
       
    }

    public void RotateAroundReferance(){
       
       /* 
        float h = Vector3.Distance(ReferanceObject.transform.position,cam.transform.position);
        float AngleInRad = slid.value * Mathf.Deg2Rad;


        float x = Mathf.Cos(AngleInRad);
        float y = Mathf.Sin(AngleInRad);
        float z = Mathf.Cos(AngleInRad) * Mathf.Sin(AngleInRad);

        Quaternion angleaxis = Quaternion.AngleAxis(AngleInRad,new Vector3(0,0,1));
        Vector3 Position = new Vector3(x,y,0);
        
        */
        //cam.transform.LookAt(ReferanceObject.transform.position);
        cam.transform.localEulerAngles = new Vector3(RotationSlider[0].value ,RotationSlider[1].value , RotationSlider[2].value);
        cam.transform.position = ReferanceObject.transform.position - cam.transform.forward * 5;
        

    }    public void ResetCameraOrientation(){
        //Here we are going to lerp when the button is pressed.
        cam.transform.position= new Vector3(0,0,-5);
        cam.transform.localEulerAngles = new Vector3 (0,0,0); 
        cam.orthographicSize = 3;
    }
    #endregion

    #region Fersnel Sliders

    public void fersnelSliderFunc(List<Slider> slider){
        for (var i = 0; i < slider.Count; i++)
        {
            rend[i].material.SetFloat("_RimPower",slider[0].value);
            rend[i].material.SetFloat("_RimWidth",slider[1].value);
        }
    }
    public void fersnelRendererFunc(Color color){
        //Debug.Log(Running);
        for (var i = 0; i < rend.Count; i++)
            rend[i].material.SetColor("_RimColor",color);
    
    }

    public void ColorPicker()
    {
        Vector2 delta;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,Input.mousePosition,null,out delta);

        float width = rect.rect.width;
        float height = rect.rect.height;

        delta += new Vector2(width * 0.5f, height*0.5f);
        float x = Mathf.Clamp(delta.x/width, 0,1);
        float y = Mathf.Clamp(delta.y/height, 0,1);
        
        int yy = Mathf.RoundToInt(y * ColorTexture.height);
        int xx = Mathf.RoundToInt(x * ColorTexture.width);
        
        Color color = ColorTexture.GetPixel(xx,yy);

        if(Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject()){
            onColorSelect?.Invoke(color);
        }
    }



    #endregion

}
