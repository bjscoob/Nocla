
<?php 
require "functions.php";
function showResetForm($username, $id){
            echo '
    
    <div style="position: absolute;
        left: 20%;
        margin-right: -20%;
        transform: translate(-20%, -20%) }">
    <br />
    <h1>Hello, '.$username.' </h1><br/><h1>Please Update Your Password</h1><br/>
    <form id="reset_form" action="rp.php" method="post">
    <input type="hidden" id= "username"  name = "username" value = '.$username.' >
    <input type="hidden" id= "userID"  name = "userID" value = '.$id.' >
    <input type="password" id="pswd1" class="blue_input" name="pswd1" placeholder="Password"><br/><br/>
    <input type="password" id="pswd2" class="blue_input" name="pswd2" placeholder="Password Again"><br/>
    
    <br />
    <br />
     <input type="submit" style ="color:#003595;" id="btn" type="button" value = "Send" ></button>
    <br />
    </form>';
}
$no_email = true;
//check for email
if(isset($_GET['email'])){
    $no_email = false;
        echo sendResetEmail($_GET['email']);
}
else{
   echo  '<html>
    <style>
    ::placeholder{
        color: #D3D3D3;
        
    }
        .blue_input{
            border-radius: 25px;
            border: 2px solid #038be9;
            background: #026ac8;
            color: white;
            text-align:center;
            width: 200px;
            height: 30px;
        }
        #btn{
            margin-left: 50px;
            background: white;
            border-radius: 25px;
            color:#003595 ;
            border: 2px solid white;
            width: 100px;
            height: 30px;
        }
        
    </style>
<body style="background-color:#003595;color:white;width:50%;">';
}
//check for token
if(isset($_GET['t'])){
    //Check if token exists in system
    if(checkAuthToken($_GET['t'])){
        showResetForm($_SESSION['user']['username'],$_SESSION['user']['id']);
    }
    //Otherwise, shoot not found message
    else{
        echo '<h1>Invalid Token.</h1>';
    }
}


    //check for completion
    if(isset($_POST['pswd1']) && isset($_POST['pswd2'])){
        $hash = password_hash($_POST['pswd1'], PASSWORD_DEFAULT);
        if(changePassword($_POST['userID'], $hash)){
            echo'<h1>Success! Password Changed.</h1><br/>';
        }
        else{
             echo'<h1>Sorry! Something went wrong.</h1><br/>';
             var_dump($_POST);
             
        showResetForm($_POST['username'],$_POST['userID']);
        }
   
}
if($no_email){echo '
</body>
</html>';}

?>