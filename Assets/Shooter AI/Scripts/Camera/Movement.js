    var speed = 10.0f;
     
    function Update()
    {
    var movement = Vector3.zero;
     
    movement.z = Input.GetAxis("Vertical");
    movement.x = Input.GetAxis("Horizontal");
     
    transform.Translate(movement * speed * Time.deltaTime, Space.Self);

if(Input.GetKey(KeyCode.R))
{
Application.LoadLevel(Application.levelCount-1);
}

    }