<?php
require "class.contact.php";
date_default_timezone_set("America/New_York");
$con = mysqli_connect("localhost","jacksonb_api","admin","jacksonb_wrdp3");
if(isset($_POST['getUsers'])){
    $sql = 'SELECT id,username,firstname,lastname,photo FROM users';
    $result = $con->query($sql);
    $Contacts = array();
    
    while($row = $result->fetch_assoc()){
        
            $row['isContact'] = "0";
            array_push($Contacts, $row);
            
        }
        $myJSON = json_encode($Contacts);
        echo $myJSON;
    
}
if(isset($_POST['getMsg'])){
    
    $sql = "SELECT * FROM messages WHERE recipient ='".$_POST['r']."' ORDER BY time DESC";
    if(!($result = $con->query($sql))){
        error_log($con->error);
        
    }
    $Messages = array();
    
    while($row = $result->fetch_assoc()){
        
            array_push($Messages, $row);
            
        }
        $myJSON = json_encode($Messages);
        echo $myJSON;
       
}


?>