<?php 
date_default_timezone_set("America/New_York");
$con = mysqli_connect("localhost","jacksonb_api","admin","jacksonb_wrdp3");
if($con){
//Activities Retrieval
 if(isset($_POST['actGET'])){
      $sql = 'SELECT * FROM activities WHERE pm_no = '.$_POST['pm'].' AND activity_time > now() - INTERVAL 2 week ORDER BY activities.activity_time DESC';
      $result = $con->query($sql) or die($conn->error);;
      $ACTIVITIES = array();
      while($row = $result->fetch_assoc()) {
        array_push($ACTIVITIES, $row);
        }
        $myJSON = json_encode($ACTIVITIES);
        echo $myJSON;
 }
//Activitiy POST
   if(isset($_POST['actPOST'])){
        $date = new DateTime();
        $now = new DateTime();
        $hour = $_POST['hour'];
        $min = $_POST['min'];
        $sec = $_POST['sec'];
        $date->setTime($hour, $min, $sec);
        //checks if given time is greater than current time, if so subtract one day
        if($now <= $date){
            $oneday = new DateInterval('P1D');
            $oneday->invert = 1;
            $date->add($oneday);
        }
        //INSERT INTO MyGuests (firstname, lastname, email) VALUES ('John', 'Doe', 'john@example.com')"
        $sql = "INSERT INTO activities (pm_no,content, activity_time, author) VALUES ('".$_POST['pm_no']."','".$_POST['content']."','".$date->format("Y-m-d H:i:s")."','".$_POST['author']."')";
        error_log("INSERT ACTIVITY: ".$sql);
        if(!$result = $con->query($sql)){
          echo("Error: ".$con->error."  ".$sql);
          error_log("Error: ".$con->error."  ".$sql);
      }
    }
//RM Retrieval
    if(isset($_POST['rmGET'])){
        $sql = 'SELECT * FROM material_assignments WHERE isCurrent = 1 AND pm = '.$_POST['pm'];
        $result = $con->query($sql);
        $RMs = array();
        while($row = $result->fetch_assoc()){
            array_push($RMs, $row);
            
        }
        $myJSON = json_encode($RMs);
        echo $myJSON;
    }
//RM POST
    if(isset($_POST['rm'])){
        $pm = $_POST['rm'];
        $material =  $_POST['material'];
        $batchno =  $_POST['batchno'];
        $bagno =  $_POST['bagno'];
        //$expdate =date('d M Y',$_POST['expdate'] ) ;
        $expdate = $_POST['expdate'];
        $exptime = "00:00:00";
        $assigndate = date("d M Y");
        $assigntime = date("H:i:s");
        //check if already expired
        if($expdate <  $assigndate){
            echo "Expired";
            return;
        }
        //check 5 days
        $possibledate = date('d M Y', strtotime('+5 days'));
        if($expdate > $possibledate){
            $expdate = $possibledate;
            $exptime = $assigntime;
        }
        //clear all other rm instances
        $sql = "UPDATE material_assignments 
                SET iscurrent = 0
                WHERE material = '".$material."' AND pm=".$pm;
        $con->query($sql);
         $sql = "INSERT INTO material_assignments 
    (pm,material,batchno,bagno,assigndate,assigntime,expdate,exptime,iscurrent) 
    VALUES(".$pm.",'".$material."','".$batchno."',".$bagno.",'".$assigndate."','".$assigntime."','".$expdate."','".$exptime."',1)";
    error_log($sql);
       if(!$result = $con->query($sql)){
          error_log("RM POST Error: ".$con->error);
          echo $con->error;
      }
      else{
          echo "Success!";
      }
      
      
    }
//SPC TIME POST 
    if(isset($_POST['spcPOST'])){
        $date = new DateTime();
        $now = new DateTime();
        $hour = $_POST['hour'];
        $min = $_POST['min'];
        $date->setTime($hour, $min, 00);
        //checks if given time is less than current time, if so add one day
        if($now >= $date){
            $date->add(new DateInterval('P1D'));
        }
        $sql = "UPDATE spc_times SET time ='".$date->format("d M Y H:i:s")."' WHERE pm_no =".$_POST['pm']." and spc_no =".$_POST['slot'];
        $con->query($sql);
    }
    
//SPC GET 
    if(isset($_POST['spcGET'])){
        $sql = 'SELECT * FROM spc_times WHERE pm_no ='.$_POST['pm'];
      if(!$result = $con->query($sql)){
          echo("Error: ".$con->error."  ".$sql);
      }
      
      $TIMES = array();
      while($row = $result->fetch_assoc()) {
        array_push($TIMES, $row);
        }
        $myJSON = json_encode($TIMES);
        echo $myJSON;
    }
//SPC CHECK
if(isset($_POST['spcCHK'])){
        $sql = "UPDATE spc_times SET isPulled =".$_POST['isChecked']." WHERE pm_no =".$_POST['pm']." and spc_no =".$_POST['slot'];
        error_log("SPC CHECK: ".$sql);
      if(!$result = $con->query($sql)){
          error_log("Error: ".$con->error);
      }
      
    }
//SPC CLEAR
if(isset($_POST['spcCLR'])){
        $sql = "UPDATE spc_times SET isPulled =0, time = null WHERE pm_no =".$_POST['pm']." and spc_no =".$_POST['slot'];
        error_log("SPC CHECK: ".$sql);
      if(!$result = $con->query($sql)){
          error_log("Error: ".$con->error);
      }
      
    }

//CHANGE NAME
if(isset($_POST['nameChange'])){
     $id = $_POST['nameChange'];
     $first =$_POST['first'];
     $last=$_POST['second'];
     $sql = "UPDATE users SET firstname ='".$first."' , lastname ='".$last."' WHERE id =".$id;
    if(!$result = $con->query($sql)){
          echo("Error: ".$con->error."  ".$sql);
          error_log("Error: ".$con->error."  ".$sql);
      }
      else{
          echo "Success ".$sql;
      }
     
 }
//CHANGE EMAIL
  if(isset($_POST['emailChange'])){
     $id = $_POST['emailChange'];
     $email =$_POST['email'];
     $sql = "UPDATE users SET email ='".$email."' WHERE id =".$id;
    if(!$result = $con->query($sql)){
          echo("Error: ".$con->error."  ".$sql);
          error_log("Error: ".$con->error."  ".$sql);
      }
      else{
          echo "Success ".$sql;
      }
     
 }
}
else{
    echo "Database connection failed";
}
?>