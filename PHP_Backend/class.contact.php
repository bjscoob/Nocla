<?php
class Contact{
    //Properies
    public $username;
    public $firstname;
    public $lastname;
    
    //Methods
    function __construct($data){
        $username = $data['username'];
        $firstname= $data['firstname'];
        $lastname = $data['lastname'];
    }
}



?>