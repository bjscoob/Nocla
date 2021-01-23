
        
<?php 

    $result = "Login Successful";
   
    ?>
<html>
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
<body style="background-color:#003595;color:white;width:50%;">
    
    <script src="http://code.jquery.com/jquery-2.1.4.min.js"></script>
    <div style="position: absolute;
        left: 20%;
        margin-right: -20%;
        transform: translate(-20%, -20%) }">
    <br />
    <form id="login_form" action="login.php" method="post">
    <input type="hidden" id= "deviceID"  name = "deviceID" value = <?php echo '"'.$_GET['id'].'"'; ?> >
    <input type="text" id="name" class="blue_input" name="name" placeholder="Username"><br/><br/>
    <input type="password" id="pswd" class="blue_input" name="pswd" placeholder="Password"><br/>
    
    <br />
    <br />
     <button id="btn" type="button" onclick="javascript: invokeCSCode($('#name').val());">Login</button>
    <br />
    </form>
    <p id="result"></p>
    
        </div>
        <script type="text/javascript">function log(str) {
            $('#result').text($('#result').text() + " " + str);
        }

        function invokeCSCode(data) {
            var values = $('#login_form').serialize();

        $.ajax({
        url: "auth.php",
        type: "post",
        data: values ,
        success: function (response) {

          try {
                invokeCSharpAction(response);
            }
            catch (err) {
                log(err);
            }
        },
        error: function(jqXHR, textStatus, errorThrown) {
           console.log(textStatus, errorThrown);
        }
        });
            
            
        }
        function getCookie(cname) {
  var name = cname + "=";
  var decodedCookie = decodeURIComponent(document.cookie);
  var ca = decodedCookie.split(';');
  for(var i = 0; i <ca.length; i++) {
    var c = ca[i];
    while (c.charAt(0) == ' ') {
      c = c.substring(1);
    }
    if (c.indexOf(name) == 0) {
      return c.substring(name.length, c.length);
    }
  }
  return "";
}
        </script>
</body>
</html>
