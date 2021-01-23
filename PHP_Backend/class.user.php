<?php 
class User{
    
    //Properties
    public $id;
    public $username;
    public $password;
    public $email;
    public $firstname;
    public $lastname;
    public $position;
    public $shift;
    public $manager;
    public $photo;
    public $pm_assignment;
    public $status;
    public $token;
    public $tokenExp;
    public $deviceID;
    
    //Methods
    function __construct($data){
       $id = $data[0] ;
        $username= $data[1] ;
       $password = $data[2]  ;
        $email=$data[3] ;
        $firstname=$data[4] ;
        $lastname=$data[5] ;
        $position=$data[6] ;
        $shift=$data[7] ;
        $manager=$data[8] ;
        $photo=$data[9] ;
        $pm_assignment=$data[10] ;
        $status=$data[11] ;
        $token=$data[12] ;
        $tokenExp=$data[13] ;
        $deviceID=$data[14] ;
    }
    
}

?>