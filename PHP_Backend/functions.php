<?php 
require "class.user.php";
date_default_timezone_set("America/New_York");
$con = mysqli_connect("localhost","jacksonb_api","admin","jacksonb_wrdp3");
if(!$con){echo "Database connection failed";}
$_SESSION['user'] = "No user found";
function isExp($dateStr){
    $expDate = DateTime::createFromFormat("d M Y H:i:s",$dateStr);
    $now = new DateTime();
    if($now < $expDate){
        return false;
    }
    else{
        return true;
    }
    
}
function checkAuthToken($token){
    $con = mysqli_connect("localhost","jacksonb_api","admin","jacksonb_wrdp3");
    if(!$con){echo "Database connection failed";}
    $sql = "SELECT * FROM users WHERE Token ='".$token."'";
    $result = $con->query($sql);
    if (mysqli_num_rows($result) == 0)
        {
        return false;
        }
    else{
        
        $row = $result->fetch_assoc();
        $_SESSION['user'] = $row;
        if(isExp($_SESSION['user']['TokenExp'] )){
            return false;
            
        }
        else{
            return true;
            
        }
    }
}
function generateAuthToken($userID){
    $con = mysqli_connect("localhost","jacksonb_api","admin","jacksonb_wrdp3");
    if(!$con){echo "Database connection failed";}
    //Create Auth Token
    $bytes = random_bytes(20);
    $token = bin2hex($bytes);
    $exp = date("d M Y H:i:s", strtotime("+30 minutes"));
    $sql = "UPDATE users SET Token = '".$token."' , TokenExp = '".$exp."' WHERE id=".$userID;
    $result = $con->query($sql);
    return $token;
}
function sendResetEmail($email){
    $response = ["response" => "Database connection failed."];
    $con = mysqli_connect("localhost","jacksonb_api","admin","jacksonb_wrdp3");
    if(!$con){echo "Database connection failed";}
    $sql = "SELECT * FROM users WHERE email ='".$email."'";
    $result = $con->query($sql);
     if ($result->num_rows == 0) {
        $response['response'] = "Email not found.";
        $myJSON = json_encode($response);
        return $myJSON;
     }
    $row = $result->fetch_assoc();
    $userID = $row['id'];
    $username = $row['username'];
    $token = generateAuthToken($userID);
    // the message
    $msg = "Hello from Nocla!\n\n
    You are receiving this email because you have  forgotten your username or password.\n
    You're username is: ".$username."\n
    Please follow this link if you need to reset your password:\n\n
        jax-apps.com/rp.php?t=".$token."
    \n\n This link will expire in 30 minutes.
    \n If this was not you, you may contact jacksonb424@gmail.com to submit a ticket.";
    // use wordwrap() if lines are longer than 70 characters
    $msg = wordwrap($msg,70);
    $mailed = mail($email,"Nocla Password Reset",$msg);
   
    if($mailed){
        $response = ["response" => "Success! Email has been sent."];
    }
    else{
        $response = ["response" => "Sorry. The email failed to send.|Try again or submit a ticket."];
    }
    $myJSON = json_encode($response);
    return $myJSON;
    
}
function changePassword($id, $hash){
    $con = mysqli_connect("localhost","jacksonb_api","admin","jacksonb_wrdp3");
    if(!$con){echo "Database connection failed";}
    $sql = "UPDATE users SET password ='".$hash."'WHERE id =".$id;
    $result = $con->query($sql);
    if (mysqli_affected_rows($con) == 0)
        {
        return false;
        }
    else{
        //Generate New token so email link is invalid
        generateAuthToken($id);
        return true;
    }
}

?>