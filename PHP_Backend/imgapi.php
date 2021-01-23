<?php 
date_default_timezone_set("America/New_York");
$con = mysqli_connect("localhost","jacksonb_api","admin","jacksonb_wrdp3");
//parameters send in via querystring
if (!isset($_GET['id']) || !isset($_GET['username']) ) {
die('{"status" : "Bad", "reason" : "Invalid Access"}');
}

$userID = $_GET['id'];
$isGood = false;
try{

$uploaddir = 'images/';
$fileName = basename($_FILES['fileToUpload']['name']);
$uploadfile = $uploaddir . basename($_FILES['fileToUpload']['name']);

//CHECK IF ITS AN IMAGE OR NOT
$allowed_types = array ('image/jpeg', 'image/png', 'image/bmp', 'image/gif' );
$fileInfo = finfo_open(FILEINFO_MIME_TYPE);
$detected_type = finfo_file( $fileInfo, $_FILES['fileToUpload']['tmp_name'] );
if ( !in_array($detected_type, $allowed_types) ) {
die ( '{"status" : "Bad", "reason" : "Not a valid image"}' );
}
//

if (move_uploaded_file($_FILES['fileToUpload']['tmp_name'], $uploadfile)) {
//echo "File is valid, and was successfully uploaded.\n";

$isGood = true;
$sql = "UPDATE users SET photo='".$fileName."' WHERE id='".$_GET['id']."'";
if(!($result = $con->query($sql))){
        error_log($con->error);
        
        echo "Database connection error.";
        
    }
    else{
        echo 'Success! '. $fileName .' is uploaded.';
    }
} else {
//echo "Possible file upload attack!\n";
echo '{"status" : "Bad", "reason" : "Unable to Upload Profile Image"}'.$uploadfile;
print_r($_FILES);
}

}
catch(Exception $e) {
echo '{"status" : "Bad", "reason" : "'.$e->getMessage().'"}';
}
?>