#include<Servo.h>
Servo servoY;
Servo servoX;
Servo servoSensor;

int motor_left[2] = {2, 3};
int motor_right[2] = {7, 8};
const int trigPin = 9;
const int echoPin = 5;
long duration;
int distance;

int steps = 2;
char inputstring[7]={'\0','\0','\0'};
String outputstring;
int bytesreceived = 0;
char MoveCommand= '0';
char CameraCommand= '0';
char Mode;
int Xangle = 90;
int Yangle = 5;

int SensorAngle = 90;

int data_valid ;



void setup() {
  // put your setup code here, to run once:

  int i;
  for(i = 0; i < 2; i++)
  {
    pinMode(motor_left[i], OUTPUT);
    pinMode(motor_right[i], OUTPUT);
  }
  servoY.attach(11);
  servoX.attach(10);
  servoSensor.attach(6);
  servoY.write(Yangle);
  servoX.write(Xangle);
  Serial.begin(9600);
 
 CameraCommand= '0';
 MoveCommand= '0';
 strcpy(inputstring,"0,0,Z");
 data_valid = 0;
 Mode='1';

}

void loop() {
  // put your main code here, to run repeatedly:
  distance = calculateDistance();
for (SensorAngle=90;SensorAngle<141; SensorAngle+=5){
  
servoSensor.write(SensorAngle);
distance = calculateDistance();
drive_forward();
delay(10);
while(distance<20){
  turn_right();
  delay(500);
  distance = calculateDistance();
}
}
for (SensorAngle=140;SensorAngle>89; SensorAngle-=5){
  
servoSensor.write(SensorAngle);
distance = calculateDistance();
drive_forward();
delay(10);
while(distance<20){
  motor_stop();
  turn_right();
  delay(500);
  distance = calculateDistance();
}
}
for (SensorAngle=90;SensorAngle>39 ; SensorAngle-=5){
servoSensor.write(SensorAngle);
distance = calculateDistance();
drive_forward();
delay(10);
while(distance<20){
  motor_stop();
  turn_left();
  delay(500);
  distance = calculateDistance();
}
}
for (SensorAngle=40;SensorAngle<91;  SensorAngle+=5){
servoSensor.write(SensorAngle);
distance = calculateDistance();
drive_forward();
delay(10);
while(distance<20){
  motor_stop();
  turn_left();
  delay(500);
  distance = calculateDistance();
}
}
}

void AutonomousMode(){
    
      int angle= SensorAngle;
      distance = calculateDistance();
      while(distance>30){
        
      for (SensorAngle=angle;SensorAngle<140; SensorAngle+=10){
       // drive_forward();
        servoSensor.write(SensorAngle);
        distance = calculateDistance();
        if(distance<30){
          motor_stop();
        }
        delay(5);
      }
      for (SensorAngle=149;SensorAngle>50 ; SensorAngle-=10){
        //drive_forward();
        servoSensor.write(SensorAngle);
        distance = calculateDistance();
         if(distance<30){
          motor_stop();
        }
        delay(5);
      }
      }
    if (SensorAngle<90){
      drive_backward();
      delay(1000);
      turn_left();
      delay(1000);
    }
    else if(SensorAngle>90){
      drive_backward();
      delay(1000);
      turn_right();
      delay(1000);
    }
    servoSensor.write(SensorAngle);
    distance = calculateDistance();
    
    while(distance>40 ){
      //drive_forward();
      distance = calculateDistance();
    }
    
}
void SerialInput(){
  
  //receive and save Serial input characters until a new line character is detected, or the char array is full.
  //if received message is greater than 14 characters
 if (Serial.available()>0 )
    { 
      bytesreceived = Serial.readBytesUntil (10,inputstring,15);
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
    delay(20);
    
}
int calculateDistance(){ 
  //cleat the trigPin 
  digitalWrite(trigPin, LOW); 
  delayMicroseconds(2);
  // Sets the trigPin on HIGH state for 10 micro seconds
  digitalWrite(trigPin, HIGH); 
  delayMicroseconds(10);
  digitalWrite(trigPin, LOW);
  duration = pulseIn(echoPin, HIGH); // Reads the echoPin, returns the sound wave travel time in microseconds
  //U(m/s)=dX(m)/dT(s) 
  //in this case Duration(time)= 2*Distance/SpeedOfSound=> 
  //Distance=SpeedOfSound*Duration/2
  // In dry air at 20 °C, the speed of sound is 343.2 m/s or 0.003432 m/Microsecond or 0,03434 cm/Microseconds
  distance= duration*0.034/2; 
  return distance;
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
