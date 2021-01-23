<?php 
require "class.nothub.php";

if(!empty($_POST)){
    //NOTE: check auth

    //init database and notification hub    
    date_default_timezone_set("America/New_York");
    $con = mysqli_connect("localhost","jacksonb_api","admin","jacksonb_wrdp3");
    $hub = new NotificationHub("Endpoint=sb://noclahub.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=lkTVul72HMyEzy5lETKbW8JVCo2Y4qgl/KExbaMlHXM=", "noclaNotifyHub");
    
    //construct message
    $content = $_POST['senderFL']."|".$_POST['content'];
    
    
    /* //APPLE
    $alert = '{"aps":{"alert":"Hello from PHP!"}}';
    $notification = new Notification("apple", $alert);
    $hub->sendNotification($notification, null);
    */
    //ANDROID
    $message = '{"data":{"msg":"'.$content.'"}}';
    $notification = new Notification("gcm", $message);
    $hub->sendNotification($notification, $_POST['recipient']);
    
    
    
    //UPDATE MESSAGES TABLE
    //get user Photo
    $sql = "SELECT photo FROM users WHERE username ='".$_POST['sender']."'";
    $result = $con->query($sql);
    $row = $result->fetch_assoc();
    $photo = $row['photo'];
    error_log("PHOTO STR: ".$photo);
    if($photo == null){
        $photo = "person.png";
    }
    $date = new DateTime();
    $dateStr= $date->format('Y-m-d H:i:s');
    error_log(implode("", $_POST));
    //get time
    $sql = "INSERT INTO messages (recipient, photo, sender, fullname, content, time) VALUES ('".$_POST['recipient']."','".$photo."','".$_POST['sender']."','".$_POST['senderFL']."','".$_POST['content']."','".$dateStr."')";
    error_log("NEW MESSAGE: ".$sql);
    $con->query($sql);
      
}
else{
    error_log("No post data!", 0);
}

?>
