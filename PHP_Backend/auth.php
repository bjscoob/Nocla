<?php
     if(isset($_POST['name']) && isset($_POST['pswd'])){
        date_default_timezone_set("America/New_York");
        $con = mysqli_connect("localhost","###","###","###");
        if($con){
            $sql = "SELECT * FROM users WHERE username = '".$_POST['name']."'";
        $res = $con->query($sql) or die($con->error);
        $row = $res->fetch_assoc();
       
            if(!empty($row)){
                $hash = $row['password'];
                if (password_verify($_POST['pswd'], $hash)) {
                        //Create Auth Token
                         $bytes = random_bytes(20);
                         $token = bin2hex($bytes);
                         //Send Auth Token 
                         $result = $token;
                         //get user data
                         $result = $result."|".$row['id'];
                         $result = $result."|".$row['username'];
                         $result = $result."|".$row['email'];
                         $result = $result."|".$row['firstname'];
                         $result = $result."|".$row['lastname'];
                         $result = $result."|".$row['position'];
                         $result = $result."|".$row['shift'];
                         $result = $result."|".$row['manager'];
                         $result = $result."|".$row['pm_assignment'];
                         $result = $result."|".$row['status'];
                         $result = $result."|".$row['photo'];
                         
                         //Create Auth session
                         $expdate = date("Y-m-d H:i:s", strtotime('+12 hours'));
                         $sql = "INSERT INTO auth_sessions (Token, User, ExpDate, DeviceId) VALUES ('".$token."', '".$row['username']."', '".$expdate."', '".$_POST['deviceID']."')"; 
                         $res = $con->query($sql) or die($con->error);
                         
                } 
                else {$result = 'Invalid password.';}
            }
            
            else{$result = "Username not found."; }
            }
        
        else {$result = "Database connection failed.";}
    }
    else{ $result = "Please enter both fields.";}
    echo $result;?>
