#include "Servo.h"
Servo servo1;
Servo servo2;
int angle = 0;
int angle2 = 0;

void setup() {
  Serial.begin(9600);
  servo1.attach(11);
  servo2.attach(10);
}

void loop() {
  delay(7000);
  servo2.write(120);
  int m = 0;
  for (int i = 0;i < 180;i++){
    servo1.write(i);
    for (int j = 0;j < 3;j++){
      if (m < analogRead(A0)){m = analogRead(A0); angle = i;}
      delay(10);
    }
    delay(50);
    Serial.println(analogRead(A0));
  }
  servo1.write(angle);
  int n = 0;
  for (int i = 120;i > 50;i--){
    servo2.write(i);
    for (int j = 0;j < 3;j++){
      if (n < analogRead(A0)){n = analogRead(A0); angle2 = i;}
      delay(10);
    }
    delay(50);
    Serial.println(analogRead(A0));
  }
  servo2.write(angle2);
  Serial.println(angle);
  Serial.println(m);
  delay(1000000000000);
}