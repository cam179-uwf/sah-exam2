#include "BluetoothSerial.h"
#include "esp_spp_api.h"

// Check if Bluetooth is available
#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

// Check Serial Port Profile
#if !defined(CONFIG_BT_SPP_ENABLED)
#error Serial Port Profile for Bluetooth is not available or not enabled. It is only available for the ESP32 chip.
#endif

enum class Direction
{
  Stopped,
  Forward,
  Backward,
  Left,
  Right
};

// some constants
const int SWITCH_DIRECTION_WAIT_TIME = 100; // in milliseconds
const int ACK = 1;
const int LEFT_SENSOR_SCREAM = 2;
const int RIGHT_SENSOR_SCREAM = 3;

// using the classic version of bluetooth:
// realized late in the process that I should 
// have used BLE to make my life so much simpler
BluetoothSerial SerialBT;

// what this device will call itself when broadcasting
String deviceName = "BoxCarESP32";

// all the pins for the motor controllers
// front motor controller pins
int ena_front = 13;
int in1_front = 12;
int in2_front = 14;
int enb_front = 25;
int in3_front = 27;
int in4_front = 26;

// back motor controller pins
int ena_back = 15;
int in1_back = 2;
int in2_back = 4;
int enb_back = 5;
int in3_back = 18;
int in4_back = 19;

// the pin for the LED that indicates if a 
// connection has been established via bluetooth
int connectionLed = 33; 
int rightIrSensor = 21;
int leftIrSensor = 32;

// booleans to remember which IR sensor was triggered
bool isRightSensorDetected = false;
bool isLeftSensorDetected = false;

// keeps track of our current speed
byte currentSpeed = 0;

// keeps track of what direction we are moving in
Direction currentDirection = Direction::Stopped;

// interrupt for the left IR sensor
void IRAM_ATTR OnLeftSensorDetected()
{
  isLeftSensorDetected = true;
}

// interrupt for the right IR sensor
void IRAM_ATTR OnRightSensorDetected()
{
  isRightSensorDetected = true;
}

void setup() 
{
  // Setup serial comm and bluetooth
  Serial.begin(115200);
  SerialBT.begin(deviceName);
  SerialBT.deleteAllBondedDevices();
  
  // register a bluetooth call back that will be raised when
  // a client is connected or disconnected
  SerialBT.register_callback(BluetoothEventsCallback);

  // all the pins setup for the motor controllers
  pinMode(ena_front, OUTPUT);
  pinMode(in1_front, OUTPUT);
  pinMode(in2_front, OUTPUT);

  pinMode(enb_front, OUTPUT);
  pinMode(in3_front, OUTPUT);
  pinMode(in4_front, OUTPUT);

  pinMode(ena_back, OUTPUT);
  pinMode(in1_back, OUTPUT);
  pinMode(in2_back, OUTPUT);

  pinMode(enb_back, OUTPUT);
  pinMode(in3_back, OUTPUT);
  pinMode(in4_back, OUTPUT);

  // LED pin setup
  pinMode(connectionLed, OUTPUT);
  
  // IR sensor pins setup
  pinMode(leftIrSensor, INPUT);
  pinMode(rightIrSensor, INPUT);

  // hardware interrupts for the left and right IR sensor pins
  // didn't get to debounce the signal so these interrupts actually fire twice
  attachInterrupt(digitalPinToInterrupt(leftIrSensor), OnLeftSensorDetected, FALLING);
  attachInterrupt(digitalPinToInterrupt(rightIrSensor), OnRightSensorDetected, FALLING);
}

void loop() 
{
  // if no client is connected do nothing
  if (!SerialBT.connected())
  {
    return;
  }
  
  // otherwise do something if the client is sending data
  if (SerialBT.available()) 
  {
    byte cmd = SerialBT.read();

    switch (cmd)
    {
      case 0: // cease operation... hard reset required
        StopMoving();
        SerialBT.write(ACK);
        Serial.println("Stop moving.");
        break;
      case 1: // move forward
        SetForward();
        SerialBT.write(ACK);
        Serial.println("Move forward.");
        break;
      case 2: // move backward
        SetBackward();
        SerialBT.write(ACK);
        Serial.println("Move backward.");
        break;
      case 3: // turn left
        SetLeft();
        SerialBT.write(ACK);
        Serial.println("Move left.");
        break;
      case 4: // turn right
        SetRight();
        SerialBT.write(ACK);
        Serial.println("Move right.");
        break;
      case 5: // speed changed
        if (SerialBT.available())
        {
          currentSpeed = SerialBT.read();
          SetSpeed(currentSpeed);
          SerialBT.write(ACK);
          Serial.println("Change speed to " + String(currentSpeed) + ".");
        }
        break;
    }
  }
  
  // if the left IR sensor is detected
  if (isLeftSensorDetected)
  {
    isLeftSensorDetected = false;
    SerialBT.write(LEFT_SENSOR_SCREAM);
    Serial.println("Left sensor detected.");
  }

  // if the right IR sensor is detected
  if (isRightSensorDetected)
  {
    isRightSensorDetected = false;
    SerialBT.write(RIGHT_SENSOR_SCREAM);
    Serial.println("Right sensor detected.");
  }

  delay(20);
}

