

#include<Servo.h>

int motor_left[] = {2, 3};
int motor_right[] = {7, 8};

Servo servoY;
Servo servoX;

int steps = 2;
char inputstring[15]={'\0','\0','\0'};
String outputstring;
int bytesreceived = 0;
char MoveCommand;
char CameraCommand;
char Mode;
int Xangle = 90;
int Yangle = 90;
int data_valid ;
  

void setup()
{
  int i;
  for(i = 0; i < 2; i++)
  {
    pinMode(motor_left[i], OUTPUT);
    pinMode(motor_right[i], OUTPUT);
  }
  servoY.attach(11);
  servoX.attach(10);
	Serial.begin(9600);
 CameraCommand= '0';
 MoveCommand= '0';
 strcpy(inputstring,"0,0,Z");
 data_valid = 0;
}

void loop() {
  // put your main code here, to run repeatedly:

        

  //receive and save Serial input characters until a new line character is detected, or the char array is full.
  //if received message is greater than 14 characters
  if (Serial.available()>0 )
    { 
      bytesreceived = Serial.readBytesUntil (10,inputstring,8);
      //read bytes until it finds a 'Z' (90), or 11 characters are reached, save into inputstring
      inputstring[bytesreceived] = 0; //make last character 0
      
    }
        
    if ( inputstring[1]==',' && inputstring[6]=='Z')
    {
      data_valid = 1;
    }
    MoveCommand= inputstring[0];
    CameraCommand = inputstring[2];
    Mode = inputstring[4];
    if (data_valid == 1)
    {
      
        calculate_Camera();
        calculate_Movement();
        servoY.write(Yangle);
        servoX.write(Xangle);
      
      
    }
     
     
    outputstring = "A,"  + String(MoveCommand) + ','+CameraCommand+","+ String(data_valid)+ /*","+String(Yangle)+","+String(Xangle)+*/",Z" + '\n';
    
    
    Serial.print (outputstring);
  
    data_valid = 0;
    delay(20);
}


void AutonomousMode(){
    
}
void calculate_Camera(){
    if  (CameraCommand == 'L')
    {
      if(Xangle <180){ 
         Xangle+=steps;
      }                   
    }
    else if (CameraCommand == 'R')
    {
      if(Xangle >10){ 
        Xangle-=steps;
      }               
    }
    else if (CameraCommand == 'U')
    {
      if(Yangle >20){  
        Yangle-=steps;
      }              
    }
    else if (CameraCommand == 'D')
    {
        if(Yangle <145){ 
             Yangle+=steps;
          }          
    }
    else if (CameraCommand == 'S')
    {
      Yangle=90;   
      Xangle=90;             
    }
    else if (CameraCommand == '7')
    {
       if(Xangle <180){ 
         Xangle+=steps;
        }
        if(Yangle >5){  
          Yangle-=steps;
        }                
    }
    else if (CameraCommand == '9')
    {
      if(Xangle >5){ 
        Xangle-=steps;
      }
      if(Yangle >5){  
        Yangle-=steps;
      }             
    }
    else if (CameraCommand == '3')
    {
      if(Xangle >5){ 
        Xangle-=steps;
      }
      if(Yangle <180){ 
          Yangle+=steps;
      }
      servoY.write(Yangle);                 
    }
    else if (CameraCommand == '1')
    {
       
      if(Xangle <180){ 
           Xangle+=steps;
      }
      servoX.write(Xangle);
      if(Yangle <180){ 
           Yangle+=steps;
      }
    }
}



void calculate_Movement()
{
  if(MoveCommand == 'B')
  {
    drive_forward();
  }
  else if(MoveCommand == 'F')
  {
    drive_backward();
  }
  else if(MoveCommand == 'L')
  {
    turn_left();
  }
   else if(MoveCommand == 'R')
  {
    turn_right();
  }
   else if(MoveCommand == 'S')
  {
    motor_stop();
  }
  else
  {
    motor_stop();
  }
}


void motor_stop(){
  digitalWrite(motor_left[0], LOW); 
  digitalWrite(motor_left[1], LOW); 
  
  digitalWrite(motor_right[0], LOW); 
  digitalWrite(motor_right[1], LOW);
  delay(25);
}

void drive_forward(){
  digitalWrite(motor_left[0], HIGH); 
  digitalWrite(motor_left[1], LOW); 
  
  digitalWrite(motor_right[0], HIGH); 
  digitalWrite(motor_right[1], LOW); 
}

void drive_backward(){
  digitalWrite(motor_left[0], LOW); 
  digitalWrite(motor_left[1], HIGH); 
  
  digitalWrite(motor_right[0], LOW); 
  digitalWrite(motor_right[1], HIGH); 
}

void turn_left(){
  digitalWrite(motor_left[0], LOW); 
  digitalWrite(motor_left[1], HIGH); 
  
  digitalWrite(motor_right[0], HIGH); 
  digitalWrite(motor_right[1], LOW);
}

void turn_right(){
  digitalWrite(motor_left[0], HIGH); 
  digitalWrite(motor_left[1], LOW); 
  
  digitalWrite(motor_right[0], LOW); 
  digitalWrite(motor_right[1], HIGH); 
}

  