// bluetooth callback
void BluetoothEventsCallback(esp_spp_cb_event_t event, esp_spp_cb_param_t *param)
{
  switch (event)
  {
    case ESP_SPP_SRV_OPEN_EVT: // client connected
      SerialBT.println("Connected");
      Serial.println("Connected");
      digitalWrite(connectionLed, HIGH);
      break;
    case ESP_SPP_CLOSE_EVT: // client disconnected
      StopMoving();
      SerialBT.flush();
      Serial.println("Client disconnected.");
      digitalWrite(connectionLed, LOW);
      break;
    default:
      break;
  }
}

// move the car forward with the currentSpeed
void SetForward()
{
  if (currentDirection == Direction::Forward) return;

  StopMoving(); // stop moving for a little bit so as to not break the motors
  currentDirection = Direction::Forward;

  delay(SWITCH_DIRECTION_WAIT_TIME);
  SetSpeed(currentSpeed);

  digitalWrite(in1_front, LOW);
  digitalWrite(in2_front, HIGH);

  digitalWrite(in3_front, LOW);
  digitalWrite(in4_front, HIGH);
  
  digitalWrite(in1_back, LOW);
  digitalWrite(in2_back, HIGH);

  digitalWrite(in3_back, LOW);
  digitalWrite(in4_back, HIGH);
}

// move the car backward with the currentSpeed
void SetBackward()
{
  if (currentDirection == Direction::Backward) return;

  StopMoving(); // stop moving for a little bit so as to not break the motors
  currentDirection = Direction::Backward;

  delay(SWITCH_DIRECTION_WAIT_TIME);
  SetSpeed(currentSpeed);

  digitalWrite(in1_front, HIGH);
  digitalWrite(in2_front, LOW);

  digitalWrite(in3_front, HIGH);
  digitalWrite(in4_front, LOW);
  
  digitalWrite(in1_back, HIGH);
  digitalWrite(in2_back, LOW);

  digitalWrite(in3_back, HIGH);
  digitalWrite(in4_back, LOW);
}

// turn the car left with the currentSpeed
void SetLeft()
{
  if (currentDirection == Direction::Left) return;

  StopMoving(); // stop moving for a little bit so as to not break the motors
  currentDirection = Direction::Left;

  delay(SWITCH_DIRECTION_WAIT_TIME);
  SetSpeed(currentSpeed);

  digitalWrite(in1_front, LOW);
  digitalWrite(in2_front, HIGH);

  digitalWrite(in3_front, HIGH);
  digitalWrite(in4_front, LOW);
  
  digitalWrite(in1_back, HIGH);
  digitalWrite(in2_back, LOW);

  digitalWrite(in3_back, LOW);
  digitalWrite(in4_back, HIGH);
}

// turn the car right with the currentSpeed
void SetRight()
{
  if (currentDirection == Direction::Right) return;

  StopMoving(); // stop moving for a little bit so as to not break the motors
  currentDirection = Direction::Right;

  delay(SWITCH_DIRECTION_WAIT_TIME);
  SetSpeed(currentSpeed);

  digitalWrite(in1_front, HIGH);
  digitalWrite(in2_front, LOW);
  
  digitalWrite(in3_front, LOW);
  digitalWrite(in4_front, HIGH);
  
  digitalWrite(in1_back, LOW);
  digitalWrite(in2_back, HIGH);
  
  digitalWrite(in3_back, HIGH);
  digitalWrite(in4_back, LOW);
}

// set the speed
void SetSpeed(const int& speed)
{
  if (currentDirection == Direction::Stopped) return;

  Serial.println("Motor speeds set to " + String(speed) + ".");
  analogWrite(ena_front, speed);
  analogWrite(enb_front, speed);
  analogWrite(ena_back, speed);
  analogWrite(enb_back, speed);
}

// stop the car from moving
void StopMoving()
{
  currentDirection = Direction::Stopped;
  analogWrite(ena_front, 0);
  analogWrite(enb_front, 0);
  analogWrite(ena_back, 0);
  analogWrite(enb_back, 0);
}